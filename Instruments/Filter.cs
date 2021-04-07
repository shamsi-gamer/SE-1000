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

                Cutoff    = (Parameter)NewFromTag("Cut",  this);
                Resonance = (Parameter)NewFromTag("Res",  this);
                Sharpness = (Parameter)NewFromTag("Shrp", this);
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
                    case "Cut":  return Cutoff    ?? (Cutoff    = (Parameter)NewFromTag("Cut",  this));
                    case "Res":  return Resonance ?? (Resonance = (Parameter)NewFromTag("Res",  this));
                    case "Shrp": return Sharpness ?? (Sharpness = (Parameter)NewFromTag("Shrp", this));
                }

                return null;
            }


            public void Delete(Song song, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Cutoff   ?.Delete(song, iSrc);
                Resonance?.Delete(song, iSrc);
                Sharpness?.Delete(song, iSrc);
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


            public override void GetLabel(out string str, out float width)
            {
                width = 110;

                str =
                      printValue(Cutoff   .CurValue, 2, true, 0).PadLeft(4) + "  "
                    + printValue(Resonance.CurValue, 2, true, 0).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Cutoff   .HasDeepParams(CurrentChannel, CurSrc)) Cutoff   .DrawLabels(sprites, x, y, dp);
                if (Resonance.HasDeepParams(CurrentChannel, CurSrc)) Resonance.DrawLabels(sprites, x, y, dp);

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var cut  = Cutoff   .CurValue;
                var res  = Resonance.CurValue;
                var shrp = Sharpness.CurValue;


                var w0 = 240f;
                var h0 = 120f;

                var x0 = x + w / 2 - w0 / 2;
                var y0 = y + h / 2 - h0 / 2;


                FillRect(sprites, x0, y0 + h0, 2, -h0, color3);
                FillRect(sprites, x0, y0 + h0, w0,  2, color3);


                DrawFilter(sprites, x0, y0, w0, h0, color5, 4, Pass, cut, res, shrp);


                var strCut = Pass > FilterPass.High ? "Freq" : "Cut";
                var strRes = Pass > FilterPass.High ? "Wid"  : "Res";

                var fs = 0.5f;

                // cutoff
                DrawString(sprites, strCut,                       x0,       y0 - 40, fs, IsCurParam("Cut") ? color6 : color3);
                DrawString(sprites, printValue(cut, 2, true, 0),  x0,       y0 - 25, fs, IsCurParam("Cut") ? color6 : color3);
                                                                  
                // resonance                                      
                DrawString(sprites, strRes,                       x0 + 100, y0 - 40, fs, IsCurParam("Res") ? color6 : color3);
                DrawString(sprites, printValue(res, 2, true, 0),  x0 + 100, y0 - 25, fs, IsCurParam("Res") ? color6 : color3);

                // sharpness
                DrawString(sprites, "Shrp",                       x0 + 200, y0 - 40, fs, IsCurParam("Shrp") ? color6 : color3);
                DrawString(sprites, printValue(shrp, 2, true, 0), x0 + 200, y0 - 25, fs, IsCurParam("Shrp") ? color6 : color3);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                var strCut  = Pass > FilterPass.High ? "Freq" : "Cut";
                var strRes  = Pass > FilterPass.High ? "Wid"  : "Res";

                DrawFuncButton(sprites, GetPassName(Pass) + " ↕", 1, w, h, false, false);
                DrawFuncButton(sprites, strCut,  2, w, h, true, Cutoff   .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, strRes,  3, w, h, true, Resonance.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Shrp",  4, w, h, true, Sharpness.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "X",     5, w, h, false, false, mainPressed.Contains(5));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                    case 1:
                        {
                            var p = (int)Pass + 1;

                            if (p > (int)FilterPass.Stop)
                                p = (int)FilterPass.Low;

                            Pass = (FilterPass)p;
                            mainPressed.Add(func);

                            break;
                        }
                    case 2: AddNextSetting("Cut");  break;
                    case 3: AddNextSetting("Res");  break;
                    case 4: AddNextSetting("Shrp"); break;
                    case 5: RemoveSetting(this);    break;
                }
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


        static float ApplyFilter(float value, Source src, float pos, TimeParams tp)
        {
            if (tp.Program.TooComplex) return value;

            if (src.Filter != null)
            {
                value *= GetFilter(
                    pos,
                    src.Filter.Pass,
                    src.Filter.Cutoff   .GetValue(tp), 
                    src.Filter.Resonance.GetValue(tp),
                    src.Filter.Sharpness.GetValue(tp));
            }

            var inst = src.Instrument;
            if (inst.Filter != null)
            {
                value *= GetFilter(
                    pos, 
                    inst.Filter.Pass,
                    inst.Filter.Cutoff   .GetValue(tp), 
                    inst.Filter.Resonance.GetValue(tp),
                    inst.Filter.Sharpness.GetValue(tp));
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
