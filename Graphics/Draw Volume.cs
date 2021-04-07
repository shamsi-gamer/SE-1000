using System;
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

            foreach (var vol in g_dspVol)
                maxVol = Math.Max(vol, maxVol);

            if (!TooComplex) DrawVolume(maxVol, dspVol1, 2);
            if (!TooComplex) DrawVolume(maxVol, dspVol2, 1);
            if (!TooComplex) DrawVolume(maxVol, dspVol3, 0);
        }


        void DrawVolume(float vol, Display dsp, int i)
        {
            if (dsp == null) return;

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
            DrawSoundLevel(sprites, xc, y - i * Volume.Height, bw, h - 295, g_volume, vol, 6.5f);

            dsp.Draw(sprites);
        }


        static void DrawSoundLevel(List<MySprite> sprites, float x, float y, float w, float h, float level, float v, float scale = 1)
        {
            var wb = w/10;
            var wg = w/20;
            var wl = w - wg - wb;
            var ws = w/4;//20;

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
                        printValue(Math.Abs(db), 0, true, 2),
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


            // current level bars
            //DrawLevelBars(
            //    sprites,
            //    x + ws,          
            //    y + h, 
            //    wl - ws, 
            //    -h * Math.Min((float)Math.Pow(v / extra, pow), 1), 
            //    h/48, 
            //    color4);


            // set value bar
            FillRect(
                sprites, 
                x + w - wb, 
                y + h + hk, 
                wb,
                -h * Math.Min((float)Math.Pow(level / extra, pow), 1), 
                SelChan > -1 && LastSetting == SelectedInstrument.Volume || scale != 1 ? color6 : color4);
        }


        static void DrawLevelBars(List<MySprite> sprites, float x, float y, float w, float h, float gap, Color c)
        {
            var _y = y;

            while (_y > y + h)
            {
                FillRect(sprites, x, _y, w, -gap, c);
                _y -= gap * 2;
            }
        }
    }
}
