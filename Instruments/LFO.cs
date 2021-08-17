using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class LFO : Setting
        {
            public ModOp        Op;

            public enum LfoType { Sine, Triangle, Saw, BackSaw, Square, Noise };
            public LfoType      Type;
                                
            public Parameter    Offset,
                                Frequency,
                                Amplitude,
                                Trigger; // how often the value is changed
                                
            public float        Phase,
                                Delta,
                                CurValue;

            public Queue<float> ValueCache;

            int                 m_count;
            int                 MaxCount = 4;



            public LFO(Setting parent, Instrument inst, Source src) 
                : base(strLfo, parent, Setting_null, True, inst, src) 
            {
                Op          = ModOp  .Add;
                Type        = LfoType.Sine;
                            
                Offset      = (Parameter)NewSettingFromTag(strOff,  this, inst, src);
                Frequency   = (Parameter)NewSettingFromTag(strFreq, this, inst, src);
                Amplitude   = (Parameter)NewSettingFromTag(strAmp,  this, inst, src);
                Trigger     = (Parameter)NewSettingFromTag(strAmp,  this, inst, src);
                            
                g_lfo.Add(this);

                Phase       = 0;
                Delta       = 1f/FPS * Frequency.Value;
                CurValue    = 0;

                ValueCache = new Queue<float>();
                for (int i = 0; i <= FPS; i++)
                    ValueCache.Enqueue(0);

                m_count     = 0;
            }



            public LFO(LFO lfo, Setting parent) 
                : base(lfo.Tag, parent, lfo.Prototype, lfo.On, lfo.Instrument, lfo.Source)
            {
                Op          = lfo.Op;
                Type        = lfo.Type;
                            
                Offset      = new Parameter(lfo.Offset,    this);
                Frequency   = new Parameter(lfo.Frequency, this);
                Amplitude   = new Parameter(lfo.Amplitude, this);
                Trigger     = new Parameter(lfo.Trigger,   this);

                g_lfo.Add(this);

                Phase       = lfo.Phase;
                Delta       = lfo.Delta;
                CurValue    = lfo.CurValue;

                ValueCache = new Queue<float>();
                foreach (var val in lfo.ValueCache)
                    ValueCache.Enqueue(val);

                m_count     = lfo.m_count;
            }



            public LFO Copy(Setting parent)
            {
                return new LFO(this, parent);
            }



            public void AdvanceTime()
            {
                Phase += Delta;

                Delta = 1f/FPS * Frequency.CurValue;
                
                if (++m_count >= MaxCount)
                {
                    ValueCache.Dequeue();
                    ValueCache.Enqueue(CurValue);

                    m_count = 0;
                }
            }



            public float UpdateValue(TimeParams tp)
            {
                if (tp.Program.TooComplex) 
                    return 0;

                if (!On)
                    return 0;

                var trig = Trigger.UpdateValue(tp);

                if (   trig == 0
                    || ((tp.LocalTime / TicksPerStep) % trig) == 0)
                    CurValue = GetUpdateValue(tp);

                m_valid = True;
                return CurValue;
            }



            float GetUpdateValue(TimeParams tp)
            {
                var value = 0f;

                var off  = Offset   .UpdateValue(tp);
                var freq = Frequency.UpdateValue(tp);
                var amp  = Amplitude.UpdateValue(tp);

                switch (Type)
                {
                    case LfoType.Sine:     value = (float)Math.Sin(Phase * Tau);                         break;
                    case LfoType.Triangle: value = (1 - 2*Math.Abs((Phase % 1)*2 - 1));                    break;
                    case LfoType.Saw:      value = (   Phase  % 1)*2 - 1;                                break;
                    case LfoType.BackSaw:  value = ((1-Phase) % 1)*2 + 1;                                break;
                    case LfoType.Square:   value = (float)(1 - 2*Math.Round(((Phase*2) % 2)/2));            break;
                    case LfoType.Noise:    value = g_random[(int)(Phase*2) % g_random.Length]*2 - 1; break;
                }

                value *= amp;

                return value;
            }



            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Op   != ModOp  .Multiply
                    || Type != LfoType.Sine
                    || Offset   .HasDeepParams(chan, src)
                    || Frequency.HasDeepParams(chan, src)
                    || Amplitude.HasDeepParams(chan, src)
                    || Trigger  .HasDeepParams(chan, src);
            }



            public override void Clear()
            {
                Offset   .Clear();
                Frequency.Clear();
                Amplitude.Clear();
                Trigger  .Clear();
            }



            public override void Reset()
            {
                base.Reset();

                Offset   .Reset();
                Frequency.Reset();
                Amplitude.Reset();
                Trigger  .Reset();
            }



            public override void Randomize()
            {
                Offset   .Randomize();
                Frequency.Randomize();
                Amplitude.Randomize();
                Trigger  .Randomize();

                Type = (LfoType)g_rnd.Next(0, 6);
            }



            public override void AdjustFromController(Clip clip)
            {
                Program.AdjustFromController(clip, Offset,    g_remote.MoveIndicator    .X*ControlSensitivity);

                Program.AdjustFromController(clip, Amplitude, g_remote.RotationIndicator.X*ControlSensitivity);
                Program.AdjustFromController(clip, Frequency, g_remote.RotationIndicator.Y*ControlSensitivity);
            }



            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strOff:  return GetOrAddParamFromTag(Offset,    tag);
                    case strFreq: return GetOrAddParamFromTag(Frequency, tag);
                    case strAmp:  return GetOrAddParamFromTag(Amplitude, tag);
                    case strTrig: return GetOrAddParamFromTag(Trigger,   tag);
                }

                return Setting_null;
            }



            public void Delete(int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Offset   .Delete(iSrc);
                Frequency.Delete(iSrc);
                Amplitude.Delete(iSrc);
                Trigger  .Delete(iSrc);
            }



            public override string Save()
            {
                return
                      Tag

                    + PS(SaveToggles())
                    + PS((int)Op)
                    + PS((int)Type)

                    + P (Amplitude.Save())
                    + P (Frequency.Save())
                    + P (Offset   .Save())
                    + P (Trigger  .Save());
            }



            uint SaveToggles()
            {
                uint f = 0;
                var  d = 0;

                WriteBit(ref f, On, d++);

                return f;
            }



            public static LFO Load(string[] data, ref int d, Instrument inst, int iSrc, Setting parent)
            {
                var tag = data[d++];
 
                var lfo = new LFO(
                    parent, 
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);

                lfo.LoadToggles(data[d++]);

                lfo.Op   = (ModOp)  int_Parse(data[d++]);
                lfo.Type = (LfoType)int_Parse(data[d++]);

                lfo.Amplitude = Parameter.Load(data, ref d, inst, iSrc, lfo, lfo.Amplitude);
                lfo.Frequency = Parameter.Load(data, ref d, inst, iSrc, lfo, lfo.Frequency);
                lfo.Offset    = Parameter.Load(data, ref d, inst, iSrc, lfo, lfo.Offset   );
                lfo.Trigger   = Parameter.Load(data, ref d, inst, iSrc, lfo, lfo.Trigger  );

                return lfo;
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
                width = 193;

                var strOsc = "";

                switch (Type)
                {
                    case LfoType.Sine:     strOsc = "∫ ";  break;
                    case LfoType.Triangle: strOsc = "/\\"; break;
                    case LfoType.Saw:      strOsc = "/ ";  break;
                    case LfoType.BackSaw:  strOsc = "\\ "; break;
                    case LfoType.Square:   strOsc = "П ";  break;
                    case LfoType.Noise:    strOsc = "X ";  break;
                }

                return
                     (Op == ModOp.Add ? "+ " : "* ")
                    + strOsc + strEmpty
                    + PrintValue(Offset   .Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Frequency.Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Amplitude.Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Trigger  .Value, 0, True, 0).PadLeft(3);
            }



            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                if (!_dp.Program.TooComplex)
                {
                    base.DrawLabels(sprites, x, y, dp);

                    if (Offset   .HasDeepParams(CurChannel, CurSrc)) Offset   .DrawLabels(sprites, x, y, dp);
                    if (Frequency.HasDeepParams(CurChannel, CurSrc)) Frequency.DrawLabels(sprites, x, y, dp);                
                    if (Amplitude.HasDeepParams(CurChannel, CurSrc)) Amplitude.DrawLabels(sprites, x, y, dp);
                    if (Trigger  .HasDeepParams(CurChannel, CurSrc)) Trigger  .DrawLabels(sprites, x, y, dp);
                }

                _dp.Next(dp);
            }



            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var pPrev = new Vector2(float_NaN, float_NaN);

                var w0     = 240f;
                var h0     = 120f;
                           
                var x0     = x + w/2 - w0/2;
                var y0     = y + h/2 - h0/2;

                var amp    = Amplitude.CurValue;
                var freq   = Frequency.CurValue;
                var off    = Offset   .CurValue;

                var isAmp  = IsCurParam(strAmp);
                var isFreq = IsCurParam(strFreq);
                var isOff  = IsCurParam(strOff);


                // draw axes
                DrawLine(sprites, x0, y0,      x0,    y0+h0,   isAmp  ? color6 : color3);
                DrawLine(sprites, x0, y0+h0/2, x0+w0, y0+h0/2, isFreq ? color6 : color3);


                var time = (long)(Phase * FPS);
                var _tp  = new TimeParams(time, time, Note_null, EditedClip.EditLength, -1, _triggerDummy, EditedClip, dp.Program);

                var val  = UpdateValue(_tp);

                // draw current value
                var blur = Type == LfoType.Noise ? Math.Pow(freq, 4) : 1;
                          
                var ty   = (float)Math.Max(y0,    y0 + h0/2 - val*h0/2 - blur  );
                var by   = (float)Math.Min(y0+h0, y0 + h0/2 - val*h0/2 + blur*2);

                var col = new Color(
                    color4.R,
                    color4.G,
                    color4.B,
                    (int)(Math.Pow(1/freq, 2.5)*0xFF));

                FillRect(sprites, x0, ty, w0, Math.Max(2, by-ty), col);


                // draw the waveform

                var f = 0;
                foreach (var v in ValueCache)
                {
                    var p = new Vector2(
                        x0 + w0 * f/FPS,
                        y0 + h0/2 - v*h0/2);

                    if (   OK(pPrev.X)
                        && OK(pPrev.Y))
                        DrawLine(sprites, pPrev, p, color4, 2);

                    pPrev = p;
                    f++;
                }


                // draw the value ball
                FillCircle(sprites, x0 + w0, y0 + h0/2 - val*h0/2, 4, color6);


                var fs = 0.5f;

                // draw amplitude label
                DrawString(
                    sprites, 
                    S_000(amp), 
                    x0 + w0/2, 
                    y0 + h0/2 - h0/2*amp - 20, 
                    fs, 
                    isAmp ? color6 : color3,
                    TA_CENTER);

                // frequency label
                DrawString(
                    sprites, 
                    S_000(Math.Pow(2, freq)-1) + (isFreq ? " Hz" : ""),
                    x0 + w0/2,
                    y0 + h0 + 3,
                    fs,
                    isFreq ? color6 : color3,
                    TA_CENTER);

                // offset label
                DrawString(
                    sprites, 
                    S_000(off) + (isOff ? " s" : ""),
                    x0 + w0/8,
                    y0 + h0 + 3,
                    fs,
                    isOff ? color6 : color3,
                    TA_CENTER);


                // TODO draw trigger
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                var strOp = (Op == ModOp.Add ? "Add " : "Mult") + "↕";

                DrawFuncButton(sprites, strOff,  0, w, y, True, Offset   .HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strFreq, 1, w, y, True, Frequency.HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strAmp,  2, w, y, True, Amplitude.HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strOp,   3, w, y, False, False);
                DrawFuncButton(sprites, "Osc ↕", 4, w, y, False, False);
                DrawFuncButton(sprites, strTrig, 5, w, y, True, Trigger  .HasDeepParams(chan, CurSrc));
            }



            public override void Func(int func)
            {
                switch (func)
                {
                    case 0: AddNextSetting(strOff);  break;
                    case 1: AddNextSetting(strFreq); break;
                    case 2: AddNextSetting(strAmp);  break;
                    case 3:
                    {
                        var newOp = (int)Op + 1;
                        if (newOp > (int)ModOp.Add) newOp = 0;
                        Op = (ModOp)newOp;
                        g_lcdPressed.Add(lcdMain+func);
                        break;
                    }
                    case 4:
                    {
                        var newOsc = (int)Type + 1;
                        if (newOsc > (int)LfoType.Noise) newOsc = 0;
                        Type = (LfoType)newOsc;
                        g_lcdPressed.Add(lcdMain+func);
                        break;
                    }
                    case 5: AddNextSetting(strTrig); break;
                }
            }



            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}
