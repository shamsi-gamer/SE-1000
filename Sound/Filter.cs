﻿using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class Filter : Setting
        {
            public Parameter Cutoff,
                             Resonance;


            public Filter() : base("Flt", null)
            {
                Cutoff    = (Parameter)NewSettingFromTag("Cut", this);
                Resonance = (Parameter)NewSettingFromTag("Res", this);
            }


            public Filter(Filter flt) : base(flt.Tag, null, flt)
            {
                Cutoff    = new Parameter(flt.Cutoff,    this);
                Resonance = new Parameter(flt.Resonance, this);
            }


            public Filter Copy()
            {
                return new Filter(this);
            }


            public override void Clear()
            {
                Cutoff   .Clear();
                Resonance.Clear();
            }


            public override void Randomize()
            {
                if (g_rnd.NextDouble() > 0.8f) Cutoff.Randomize();
                else                           Cutoff.Clear();

                if (g_rnd.NextDouble() > 0.8f) Resonance.Randomize();
                else                           Resonance.Clear();
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(song, Cutoff,     g_remote.RotationIndicator.Y/ControlSensitivity);
                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(song, Resonance, -g_remote.RotationIndicator.X/ControlSensitivity);
            }


            //public override Setting NewSetting(string tag)
            //{
            //    switch (tag)
            //    {
            //        case "Cut": return Cutoff;
            //        case "Res": return Resonance;
            //    }

            //    return null;
            //}


            public override string Save()
            {
                return
                      W(Tag)

                    + W(Cutoff   .Save())
                    +   Resonance.Save();
            }


            public static Filter Load(string[] data, ref int i)
            {
                var tag = data[i++];
 
                var flt = new Filter();

                flt.Cutoff    = Parameter.Load(data, ref i, flt);
                flt.Resonance = Parameter.Load(data, ref i, flt);

                return flt;
            }
        }


        static float GetCos(float f, float start, float end, float bias, float pow)
        {
            var _f  = f;
            var val = 1f;

            if (bias >= 0)
            { 
                bias = (bias+1)/2;

                _f  = (float)Math.Pow(_f, pow*bias);
                val = (float)Math.Cos(((start + (end-start)*_f) + 0.5) * Tau);
            }
            else if (bias < 0)
            { 
                _f  = 1-(float)Math.Pow(1-_f, 1+pow*Math.Abs(bias));
                val = (float)Math.Cos(((start + (end-start)*_f) + 0.5) * Tau);
            }

            val = (val+1)/2;

            var _min = 1 - (float)(Math.Cos(start * Tau) + 1) / 2;
            var _max = 1 - (float)(Math.Cos(end   * Tau) + 1) / 2;

            var min = Math.Min(_min, _max);
            var max = Math.Max(_min, _max);

            val = (val - min) / (max - min);

            return val;
        }


        static float GetFilter(float f, float cut, float res)
        {
            var val = 1f;

            var _f = f - cut;

                 if (_f >=  1.25f              ) val = 0;
            else if (_f >=  1     && _f < 1.25f) val =     GetCos((_f-1)*4, 0.5f,  0.75f, -res,  3) * (1+res); // low end
            else if (_f >=  0.5f  && _f < 1    ) val = 1 + GetCos(2*_f-1,   0,     0.5f,   res, 10) *  res;    // low middle
            else if (_f >=  0     && _f < 0.5f ) val = 1 + GetCos(2*_f,     0.5f,  1,     -res, 10) *  res;    // high middle
            else if (_f >= -0.25f && _f < 0    ) val =     GetCos( -_f*4,   0.5f,  0.75f, -res,  3) * (1+res); // high end
            else if (_f <  -0.25f              ) val = 0;
 
            return MinMax(0, val, 2);
            //return GetCos(f, 0.5f, 0.75f, cut);
        }


        static void DrawFilter(List<MySprite> sprites, float x, float y, float w, float h, Color color, float width, float cut, float res)
        {
            var step = 1/64f;
            var prev = fN;

            for (var f = 0f; f <= 1; f += step)
            {
                var val = GetFilter(f, cut, res) / 2;

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


        static float ApplyFilter(float value, Source src, float pos, long gTime, long lTime, long sTime, int len, Note note, int iSrc, List<TriggerValue> triggerValues)
        {
            if (src.Filter != null)
            {
                value *= GetFilter(
                    pos, 
                    src.Filter.Cutoff   .GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues), 
                    src.Filter.Resonance.GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues));
            }

            var inst = src.Instrument;
            if (inst.Filter != null)
            {
                value *= GetFilter(
                    pos, 
                    inst.Filter.Cutoff   .GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues), 
                    inst.Filter.Resonance.GetValue(gTime, lTime, sTime, len, note, iSrc, triggerValues));
            }
            
            return value;
        }
    }
}
