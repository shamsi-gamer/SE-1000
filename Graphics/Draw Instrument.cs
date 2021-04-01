using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawInstrument(List<MySprite> sprites, float x, float y, float w, float h)
        {
            var rh         = h - 50;
            var instHeight = h - 80;

            var inst = SelectedInstrument;

            var lw = 196;

            var w4 = (w-lw)/4;
            var w3 = (w-lw)/3;
            var w2 = (w-lw)/2;


            FillRect(sprites, x,      y, w,    h,  color0);

            // settings background
            FillRect(sprites, x + lw, y, w-lw, instHeight, color2);


            DrawInstrumentList(sprites, x, y, lw, instHeight, SelectedChannel);


            FillRect(sprites, x + lw,      0,    w2, rh/4, color3);
            FillRect(sprites, x + lw,      rh/8, w2, 1,    color5);
                                           
            FillRect(sprites, x + lw + w2, 0,     1, rh/4, color5);

            //DrawInstSample(
            //    sprites,
            //    x + lw,
            //    y + rh/16,
            //    w2,
            //    rh/8,
            //    inst,
            //    F,
            //    timeScale);


            // instrument separator line
            FillRect(sprites, x + lw, rh/4, w-lw, 1, color5);


            // separate sources
            DrawLine(sprites, 
                x + lw + w3, 
                rh/4,
                x + lw + w3,
                instHeight,
                color6);


            // draw time position line
            //foreach (var note in g_notes)
            //{
            //    if (   note.Instrument == inst
            //        && g_time - note.PatTime >= 0
            //        && g_time - note.PatTime < note.FrameLength)
            //    {
            //        FillRect(sprites, 
            //            x + lw + w2 * (g_time - note.PatTime) / (float)FPS / timeScale,
            //            0,
            //            3,
            //            rh/4,
            //            color6);
            //    }
            //}



            DrawInstrumentLabels(
                sprites,
                SelectedInstrument,
                x + lw + w2 + 21,
                y + 5);


            DrawCurrentSetting(
                sprites, 
                x + lw, 
                y + rh/4, 
                (w-lw)/3, 
                instHeight - rh/4);

            DrawSourceList(
                sprites, 
                x + lw + (w-lw)/3, 
                y + rh/4, 
                (w-lw)*2/3f, 
                instHeight - rh/4, 
                inst);


            // bottom func separator
            FillRect(sprites, x, y + instHeight, w, 1, color6);
            
            DrawFuncButtons(sprites, w, h - 74, g_song);
        }


        void DrawInstrumentLabels(List<MySprite> sprites, Instrument inst, float x, float y)
        {
            var dp = new DrawParams(false, this);

            inst.Volume   .DrawLabel(sprites, x, y + dp.OffY, dp);
            inst.Tune    ?.DrawLabel(sprites, x, y + dp.OffY, dp);
            inst.Filter  ?.DrawLabel(sprites, x, y + dp.OffY, dp);
            inst.Delay   ?.DrawLabel(sprites, x, y + dp.OffY, dp);
            inst.Arpeggio?.DrawLabel(sprites, x, y + dp.OffY, dp);
        }


        void DrawSourceLabels(List<MySprite> sprites, Source src, float x, float y, DrawParams dp)
        {
            src.Offset   ?.DrawLabel(sprites, x, y + dp.OffY, dp);
            src.Volume    .DrawLabel(sprites, x, y + dp.OffY, dp);
            src.Tune     ?.DrawLabel(sprites, x, y + dp.OffY, dp);
            src.Harmonics?.DrawLabel(sprites, x, y + dp.OffY, dp);
            src.Filter   ?.DrawLabel(sprites, x, y + dp.OffY, dp);
            src.Delay    ?.DrawLabel(sprites, x, y + dp.OffY, dp);
        }


        void DrawInstrumentList(List<MySprite> sprites, float x, float y, float w, float h, Channel chan)
        {
            var maxNameLength = 13;
            var sw = 20;

            var iInst = g_inst.IndexOf(chan.Instrument);
            var step  = 28.5f;

            if (SelChan > -1 && CurSrc < 0)
                FillRect(sprites, x + sw, y + (iInst - g_instOff) * step, w - sw, step, color6);

            for (int i = g_instOff; i < Math.Min(g_instOff + maxDspInst, g_inst.Count); i++)
            {
                var inst = g_inst[i];

                DrawString(
                    sprites, 
                    inst.Name.Substring(0, Math.Min(inst.Name.Length, maxNameLength)),
                    x + sw + 5,
                    y + (i - g_instOff) * step,
                    0.7f,
                    inst == chan.Instrument 
                    ? (CurSrc > -1 ? color6 : color0) 
                    : (CurSrc > -1 ? color3 : color6));
            }

            FillRect(sprites, x + w - 4, y, 4, h, color6);

            if (SelChan > -1 && g_inst.Count > maxDspInst)
            {
                var bh = h / (float)g_inst.Count;
                FillRect(sprites, x, y + bh * iInst, sw, bh, color6);
            }
        }


        void DrawSourceList(List<MySprite> sprites, float x, float y, float w, float h, Instrument inst)
        {
            var nSrc = inst.Sources.Count;
            var sw   = 20;


            var iy = y - g_srcOff;

            for (int i = g_srcOff; i < Math.Min(g_srcOff + maxDspSrc, nSrc); i++)
            {
                var src    = inst.Sources[i];
                var active = i == CurSrc && CurSet < 0;

                //var ssh = 0f;
                var dp1 = new DrawParams(active, this);
                DrawSourceLabels(null, inst.Sources[i], 0, 0, dp1);//, ref ssh);

                var sh = dp1.OffY + 20;

                if (CurSrc == i && CurSet < 0)
                    FillRect(sprites, x + sw, iy, w - sw, sh, color6);


                var col_0 = src.On && CurSrc > -1 && CurSet < 0 ? color6 : color3;
                var col_1 = src.On && CurSrc > -1 && CurSet < 0 ? color0 : color5;

                if (src.Oscillator == OscSample)
                {
                    DrawString(sprites, src.Oscillator.ShortName, x + sw + 10, iy + sh/2 - 10, 0.7f, i == CurSrc ? col_1 : col_0, TaC);
                }
                else
                { 
                    DrawSrcSample(sprites, x + sw + 10, iy + sh/2 - 10, 50, 20, src, active, CurSrc > -1);
                    DrawString(sprites, src.Oscillator.ShortName, x + sw + 100, iy + sh/2 - 10, 0.6f, active ? col_1 : col_0, TaC);
                }


                var dp2 = new DrawParams(active, this);
                DrawSourceLabels(sprites, src, x + sw + 139, iy + sh/2 - dp1.OffY/2 + 2, dp2);

                FillRect(sprites, x + sw, iy + sh, w - sw, 1, color3);

                iy += sh;
            }


            if (CurSrc > -1 && nSrc > 8)
            {
                var bh = h / (float)nSrc;
                FillRect(sprites, x, y + bh * CurSrc, sw, bh, color6);
            }
        }
    }
}
