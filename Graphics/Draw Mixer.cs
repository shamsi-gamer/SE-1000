using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void DrawMixer()
        {
            DrawMixer(dspMixer1, 0);
            DrawMixer(dspMixer2, 6);

            float maxVol = 0;

            foreach (var vol in g_vol)
                maxVol = Math.Max(vol, maxVol);

            DrawVolume(maxVol, dspVol1, 2);
            DrawVolume(maxVol, dspVol2, 1);
            DrawVolume(maxVol, dspVol3, 0);
        }


        void DrawMixer(Display dsp, int first)
        {
            if (dsp == null) return;

            var bb = 75.5f;
            var bw = 95;


            var Volume = dsp.Viewport;

            var x = Volume.X;
            var y = Volume.Y + 20;
            var w = Volume.Width;
            var h = Volume.Height - 100;


            var sprites = new List<MySprite>();

            FillRect(sprites, x, Volume.Y, w, Volume.Height, color0);

            for (int ch = first; ch < first + 6; ch++)
            {
                var chan = CurrentPattern.Channels[ch];

                var xc = x + bb/2 + (ch - first) * (bw + bb);

                DrawSoundLevel(sprites, xc, y, bw, h - 15, chan.Volume, g_vol[ch], 2);//, ch >= 6 ? this : null);
                FillRect(sprites, xc, y + h + 6, bw, 76, chan.On ^ mixerPressed_.Contains(ch) ? color6 : color0);

                DrawString(sprites, chan.Instrument.Name, xc + bw/2 + 3, y + h + 14, 0.5f, chan.On ? color0 : color6, TextAlignment.CENTER);
                DrawString(sprites, (ch + 1).ToString(),  xc + bw/2 + 3, y + h + 35, 1.2f, chan.On ? color0 : color6, TextAlignment.CENTER);
            }

            dsp.Draw(sprites);
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


        void DrawLevelBars(List<MySprite> sprites, float x, float y, float w, float h, float gap, Color c)
        {
            var _y = y;

            while (_y > y + h)
            {
                FillRect(sprites, x, _y, w, -gap, c);
                _y -= gap * 2;
            }
        }


        void DrawSoundLevel(List<MySprite> sprites, float x, float y, float w, float h, float level, float v, float scale = 1)
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
            DrawLevelBars(
                sprites,
                x + ws,          
                y + h, 
                wl - ws, 
                -h * Math.Min((float)Math.Pow(v / extra, pow), 1), 
                h/48, 
                color4);


            // set value bar
            FillRect(
                sprites, 
                x + w - wb, 
                y + h + hk, 
                wb,
                -h * Math.Min((float)Math.Pow(level / extra, pow), 1), 
                SelChan > -1 && LastSetting == SelectedInstrument.Volume || scale != 1 ? color6 : color4);
        }
    }
}
