﻿using System;
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

            var inst = g_clip.SelectedInstrument;

            var lw = 196;

            var w4 = (w-lw)/4;
            var w3 = (w-lw)/3;
            var w2 = (w-lw)/2;


            FillRect(sprites, x, y, w, h, color0);

            // settings background
            FillRect(sprites, x + lw, y, w-lw, instHeight, color2);


            DrawInstrumentList(sprites, x, y, lw, instHeight, g_clip.SelectedChannel);


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


            var dpInst = new DrawParams(this);
            g_clip.SelectedInstrument.DrawLabels(sprites, x + lw + w2 + 21, y + 5, dpInst);


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
            
            DrawFuncButtons(sprites, w, h, g_clip);
        }


        void DrawInstrumentList(List<MySprite> sprites, float x, float y, float w, float h, Channel chan)
        {
            var maxNameLength = 13;
            var sw = 20;

            var iInst = g_inst.IndexOf(chan.Instrument);
            var step  = 28.5f;

            if (g_clip.SelChan > -1 && g_clip.CurSrc < 0)
                FillRect(sprites, x + sw, y + (iInst - g_clip.InstOff) * step, w - sw, step, color6);

            for (int i = g_clip.InstOff; i < Math.Min(g_clip.InstOff + maxDspInst, g_inst.Count); i++)
            {
                var inst = g_inst[i];

                DrawString(
                    sprites, 
                    inst.Name.Substring(0, Math.Min(inst.Name.Length, maxNameLength)),
                    x + sw + 5,
                    y + (i - g_clip.InstOff) * step,
                    0.7f,
                    inst == chan.Instrument 
                    ? (g_clip.CurSrc > -1 ? color6 : color0) 
                    : (g_clip.CurSrc > -1 ? color3 : color6));
            }

            FillRect(sprites, x + w - 4, y, 4, h, color6);

            if (g_clip.SelChan > -1 && g_inst.Count > maxDspInst)
            {
                var bh = h / (float)g_inst.Count;
                FillRect(sprites, x, y + bh * iInst, sw, bh, color6);
            }
        }


        void DrawSourceList(List<MySprite> sprites, float x, float y, float w, float h, Instrument inst)
        {
            var nSrc = inst.Sources.Count;
            var sw   = 20;

            var iy = y - g_clip.SrcOff;

            for (int i = g_clip.SrcOff; i < Math.Min(g_clip.SrcOff + maxDspSrc, nSrc); i++)
                inst.Sources[i].DrawSource(sprites, x + sw, ref iy, w - sw, this);

            if (g_clip.CurSrc > -1 && nSrc > 8)
            {
                var bh = h / (float)nSrc;
                FillRect(sprites, x, y + bh * g_clip.CurSrc, sw, bh, color6);
            }
        }
    }
}
