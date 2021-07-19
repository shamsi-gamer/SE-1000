using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using System.Text;


namespace IngameScript
{
    partial class Program
    {
        void DrawClip()
        {
            if (!TooComplex) DrawClip(dspClip1, 0);
            if (!TooComplex) DrawClip(dspClip2, 1);
        }



        void DrawClip(Display dsp, int nDsp)
        {
            if (!OK(dsp)) return;

            var v = dsp.Viewport;

            var x = v.X;
            var y = v.Y;
            var w = v.Width;
            var h = v.Height;

            var sprites = new List<MySprite>();


            if (ShowClip)
            { 
                FillRect(sprites, x, y, w, h, color0);


                var xt = 256f;
                var wt = xt / g_patSteps;
                var ht = wt;

                var pw = wt * g_patSteps;
                var ph = ht * g_nChans;

                var pxCur = x - (nDsp*4 + EditedClip.SongOff) * pw + EditPat * pw;
                var py    = y + h/2 - ph/2;

                var first = nDsp * 4 + EditedClip.SongOff;
                var next  = Math.Min((nDsp + 1) * 4 + EditedClip.SongOff, EditedClip.Patterns.Count);

                var curBlock = EditedClip.GetBlock(EditPat);

                var _f = first - EditedClip.SongOff;


                for (int p = first; p < next; p++)
                {
                    var _p = p - EditedClip.SongOff;

                    var px = x - _f * pw + _p * pw;

                    FillRect(sprites, px, py, pw, ph, color1);

                    FillRect(sprites, px,      py, 1, ph, color4);
                    FillRect(sprites, px + pw, py, 1, ph, color4);

                    var bo = 30;

                    var block = EditedClip.GetBlock(p);
                    if (OK(block))
                    {
                        var c = curBlock == block ? color3 : color2;

                        FillRect(sprites, px, py - bo, xt, bo, c);
                        FillRect(sprites, px, py + ph, xt, bo, c);
                    }

                    DrawBrackets(sprites, p, px, py - bo, pw, ph + bo*2, 40, 2);
                }


                if (!EditedClip.Piano)
                {
                    for (int p = first; p < next; p++)
                    {
                        var px = x - (nDsp * 4 + EditedClip.SongOff) * pw + p * pw;
                        var ch = ph / g_nChans;
                        var cy = py + ph - (CurChan + 1) * ch;

                        FillRect(sprites, px, cy, pw, ch, color3);
                    }
                }


                // draw edit position
                if (   EditedClip.EditPos >= first * g_patSteps
                    && EditedClip.EditPos <  next  * g_patSteps)
                {
                    var pl    = x - pw * (nDsp * 4 * pw + EditPat + EditedClip.SongOff);
                    var xTick = wt * EditedClip.EditPos;

                    FillRect(
                        sprites,
                        xTick + 2, 
                        py, 
                        wt - 4,
                        ph + (EditedClip.ParamKeys || EditedClip.ParamAuto ? ph/5 : 0),
                        color3);
                }


                // draw patterns
                for (int p = first; p < next; p++)
                {
                    var _p = p - EditedClip.SongOff;
                    var px = x - _f * pw + _p * pw;

                    if (EditedClip.Piano 
                          && LockView != 1
                       || LockView == 2) DrawPianoRoll(sprites, px, py, pw, ph, EditedClip, p, 1);
                    else                 DrawPattern  (sprites, px, py, pw, ph, EditedClip, p, 1, False);

                    if (OK(CurSet))
                    { 
                        if (EditedClip.ParamKeys)
                        {
                            FillRect     (sprites, px, py+ph+ph/5, pw, 1,    color3);
                            DrawParamKeys(sprites, px, py + ph,    pw, ph/5, EditedClip, p, CurChan);
                        }
                        else if (EditedClip.ParamAuto)
                        {
                            FillRect(sprites, px, py + ph + ph/5, pw, 1, color3);
                        }
                    }
                }

                if (EditedClip.ParamAuto)
                { 
                    var fx   = x - _f * pw;
                    var wMax = Math.Min(EditedClip.Patterns.Count * pw, w * 2);

                    DrawParamAuto(sprites, fx, py + ph, pw, ph/5, wMax, EditedClip, 0, CurChan);
                }


                // draw current pattern box
                DrawRect(sprites, pxCur + 1, py, wt * g_patSteps - 2, ph, color6, 2);


                var track = EditedClip.Track;

                // draw play position
                if (   Playing
                    && track.PlayPat < EditedClip.Patterns.Count
                    && EditedClipIsPlaying)
                {
                    var pl    = x  - pw * (nDsp * 4 + EditedClip.SongOff);
                    var xTick = pl + wt * (int)track.PlayStep;

                    FillRect(sprites, xTick, py, wt, ph, color6);

                    if (      EditedClip.Piano 
                           && LockView != 1
                        || LockView == 2) DrawPianoNeg  (sprites, pl, py, pw, ph, EditedClip, track.PlayPat, (int)track.PlayStep, False);
                    else                  DrawPatternNeg(sprites, pl, py, pw, ph, EditedClip, track.PlayPat, (int)track.PlayStep, False);
                }


                DrawClipScrollBar(sprites, x, y, w, h, nDsp, pw, curBlock, track);


                for (int p = first; p < next; p++)
                {
                    var _p = p - EditedClip.SongOff;

                    var px = x - _f * pw + _p * pw;

                    var b = EditedClip.GetBlock(p);
                    var c = OK(b) && b == curBlock ? color5 : color4;

                    DrawString(sprites, S(p + 1), px + 8, py - 28, 0.8f, c);

                    var m = Array.FindIndex(EditedClip.Mems, _m => _m == p);
                    if (OK(m)) DrawString(sprites, S((char)(65 + m)), px + 8, py - 68, 1, color4);
                }


                DrawClipFuncButtons(sprites, w, h, nDsp);


                if (nDsp == 1)
                {
                    var min = Math.Max(0, g_log.Count - 30);
                    var sb  = new StringBuilder();

                    for (int i = min; i < g_log.Count; i++)
                    { 
                        DrawString(
                            sprites,
                            S(g_logTime[i]) + ": " + g_log[i], 
                            x + 6, 
                            y + (i-min) * 25*0.5f, 
                            0.4f, 
                            color6);
                    }
                }
            }
            else
                FillRect(sprites, x, y, w, h, color0);


            if (!TooComplex)
                dsp.Draw(sprites);
        }



