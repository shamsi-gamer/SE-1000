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
            public enum LfoOp   { Multiply, Add };
            public enum LfoType { Sine, Triangle, Saw, BackSaw, Square, Noise };

            public LfoOp        Op;
            public LfoType      Type;
                                
            public Parameter    Amplitude,
                                Frequency,
                                Offset;
                                
            public float        Phase,
                                Delta,

                                CurValue;

            public Queue<float> ValueCache;

                  int           m_count;
            const int           MaxCount = 4;

 


            public LFO(Setting parent) : base("LFO", parent) 
            {
                Op          = LfoOp  .Multiply;
                Type        = LfoType.Sine;
                            
                Amplitude   = (Parameter)NewSettingFromTag("Amp",  this);
                Frequency   = (Parameter)NewSettingFromTag("Freq", this);
                Offset      = (Parameter)NewSettingFromTag("Off",  this);
                            
                CurValue    = 0;

                g_lfo.Add(this);

                Phase       = 0;
                Delta       = 1f/FPS * Frequency.Value;

                ValueCache = new Queue<float>();
                for (int i = 0; i <= FPS; i++)
                    ValueCache.Enqueue(0);

                m_count     = 0;
            }


            public LFO(LFO lfo, Setting parent) 
                : base(lfo.Tag, parent, lfo.Prototype)
            {
                Op          = lfo.Op;
                Type        = lfo.Type;
                            
                Amplitude   = new Parameter(lfo.Amplitude, this);
                Frequency   = new Parameter(lfo.Frequency, this);
                Offset      = new Parameter(lfo.Offset,    this);
                            
                CurValue    = lfo.CurValue;

                g_lfo.Add(this);

                Phase       = lfo.Phase;
                Delta       = lfo.Delta;

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
                if (tp.Program.TooComplex) return 0;

                var amp  = Amplitude.UpdateValue(tp);
                var freq = Frequency.UpdateValue(tp);
                var off  = Offset   .UpdateValue(tp);

                switch (Type)
                {
                    case LfoType.Sine:     CurValue = (float)Math.Sin(Phase * Tau);                   break;
                    case LfoType.Triangle: CurValue = (1 - 2* Math.Abs(2*(Phase % 1)-1));             break;
                    case LfoType.Saw:      CurValue = (   Phase  % 1);                                break;
                    case LfoType.BackSaw:  CurValue = ((1-Phase) % 1);                                break;
                    case LfoType.Square:   CurValue = (float)(1 - 2* Math.Round((Phase % 2)/2));      break;
                    case LfoType.Noise:    CurValue = g_random[(int)(Phase * FPS) % g_random.Length]; break;
                }

                CurValue *= amp;

                return CurValue;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Op   != LfoOp  .Multiply
                    || Type != LfoType.Sine
                    || Amplitude.HasDeepParams(chan, src)
                    || Frequency.HasDeepParams(chan, src)
                    || Offset   .HasDeepParams(chan, src);
            }


            public override void Remove(Setting setting)
            {
                     if (setting == Amplitude) Amplitude = null;
                else if (setting == Frequency) Frequency = null;
                else if (setting == Offset)    Offset    = null;
            }


            public override void Clear()
            {
                Amplitude.Clear();
                Frequency.Clear();
                Offset   .Clear();
            }

            public override void Randomize(Program prog)
            {
                Amplitude.Randomize(prog);
                Frequency.Randomize(prog);
                Offset   .Randomize(prog);

                Type = (LfoType)g_rnd.Next(0, 6);
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.MoveIndicator    .X != 0) prog.AdjustFromController(song, Offset,    g_remote.MoveIndicator    .X/ControlSensitivity);

                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(song, Amplitude, g_remote.RotationIndicator.X/ControlSensitivity);
                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(song, Frequency, g_remote.RotationIndicator.Y/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Amp":  return Amplitude ?? (Amplitude = (Parameter)NewSettingFromTag("Amp",  this));
                    case "Freq": return Frequency ?? (Frequency = (Parameter)NewSettingFromTag("Freq", this));
                    case "Off":  return Offset    ?? (Offset    = (Parameter)NewSettingFromTag("Off",  this));
                }

                return null;
            }


            public void Delete(Song song, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Amplitude.Delete(song, iSrc);
                Frequency.Delete(song, iSrc);
                Offset   .Delete(song, iSrc);
            }


            public override string Save()
            {
                return
                      W (Tag)

                    + WS((int)Op)
                    + WS((int)Type)

                    + W (Amplitude.Save())
                    + W (Frequency.Save())
                    +    Offset   .Save();
            }


            public static LFO Load(string[] data, ref int i, Instrument inst, int iSrc, Setting parent)
            {
                var tag = data[i++];
 
                var lfo = new LFO(parent);

                lfo.Op   = (LfoOp)  int.Parse(data[i++]);
                lfo.Type = (LfoType)int.Parse(data[i++]);

                lfo.Amplitude = Parameter.Load(data, ref i, inst, iSrc, lfo);
                lfo.Frequency = Parameter.Load(data, ref i, inst, iSrc, lfo);
                lfo.Offset    = Parameter.Load(data, ref i, inst, iSrc, lfo);

                return lfo;
            }


            public override void GetLabel(out string str, out float width)
            {
                width = 175;

                var strOsc = "";

                switch (Type)
                {
                    case LfoType.Sine:     strOsc = "∫ ";  break;
                    case LfoType.Triangle: strOsc = "/\\"; break;
                    case LfoType.Saw:      strOsc = "/ ";  break;
                    case LfoType.BackSaw:  strOsc = "\\ "; break;
                    case LfoType.Square:   strOsc = "П ";  break;
                    case LfoType.Noise:    strOsc = "╫ ";  break;
                }

                str =
                     (Op == LfoOp.Add ? "+ " : "* ")
                    + strOsc + " "
                    + printValue(Amplitude.CurValue, 2, true, 0).PadLeft(4) + " "
                    + printValue(Frequency.CurValue, 2, true, 0).PadLeft(4) + " "
                    + printValue(Offset   .CurValue, 2, true, 0).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Frequency.HasDeepParams(CurrentChannel, CurSrc)) Frequency.DrawLabels(sprites, x, y, dp);                
                if (Amplitude.HasDeepParams(CurrentChannel, CurSrc)) Amplitude.DrawLabels(sprites, x, y, dp);
                if (Offset   .HasDeepParams(CurrentChannel, CurSrc)) Offset   .DrawLabels(sprites, x, y, dp);

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var pPrev = new Vector2(fN, fN);

                var w0     = 240f;
                var h0     = 120f;
                           
                var x0     = x + w/2 - w0/2;
                var y0     = y + h/2 - h0/2;

                var amp    = Amplitude.CurValue;
                var freq   = Frequency.CurValue;
                var off    = Offset   .CurValue;

                var isAmp  = IsCurParam("Amp" );
                var isFreq = IsCurParam("Freq");
                var isOff  = IsCurParam("Off" );


                // draw axes
                DrawLine(sprites, x0, y0,      x0,    y0+h0,   isAmp  ? color6 : color3);
                DrawLine(sprites, x0, y0+h0/2, x0+w0, y0+h0/2, isFreq ? color6 : color3);


                var time = (long)(Phase * FPS);
                var _tp  = new TimeParams(time, time, time, null, EditLength, -1, _triggerDummy, dp.Program);

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
                    S_00(amp), 
                    x0 + w0/2, 
                    y0 + h0/2 - h0/2*amp - 20, 
                    fs, 
                    isAmp ? color6 : color3,
                    TaC);

                // frequency label
                DrawString(
                    sprites, 
                    S_00(Math.Pow(2, freq)-1) + (isFreq ? " Hz" : ""),
                    x0 + w0/2,
                    y0 + h0 + 3,
                    fs,
                    isFreq ? color6 : color3,
                    TaC);

                // offset label
                DrawString(
                    sprites, 
                    S_00(off) + (isOff ? " s" : ""),
                    x0,
                    y0 + h0 + 3,
                    fs,
                    isOff ? color6 : color3,
                    TaC);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                DrawFuncButton(sprites, (Op == LfoOp.Add ? "Add " : "Mult") + "↕", 0, w, y, false, false);
                DrawFuncButton(sprites, "Amp",   1, w, y, true, Amplitude.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Freq",  2, w, y, true, Frequency.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Off",   3, w, y, true, Offset   .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Osc ↕", 4, w, y, false, false);
                DrawFuncButton(sprites, "X",     5, w, y, false, false, mainPressed.Contains(5));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                    case 0:
                    {
                        var newOp = (int)Op + 1;
                        if (newOp > (int)LfoOp.Add) newOp = 0;
                        Op = (LfoOp)newOp;
                        mainPressed.Add(func);
                        break;
                    }
                    case 1: AddNextSetting("Amp");  break;
                    case 2: AddNextSetting("Freq"); break;
                    case 3: AddNextSetting("Off");  break;
                    case 4:
                    {
                        var newOsc = (int)Type + 1;
                        if (newOsc > (int)LfoType.Noise) newOsc = 0;
                        Type = (LfoType)newOsc;
                        mainPressed.Add(func);
                        break;
                    }
                    case 5: RemoveSetting(this); break;
                }
            }
        }
    }
}
