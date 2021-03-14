using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        //float[] g_sample      = null;
        bool g_sampleValid = false;


        //void UpdateSample(Instrument inst, float w, float timeScale)
        //{
        //    var iw = (int)Math.Ceiling(w);

        //    if (   g_sample == null
        //        || g_sample.Length != iw)
        //        g_sample = new float[iw];


        //    float prev = float.NaN;

        //    for (int t = 0; t < w; t += 2)
        //    {
        //        float val = 0;

        //        for (int i = 0; i < inst.Sources.Count; i++)
        //        {
        //            var src = inst.Sources[i];
        //            if (!src.On) continue;

        //            var freq = (float)(Math.Pow(2, (showNote-69)/12f) / 8);

        //            val += src.GetWaveform(t/w * freq);
        //        }


        //        g_sample[t] = MinMax(-2, val * inst.Volume.CurValue, 2);

        //        if (t > 0)
        //            g_sample[t-1] = (g_sample[t] + prev)/2;

        //        prev = g_sample[t];
        //    }

        //    g_sampleValid = true;
        //}


        //void DrawInstSample(List<MySprite> sprites, float x, float y, float w, float h, Instrument inst, bool active, float timeScale)
        //{
        //    if (!g_sampleValid)
        //        UpdateSample(inst, w, timeScale);


        //    var pPrev = new Vector2(float.NaN, float.NaN);
        //    var iw    = (int)w; // not ceiling like in UpdateSample() because this is for drawing

        //    for (int t = 0; t < iw; t++)
        //    {
        //        var p = new Vector2(
        //            x + w * t/w,
        //            y + h/2 - g_sample[t] * h/2);

        //        if (   OK(pPrev.X)
        //            && OK(pPrev.Y))
        //            DrawLine(sprites, pPrev, p, active ? color3 : color6, active ? 2 : 1);

        //        pPrev = p;
        //    }
        //}


        void DrawSrcSample(List<MySprite> sprites, float x, float y, float w, float h, Source src, bool active, bool bright)
        {
            var col_0 = src.On && bright ? color6 : color3;
            var col_1 = src.On && bright ? color0 : color5;

            var pPrev = new Vector2(float.NaN, float.NaN);


            var df = 1/24f;

            for (float f = 0; f <= 1; f += df)
            {
                var wf = 0f;
                
                if (src.Oscillator == OscClick)
                { 
                         if (f == 0   ) wf =  0;
                    else if (f == df  ) wf =  1;
                    else if (f == df*2) wf = -1;
                    else                wf =  0;
                }
                else if (src.Oscillator == OscCrunch)
                { 
                    var _f = f % (1/4f);

                         if (fequal(_f, 0   )) wf =  0;
                    else if (fequal(_f, df  )) wf =  1;
                    else if (fequal(_f, df*2)) wf = -1;
                    else                       wf =  0;
                }
                else 
                { 
                    wf = src.GetWaveform(f*2.1f / Tau);
                }

                var p = new Vector2(
                    x + w * f,
                    y + h/2 - wf * h/2);

                if (   OK(pPrev.X)
                    && OK(pPrev.Y))
                    DrawLine(sprites, pPrev, p, active ? col_1 : col_0, active ? 2 : 1);

                pPrev = p;
            }
        }


        void DrawHarmonicsSample(List<MySprite> sprites, float x, float y, float w, float h, Song song, Harmonics hrm)
        {
            var pPrev = new Vector2(float.NaN, float.NaN);


            var df = 1/48f;

            for (float f = 0; f < 1+df/2; f += df)
            {
                var wf = 0f;
                
                for (int i = 0; i < hrm.Tones.Length; i++)
                {
                    var val = hrm.Tones[i].CurValue;
                    wf += (float)Math.Sin(f*(i+1) * Tau) * val;
                }

                var p = new Vector2(
                    x + w * f,
                    y + h/2 - wf * h/2);

                if (   OK(pPrev.X)
                    && OK(pPrev.Y))
                    DrawLine(sprites, pPrev, p, color6, 2);

                pPrev = p;
            }
        } 
    }
}
