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


            FillRect(sprites, x, y, w, h, color0);


            if (OK(EditClip))
            {
                var xt = 256f;
                var wt = xt / g_patSteps;
                var ht = wt;

                var pw = wt * g_patSteps;
                var ph = ht * g_nChans;

                var pxCur = x - (nDsp*4 + EditClip.SongOff) * pw + CurPat * pw;
                var py = y + h/2 - ph/2;

                var first = nDsp * 4 + EditClip.SongOff;
                var next = Math.Min((nDsp + 1) * 4 + EditClip.SongOff, EditClip.Patterns.Count);

                var curBlock = EditClip.GetBlock(CurPat);

                var _f = first - EditClip.SongOff;


                for (int p = first; p < next; p++)
                {
                    var _p = p - EditClip.SongOff;

                    var px = x - _f * pw + _p * pw;

                    FillRect(sprites, px, py, pw, ph, color1);

                    FillRect(sprites, px,      py, 1, ph, color4);
                    FillRect(sprites, px + pw, py, 1, ph, color4);

                    var bo = 30;

                    var block = EditClip.GetBlock(p);
                    if (OK(block))
                    {
                        var c = curBlock == block ? color3 : color2;

                        FillRect(sprites, px, py - bo, xt, bo, c);
                        FillRect(sprites, px, py + ph, xt, bo, c);
                    }

                    DrawBrackets(sprites, p, px, py - bo, pw, ph + bo*2, 40, 2);
                }


                if (!EditClip.Piano)
                {
                    for (int p = first; p < next; p++)
                    {
                        var px = x - (nDsp * 4 + EditClip.SongOff) * pw + p * pw;
                        var ch = ph / g_nChans;
                        var cy = py + ph - (CurChan + 1) * ch;

                        FillRect(sprites, px, cy, pw, ch, color3);
                    }
                }


                // draw edit position
                if (   EditClip.EditPos >= first * g_patSteps
                    && EditClip.EditPos <  next  * g_patSteps)
                {
                    var pl    = x - pw * (nDsp * 4 * pw + CurPat + EditClip.SongOff);
                    var xTick = wt * EditClip.EditPos;

                    FillRect(
                        sprites,
                        xTick + 2, 
                        py, 
                        wt - 4,
                        ph + (EditClip.ParamKeys || EditClip.ParamAuto ? ph/5 : 0),
                        color3);
                }


                // draw keys
                for (int p = first; p < next; p++)
                {
                    var _p = p - EditClip.SongOff;
                    var px = x - _f * pw + _p * pw;

                    if (EditClip.Piano) DrawPianoRoll(sprites, px, py, pw, ph, EditClip, p, 1, F, g_patSteps);
                    else         DrawPattern  (sprites, px, py, pw, ph, EditClip, p, 1, F);

                    if (EditClip.ParamKeys)
                    {
                        FillRect     (sprites, px, py+ph+ph/5, pw, 1,    color3);
                        DrawParamKeys(sprites, px, py + ph,    pw, ph/5, EditClip, p, CurChan);
                    }
                    else if (EditClip.ParamAuto)
                    {
                        FillRect(sprites, px, py + ph + ph/5, pw, 1, color3);
                    }
                }

                if (EditClip.ParamAuto)
                { 
                    var fx   = x - _f * pw;
                    var wMax = Math.Min(EditClip.Patterns.Count * pw, w * 2);

                    DrawParamAuto(sprites, fx, py + ph, pw, ph/5, wMax, EditClip, 0, CurChan);
                }


                // draw current pattern box
                DrawRect(sprites, pxCur + 1, py, wt * g_patSteps - 2, ph, color6, 2);


                // draw play position
                if (   g_playing
                    && PlayPat < EditClip.Patterns.Count)
                {
                    var pl    = x  - pw * (nDsp * 4 + EditClip.SongOff);
                    var xTick = pl + wt * (int)PlayStep;

                    FillRect(sprites, xTick, py, wt, ph, color6);

                    if (EditClip.Piano) DrawPianoNeg  (sprites, pl, py, pw, ph, EditClip, PlayPat, (int)PlayStep, F);
                    else                DrawPatternNeg(sprites, pl, py, pw, ph, EditClip, PlayPat, (int)PlayStep, F);
                }


                if (EditClip.Patterns.Count > maxDspPats)
                {
                    var bw = (w * 2) / (float)EditClip.Patterns.Count;
                    var sh = 29;

                    var px = x - nDsp * 4 * pw;
                    var by = y + h - sh;

                    for (int p = 0; p < EditClip.Patterns.Count; p++)
                    {
                        FillRect(sprites, px + bw * p + 1, by, 1, sh, color4);

                        var m = Array.FindIndex(EditClip.Mems, _m => _m == p);
                        if (m > -1) DrawString(sprites, S((char)(65 + m)), px + 5, by - 30, 0.7f, color4);
                    }

                    foreach (var b in EditClip.Blocks)
                    {
                        var bx = px + bw * b.First + 1;
                        var sw = bw * b.Len - 2;

                        FillRect(sprites, bx, by, sw, sh, b == curBlock ? color3 : color2);

                        for (int i = 1; i < b.Len; i++)
                            FillRect(sprites, bx + bw * i, by, 1, sh, color5);

                        DrawLeftBracket (sprites, bx, by, 16, sh, 1);
                        DrawRightBracket(sprites, bx + sw, by, 16, sh, 1);
                    }

                    FillRect(sprites, px + bw * CurPat, by, bw, sh, color4);

                    if (OK(PlayStep))
                        FillRect(sprites, px + bw / g_patSteps * PlayStep, by, 4, sh, color6);
                }


                for (int p = first; p < next; p++)
                {
                    var _p = p - EditClip.SongOff;

                    var px = x - _f * pw + _p * pw;

                    var b = EditClip.GetBlock(p);
                    var c = OK(b) && b == curBlock ? color5 : color4;

                    DrawString(sprites, S(p + 1), px + 8, py - 28, 0.8f, c);

                    var m = Array.FindIndex(EditClip.Mems, _m => _m == p);
                    if (m > -1) DrawString(sprites, S((char)(65 + m)), px + 8, py - 68, 1, color4);
                }


                DrawClipFuncButtons(sprites, w, h, nDsp);
            }


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


            dsp.Draw(sprites);
        }


        void DrawClipFuncButtons(List<MySprite> sprites, float w, float h, int nDsp)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (nDsp == 0)
            {
                DrawButton(sprites, strDel, 0, 6, w, h, IsPressed(lcdClip+ 0));
                DrawButton(sprites, "Dup",  1, 6, w, h, IsPressed(lcdClip+ 1));
                DrawButton(sprites, "New",  2, 6, w, h, IsPressed(lcdClip+ 2));

                DrawButton(sprites, "Cue",  4, 6, w, h, EditClip.Track.NextPat > -1);         
                DrawButton(sprites, "◄",    5, 6, w, h, IsPressed(lcdClip+ 5) ^ EditClip.MovePat); }
         else { DrawButton(sprites, "►",    0, 6, w, h, IsPressed(lcdClip+ 6) ^ EditClip.MovePat);
                DrawButton(sprites, "◄►",   1, 6, w, h, EditClip.MovePat);

                DrawButton(sprites, "[",    3, 6, w, h, EditClip.In);
                DrawButton(sprites, "]",    4, 6, w, h, EditClip.Out);
                DrawButton(sprites, "X",    5, 6, w, h, IsPressed(lcdClip+11));
            }
        }


        void DrawBrackets(List<MySprite> sprites, int p, float x, float y, float w, float h, float bw, float bh)
        {
            if (OK(EditClip.Blocks.Find(b => p == b.First))) DrawLeftBracket (sprites, x,     y, bw, h, bh);
            if (OK(EditClip.Blocks.Find(b => p == b.Last ))) DrawRightBracket(sprites, x + w, y, bw, h, bh);
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
