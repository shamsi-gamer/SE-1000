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
            protected float  m_value;

            public float     PrevValue = 0,
                             CurValue  = 0,
                             
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


            public Parameter(string tag, float min, float max, float normalMin, float normalMax, float delta, float bigDelta, float defVal, Setting parent, Instrument inst, Source src) 
                : base(tag, parent, null, inst, src)
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
                : base(tag != "" ? tag : param.Tag, parent, param.Prototype, param.Instrument, param.Source)
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
                else
                {
                    m_value  = MinMax(Min, val, Max);
                    CurValue = m_value;
                }
            }


            public void LimitValue(ref float val)
            {
                val = MinMax(Min, val, Max);
            }


            public float UpdateValue(TimeParams tp)
            {
                if ( /*m_valid
                    ||*/tp.Program.TooComplex) 
                    return CurValue;


                var value = GetKeyValue(tp.Note, tp.SourceIndex);
                var path  = GetPath(tp.SourceIndex);

                if (   tp.TriggerValues.Find(v => v.Path == path) == null
                    && Lfo != null)
                {
                    var lfo = Lfo.UpdateValue(tp);

                    if (Lfo.Op == ModOp.Add) value += lfo * Math.Abs(Max - Min) / 2;
                    else                     value *= lfo;

                    if (ParentIsEnvelope)
                        tp.TriggerValues.Add(new TriggerValue(path, MinMax(Min, value, Max)));
                }

                if (Modulate != null)
                {
                    var mod = Modulate.UpdateValue(tp);

                    if (Modulate.Op == ModOp.Add) value += mod * Math.Abs(Max - Min) / 2;
                    else                          value *= mod;
                }

                if (Envelope != null)
                    value *= Envelope.UpdateValue(tp);


                float auto;

                if (   Tag == strTune
                    || Tag == strOff) auto = 0;
                else                  auto = 1;


                if (tp.Note != null)
                { 
                    var val = GetAutoValue(
                        tp.Note.PatStep, 
                        tp.Note.PatIndex, 
                        tp.Note.iChan, 
                        path);
                    
                    if (OK(val))
                    {
                        if (   Tag == strTune
                            || Tag == strOff) auto += val;
                        else                  auto *= val;
                    }

                    if (   Tag == strTune
                        || Tag == strOff) value += auto;
                    else                  value *= auto;
                }

                CurValue = MinMax(Min, value, Max);
                m_valid  = true;

                return CurValue;
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


            public float AdjustValue(float value, float delta, bool shift, bool scale = false)
            {
                     if (Tag == strAtt
                      && HasTag(Parent, strEnv)) ((Envelope)Parent).TrigAttack  = fN;
                else if (Tag == strDec)          ((Envelope)Parent).TrigDecay   = fN;
                else if (Tag == strSus)          ((Envelope)Parent).TrigSustain = fN;
                else if (Tag == strRel
                      && HasTag(Parent, strEnv)) ((Envelope)Parent).TrigRelease = fN;

                if (scale)
                {
                    var dv = 
                        shift
                        ? (delta > 0 ? 1 + BigDelta : 1 - BigDelta)
                        : (delta > 0 ? 1 +    Delta : 1 -    Delta);

                    return value * dv;
                }
                else
                {
                    var dv = delta * (shift ? BigDelta : Delta);

                    if (Tag == strFreq)
                    { 
                        dv *= (float)Math.Pow(Math.Sqrt(2), value);
                        var _delta = 1f/FPS * (value + dv);
                        ((LFO)Parent).Delta = _delta;
                    }

                    return value + dv;
                }
            }


            public override void AdjustFromController(Clip clip, Program prog)
            {
                if (g_remote.RotationIndicator.X != 0) 
                    prog.AdjustFromController(clip, this, -g_remote.RotationIndicator.X/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strTrig: return GetOrAddParamFromTag(Trigger, tag);
                    case strEnv:  return Envelope ?? (Envelope = new Envelope(this, Instrument, Source));
                    case strLfo:  return Lfo      ?? (Lfo      = new LFO     (this, Instrument, Source));
                    case strMod:  return Modulate ?? (Modulate = new Modulate(this, Instrument, Source));
                }

                return null;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Trigger  != null
                    || Envelope != null
                    || Lfo      != null
                    || Modulate != null
                    || (chan?.HasKeys(GetPath(src)) ?? false)
                    || _IsCurrent;
            }


            public override void DeleteSetting(Setting setting)
            {
                     if (setting == Trigger )                           Trigger  = null;
                else if (setting == Envelope)                           Envelope = null;
                else if (setting == Lfo     ) { g_lfo.Remove(Lfo);      Lfo      = null; }
                else if (setting == Modulate) { g_mod.Remove(Modulate); Modulate = null; }
            }


            public override void Clear()
            {
                m_value = Default;

                Trigger ?.Clear();
                Envelope?.Clear();
                Lfo     ?.Clear();
                Modulate?.Clear();

                Trigger  = null;
                Envelope = null;

                if (Lfo != null) g_lfo.Remove(Lfo);
                Lfo = null;

                if (Modulate != null) g_mod.Remove(Modulate);
                Modulate = null;
            }


            public override void Reset()
            {
                base.Reset();

                Trigger ?.Reset();
                Envelope?.Reset();
                Lfo     ?.Reset();
                Modulate?.Reset();
            }


            public override void Randomize(Program prog)
            {
                m_value = NormalMin + RND * (NormalMax - NormalMin);
                
                if (   !prog.TooComplex
                    && !AnyParentIsEnvelope
                    && (  !IsDigit(Tag[0]) && RND > 0.5f
                        || IsDigit(Tag[0]) && RND > 0.9f))
                {
                    Envelope = new Envelope(this, Instrument, Source);
                    Envelope.Randomize(prog);
                }
                else 
                    Envelope = null;


                if (   !prog.TooComplex
                    && RND > 0.8f)
                {
                    Lfo = new LFO(this, Instrument, Source);
                    Lfo.Randomize(prog);
                }
                else
                { 
                    if (Lfo != null)
                        g_lfo.Remove(Lfo);

                    Lfo = null;
                }
            }


            public void Delete(Clip clip, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                foreach (var pat in clip.Patterns)
                {
                    foreach (var chan in pat.Channels)
                    {
                        chan.AutoKeys = chan.AutoKeys.FindAll(k => k.Path != GetPath(iSrc));

                        foreach (var note in chan.Notes)
                            note.Keys = note.Keys.FindAll(k => k.Path != GetPath(iSrc));
                    }
                }

                Trigger ?.Delete(clip, iSrc);
                Envelope?.Delete(clip, iSrc);
                Lfo     ?.Delete(clip, iSrc);
                Modulate?.Delete(clip, iSrc);
            }


            public override string Save()
            {
                var nSettings = 0;
                
                if (Trigger  != null) nSettings++;
                if (Envelope != null) nSettings++;
                if (Lfo      != null) nSettings++;
                if (Modulate != null) nSettings++;

                return
                      W(Tag)

                    + WS(m_value.ToString("0.######")) 
                    +  S(nSettings)

                    + Program.SaveSetting(Trigger)
                    + Program.SaveSetting(Envelope)
                    + Program.SaveSetting(Lfo)
                    + Program.SaveSetting(Modulate);
            }


            public static Parameter Load(Session session, string[] data, ref int i, Instrument inst, int iSrc, Setting parent, Parameter proto = null)
            {
                var tag = data[i++];

                Parameter param;

                     if (proto  != null) param = proto;                                                    
                else if (parent != null) param = (Parameter)parent.GetOrAddSettingFromTag(tag);            
                else if (iSrc > -1)      param = (Parameter)inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
                else                     param = (Parameter)inst.GetOrAddSettingFromTag(tag);              

                param.m_value = float.Parse(data[i++]);

                var nSettings = int.Parse(data[i++]);

                while (nSettings-- > 0)
                {
                    switch (data[i])
                    { 
                        case strTrig: param.Trigger  =           Load(session, data, ref i, inst, iSrc, param); break;
                        case strEnv:  param.Envelope = Envelope .Load(session, data, ref i, inst, iSrc, param); break;
                        case strLfo:  param.Lfo      = LFO      .Load(session, data, ref i, inst, iSrc, param); break;
                        case strMod:  param.Modulate = Modulate .Load(session, data, ref i, inst, iSrc, param); break;
                    }
                }

                return param;
            }


            public override string GetLabel(out float width)
            {
                width = 70f; 
                
                return
                    Tag == strVol
                    ? printValue(100 * Math.Log10(Value), 0, true, 0).PadLeft(4)
                    : printValue(CurValue, 2, true, 1).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                Trigger ?.DrawLabels(sprites, x, y, dp); 
                Envelope?.DrawLabels(sprites, x, y, dp); 
                Lfo     ?.DrawLabels(sprites, x, y, dp); 
                Modulate?.DrawLabels(sprites, x, y, dp); 

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {   
                var valWidth  =  60;
                var valHeight = 180;

                if (   Tag == strAtt
                    || Tag == strDec
                    || Tag == strSus
                    || Tag == strRel
                    || Tag == strAmt
                    || Tag == strAmp
                    || Tag == strFreq
                    ||    Parent != null // LFO offset has a parent
                       && Tag == strOff
                    || Tag == strDry
                    || Tag == strCnt
                    || Tag == strTime
                    || Tag == strLvl
                    || Tag == strPow
                    || Tag == strCut
                    || Tag == strRes
                    || Tag == strShrp)
                {
                    Parent.DrawSetting(sprites, x, y, w, h, dp);
                }
                else if (Tag == strVol)
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
                      && Tag    == strOff)
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

                if (Tag == strVol)
                {
                    DrawSoundLevel(sprites, x + bx + 20, y + by + 90, 60, 120, Value, CurValue);

                    var db = printValue(100 * Math.Log10(Value), 0, true, 0) + " dB";
                    DrawString(sprites, db, x + bx + 100, y + by + 180, 1f, color6);
                }
                else if (Parent == null // source offset has no parent
                      && Tag    == strOff)
                    DrawValueHorizontal(sprites, x + bx +  5, y + by + 90, 180,  50, Min, Max, Value, CurValue, Tag);
                else
                    DrawValueVertical  (sprites, x + bx + 20, y + by + 90,  60, 120, Min, Max, Value, CurValue, Tag, false);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                if (   !AnyParentIsEnvelope
                    && !HasTagOrAnyParent(this,strTrig))
                {
                    DrawFuncButton(sprites, strTrig, 0, w, h, true, Trigger  != null);
                    DrawFuncButton(sprites, strEnv,  1, w, h, true, Envelope != null);
                }

                DrawFuncButton(sprites, strLfo, 2, w, h, true, Lfo      != null);
                DrawFuncButton(sprites, strMod, 3, w, h, true, Modulate != null);
                DrawFuncButton(sprites, "Key",  4, w, h, true, chan.HasNoteKeys(GetPath(CurSrc)));
                DrawFuncButton(sprites, "Auto", 5, w, h, true, chan.HasAutoKeys(GetPath(CurSrc)));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                case 0:
                    if (   AnyParentIsEnvelope
                        || HasTagOrAnyParent(this, strTrig)) break;
                    AddNextSetting(strTrig);
                    break;

                case 1:
                    if (AnyParentIsEnvelope) break;
                    AddNextSetting(strEnv);
                    break;

                case 2: AddNextSetting(strLfo); break;
                case 3: AddNextSetting(strMod); break;

                case 4: CurClip.ParamKeys = true; /*UpdateChordLabels();*/ break;
                case 5: CurClip.ParamAuto = true; /*UpdateChordLabels();*/ break;
                }
            }


            public override bool CanDelete()
            {
                return Tag == strOff;
            }


            //public Color ValueColor { get
            //{
            //    var color =
            //        IsCurParam(Tag)
            //        ? (Value != PrevValue
            //           ? color6
            //           : color5)
            //        : color3;

            //    PrevValue = Value;

            //    return color;
            //} }
        }


        static Parameter NewHarmonicParam(int i, Setting parent, Instrument inst, Source src)
        {
            return new Parameter(S(i), 0, 1, 0.1f, 0.9f, 0.01f, 0.1f, i == 0 ? 1 : 0, parent, inst, src);
        }
    }
}
