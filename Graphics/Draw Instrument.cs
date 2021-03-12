using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawInstrument(List<MySprite> sprites, float x, float y, float w, float h)
        {
            var timeScale = 2;

            var rh = h - 50;

            var inst = SelectedInstrument(g_song);

            var lw = 196;

            var w4 = (w-lw)/4;
            var w3 = (w-lw)/3;
            var w2 = (w-lw)/2;


            FillRect(sprites, x,      y, w,    h,  color0);
            FillRect(sprites, x + lw, y, w-lw, rh, color2);


            DrawInstrumentList(sprites, x, y, lw, rh, SelectedChannel(g_song));


            FillRect(sprites, x + lw,      0,    w2, rh/4, color3);
            FillRect(sprites, x + lw,      rh/8, w2, 1,    color5);
                                           
            FillRect(sprites, x + lw + w2, 0,    1,  rh/4, color5);

            //DrawInstSample(
            //    sprites,
            //    x + lw,
            //    y + rh/16,
            //    w2,
            //    rh/8,
            //    inst,
            //    false,
            //    timeScale);


            FillRect(sprites, x + lw, rh/4, w-lw, 1, color5);


            // separate sources
            DrawLine(sprites, 
                x + lw + w3, 
                rh/4,
                x + lw + w3,
                rh,
                color6);


            // draw time position line
            foreach (var note in g_notes)
            {
                if (   note.Instrument == inst
                    && g_time - note.PatTime >= 0
                    && g_time - note.PatTime < note.FrameLength)
                {
                    FillRect(sprites, 
                        x + lw + w2 * (g_time - note.PatTime) / (float)FPS / timeScale,
                        0,
                        3,
                        rh/4,
                        color6);
                }
            }


            DrawSettings(sprites, x + lw, y + rh/4 + 20, (w-lw)/3, rh - rh/4);

            DrawInstrumentSettings(
                sprites,
                x + lw + w2 + 21, 
                y + 5, 
                SelectedInstrument(g_song));


            DrawSourceList(sprites, x + lw + (w-lw)/3, y + rh/4, (w-lw)*2/3f, rh - rh/4, inst);

            FillRect(sprites, x, y + rh, w, 1, color6);
            DrawFuncButtons(sprites, w, h, g_song);
        }


        void DrawInstrumentSettings(List<MySprite> sprites, float x, float y, Instrument inst)
        {
            float yo = 0;
                                       DrawSetting(sprites, inst.Volume,   x, y + yo, ref yo, false);
            if (inst.Tune     != null) DrawSetting(sprites, inst.Tune,     x, y + yo, ref yo, false);
            if (inst.Filter   != null) DrawSetting(sprites, inst.Filter,   x, y + yo, ref yo, false);
            if (inst.Delay    != null) DrawSetting(sprites, inst.Delay,    x, y + yo, ref yo, false);
            if (inst.Arpeggio != null) DrawSetting(sprites, inst.Arpeggio, x, y + yo, ref yo, false);
        }


        void DrawSourceSettings(List<MySprite> sprites, float x, float y, Source src, bool active, ref float yo)
        {
            if (src.Offset    != null) DrawSetting(sprites, src.Offset,    x, y + yo, ref yo, active);
                                       DrawSetting(sprites, src.Volume,    x, y + yo, ref yo, active);
            if (src.Tune      != null) DrawSetting(sprites, src.Tune,      x, y + yo, ref yo, active);
            if (src.Harmonics != null) DrawSetting(sprites, src.Harmonics, x, y + yo, ref yo, active);
            if (src.Filter    != null) DrawSetting(sprites, src.Filter,    x, y + yo, ref yo, active);
            if (src.Delay     != null) DrawSetting(sprites, src.Delay,     x, y + yo, ref yo, active);
        }


        void DrawInstrumentList(List<MySprite> sprites, float x, float y, float w, float h, Channel chan)
        {
            var maxNameLength = 13;
            var sw = 20;

            var iInst = g_inst.IndexOf(chan.Instrument);
            var step  = 28.5f;

            if (g_song.SelChan > -1 && g_song.CurSrc < 0)
                FillRect(sprites, x + sw, y + (iInst - instOff) * step, w - sw, step, color6);

            for (int i = instOff; i < Math.Min(instOff + maxDspInst, g_inst.Count); i++)
            {
                var inst = g_inst[i];

                DrawString(
                    sprites, 
                    inst.Name.Substring(0, Math.Min(inst.Name.Length, maxNameLength)),
                    x + sw + 5,
                    y + (i - instOff) * step,
                    0.7f,
                    inst == chan.Instrument 
                    ? (g_song.CurSrc > -1 ? color6 : color0) 
                    : (g_song.CurSrc > -1 ? color3 : color6));
            }

            FillRect(sprites, x + w - 4, y, 4, h, color6);

            if (g_song.SelChan > -1 && g_inst.Count > maxDspInst)
            {
                var bh = h / (float)g_inst.Count;
                FillRect(sprites, x, y + bh * iInst, sw, bh, color6);
            }
        }


        void DrawSourceList(List<MySprite> sprites, float x, float y, float w, float h, Instrument inst)
        {
            var nSrc = inst.Sources.Count;
            var sw   = 20;


            var iy = y - srcOff;

            for (int i = srcOff; i < Math.Min(srcOff + maxDspSrc, nSrc); i++)
            {
                var src    = inst.Sources[i];
                var active = i == g_song.CurSrc;

                var ssh = 0f;
                DrawSourceSettings(null, 0, 0, inst.Sources[i], active, ref ssh);
                var sh = ssh + 20;

                if (g_song.CurSrc == i)
                    FillRect(sprites, x + sw, iy, w - sw, sh, color6);


                var col_0 = src.On && g_song.CurSrc > -1 ? color6 : color3;
                var col_1 = src.On && g_song.CurSrc > -1 ? color0 : color5;

                if (   src.Oscillator == Oscillator.Samples1
                    || src.Oscillator == Oscillator.Samples2)
                {
                    var strSamples = "Samples " + (1 + src.Oscillator - Oscillator.Samples1).ToString();
                    DrawString(sprites, strSamples, x + sw + 10, iy + sh/2 - 10, 0.7f, i == g_song.CurSrc ? col_1 : col_0, TextAlignment.CENTER);
                }
                else
                { 
                    DrawSrcSample(sprites, x + sw + 10, iy + sh/2 - 10, 50, 20, src, active, g_song.CurSrc > -1);

                    var srcName = "";

                    switch (src.Oscillator)
                    {
                    case Oscillator.Sine:      srcName = "Sine";  break;
                    case Oscillator.Triangle:  srcName = "Tri";   break;
                    case Oscillator.Saw:       srcName = "Saw";   break;
                    case Oscillator.Square:    srcName = "Sqr";   break;
                    case Oscillator.LowNoise:  srcName = "Lo #";  break;
                    case Oscillator.HighNoise: srcName = "Hi #";  break;
                    case Oscillator.BandNoise: srcName = "Bnd #"; break;
                    }

                    DrawString(sprites, srcName, x + sw + 100, iy + sh/2 - 10, 0.6f, active ? col_1 : col_0, TextAlignment.CENTER);
                }


                var yo = 0f;
                DrawSourceSettings(sprites, x + sw + 139, iy + sh/2 - ssh/2 + 2, src, active, ref yo);

                FillRect(sprites, x + sw, iy + sh, w - sw, 1, color3);

                iy += sh;
            }


            if (g_song.CurSrc > -1 && nSrc > 8)
            {
                var bh = h / (float)nSrc;
                FillRect(sprites, x, y + bh * g_song.CurSrc, sw, bh, color6);
            }
        }
    }
}
