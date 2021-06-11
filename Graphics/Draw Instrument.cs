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

            var inst = SelInstrument;

            var lw = 196;

            var w4 = (w-lw)/4;
            var w3 = (w-lw)/3;
            var w2 = (w-lw)/2;


            FillRect(sprites, x, y, w, h, color0);

            // settings background
            FillRect(sprites, x + lw, y, w-lw, instHeight, color2);


            DrawInstrumentList(sprites, x, y, lw, instHeight, SelChannel);


            FillRect(sprites, x + lw,      0,    w2, rh/4, color3);
            FillRect(sprites, x + lw,      rh/8, w2, 1,    color5);
                                           
            FillRect(sprites, x + lw + w2, 0,     1, rh/4, color5);


            // instrument separator line
            FillRect(sprites, x + lw, rh/4, w-lw, 1, color5);


            // separate sources
            DrawLine(sprites, 
                x + lw + w3, 
                rh/4,
                x + lw + w3,
                instHeight,
                color6);


            if (!TooComplex)
            { 
                ClipDraw(sprites, x + lw + w2, 0, w - x - lw - w2, rh/4);

                var dpInst = new DrawParams(this);
                SelInstrument.DrawLabels(sprites, x + lw + w2 + 21, y + 5, dpInst);

                ClearClip(sprites);
            }


            if (!TooComplex)
                DrawCurrentSetting(
                    sprites, 
                    x + lw, 
                    y + rh/4, 
                    (w-lw)/3, 
                    instHeight - rh/4);

            if (!TooComplex)
                DrawSourceList(
                    sprites,
                    x + lw + (w - lw)/3,
                    y + rh/4,
                    (w - lw)*2/3f,
                    instHeight - rh/4,
                    inst);


            // bottom func separator
            FillRect(sprites, x, y + instHeight, w, 1, color6);

            if (!TooComplex)
                DrawFuncButtons(sprites, w, h, EditedClip);
        }


        void DrawInstrumentList(List<MySprite> sprites, float x, float y, float w, float h, Channel chan)
        {
            var maxNameLength = 13;
            var sw = 20;

            var iInst = Instruments.IndexOf(chan.Instrument);
            var step  = 28.5f;

            if (   OK(SelChan) 
                && CurSrc < 0)
                FillRect(sprites, x + sw, y + (iInst - EditedClip.InstOff) * step, w - sw, step, color6);

            for (int i = EditedClip.InstOff; i < Math.Min(EditedClip.InstOff + maxDspInst, Instruments.Count); i++)
            {
                if (TooComplex) break;

                var inst = Instruments[i];

                DrawString(
                    sprites, 
                    inst.Name.Substring(0, Math.Min(inst.Name.Length, maxNameLength)),
                    x + sw + 5,
                    y + (i - EditedClip.InstOff) * step,
                    0.7f,
                    inst == chan.Instrument 
                    ? (OK(CurSrc) ? color6 : color0) 
                    : (OK(CurSrc) ? color3 : color6));
            }

            FillRect(sprites, x + w - 4, y, 4, h, color6);

            if (   OK(SelChan) 
                && Instruments.Count > maxDspInst)
            {
                var bh = h / (float)Instruments.Count;
                FillRect(sprites, x, y + bh * iInst, sw, bh, color6);
            }
        }


        void DrawSourceList(List<MySprite> sprites, float x, float y, float w, float h, Instrument inst)
        {
            ClipDraw(sprites, x, y, w, h);

            var nSrc = inst.Sources.Count;
            var sw   = 20;

            var iy = y - EditedClip.SrcOff;

            for (int i = EditedClip.SrcOff; i < Math.Min(EditedClip.SrcOff + maxDspSrc, nSrc); i++)
            {
                if (TooComplex) break;
                inst.Sources[i].DrawSource(sprites, x + sw, ref iy, w - sw, this);
            }

            if (OK(CurSrc) && nSrc > 8)
            {
                var bh = h / (float)nSrc;
                FillRect(sprites, x, y + bh * CurSrc, sw, bh, color6);
            }

            ClearClip(sprites);
        }
    }
}
