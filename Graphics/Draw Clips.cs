using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawClips()
        {
            if (!TooComplex) DrawClips(dspMixer1, 0);
            if (!TooComplex) DrawClips(dspMixer2, 1);
        }


        void DrawClips(Display dsp, int nDsp)
        {
            if (dsp == null) return;


            var sprites = new List<MySprite>();

            var x = dsp.Viewport.X;
            var y = dsp.Viewport.Y;
            var w = dsp.Viewport.Width;
            var h = dsp.Viewport.Height;

            var gap = 8f;


            FillRect(sprites, x, y, w, h, color0);

            
            for (int iy = 0; iy < 4; iy++)
            {
                var track = g_tracks[iy];

                for (int ix = nDsp*6 + 0; ix < nDsp*6 + 6; ix++)
                {
                    var iClip = track.Indices.FindIndex(i => i == ix);
                    var col   = iClip < 0 ? color2 : (track.CurIndex == iy ? color5 : color3);

                    var cx = x + w/6*(ix - nDsp*6);
                    var cy = y + h/4*iy;
                    
                    FillRect(
                        sprites, 
                        cx + gap/2, 
                        cy + gap/2, 
                        w/6 - gap,
                        h/4 - gap, 
                        col); 

                    if (iClip < 0) continue;
                    var clip = track.Clips[iClip];

                    var name = clip.Name.Split('\n')[0];
                    var nNameLines = clip.Name.Split(' ').Length;

                    DrawString(
                        sprites,
                        name.Replace(' ', '\n'),
                        cx + w/12,
                        cy + h/8 - 15 - (nNameLines-1)*20,
                        1,
                        color0,
                        TaC);
                }
            }


            dsp.Draw(sprites);
        }
    }
}
