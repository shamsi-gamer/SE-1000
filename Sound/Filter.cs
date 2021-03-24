using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public enum FilterPass { Low, High, Band };

        public class Filter : Setting
        {
            public FilterPass Pass;

            public Parameter  Cutoff,
                              Resonance,
                              Sharpness;


            public Filter() : base("Flt", null)
            {
                Pass      = FilterPass.Low;

                Cutoff    = (Parameter)NewSettingFromTag("Cut",  this);
                Resonance = (Parameter)NewSettingFromTag("Res",  this);
                Sharpness = (Parameter)NewSettingFromTag("Shrp", this);
            }


            public Filter(Filter flt) : base(flt.Tag, null, flt)
            {
                Pass      = flt.Pass;

                Cutoff    = new Parameter(flt.Cutoff,    this);
                Resonance = new Parameter(flt.Resonance, this);
                Sharpness = new Parameter(flt.Sharpness, this);
            }


            public Filter Copy()
            {
                return new Filter(this);
            }


            public override void Clear()
            {
                Cutoff   .Clear();
                Resonance.Clear();
                Sharpness.Clear();
            }


            public override void Randomize(Program prog)
            {
                if (prog.TooComplex) return;

                Pass = (FilterPass)g_rnd.Next((int)FilterPass.Band + 1);

                if (RND > 0.8f) Cutoff.Randomize(prog);
                else            Cutoff.Clear();

                if (RND > 0.8f) Resonance.Randomize(prog);
                else            Resonance.Clear();

                if (RND > 0.8f) Sharpness.Randomize(prog);
                else            Sharpness.Clear();
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.MoveIndicator    .Z != 0) prog.AdjustFromController(song, Sharpness, -g_remote.MoveIndicator    .Z/ControlSensitivity);

                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(song, Cutoff,     g_remote.RotationIndicator.Y/ControlSensitivity);
                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(song, Resonance, -g_remote.RotationIndicator.X/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Cut":  return Cutoff    ?? (Cutoff    = (Parameter)NewSettingFromTag("Cut",  this));
                    case "Res":  return Resonance ?? (Resonance = (Parameter)NewSettingFromTag("Res",  this));
                    case "Shrp": return Sharpness ?? (Sharpness = (Parameter)NewSettingFromTag("Shrp", this));
                }

                return null;
            }


            public override string Save()
            {
                return
                      W(Tag)

                    + WS((int)Pass)
                    + W(Cutoff   .Save())
                    + W(Resonance.Save())
                    +   Sharpness.Save();
            }


            public static Filter Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tag = data[i++];
 
                var flt = new Filter();

                flt.Pass      = (FilterPass)int.Parse(data[i++]);
                flt.Cutoff    = Parameter.Load(data, ref i, inst, iSrc, flt);
                flt.Resonance = Parameter.Load(data, ref i, inst, iSrc, flt);
                flt.Sharpness = Parameter.Load(data, ref i, inst, iSrc, flt);

                return flt;
            }
        }


        static float GetFilter(float f, FilterPass pass, float cut, float res, float shrp)
        {
            var val = 1f;
            var rw  = 1 - shrp;

            if (pass == FilterPass.Low)
            {
                var c = 1 - (float)Math.Pow(f / ((1+rw) * cut), (6.4f/rw-1)*cut + 1);
                var r = (1 - (float)Math.Cos(1/rw*Tau*MinMax(0, f + rw - (1+rw)*cut, rw))) / 3;

                val = c + r * res;
            }
            else if (pass == FilterPass.High)
            {
                var c = 1 - (float)Math.Pow(f / ((1 + rw) * cut), (6.4f / rw - 1) * cut + 1);
                var r = (1 - (float)Math.Cos(1 / rw * Tau * MinMax(0, f + rw - (1 + rw) * cut, rw))) / 3;

                val = c + r * res;
            }
            else if (pass == FilterPass.Band)
            {
                //var _f = f - cut;

                //     if (_f >=  1.25f              ) val = 0;
                //else if (_f >=  1     && _f < 1.25f) val =     GetCos((_f-1)*4, 0.5f,  0.75f, -res,  3) * (1+res); // low end
                //else if (_f >=  0.5f  && _f < 1    ) val = 1 + GetCos(2*_f-1,   0,     0.5f,   res, 10) *  res;    // low middle
                //else if (_f >=  0     && _f < 0.5f ) val = 1 + GetCos(2*_f,     0.5f,  1,     -res, 10) *  res;    // high middle
                //else if (_f >= -0.25f && _f < 0    ) val =     GetCos( -_f*4,   0.5f,  0.75f, -res,  3) * (1+res); // high end
                //else if (_f <  -0.25f              ) val = 0;
            }
 
            return MinMax(0, val, 2);
        }


        static void DrawFilter(List<MySprite> sprites, float x, float y, float w, float h, Color color, float width, FilterPass pass, float cut, float res, float shrp)
        {
            var step = 1/64f;
            var prev = fN;

            for (var f = 0f; f <= 1; f += step)
            {
                var val = GetFilter(f, pass, cut, res, shrp) / 2;

                if (f > 0)
                { 
                    DrawLine(
                        sprites, 
                        x + w*(f-step), 
                        y + h - prev*h,
                        x + w*f,
                        y + h - val*h,
                        color,
                        width);
                }

                prev = val;
            }
        }


        static float ApplyFilter(float value, Source src, float pos, long gTime, long lTime, long sTime, int len, Note note, int iSrc, List<TriggerValue> triggerValues, Program prog)
        {
            if (prog.TooComplex) return value;


            if (src.Filter != null)
            {
                value *= GetFilter(
                    pos,
                    src.Filter.Pass,
                    src.Filter.Cutoff   .GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues, prog), 
                    src.Filter.Resonance.GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues, prog),
                    src.Filter.Sharpness.GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues, prog));
            }

            var inst = src.Instrument;
            if (inst.Filter != null)
            {
                value *= GetFilter(
                    pos, 
                    inst.Filter.Pass,
                    inst.Filter.Cutoff   .GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues, prog), 
                    inst.Filter.Resonance.GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues, prog),
                    inst.Filter.Sharpness.GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues, prog));
            }
            
            return value;
        }


        static string GetPassName(FilterPass pass)
        {
            switch (pass)
            {
                case FilterPass.Low:  return "Lo";
                case FilterPass.High: return "Hi";
                case FilterPass.Band: return "Bnd";
            }

            return "";
        }
    }
}
