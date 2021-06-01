using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


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
            if (!OK(dsp)) return;


            var sprites = new List<MySprite>();

            var x = dsp.Viewport.X;
            var y = dsp.Viewport.Y;
            var w = dsp.Viewport.Width;
            var h = dsp.Viewport.Height;

            var gap = 8f;


            FillRect(sprites, x, y, w, h, color0);

            
            for (int iy = 0; iy < Math.Min(Tracks.Count, 4); iy++)
            {
                var track = Tracks[iy];

                for (int ix = nDsp*6 + 0; ix < nDsp*6 + 6; ix++)
                {
                    var clip       = track.Clips[ix];
                    var isPlayClip = ix == track.PlayClip;


                    var cw = w/6;
                    var ch = h/4;

                    var cx = x + cw*(ix - nDsp*6);
                    var cy = y + ch*iy;

                    var lx = cx + gap/2;
                    var ly = cy + gap/2;
                    var lw = cw - gap;
                    var lh = ch - gap;


                    var cancel = 
                           OK(track.NextClip) 
                        || !isPlayClip;

                    // clip rectangle
                    var cnw = cancel ? 0 : 14;
                    
                    Color col; 
                    
                         if (!OK(clip))  col = color1;
                    else if (isPlayClip) col = color3;
                    else                 col = color2;

                    FillRect(sprites, lx + cnw, ly + cnw, lw - cnw*2, lh - cnw*2, col);


                    if (!OK(clip)) continue;


                    // next in queue
                    if (ix == track.NextClip)
                        DrawRect(sprites, lx+7, ly+7, lw-14, lh-14, color3, 14);


                    // edited clip
                    if (clip == EditedClip)
                    { 
                        var editCol = color3;

                             if (ix == track.NextClip
                              && ix == track.PlayClip) editCol = color4;
                        else if (isPlayClip)           editCol = color5;

                        var ew = cancel ?  0 : 14;
                        var de = cancel ?  0 :  5;
                        var dh = cancel ? 14 :  3;

                        var ey = ly - de + lh - 7;

                        DrawLine(sprites, lx+ew, ey, lx-ew+lw, ey, editCol, dh);
                    }


                    // pattern marks
                    for (int j = 1; j < clip.Patterns.Count; j++)
                    {
                        var pw = cw / clip.Patterns.Count;
                        var jx = lx - 2 + j*pw;

                        DrawLine(sprites, jx, ly, jx, ly + lh, isPlayClip ? color6 : color4, 1); 
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
                        isPlayClip ? color6 : color5,
                        TA_CENTER);


                    // clip is moved/duplicated
                    if (clip == ClipCopy)
                        DrawRect(sprites, lx+2, ly+2, lw-4, lh-4, isPlayClip ? color6 : color5, 4);


                    // position in played clip
                    if (isPlayClip)
                    {
                        var pw       = lw / clip.Patterns.Count;
                        var px       = lx + (track.PlayStep / g_patSteps) * pw;
                        var patStart = track.PlayStep - track.PlayPat * g_patSteps <= 1;

                        DrawLine(sprites, px, ly, px, ly + lh, color6, patStart ? 9 : 5); 
                    }
                }
            }


            dsp.Draw(sprites);
        }
    }
}
