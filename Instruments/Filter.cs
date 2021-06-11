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


            public Filter(Instrument inst, Source src) 
                : base(strFlt, Setting_null, Setting_null, inst, src)
            {
                Pass      = FilterPass.Low;

                Cutoff    = (Parameter)NewSettingFromTag(strCut,  this, inst, src);
                Resonance = (Parameter)NewSettingFromTag(strRes,  this, inst, src);
                Sharpness = (Parameter)NewSettingFromTag(strShrp, this, inst, src);
            }


            public Filter(Filter flt) : base(flt.Tag, Setting_null, flt, flt.Instrument, flt.Source)
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


            public override void Reset()
            {
                base.Reset();

                Cutoff   .Reset();
                Resonance.Reset();
                Sharpness.Reset();
            }


            public override void Randomize(Program prog)
            {
                if (prog.TooComplex) return;

                Pass = (FilterPass)g_rnd.Next((int)FilterPass.Band + 1);

                if (RND > 0.8f) Cutoff   .Randomize(prog); else Cutoff   .Clear();
                if (RND > 0.8f) Resonance.Randomize(prog); else Resonance.Clear();
                if (RND > 0.8f) Sharpness.Randomize(prog); else Sharpness.Clear();
            }


            public override void AdjustFromController(Clip clip)
            {
                if (g_remote.MoveIndicator    .Z != 0) Program.AdjustFromController(clip, Sharpness, -g_remote.MoveIndicator    .Z/ControlSensitivity);

                if (g_remote.RotationIndicator.Y != 0) Program.AdjustFromController(clip, Cutoff,     g_remote.RotationIndicator.Y/ControlSensitivity);
                if (g_remote.RotationIndicator.X != 0) Program.AdjustFromController(clip, Resonance, -g_remote.RotationIndicator.X/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strCut:  return GetOrAddParamFromTag(Cutoff,    tag);
                    case strRes:  return GetOrAddParamFromTag(Resonance, tag);
                    case strShrp: return GetOrAddParamFromTag(Sharpness, tag);
                }

                return Setting_null;
            }


            public void Delete(Clip clip, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Cutoff   ?.Delete(clip, iSrc);
                Resonance?.Delete(clip, iSrc);
                Sharpness?.Delete(clip, iSrc);
            }


            public override string Save()
            {
                return
                      W(Tag)

                    + WS((int)Pass)
                    + W (Cutoff   .Save())
                    + W (Resonance.Save())
                    +    Sharpness.Save();
            }


            public static Filter Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tag = data[i++];
 
                var flt = new Filter(
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);

                flt.Pass      = (FilterPass)int_Parse(data[i++]);
                flt.Cutoff    = Parameter.Load(data, ref i, inst, iSrc, flt, flt.Cutoff   );
                flt.Resonance = Parameter.Load(data, ref i, inst, iSrc, flt, flt.Resonance);
                flt.Sharpness = Parameter.Load(data, ref i, inst, iSrc, flt, flt.Sharpness);

                return flt;
            }


            public override string GetLabel(out float width)
            {
                width = 138;

                return
                      PrintValue(Cutoff   .Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Resonance.Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Sharpness.Value, 2, True, 0).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                if (!_dp.Program.TooComplex)
                {
                    base.DrawLabels(sprites, x, y, dp);

                    if (Cutoff   .HasDeepParams(CurChannel, CurSrc)) Cutoff   .DrawLabels(sprites, x, y, dp);
                    if (Resonance.HasDeepParams(CurChannel, CurSrc)) Resonance.DrawLabels(sprites, x, y, dp);
                }

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var cut     = Cutoff   .Value;
                var res     = Resonance.Value;
                var shrp    = Sharpness.Value;

                var curCut  = Cutoff   .CurValue;
                var curRes  = Resonance.CurValue;
                var curShrp = Sharpness.CurValue;

                var w0      = 240f;
                var h0      = 120f;
                            
                var x0      = x + w/2 - w0/2;
                var y0      = y + h/2 - h0/2;


                FillRect(sprites, x0, y0 + h0, 2, -h0, color3);
                FillRect(sprites, x0, y0 + h0, w0,  2, color3);


                DrawFilter(sprites, x0, y0, w0, h0, color3, 4, Pass, curCut, curRes, curShrp);
                DrawFilter(sprites, x0, y0, w0, h0, color5, 4, Pass, cut,    res,    shrp);


                var _strCut = Pass > FilterPass.High ? strFreq : strCut;
                var _strRes = Pass > FilterPass.High ? strWid  : strRes;

                var fs = 0.5f;

                // cutoff
                DrawString(sprites, strCut,                       x0,       y0 - 40, fs, IsCurParam(strCut) ? color6 : color4);
                DrawString(sprites, PrintValue(cut, 2, True, 0),  x0,       y0 - 25, fs, IsCurParam(strCut) ? color6 : color4);
                                                                  
                // resonance                                      
                DrawString(sprites, strRes,                       x0 + 100, y0 - 40, fs, IsCurParam(strRes) ? color6 : color4);
                DrawString(sprites, PrintValue(res, 2, True, 0),  x0 + 100, y0 - 25, fs, IsCurParam(strRes) ? color6 : color4);

                // sharpness
                DrawString(sprites, strShrp,                      x0 + 200, y0 - 40, fs, IsCurParam(strShrp) ? color6 : color4);
                DrawString(sprites, PrintValue(shrp, 2, True, 0), x0 + 200, y0 - 25, fs, IsCurParam(strShrp) ? color6 : color4);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                var _strCut  = Pass > FilterPass.High ? strFreq : strCut;
                var _strRes  = Pass > FilterPass.High ? strWid  : strRes;

                DrawFuncButton(sprites, GetPassName(Pass) + " ↕", 1, w, h, False, False);
                DrawFuncButton(sprites, _strCut,  2, w, h, True, Cutoff   .HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, _strRes,  3, w, h, True, Resonance.HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strShrp,  4, w, h, True, Sharpness.HasDeepParams(chan, CurSrc));
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
                            g_lcdPressed.Add(func);

                            break;
                        }
                    case 2: AddNextSetting(strCut);  break;
                    case 3: AddNextSetting(strRes);  break;
                    case 4: AddNextSetting(strShrp); break;
                }
            }


            public override bool CanDelete()
            {
                return True;
            }
        }


        static float GetFilter(float f, FilterPass pass, float cut, float res, float shrp)
        {
            shrp = Math.Min(shrp, 1);

            var val = 1f;
            var rw  = Nozero(1-shrp);

            if (pass == FilterPass.Low)
            {
                var cw = Nozero((1+rw) * cut);
                var c  =  1 - (float)Math.Pow(f/cw, (6.4f/rw-1)*cut + 1);
                var r  = (1 - (float)Math.Cos(1/rw*Tau*MinMax(0, f + rw - cw, rw))) / 3;

                val = c + r * res;
            }
            else if (pass == FilterPass.High)
            {
                var cw = Nozero((1+rw) * (1-cut));
                var c  = 1 - (float)Math.Pow((1-f) / cw, (6.4f/rw-1)*(1-cut) + 1);
                var r  = (1 - (float)Math.Cos(1/rw*Tau*MinMax(0, 1-f + rw - cw, rw))) / 3;

                val = c + r * res;
            }
            else if (pass == FilterPass.Band)
            {
                var f1 =  f - cut - res/2;
                var f2 = -f + cut - res/2;

                var s = Nozero(1-shrp);

                     if (0 <= f1 && f1 <= 1) val = 1 - (float)Math.Pow(f1/s, 1/s);
                else if (0 <= f2 && f2 <= 1) val = 1 - (float)Math.Pow(f2/s, 1/s);
                else if (f > cut - res/2
                      && f < cut + res/2)    val = 1;
                else                         val = 0;

            }
            else if (pass == FilterPass.Stop)
            {
                var f1 =  f - cut - res/2;
                var f2 = -f + cut - res/2;

                var s = 2 * Nozero(1-shrp);

                     if (0 <= f1 && f1 <= 1-shrp) val = (1 - (float)Math.Cos(Tau*f1/s)) / 2;
                else if (0 <= f2 && f2 <= 1-shrp) val = (1 - (float)Math.Cos(Tau*f2/s)) / 2;
                else if (f > cut - res/2
                      && f < cut + res/2)         val = 0;
                else                              val = 1;
            }

            return MinMax(0, val, 2);
        }


        static void DrawFilter(List<MySprite> sprites, float x, float y, float w, float h, Color color, float width, FilterPass pass, float cut, float res, float shrp)
        {
            var step = 1/48f;
            var prev = float_NaN;

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

            if (OK(src.Filter))
            {
                value *= GetFilter(
                    pos,
                    src.Filter.Pass,
                    src.Filter.Cutoff   .UpdateValue(tp), 
                    src.Filter.Resonance.UpdateValue(tp),
                    src.Filter.Sharpness.UpdateValue(tp));
            }

            var inst = src.Instrument;
            if (OK(inst.Filter))
            {
                value *= GetFilter(
                    pos, 
                    inst.Filter.Pass,
                    inst.Filter.Cutoff   .UpdateValue(tp), 
                    inst.Filter.Resonance.UpdateValue(tp),
                    inst.Filter.Sharpness.UpdateValue(tp));
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
