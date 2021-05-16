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
            if (!TooComplex) DrawMixer(dspMixer1, 0);
            if (!TooComplex) DrawMixer(dspMixer2, 6);
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
                if (TooComplex) break;
                    
                var chan = CurPattern.Channels[ch];

                var xc = x + bb/2 + (ch - first) * (bw + bb);

                var col = chan.Notes.Count > 0 ? color6 : color3;

                DrawSoundLevel(sprites, xc, y,         bw, h - 15, chan.Volume, g_dspVol[ch], chan, 2);
                FillRect      (sprites, xc, y + h + 6, bw, 76,     chan.On ^ _lcdPressed.Contains(ch) ? col : color0);

                DrawString(sprites, chan.Instrument.Name, xc + bw/2 + 3, y + h + 14, 0.5f, chan.On ? color0 : col, TaC);
                DrawString(sprites, S(ch + 1),            xc + bw/2 + 3, y + h + 35, 1.2f, chan.On ? color0 : col, TaC);
            }

            dsp.Draw(sprites);
        }
    }
}
