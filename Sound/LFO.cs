using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class LFO : Setting
        {
            public enum LfoType { Sine, Triangle, Saw, BackSaw, Square, Noise };

            public LfoType   Type;

            public Parameter Amplitude,
                             Frequency,
                             Offset;

            public float     CurValue = 0;


            public LFO(Setting parent) : base("LFO", parent) 
            {
                Type      = LfoType.Sine;

                Amplitude = NewParamFromTag("Amp",  this);
                Frequency = NewParamFromTag("Freq", this);
                Offset    = NewParamFromTag("Off",  this);
            }


            public LFO(LFO lfo, Setting parent) : base(lfo.Tag, parent, lfo.Prototype)
            {
                Type      = lfo.Type;

                Amplitude = new Parameter(lfo.Amplitude, this);
                Frequency = new Parameter(lfo.Frequency, this);
                Offset    = new Parameter(lfo.Offset,    this);
            }


            public LFO Copy(Setting parent)
            {
                return new LFO(this, parent);
            }


            public float GetValue(long gTime, long lTime, long sTime, int noteLen, Note note, int src, List<TriggerValue> triggerValues)
            {
                // an offset != 0 locks the LFO to the song, a 0 offset leaves it free
                var time = Offset.GetKeyValue(note, src) > 0 ? lTime : gTime;

                var amp  = Amplitude.GetValue(gTime, time, sTime, noteLen, note, src, triggerValues);
                var freq = Frequency.GetValue(gTime, time, sTime, noteLen, note, src, triggerValues);
                var off  = Offset   .GetValue(gTime, time, sTime, noteLen, note, src, triggerValues);

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

                return CurValue;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Amplitude.HasDeepParams(chan, src)
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

            public override void Randomize()
            {
                Amplitude.Randomize();
                Frequency.Randomize();
                Offset   .Randomize();

                Type = (LfoType)g_rnd.Next(0, 6);
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.MoveIndicator.X != 0) prog.AdjustFromController(song, Offset, g_remote.MoveIndicator.X/ControlSensitivity);

                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(song, Amplitude, g_remote.RotationIndicator.X/ControlSensitivity);
                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(song, Frequency, g_remote.RotationIndicator.Y/ControlSensitivity);
            }


            public override string Save()
            {
                return
                      W (Tag)
                    + WS((int)Type)
                    + W (Amplitude.Save())
                    + W (Frequency.Save())
                    +    Offset   .Save();
            }


            public static LFO Load(string[] data, ref int i, Setting parent)
            {
                var tag = data[i++];
 
                var lfo = new LFO(parent);

                lfo.Type = (LfoType)int.Parse(data[i++]);

                lfo.Amplitude = Parameter.Load(data, ref i, lfo);
                lfo.Frequency = Parameter.Load(data, ref i, lfo);
                lfo.Offset    = Parameter.Load(data, ref i, lfo);

                return lfo;
            }
        }
    }
}
