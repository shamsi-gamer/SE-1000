using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        //float[] g_sample      = null;
        //bool g_sampleValid = F;


        //void UpdateSample(Instrument inst, float w, float timeScale)
        //{
        //    var iw = (int)Math.Ceiling(w);

        //    if (   g_sample == null
        //        || g_sample.Length != iw)
        //        g_sample = new float[iw];


        //    float prev = fN;

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

        //    g_sampleValid = T;
        //}


        //void DrawInstSample(List<MySprite> sprites, float x, float y, float w, float h, Instrument inst, bool active, float timeScale)
        //{
        //    if (!g_sampleValid)
        //        UpdateSample(inst, w, timeScale);


        //    var pPrev = new Vector2(fN, fN);
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




    }
}
