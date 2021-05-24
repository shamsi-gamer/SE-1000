﻿using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void DrawVolume()
        {
            float maxVol = 0;

            foreach (var vol in EditClip.Track.DspVol)
                maxVol = Math.Max(vol, maxVol);

            if (!TooComplex) DrawVolume(maxVol, dspVol1, 2);
            if (!TooComplex) DrawVolume(maxVol, dspVol2, 1);
            if (!TooComplex) DrawVolume(maxVol, dspVol3, 0);
        }


        void DrawVolume(float vol, Display dsp, int i)
        {
            if (!OK(dsp)) return;

            var bb = 200;
            var bw = 300;

            var Volume = dsp.Viewport;

            var x = Volume.X;
            var y = Volume.Y + 55;
            var w = Volume.Width;
            var h = Volume.Height * 3 - 40;


            var sprites = new List<MySprite>();

            FillRect(sprites, x, Volume.Y, w, Volume.Height, color0);

            var xc = x + bb/2;
            DrawSoundLevel(sprites, xc, y - i * Volume.Height, bw, h - 295, EditClip.Volume, vol, null, 6.5f);

            dsp.Draw(sprites);
        }


        static void DrawSoundLevel(List<MySprite> sprites, float x, float y, float w, float h, float level, float v, Channel chan = null, float scale = 1)
        {
            var wb = w/10;
            var wg = w/20;
            var wl = w - wg - wb;
            var ws = w/4;

            var hk = h/150;
            var sy = 7 * scale;


            var nMarks = 8;
            var pow    = 3f;
            var extra  = 1.15f;

            
            // current level
            FillRect(
                sprites,
                x + ws,
                y + h,
                wl - ws,
                -h * Math.Min((float)Math.Pow(v / extra, pow), 1),
                color4);


            // value marks
            for (int i = 0; i <= nMarks; i++)
            {
                var val = (float)Math.Pow(i / (float)nMarks, pow);

                FillRect(sprites, 
                    x,
                    y + h - (h - hk) * val,
                    wl,
                    hk,
                    color3);

                if (i > (scale > 1 ? 1 : 2))
                {
                    var db = 100 * (float)Math.Log10(i / (float)nMarks * extra);

                    DrawString(sprites, 
                        PrintValue(Math.Abs(db), 0, T, 2),
                        x,
                        y - sy + h - (h - hk) * val,
                        0.25f * scale,
                        color4);
                }
            }

            DrawString(
                sprites, 
                "∞",
                x,
                y + h - hk,
                0.45f * scale,
                color3);


            var brightCol = 
                      SelChan > -1 
                   && LastSetting == SelInstrument.Volume 
                || scale != 1 
                ? color6 
                : color4;

            var col = 
                !OK(chan)
                ? color6
                : (chan.Notes.Count > 0 ? brightCol : color3);

            // set value bar
            FillRect(
                sprites, 
                x + w - wb, 
                y + h + hk, 
                wb,
                -h * Math.Min((float)Math.Pow(level / extra, pow), 1), 
                col);
        }
    }
}
