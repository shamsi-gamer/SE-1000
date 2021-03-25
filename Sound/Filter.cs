using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public enum FilterPass { Low, High, Band, Stop };

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
            shrp = Math.Min(shrp, 0.9999f);

            var val = 1f;
            var rw  = 1 - shrp;

            if (pass == FilterPass.Low)
            {
                var cw = (1+rw) * cut;
                var c  = 1 - (float)Math.Pow(f / cw, (6.4f/rw-1)*cut + 1);
                var r  = (1 - (float)Math.Cos(1/rw*Tau*MinMax(0, f + rw - cw, rw))) / 4;

                val = c + r * res;
            }
            else if (pass == FilterPass.High)
            {
                var cw = (1+rw) * (1-cut);
                var c  = 1 - (float)Math.Pow((1-f) / cw, (6.4f/rw-1)*(1-cut) + 1);
                var r  = (1 - (float)Math.Cos(1/rw*Tau*MinMax(0, 1-f + rw - cw, rw))) / 4;

                val = c + r * res;
            }
            else if (pass == FilterPass.Band)
            {
                var f1 =  f - cut - res/2;
                var f2 = -f + cut - res/2;

                     if (0 <= f1 && f1 <= 1) val = 1 - (float)Math.Pow(f1/(1-shrp), 1/(1-shrp));
                else if (0 <= f2 && f2 <= 1) val = 1 - (float)Math.Pow(f2/(1-shrp), 1/(1-shrp));
                else if (f > cut - res/2
                      && f < cut + res/2)    val = 1;
                else                         val = 0;

            }
            else if (pass == FilterPass.Stop)
            {
                var f1 =  f - cut - res/2;
                var f2 = -f + cut - res/2;

                     if (0 <= f1 && f1 <= 1-shrp) val = (1 - (float)Math.Cos(Tau*f1 / (2 * (1-shrp)))) / 2;
                else if (0 <= f2 && f2 <= 1-shrp) val = (1 - (float)Math.Cos(Tau*f2 / (2 * (1-shrp)))) / 2;
                else if (f > cut - res/2
                      && f < cut + res/2)         val = 0;
                else                              val = 1;
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
                case FilterPass.Stop: return "Stp";
            }

            return "";
        }
    }
}
