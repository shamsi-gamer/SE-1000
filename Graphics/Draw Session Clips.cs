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
                    var isCurClip = iClip == track.PlayClip;

                    var cw = w/6;
                    var ch = h/4;

                    var cx = x + cw*(ix - nDsp*6);
                    var cy = y + ch*iy;

                    var lx = cx + gap/2;
                    var ly = cy + gap/2;
                    var lw = cw - gap;
                    var lh = ch - gap;


                    // clip rectangle
                    var col = iClip < 0 ? color1 : (isCurClip ? color3 : color1);
                    FillRect(sprites, lx, ly, lw, lh, col);


                    if (iClip != ix) continue;


                    // pattern marks
                    var clip = track.Clips[iClip];

                    for (int j = 1; j < clip.Patterns.Count; j++)
                    {
                        var pw = cw / clip.Patterns.Count;
                        var jx = lx + j*pw;

                        DrawLine(sprites, jx, ly, jx, ly + lh, isCurClip ? color5 : color3, 1); 
                    }


                    // next in queue
                    if (iClip == track.NextClip)
                        DrawRect(sprites, lx+1, ly+1, lw-2, lh-2, color5, 2);


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


                    // position in current clip
                    if (iClip == track.PlayClip)
                    {
                        var pw = lw / clip.Patterns.Count;
                        var px = lx + (track.PlayStep / g_patSteps) * pw;
                        var beatStart = track.PlayStep - track.PlayPat * g_patSteps == 0;
                        DrawLine(sprites, px, ly, px, ly + lh, color6, beatStart ? 9 : 5); 
                    }
                }
            }


            dsp.Draw(sprites);
        }
    }
}