        void DrawClipScrollBar(List<MySprite> sprites, float x, float y, float w, float h, int nDsp, float pw, Block curBlock, Track track)
        {
            if (EditedClip.Patterns.Count > maxDspPats)
            {
                var bw = (w * 2) / EditedClip.Patterns.Count;
                var sh = 29;

                var px = x - nDsp * 4 * pw;


                for (int p = 0; p < EditedClip.Patterns.Count; p++)
                {
                    FillRect(sprites, px + bw * p + 1, y, 1, sh, color4);

                    var m = Array.FindIndex(EditedClip.Mems, _m => _m == p);
                    if (OK(m)) DrawString(sprites, S((char)(65 + m)), px + 5, y - 30, 0.7f, color4);
                }


                foreach (var b in EditedClip.Blocks)
                {
                    var bx = px + bw * b.First + 1;
                    var sw = bw * b.Len - 2;

                    FillRect(sprites, bx, y, sw, sh, b == curBlock ? color2 : color1);

                    for (int i = 1; i < b.Len; i++)
                        FillRect(sprites, bx + bw * i, y, 1, sh, color5);

                    DrawLeftBracket (sprites, bx,      y, 16, sh, 1);
                    DrawRightBracket(sprites, bx + sw, y, 16, sh, 1);
                }


                FillRect(sprites, px + bw * EditPat, y, bw, sh, color3);


                if (OK(track.PlayStep)
                    && track.PlayClip == EditedClip.Index)
                    FillRect(sprites, px + bw / g_patSteps * track.PlayStep, y, 4, sh, color6);
            }
        }



        void DrawClipFuncButtons(List<MySprite> sprites, float w, float h, int nDsp)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (nDsp == 0)
            {
                DrawButton(sprites, strDel, 0, 6, w, h, IsPressed(lcdClip + 0));
                DrawButton(sprites, "Dup",  1, 6, w, h, IsPressed(lcdClip + 1));
                DrawButton(sprites, "New",  2, 6, w, h, IsPressed(lcdClip + 2));

                DrawButton(sprites, "Cue",  4, 6, w, h, OK(EditedClip.Track.NextPat) || IsPressed(lcdClip + 4));         
                DrawButton(sprites, "◄",    5, 6, w, h, IsPressed(lcdClip + 5) ^ EditedClip.MovePat); }
         else { DrawButton(sprites, "►",    0, 6, w, h, IsPressed(lcdClip + 6) ^ EditedClip.MovePat);
                DrawButton(sprites, "◄►",   1, 6, w, h, EditedClip.MovePat);

                DrawButton(sprites, "[",    3, 6, w, h, EditedClip.In);
                DrawButton(sprites, "]",    4, 6, w, h, EditedClip.Out);
                DrawButton(sprites, "X",    5, 6, w, h, IsPressed(lcdClip + 11));
            }
        }



        void DrawBrackets(List<MySprite> sprites, int p, float x, float y, float w, float h, float bw, float bh)
        {
            if (OK(EditedClip.Blocks.Find(b => p == b.First))) DrawLeftBracket (sprites, x,     y, bw, h, bh);
            if (OK(EditedClip.Blocks.Find(b => p == b.Last ))) DrawRightBracket(sprites, x + w, y, bw, h, bh);
        }



        void DrawLeftBracket(List<MySprite> sprites, float x, float y, float w, float h, float bh)
        {
            FillRect(sprites, x, y, bh, h, color4);

            FillRect(sprites, x, y,          w, bh, color4);
            FillRect(sprites, x, y + h - bh, w, bh, color4);
        }



        void DrawRightBracket(List<MySprite> sprites, float x, float y, float w, float h, float bh)
        {
            FillRect(sprites, x - bh - 1, y, bh, h, color4);

            FillRect(sprites, x - w, y,          w, bh, color4);
            FillRect(sprites, x - w, y + h - bh, w, bh, color4);
        }
    }
}
