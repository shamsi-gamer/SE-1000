using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


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
            if (!OK(dsp)) return;

            var v = dsp.Viewport;

            var x = v.X;
            var y = v.Y + 20;
            var w = v.Width;
            var h = v.Height - 100;

            var bw = w/6;
            var bb = 20;
            var vb = 70;
            var rw = bw - bb;

            var ry = y + h;


            var sprites = new List<MySprite>();

            FillRect(sprites, x, v.Y, w, v.Height, color0);

            for (int ch = first; ch < first + 6; ch++)
            {
                if (TooComplex) break;
                    
                var chan = CurPattern.Channels[ch];

                var rx  = x + (ch-first) * bw + bb/2;
                var col = chan.Notes.Count > 0 ? color6 : color3;

                FillRect(sprites, rx, ry + 6, rw, 76, chan.On ^ IsPressed(lcdMixer+ch) ? col : color0);
                          
                DrawString(sprites, chan.Instrument.Name, rx + rw/2 + 3, ry + 14, 0.5f, chan.On ? color0 : col, TA_CENTER);
                DrawString(sprites, S(ch + 1),            rx + rw/2 + 3, ry + 35, 1.2f, chan.On ? color0 : col, TA_CENTER);

                DrawSoundLevel(sprites, rx + vb/2, y, rw - vb, h - 15, chan.Volume, EditedClip.Track.DspVol[ch], chan, 2);
            }

            dsp.Draw(sprites);
        }
    }
}
