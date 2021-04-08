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
                                
            public float        CurValue;

            public Queue<float> ValueCache;


            public LFO(Setting parent) : base("LFO", parent) 
            {
                Op        = LfoOp  .Multiply;
                Type      = LfoType.Sine;

                Amplitude = (Parameter)NewSettingFromTag("Amp",  this);
                Frequency = (Parameter)NewSettingFromTag("Freq", this);
                Offset    = (Parameter)NewSettingFromTag("Off",  this);

                CurValue  = 0;

                ValueCache = new Queue<float>();
                for (int i = 0; i <= FPS; i++)
                    ValueCache.Enqueue(0);
            }


            public LFO(LFO lfo, Setting parent) : base(lfo.Tag, parent, lfo.Prototype)
            {
                Op        = lfo.Op;
                Type      = lfo.Type;

                Amplitude = new Parameter(lfo.Amplitude, this);
                Frequency = new Parameter(lfo.Frequency, this);
                Offset    = new Parameter(lfo.Offset,    this);
            }


            public LFO Copy(Setting parent)
            {
                return new LFO(this, parent);
            }


            public float GetValue(TimeParams tp)
            {
                if (tp.Program.TooComplex) return 0;

                // an offset != 0 locks the LFO to the song, a 0 offset leaves it free
                var time = 
                    Offset.GetKeyValue(tp.Note, tp.SourceIndex) > 0 
                    ? tp.LocalTime 
                    : tp.GlobalTime;

                var amp  = Amplitude.GetValue(tp);
                var freq = Frequency.GetValue(tp);
                var off  = Offset   .GetValue(tp);

                var f = (float)(Math.Pow(2, freq) - 1);

                var L = FPS / f;
                var t = (time % L) / L;

                switch (Type)
                {
                    case LfoType.Sine:    CurValue = amp * (float)Math.Sin(t * Tau); break;
                    case LfoType.Triangle:
                    { 
                             if (t <  0.25f)              t = t / 0.25f;
                        else if (t >= 0.25f && t < 0.75f) t = 1 - 4 * (t - 0.25f);
                        else                              t = (t - 0.75f) / 0.25f - 1; 

                        CurValue = amp * t;
                        break;
                    }
                    case LfoType.Saw:     CurValue = amp * (t*2 - 1); break;
                    case LfoType.BackSaw: CurValue = amp * (1 - t*2); break;
                    case LfoType.Square:  CurValue = amp * (t < 0.5 ? 1 : -1); break;
                    case LfoType.Noise:   CurValue = amp * g_random[(int)(time/(float)FPS * f) % g_random.Length]; break;
                }

                ValueCache.Dequeue();
                ValueCache.Enqueue(CurValue);

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
                width = 153;

                str =
                      printValue(Amplitude.CurValue, 2, true, 0).PadLeft(4) + "  "
                    + printValue(Frequency.CurValue, 2, true, 0).PadLeft(4) + "  "
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


                // draw current value
                var lTime = g_song.PlayTime > -1 ? g_time - g_song.StartTime : 0;

                var val   = CurValue;
                var blur  = Type == LFO.LfoType.Noise ? Math.Pow(freq, 4) : 1;
                          
                var ty    = (float)Math.Max(y0,    y0 + h0/2 - val*h0/2 - blur  );
                var by    = (float)Math.Min(y0+h0, y0 + h0/2 - val*h0/2 + blur*2);

                var col = new Color(
                    color4.R,
                    color4.G,
                    color4.B,
                    (int)(Math.Pow(1/freq, 2.5)*0xFF));

                FillRect(sprites, x0, ty, w0, Math.Max(2, by-ty), col);


                var cache = ValueCache.ToArray();

                // draw the waveform
                for (long f = 0; f < FPS; f++)
                {
                    var tp = new TimeParams(
                        f + g_time,
                        f + lTime,
                        f + g_time - g_song.StartTime,
                        null,
                        EditLength,
                        -1,
                        _triggerDummy,
                        dp.Program);

                    var v = cache[f];

                    var p = new Vector2(
                        x0 + w0 * f/(float)FPS,
                        y0 + h0/2 - v*h0/2);

                    if (   OK(pPrev.X)
                        && OK(pPrev.Y))
                        DrawLine(sprites, pPrev, p, color4, 2);

                    pPrev = p;
                }


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
