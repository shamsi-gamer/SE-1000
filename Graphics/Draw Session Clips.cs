using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawSessionClips()
        {
            if (!TooComplex) DrawSessionClips(dspMixer1, 0);
            if (!TooComplex) DrawSessionClips(dspMixer2, 1);
        }


        void DrawSessionClips(Display dsp, int nDsp)
        {
            if (dsp == null) return;


            var sprites = new List<MySprite>();

            var x = dsp.Viewport.X;
            var y = dsp.Viewport.Y;
            var w = dsp.Viewport.Width;
            var h = dsp.Viewport.Height;

            var gap = 8f;


            FillRect(sprites, x, y, w, h, color0);

            
            for (int iy = 0; iy < Math.Min(g_session.Tracks.Count, 4); iy++)
            {
                var track = g_session.Tracks[iy];

                for (int ix = nDsp*6 + 0; ix < nDsp*6 + 6; ix++)
                {
                    var iClip = track.Indices.FindIndex(i => i == ix);
                    var col   = iClip < 0 ? color2 : color4;

                    var cx = x + w/6*(ix - nDsp*6);
                    var cy = y + h/4*iy;
                    
                    FillRect(
                        sprites, 
                        cx + gap/2, 
                        cy + gap/2, 
                        w/6 - gap,
                        h/4 - gap, 
                        col); 

                    if (iClip != ix) continue;
                    var clip = track.Clips[iClip];

                    // current clip of track
                    if (iClip == track.CurIndex)
                        DrawRect(
                            sprites, 
                            cx + gap/2 + 3, 
                            cy + gap/2 + 3, 
                            w/6 - gap - 6,
                            h/4 - gap - 6, 
                            color5,
                            6); 

                    // current clip of session
                    if (clip == g_session.CurClip)
                        DrawRect(
                            sprites, 
                            cx + gap/2 + 11, 
                            cy + gap/2 + 11, 
                            w/6 - gap - 22,
                            h/4 - gap - 22, 
                            color5,
                            4); 

                    var name       = clip.Name.Split('\n')[0];
                    var nNameLines = clip.Name.Split(' ').Length;

                    DrawString(
                        sprites,
                        name.Replace(' ', '\n'),
                        cx + w/12,
                        cy + h/8 - 15 - (nNameLines-1)*20,
                        1,
                        color0,
                        TaC);


                    // debug
                    DrawString(
                        sprites,
                        $"iClip = {iClip}",
                        cx + 5,
                        cy + 5,
                        0.4f,
                        color0);
                }
            }


            dsp.Draw(sprites);
        }
    }
}
