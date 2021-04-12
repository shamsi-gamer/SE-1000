using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class Parameter : Setting
        {
            float            m_value;

            public float     CurValue = 0,
                             
                             Default,
                             
                             Min,
                             Max,
                             
                             NormalMin,
                             NormalMax,
                             
                             Delta,
                             BigDelta;

            public Parameter Trigger;
            public Envelope  Envelope;
            public LFO       Lfo;
            public Modulate  Modulate;


            public Parameter(string tag, float min, float max, float normalMin, float normalMax, float delta, float bigDelta, float defVal, Setting parent) : base(tag, parent)
            {
                Tag       = tag;

                m_value   = defVal;
                Default   = defVal;

                Min       = min;
                Max       = max;

                NormalMin = normalMin;
                NormalMax = normalMax;
                          
                Delta     = delta;
                BigDelta  = bigDelta;

                Trigger   = null;
                Envelope  = null;
                Lfo       = null;
                Modulate  = null;
            }


            public Parameter(Parameter param, Setting parent, string tag = "", bool copy = true) 
                : base(tag != "" ? tag : param.Tag, parent, param.Prototype)
            {
                m_value   = param.m_value;

                Min       = param.Min;
                Max       = param.Max;

                NormalMin = param.NormalMin;
                NormalMax = param.NormalMax;

                Delta     = param.Delta;
                BigDelta  = param.BigDelta;

                Trigger   = copy ? param.Trigger ?.Copy(this) : null;
                Envelope  = copy ? param.Envelope?.Copy(this) : null;
                Lfo       = copy ? param.Lfo     ?.Copy(this) : null;
                Modulate  = copy ? param.Modulate?.Copy(this) : null;
            }


            public Parameter Copy(Setting parent) => new Parameter(this, parent);


            public float Value { get { return m_value; } }


            public void SetValue(float val, Note note, int src)
            {
                if (note != null)
                {
                    var key = note.Keys.Find(k => k.Path == GetPath(src));

                    if (key != null) key.Value = MinMax(Min, val, Max);
                    else             m_value   = MinMax(Min, val, Max);
                }
                else m_value = MinMax(Min, val, Max);
            }


            public float GetValue(TimeParams tp)
            {
                if (tp.Program.TooComplex) return CurValue;


                var value = GetKeyValue(tp.Note, tp.SourceIndex);
                var path  = GetPath(tp.SourceIndex);

                if (   /*tp.TriggerValues.Find(v => v.Path == path) == null
                    &&*/ Lfo != null)
                {
                    //var _tp = new TimeParams(tp);

                    //_tp.LocalTime  = (long)(Lfo.Time.Value * FPS);
                    //_tp.GlobalTime = _tp.LocalTime;

                    var lfo = Lfo.GetValue(tp);//_tp);

                    if (Lfo.Op == LFO.LfoOp.Add) value += lfo * Math.Abs(Max - Min) / 2;
                    else                         value *= lfo;

                    //if (   ParentIsEnvelope
                    //    || Parent.Tag == "Trig")
                    //    tp.TriggerValues.Add(new TriggerValue(path, value));
                }

                
                if (Envelope != null)
                    value *= Envelope.GetValue(tp);


                float auto;

                if (   Tag == "Tune"
                    || Tag == "Off") auto = 0;
                else                 auto = 1;


                if (tp.Note != null)
                { 
                    var val = GetAutoValue(
                        tp.Note.PatStep, 
                        tp.Note.PatIndex, 
                        tp.Note.iChan, 
                        path);
                    
                    if (OK(val))
                    {
                        if (   Tag == "Tune"
                            || Tag == "Off") auto += val;
                        else                 auto *= val;
                    }

                    if (   Tag == "Tune"
                        || Tag == "Off") value += auto;
                    else                 value *= auto;
                }

                return CurValue = MinMax(Min, value, Max);
            }


            public float GetKeyValue(Note note, int src)
            {
                if (note != null)
                {
                    var key = note.Keys.Find(k => k.Path == GetPath(src));
                    return key?.Value ?? m_value; 
                }

                else return m_value;
            }


            public static float GetAutoValue(float songStep, int pat, int ch, string path)
            {
                var prevKey = PrevSongAutoKey(songStep, pat, ch, path);
                var nextKey = NextSongAutoKey(songStep, pat, ch, path);

                     if (prevKey == null && nextKey == null) return fN;
                else if (prevKey != null && nextKey == null) return prevKey.Value;
                else if (prevKey == null && nextKey != null) return nextKey.Value;
                else
                    return prevKey.Value + (nextKey.Value - prevKey.Value) * (songStep - prevKey.StepTime) / (nextKey.StepTime - prevKey.StepTime);
            }


            public float AdjustValue(float value, float delta, bool shift)
            {
                     if (Tag == "Att") ((Envelope)Parent).TrigAttack  = fN;
                else if (Tag == "Dec") ((Envelope)Parent).TrigDecay   = fN;
                else if (Tag == "Sus") ((Envelope)Parent).TrigSustain = fN;
                else if (Tag == "Rel") ((Envelope)Parent).TrigRelease = fN;

                var dv = delta * (shift ? BigDelta : Delta);

                if (Tag == "Freq")
                { 
                    dv *= (float)Math.Pow(Math.Sqrt(2), value);

                    var _delta = 1f/FPS * (value + dv);
                    
                    ((LFO)Parent).Delta  = _delta;
                }

                return value + dv;
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.RotationIndicator.X != 0) 
                    prog.AdjustFromController(song, this, -g_remote.RotationIndicator.X/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Trig": return Trigger  ?? (Trigger  = (Parameter)NewSettingFromTag(tag, this));
                    case "Env":  return Envelope ?? (Envelope = new Envelope(this));
                    case "LFO":  return Lfo      ?? (Lfo      = new LFO     (this));
                    case "Mod":  return Modulate ?? (Modulate = new Modulate(this));
                }

                return null;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Trigger  != null
                    || Envelope != null
                    || Lfo      != null
                    || (chan?.HasKeys(GetPath(src)) ?? false)
                    || _IsCurrent;
            }


            public override void Remove(Setting setting)
            {
                     if (setting == Trigger)  Trigger  = null;
                else if (setting == Envelope) Envelope = null;
                else if (setting == Lfo)    
                {
                    g_lfo.Remove(Lfo);
                    Lfo = null; 
                }
            }


            public override void Clear()
            {
                m_value = Default;

                Trigger ?.Clear();
                Envelope?.Clear();
                Lfo     ?.Clear();

                Trigger  = null;
                Envelope = null;

                if (Lfo != null) g_lfo.Remove(Lfo);
                Lfo = null;
            }


            public override void Randomize(Program prog)
            {
                m_value = NormalMin + RND * (NormalMax - NormalMin);
                
                if (   !prog.TooComplex
                    && !AnyParentIsEnvelope
                    && (  !IsDigit(Tag[0]) && RND > 0.5f
                        || IsDigit(Tag[0]) && RND > 0.9f))
                {
                    Envelope = new Envelope(this);
                    Envelope.Randomize(prog);
                }
                else 
                    Envelope = null;


                if (   !prog.TooComplex
                    //&& !HasTagOrParent(this, "Att")
                    //&& !HasTagOrParent(this, "Dec")
                    //&& !HasTagOrParent(this, "Sus")
                    //&& !HasTagOrParent(this, "Rel")
                    && RND > 0.8f)
                {
                    Lfo = new LFO(this);
                    Lfo.Randomize(prog);
                }
                else
                { 
                    if (Lfo != null)
                        g_lfo.Remove(Lfo);

                    Lfo = null;
                }
            }


            public void Delete(Song song, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                foreach (var pat in song.Patterns)
                {
                    foreach (var chan in pat.Channels)
                    {
                        chan.AutoKeys = chan.AutoKeys.FindAll(k => k.Path != GetPath(iSrc));

                        foreach (var note in chan.Notes)
                            note.Keys = note.Keys.FindAll(k => k.Path != GetPath(iSrc));
                    }
                }

                Trigger ?.Delete(song, iSrc);
                Envelope?.Delete(song, iSrc);
                Lfo     ?.Delete(song, iSrc);
                Modulate?.Delete(song, iSrc);
            }


            public override string Save()
            {
                return
                      W(Tag)

                    + S(m_value)

                    + Program.Save(Trigger)
                    + Program.Save(Envelope)
                    + Program.Save(Lfo)
                    + Program.Save(Modulate);
            }


            public static Parameter Load(string[] data, ref int i, Instrument inst, int iSrc, Setting parent, Parameter proto = null)
            {
                var tag = data[i++];

                Parameter param;

                     if (proto  != null) param = proto;                                                    
                else if (parent != null) param = (Parameter)parent.GetOrAddSettingFromTag(tag);            
                else if (iSrc > -1)      param = (Parameter)inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
                else                     param = (Parameter)inst.GetOrAddSettingFromTag(tag);              

                param.m_value = float.Parse(data[i++]);

                while (i < data.Length
                    && (   data[i] == "Trig" 
                        || data[i] == "Env" 
                        || data[i] == "LFO" 
                        || data[i] == "Mod"))
                {
                    switch (data[i])
                    { 
                        case "Trig": param.Trigger  =           Load(data, ref i, inst, iSrc, param); break;
                        case "Env":  param.Envelope = Envelope .Load(data, ref i, inst, iSrc, param); break;
                        case "LFO":  param.Lfo      = LFO      .Load(data, ref i, inst, iSrc, param); break;
                        case "Mod":  param.Modulate = Modulate .Load(data, ref i, inst, iSrc, param); break;
                    }
                }

                return param;
            }


            public override void GetLabel(out string str, out float width)
            {
                if (Tag == "Vol")
                {
                    width = 72f;
                    str   = printValue(100 * Math.Log10(Value), 0, true, 0).PadLeft(4);
                }
                else
                {
                    width = 85f; 
                    str   = printValue(CurValue, 2, true, 1).PadLeft(6);
                }
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Trigger  != null) Trigger .DrawLabels(sprites, x, y, dp); 
                if (Envelope != null) Envelope.DrawLabels(sprites, x, y, dp); 
                if (Lfo      != null) Lfo     .DrawLabels(sprites, x, y, dp); 

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {   
                var valWidth  =  60;
                var valHeight = 180;

                if (   Tag == "Att"
                    || Tag == "Dec"
                    || Tag == "Sus"
                    || Tag == "Rel"
                    || Tag == "Amp"
                    || Tag == "Freq"
                    ||    Parent != null // LFO offset has a parent
                       && Tag == "Off"
                    || Tag == "Dry"
                    || Tag == "Cnt"
                    || Tag == "Time"
                    || Tag == "Lvl"
                    || Tag == "Pow"
                    || Tag == "Cut"
                    || Tag == "Res"
                    || Tag == "Shrp")
                {
                    Parent.DrawSetting(sprites, x, y, w, h, dp);
                }
                else if (Tag == "Vol")
                {
                    DrawSoundLevel(
                        sprites, 
                        x + w/2 - valWidth, 
                        y + h/2 - valHeight/2, 
                        valWidth, 
                        valHeight, 
                        Value, 
                        CurValue);

                    DrawString(
                        sprites,
                        printValue(100 * Math.Log10(Value), 0, true, 0) + " dB", 
                        x + w/2 + 10, 
                        y + h/2 + valHeight/2 - 30,
                        1f, 
                        color6);
                }
                else if (Parent == null // source offset has no parent
                      && Tag    == "Off")
                { 
                    DrawValueHorizontal(
                        sprites, 
                        x + w/2 - valHeight/2, // this is deliberately backwards
                        y + h/2 - valWidth /2,
                        valHeight,
                        valWidth, 
                        Min, Max, 
                        Value, CurValue, 
                        Tag);
                }
                else
                { 
                    DrawValueVertical(
                        sprites, 
                        x + w/2 - valWidth, 
                        y + h/2 - valHeight/2,
                        valWidth,
                        valHeight, 
                        Min, Max, 
                        Value, CurValue, 
                        Tag, 
                        false);
                }
            }


            public void DrawSettingValues(List<MySprite> sprites, float x, float y, float w, float h, float vol, Program prog)
            {
                var bx = 40;
                var by = 55;

                if (Tag == "Vol")
                {
                    DrawSoundLevel(sprites, x + bx + 20, y + by + 90, 60, 120, Value, CurValue);

                    var db = printValue(100 * Math.Log10(Value), 0, true, 0) + " dB";
                    DrawString(sprites, db, x + bx + 100, y + by + 180, 1f, color6);
                }
                else if (Parent == null // source offset has no parent
                      && Tag    == "Off")
                    DrawValueHorizontal(sprites, x + bx +  5, y + by + 90, 180,  50, Min, Max, Value, CurValue, Tag);
                else
                    DrawValueVertical  (sprites, x + bx + 20, y + by + 90,  60, 120, Min, Max, Value, CurValue, Tag, false);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                if (   !AnyParentIsEnvelope
                    && !HasTagOrAnyParent(this,"Trig"))
                {
                    DrawFuncButton(sprites, "Trig", 0, w, h, true, Trigger  != null);
                    DrawFuncButton(sprites, "Env",  1, w, h, true, Envelope != null);
                }

                if (   !AnyParentIsEnvelope
                    && Tag != "Vol"
                    && (   Parent == null
                        || Parent.GetType() != typeof(Harmonics)))
                    DrawFuncButton(sprites, "X", 5, w, h, false, false, mainPressed.Contains(5));

                DrawFuncButton(sprites, "LFO",  2, w, h, true, Lfo != null);

                DrawFuncButton(sprites, "Key",  3, w, h, true, chan.HasNoteKeys(GetPath(CurSrc)));
                DrawFuncButton(sprites, "Auto", 4, w, h, true, chan.HasAutoKeys(GetPath(CurSrc)));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                case 0:
                    if (   AnyParentIsEnvelope
                        || HasTagOrAnyParent(this, "Trig")) break;
                    AddNextSetting("Trig");
                    break;

                case 1:
                    if (AnyParentIsEnvelope) break;
                    AddNextSetting("Env");
                    break;

                case 2:
                    AddNextSetting("LFO");
                    break;

                case 3: g_paramKeys = true; UpdateChordLights(); break;
                case 4: g_paramAuto = true; UpdateChordLights(); break;

                case 5: 
                    if (   ParentIsEnvelope

                        || Tag == "Amp"
                        || Tag == "Freq"

                        ||    Parent != null
                           && Tag == "Off"
                       
                        ||    Parent != null
                           && Parent.GetType() == typeof(Harmonics)

                        || Tag == "Cut"
                        || Tag == "Res"

                        || Tag == "Len"
                        || Tag == "Scl")
                        break;

                    RemoveSetting(this); 
                    break;
                }
            }


            public bool ParentIsEnvelope { get 
            {
                return
                       Tag == "Att"
                    || Tag == "Dec"
                    || Tag == "Sus"
                    || Tag == "Rel";
            } }


            public bool AnyParentIsEnvelope { get 
            {
                return
                       HasTagOrAnyParent(this, "Att")
                    || HasTagOrAnyParent(this, "Dec")
                    || HasTagOrAnyParent(this, "Sus")
                    || HasTagOrAnyParent(this, "Rel");
            } }
        }


        static Parameter NewHarmonicParam(int i, Setting parent)
        {
            return new Parameter(S(i), 0, 1, 0.1f, 0.9f, 0.01f, 0.1f, i == 0 ? 1 : 0, parent);
        }
    }
}
