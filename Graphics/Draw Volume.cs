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
            float vol = 0;

            if (ShowMixer > 0)
            { 
                foreach (var v in EditedClip.Track.DspVol)
                    vol = Math.Max(vol, v);
            }
            else
            { 
                foreach (var track in Tracks)
                    foreach (var v in track.DspVol)
                        vol = Math.Max(vol, v);
            }

            if (!TooComplex) DrawVolume(vol, dspVol1, 2);
            if (!TooComplex) DrawVolume(vol, dspVol2, 1);
            if (!TooComplex) DrawVolume(vol, dspVol3, 0);
        }



        void DrawVolume(float vol, Display dsp, int i)
        {
            if (!OK(dsp)) return;

            var bb = 200;
            var bw = 300;

            var v = dsp.Viewport;

            var x = v.X;
            var y = v.Y + 55;
            var w = v.Width;
            var h = v.Height * 3 - 40;


            var sprites = new List<MySprite>();

            FillRect(sprites, x, v.Y, w, v.Height, color0);

            var xc = x + bb/2;

            DrawSoundLevel(
                sprites, 
                xc, 
                y - i * v.Height, 
                bw, 
                h - 295, 
                EditedClip.Volume, 
                vol,
                Channel_null,
                6.5f,
                ShowMixer > 0);

            
            if (   ShowMixer > 0
                && i == 2)
            {
                DrawClipName(
                    sprites, 
                    EditedClip.Name, 
                    x + w/2, 
                    v.Y + v.Height - 130, 
                    2f, 
                    color5);
            }


            dsp.Draw(sprites);
        }



        static void DrawSoundLevel(List<MySprite> sprites, float x, float y, float w, float h, float level, float v, Channel chan = Channel_null, float scale = 1, bool drawSetValue = True)
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
                        PrintValue(Math.Abs(db), 0, True, 2),
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
                      OK(SelChan) 
                   && EditedClip.LastSetting == SelInstrument.Volume 
                || scale != 1 
                ? color6 
                : color4;

            var col = 
                !OK(chan)
                ? color6
                : (chan.Notes.Count > 0 ? brightCol : color3);

            // set value bar
            if (drawSetValue)
            {
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
}
