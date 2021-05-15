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
                    var iClip     = track.Indices.FindIndex(i => i == ix);
                    var isCurClip = iClip == track.CurIndex;

                    var cw = w/6;
                    var ch = h/4;

                    var cx = x + cw*(ix - nDsp*6);
                    var cy = y + ch*iy;
                    
                    FillRect(
                        sprites, 
                        cx + gap/2, 
                        cy + gap/2, 
                        cw - gap,
                        ch - gap,
                        iClip < 0 ? color1 : (isCurClip ? color3 : color1));

                    if (iClip != ix) continue;
                    var clip = track.Clips[iClip];

                    for (int j = 1; j < clip.Patterns.Count; j++)
                    {
                        var pw = cw / clip.Patterns.Count;
                        var lx = cx + gap/2 + j*pw;

                        DrawLine(
                            sprites, 
                            lx,
                            cy, 
                            lx, 
                            cy + ch,
                            isCurClip ? color4 : color2, 
                            6); 
                    }


                    // current clip of session
                    if (clip == g_session.CurClip)
                        DrawRect(
                            sprites, 
                            cx + gap/2 + 3, 
                            cy + gap/2 + 3, 
                            cw - gap - 6,
                            ch - gap - 6, 
                            color5,
                            6);

                    // position in current clip
                    if (iClip == track.CurIndex)
                    {
                        var pw = cw / clip.Patterns.Count;
                        var px = cx + gap/2 + (track.PlayStep / g_patSteps) * pw;
                        DrawLine(sprites, px, cy, px, cy + ch, color6, 6); 
                    }


                    // clip name
                    var name       = clip.Name.Split('\n')[0];
                    var nNameLines = clip.Name.Split(' ').Length;

                    DrawString(
                        sprites,
                        name.Replace(' ', '\n'),
                        cx + w/12,
                        cy + h/8 - 15 - (nNameLines-1)*20,
                        1,
                        isCurClip ? color0 : color4,
                        TaC);
                }
            }


            dsp.Draw(sprites);
        }
    }
}
