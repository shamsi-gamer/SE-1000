using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;



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

            public bool      CanHaveEnvelope;

            public Bias      Bias;
            public Envelope  Envelope;
            public LFO       Lfo;
            public Modulate  Modulate;

            public int       IntBias; // used for Parameters where the conceptual value is an int
                                      // and a Mod or LFO distance (Max-Min) is needed



            public Parameter(string tag, float min, float max, float normalMin, float normalMax, float delta, float bigDelta, float defVal, bool canHaveEnvelope, Setting parent, Instrument inst, Source src) 
                : base(tag, parent, Setting_null, True, inst, src)
            {
                Tag             = tag;
                                
                m_value         = defVal;
                Default         = defVal;
                                
                Min             = min;
                Max             = max;
                                
                NormalMin       = normalMin;
                NormalMax       = normalMax;
                                
                Delta           = delta;
                BigDelta        = bigDelta;

                CanHaveEnvelope = canHaveEnvelope;

                Bias            =      Bias_null;
                Envelope        =  Envelope_null;
                Lfo             =       LFO_null;
                Modulate        =  Modulate_null;

                IntBias         = 0;
            }



            public Parameter(Parameter param, Setting parent, string tag = "", bool copy = True) 
                : base(tag != "" ? tag : param.Tag, parent, param.Prototype, param.On, param.Instrument, param.Source)
            {
                m_value         = param.m_value;
                                
                Min             = param.Min;
                Max             = param.Max;
                                
                NormalMin       = param.NormalMin;
                NormalMax       = param.NormalMax;
                                
                Delta           = param.Delta;
                BigDelta        = param.BigDelta;

                CanHaveEnvelope = param.CanHaveEnvelope;

                Bias            = copy ? param.Bias    ?.Copy(this) :     Bias_null;
                Envelope        = copy ? param.Envelope?.Copy(this) : Envelope_null;
                Lfo             = copy ? param.Lfo     ?.Copy(this) :      LFO_null;
                Modulate        = copy ? param.Modulate?.Copy(this) : Modulate_null;

                IntBias         = copy ? param.IntBias              : 0;
            }



            public Parameter Copy(Setting parent) => new Parameter(this, parent);


            public float Value => m_value;



            public virtual void SetValue(float val, Note note)
            {
                if (OK(note))
                {
                    var key = note.Keys.Find(k => k.Path == Path);

                    if (OK(key)) key.Value = MinMax(Min, val, Max);
                    else         m_value   = MinMax(Min, val, Max);
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



            public virtual float UpdateValue(TimeParams tp)
            {
                if (tp.Program.TooComplex) 
                    return 0;

                float value;


                if (On)
                {
                    if (OK(tp.Note))
                    { 
                        var val = GetAutoValue(
                            tp.Clip,
                            tp.Note.Step, // not ClipStep because played notes are in clip time already
                            tp.Note.iChan, 
                            Path);

                        value = OK(val) ? val : GetKeyValue(tp.Note);
                    }
                    else
                        value = GetKeyValue(tp.Note);


                    if (   OK(Lfo)
                        && Lfo.On
                        && !OK(tp.TriggerValues.Find(v => v.Path == Path)))
                    {
                        var lfo = Lfo.UpdateValue(tp);

                        if (IsSettingType(this, typeof(TuneChord)))
                            lfo = (lfo+1)/2;

                             if (Lfo.Op == ModOp.Add     ) value += lfo * Math.Abs(Max-Min+IntBias);
                        else if (Lfo.Op == ModOp.Multiply) value *= lfo;
                        else                               value  = lfo * Math.Abs(Max-Min+IntBias);

                        if (ParentIsEnvelope)
                            tp.TriggerValues.Add(new TriggerValue(Path, MinMax(Min, value, Max)));
                    }


                    if (   OK(Envelope)
                        && Envelope.On)
                        value *= Envelope.UpdateValue(tp);


                    if (   OK(Modulate)
                        && Modulate.On)
                    {
                        var mod = Modulate.UpdateValue(tp);

                             if (Modulate.Op == ModOp.Add)      value += mod * Math.Abs(Max-Min);
                        else if (Modulate.Op == ModOp.Multiply) value *= mod;
                        else                                    value  = mod;
                    }


                    if (   OK(Bias)
                        && Bias.On)
                        value *= Bias.UpdateValue(tp);
                }
                else
                { 
                    value = Default;
                }


                CurValue = MinMax(Min, value, Max);
                
                m_valid  = True;
                return CurValue;
            }



            public float GetKeyValue(Note note)
            {
                if (OK(note))
                {
                    var key = note.Keys.Find(k => k.Path == Path);
                    return key?.Value ?? m_value; 
                }
                else return m_value;
            }



            public static float GetAutoValue(Clip clip, float clipStep, int ch, string path)
            {
                var prevKey = PrevClipAutoKey(clip, clipStep, ch, path);
                var nextKey = NextClipAutoKey(clip, clipStep, ch, path);

                     if (!OK(prevKey) && !OK(nextKey)) return float_NaN;
                else if ( OK(prevKey) && !OK(nextKey)) return prevKey.Value;
                else if (!OK(prevKey) &&  OK(nextKey)) return nextKey.Value;
                else
                    return prevKey.Value + (nextKey.Value - prevKey.Value) * (clipStep - clip.Track.StartStep - prevKey.Step) / (nextKey.Step - prevKey.Step);
            }



            public float AdjustValue(float value, float delta, bool shift, bool scale = False)
            {
                     if (Tag == strStr)          ((Envelope)Parent).TrigStart   = float_NaN;
                else if (Tag == strAtt
                      && HasTag(Parent, strEnv)) ((Envelope)Parent).TrigAttack  = float_NaN;
                else if (Tag == strDec)          ((Envelope)Parent).TrigDecay   = float_NaN;
                else if (Tag == strSus)          ((Envelope)Parent).TrigSustain = float_NaN;
                else if (Tag == strRel
                      && HasTag(Parent, strEnv)) ((Envelope)Parent).TrigRelease = float_NaN;

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
                        dv *= (float)Math.Pow(value, 1.25f);
                        var _delta = 1f/FPS * (value + dv);
                        ((LFO)Parent).Delta = _delta;
                    }

                    return value + dv;
                }
            }



            public override void AdjustFromController(Clip clip)
            {
                Program.AdjustFromController(clip, this, -g_remote.RotationIndicator.X*ControlSensitivity);
            }



            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strBias: return Bias     ?? (Bias     = new Bias    (this, Instrument, Source));
                    case strEnv:  return Envelope ?? (Envelope = new Envelope(this, Instrument, Source));
                    case strLfo:  return Lfo      ?? (Lfo      = new LFO     (this, Instrument, Source));
                    case strMod:  return Modulate ?? (Modulate = new Modulate(this, Instrument, Source));
                }

                return Setting_null;
            }



            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       OK(Bias)
                    || OK(Envelope)
                    || OK(Lfo     )
                    || OK(Modulate)
                    || (chan?.HasKeys(Path) ?? False)
                    || _IsCurrent;
            }



            public override void DeleteSetting(Setting setting)
            {
                     if (setting == Bias    )                           Bias     =      Bias_null;
                else if (setting == Envelope)                           Envelope =  Envelope_null;
                else if (setting == Lfo     ) { g_lfo.Remove(Lfo);      Lfo      =       LFO_null; }
                else if (setting == Modulate) { g_mod.Remove(Modulate); Modulate =  Modulate_null; }
            }



            public override void Clear()
            {
                m_value = Default;

                Bias    ?.Clear();
                Envelope?.Clear();
                Lfo     ?.Clear();
                Modulate?.Clear();

                Bias     =     Bias_null;
                Envelope = Envelope_null;

                if (OK(Lfo)) g_lfo.Remove(Lfo);
                Lfo = LFO_null;

                if (OK(Modulate)) g_mod.Remove(Modulate);
                Modulate = Modulate_null;
            }



            public override void Reset()
            {
                base.Reset();

                Bias    ?.Reset();
                Envelope?.Reset();
                Lfo     ?.Reset();
                Modulate?.Reset();
            }



            public override void Randomize()
            {
                var prog = Instrument.Program;


                m_value = NormalMin + RND * (NormalMax - NormalMin);
                

                //if (   !prog.TooComplex
                //    && !AnyParentIsBias
                //    &&  RND > 0.9f)
                //{
                //    Bias = new Bias(this, Instrument, Source);
                //    Bias.Randomize();
                //}
                //else 
                //    Bias = Bias_null;


                if (    CanHaveEnvelope
                    && !prog.TooComplex
                    && !AnyParentIsEnvelope
                    && (  !IsDigit(Tag[0]) && RND > 0.9f
                        || IsDigit(Tag[0]) && RND > 0.9f))
                {
                    Envelope = new Envelope(this, Instrument, Source);
                    Envelope.Randomize();
                }
                else 
                    Envelope = Envelope_null;


                if (  !prog.TooComplex
                    && RND > 0.9f)
                {
                    Lfo = new LFO(this, Instrument, Source);
                    Lfo.Randomize();
                }
                else
                { 
                    if (OK(Lfo))
                        g_lfo.Remove(Lfo);

                    Lfo = LFO_null;
                }
            }



            public virtual void Delete(int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                var srcIndex = Source?.Index ?? -1;

                foreach (var track in Tracks)
                { 
                    foreach (var clip in track.Clips)
                    { 
                        if (Instrument.Program.TooComplex) 
                            return;

                        if (!OK(clip)) 
                            continue;

                        foreach (var pat in clip.Patterns)
                        {
                            if (Instrument.Program.TooComplex)
                                return;

                            foreach (var chan in pat.Channels)
                            {
                                chan.AutoKeys.RemoveAll(k => k.Path == Path);

                                foreach (var note in chan.Notes)
                                    note.Keys.RemoveAll(k => k.Path == Path);
                            }
                        }
                     
                        clip.UpdateAutoKeys();
                    }
                }


                Bias    ?.Delete(iSrc);
                Envelope?.Delete(iSrc);
                Lfo     ?.Delete(iSrc);
                Modulate?.Delete(iSrc);
            }



            public override string Save()
            {
                var nSettings = 0;
                
                if (OK(Bias    )) nSettings++;
                if (OK(Envelope)) nSettings++;
                if (OK(Lfo     )) nSettings++;
                if (OK(Modulate)) nSettings++;

                return
                      Tag

                    + PS(SaveToggles())

                    + PS(m_value.ToString("0.######")) 
                    + PS(nSettings)

                    + SaveSetting(Bias    )
                    + SaveSetting(Envelope)
                    + SaveSetting(Lfo     )
                    + SaveSetting(Modulate);
            }



            uint SaveToggles()
            {
                uint f = 0;
                var  d = 0;

                WriteBit(ref f, On, d++);

                return f;
            }



            public static Parameter Load(string[] data, ref int d, Instrument inst, int iSrc, Setting parent, Parameter proto = Parameter_null)
            {
                var tag = data[d++];



                Parameter param;

                     if (OK(proto )) param = proto;                                                    
                else if (OK(parent)) param = (Parameter)parent.GetOrAddSettingFromTag(tag);            
                else if (OK(iSrc))   param = (Parameter)inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
                else                 param = (Parameter)inst.GetOrAddSettingFromTag(tag);              

                param.LoadToggles(data[d++]);

                param.m_value = float.Parse(data[d++]);

                
                var nSettings = int_Parse(data[d++]);

                while (nSettings-- > 0)
                {
                    switch (data[d])
                    { 
                        case strBias: param.Bias     = Bias    .Load(data, ref d, inst, iSrc, param); break;
                        case strEnv:  param.Envelope = Envelope.Load(data, ref d, inst, iSrc, param); break;
                        case strLfo:  param.Lfo      = LFO     .Load(data, ref d, inst, iSrc, param); break;
                        case strMod:  param.Modulate = Modulate.Load(data, ref d, inst, iSrc, param); break;
                    }
                }


                return param;
            }



            bool LoadToggles(string toggles)
            {
                uint f;
                if (!uint.TryParse(toggles, out f)) return False;

                var d = 0;

                On = ReadBit(f, d++);

                return True;
            }



            public override string GetLabel(out float width)
            {
                if (Tag == strVol)
                { 
                    width = 70; 
                    return PrintValue(100 * Math.Log10(CurValue), 0, True, 0).PadLeft(4);
                }
                else
                {
                    width = 90;
                    return PrintValue(CurValue, 2, True, 1).PadLeft(5);
                }
            }



            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                if (!_dp.Program.TooComplex)
                {
                    base.DrawLabels(sprites, x, y, dp);

                    Bias    ?.DrawLabels(sprites, x, y, dp); 
                    Envelope?.DrawLabels(sprites, x, y, dp); 
                    Lfo     ?.DrawLabels(sprites, x, y, dp); 
                    Modulate?.DrawLabels(sprites, x, y, dp); 
                }

                _dp.Next(dp);
            }



            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {   
                var valWidth  =  60;
                var valHeight = 180;

                if (   Tag == strLow
                    || Tag == strHigh
                    || Tag == strStr
                    || Tag == strAtt
                    || Tag == strDec
                    || Tag == strSus
                    || Tag == strRel
                    || Tag == strAmt
                    || Tag == strAmp
                    || Tag == strFreq
                    ||    OK(Parent) // LFO offset has a parent
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
                        PrintValue(100 * Math.Log10(Value), 0, True, 0) + " dB", 
                        x + w/2 + 10, 
                        y + h/2 + valHeight/2 - 30,
                        1f, 
                        color6);
                }
                else if (!OK(Parent) // source offset has no parent
                       && Tag == strOff)
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
                    int dec;

                         if (Tag == strTune) dec = 1;
                    else if (Tag == strStep) dec = 0;
                    else                     dec = 3;

                    DrawValueVertical(
                        sprites, 
                        x + w/2 - valWidth, 
                        y + h/2 - valHeight/2,
                        valWidth,
                        valHeight, 
                        Min, Max, 
                        Value, CurValue, 
                        Tag, 
                        dec);
                }
            }



            public void DrawSettingValues(List<MySprite> sprites, float x, float y)
            {
                var bx = 40;
                var by = 55;

                if (Tag == strVol)
                {
                    DrawSoundLevel(sprites, x + bx + 20, y + by + 90, 60, 120, Value, CurValue);

                    var db = PrintValue(100 * Math.Log10(Value), 0, True, 0) + " dB";
                    DrawString(sprites, db, x + bx + 100, y + by + 180, 1f, color6);
                }
                else if (!OK(Parent) // source offset has no parent
                      && Tag    == strOff)
                    DrawValueHorizontal(sprites, x + bx +  5, y + by + 90, 180,  50, Min, Max, Value, CurValue, Tag);
                else
                { 
                    var dec = Tag == strTune ? 1 : 2;
                    DrawValueVertical  (sprites, x + bx + 20, y + by + 90,  60, 120, Min, Max, Value, CurValue, Tag, dec);
                }
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                DrawFuncButton(sprites, strBias, 0, w, h, True, OK(Bias));

                if (    CanHaveEnvelope
                    && !AnyParentIsBias
                    && !AnyParentIsEnvelope)
                    DrawFuncButton(sprites, strEnv, 1, w, h, True, OK(Envelope));

                DrawFuncButton(sprites, strLfo, 2, w, h, True, OK(Lfo));
                DrawFuncButton(sprites, strMod, 3, w, h, True, OK(Modulate));
                DrawFuncButton(sprites, "Key",  4, w, h, True, chan.HasNoteKeys(Path));
                DrawFuncButton(sprites, "Auto", 5, w, h, True, chan.HasAutoKeys(Path));
            }



            public override void Func(int func)
            {
                switch (func)
                {
                case 0: AddNextSetting(strBias); break;

                case 1:
                    if (    CanHaveEnvelope
                        && !AnyParentIsEnvelope) 
                        AddNextSetting(strEnv);

                    break;

                case 2: AddNextSetting(strLfo); break;
                case 3: AddNextSetting(strMod); break;

                case 4: EditedClip.ParamKeys = True; break;
                case 5: EditedClip.ParamAuto = True; break;
                }
            }



            public override bool CanDelete()
            {
                return Tag == strOff;
            }
        }



        static Parameter NewHarmonicParam(int i, Setting parent, Instrument inst, Source src)
        {
            return new Parameter(S(i), 0, 1, 0.1f, 0.9f, 0.01f, 0.1f, i == 0 ? 1 : 0, True, parent, inst, src);
        }
    }
}
