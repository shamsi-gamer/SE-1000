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

                    Color colTop, colBottom; 
                    
                         if (!OK(clip))  { colTop = color1; colBottom = color1; }
                    else if (isPlayClip) { colTop = color3; colBottom = color4; }
                    else                 { colTop = color2; colBottom = color3; }

                    FillRect(sprites, lx, ly, lw, lh, colTop);


                    float vol = 0;

                    foreach (var v in track.DspVol)
                        vol = Math.Max(v, vol);

                    vol = Math.Min(vol*2/3f, 1);


                    // draw rectangle
                    if (isPlayClip)
                        FillRect(sprites, lx, ly+lh, lw, -vol*lh, colBottom);


                    if (!OK(clip)) continue;


                    // next in queue
                    if (  !isPlayClip 
                        && ix == track.NextClip
                        && track.NextClip != track.PlayClip)
                        DrawRect(sprites, lx+7, ly+7, lw-14, lh-14, color3, 14);
                    else if (isPlayClip && !OK(track.NextClip))
                        DrawRect(sprites, lx+7, ly+7, lw-14, lh-14, color0, 14);


                    // edited clip
                    if (clip == EditedClip)
                    { 
                        var editCol = color3;

                             if (ix == track.NextClip
                              && ix == track.PlayClip) editCol = vol > 14/lh ? color5 : color4;
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
                        var pw = (cw-gap) / clip.Patterns.Count;
                        var jx = lx - 2 + j*pw;

                        DrawLine(
                            sprites, 
                            jx, 
                            ly, 
                            jx, 
                            ly + lh, 
                            isPlayClip ? color6 : color5,
                            j % 4 == 0 ? 3 : 1);
                    }


                    // set volume
                    FillRect(
                        sprites, 
                        lx+lw-6, 
                        ly+lh, 
                        6, 
                        -lh*clip.Volume*2/3f, 
                        isPlayClip ? color6 : color3);

                    FillRect(
                        sprites, 
                        lx+lw-6, 
                        ly+lh/3, 
                        6, 
                        4, 
                        isPlayClip 
                        ? (clip.Volume > 1 ? color4 : color6) 
                        : (clip.Volume > 1 ? color2 : color3));


                    // clip name
                    DrawClipName(
                        sprites, 
                        clip.Name, 
                        cx + w/12, 
                        cy + h/8 -15, 
                        1, 
                        isPlayClip ? color6 : color5);


                    // clip is moved/duplicated
                    if (clip == ClipCopy)
                        DrawRect(sprites, lx+2, ly+2, lw-4, lh-4, isPlayClip ? color6 : color5, 4);


                    // position in played clip
                    if (isPlayClip)
                    {
                        var pw       = lw / clip.Patterns.Count;
                        var px       = lx + (track.PlayStep / g_patSteps) * pw - 2;
                        //var patStart = track.PlayStep - track.PlayPat * g_patSteps <= 1;
                        
                        var oy = clip.Track.PlayTime % (g_patSteps * TicksPerStep) < 10 ? gap : 0;

                        DrawLine(sprites, px, ly-oy, px, ly+lh+oy, color6, 9); 
                    }
                }
            }


            dsp.Draw(sprites);
        }


        static void DrawClipName(List<MySprite> sprites, string name, float x, float y, float size, Color color)
        {
            var nNameLines = name.Split(' ').Length;

            DrawString(
                sprites,
                name.Replace(' ', '\n'),
                x,
                y - (nNameLines-1)*20*size,
                size,
                color,
                TA_CENTER);
        }
    }
}
