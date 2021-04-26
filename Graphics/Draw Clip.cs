using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


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
            if (dsp == null) return;

            var sprites = new List<MySprite>();


            var x = dsp.Viewport.X;
            var y = dsp.Viewport.Y;
            var w = dsp.Viewport.Width;
            var h = dsp.Viewport.Height;


            FillRect(sprites, x, y, w, h, color0);


            var xt = 256f;
            var wt = xt / g_nSteps;
            var ht = wt;

            var pw = wt * g_nSteps;
            var ph = ht * g_nChans;

            var pxCur = x - (nDsp*4 + g_clip.SongOff) * pw + g_clip.CurPat * pw;
            var py = y + h/2 - ph/2;

            var first = nDsp * 4 + g_clip.SongOff;
            var next = Math.Min((nDsp + 1) * 4 + g_clip.SongOff, g_clip.Patterns.Count);

            var curBlock = g_clip.GetBlock(g_clip.CurPat);

            var _f = first - g_clip.SongOff;


            for (int p = first; p < next; p++)
            {
                var _p = p - g_clip.SongOff;

                var px = x - _f * pw + _p * pw;

                FillRect(sprites, px, py, pw, ph, color1);

                FillRect(sprites, px,      py, 1, ph, color4);
                FillRect(sprites, px + pw, py, 1, ph, color4);

                var bo = 30;

                var block = g_clip.GetBlock(p);
                if (block != null)
                {
                    var c = curBlock == block ? color3 : color2;

                    FillRect(sprites, px, py - bo, xt, bo, c);
                    FillRect(sprites, px, py + ph, xt, bo, c);
                }

                DrawBrackets(sprites, p, px, py - bo, pw, ph + bo*2, 40, 2);
            }


            if (!g_clip.Piano)
            {
                for (int p = first; p < next; p++)
                {
                    var px = x - (nDsp * 4 + g_clip.SongOff) * pw + p * pw;
                    var ch = ph / g_nChans;
                    var cy = py + ph - (g_clip.CurChan + 1) * ch;

                    FillRect(sprites, px, cy, pw, ch, color3);
                }
            }


            // draw edit position
            if (   g_clip.EditPos >= first * g_nSteps
                && g_clip.EditPos <  next  * g_nSteps)
            {
                var pl    = x - pw * (nDsp * 4 * pw + g_clip.CurPat + g_clip.SongOff);
                var xTick = wt * g_clip.EditPos;

                FillRect(
                    sprites,
                    xTick + 2, 
                    py, 
                    wt - 4,
                    ph + (g_clip.ParamKeys || g_clip.ParamAuto ? ph/5 : 0),
                    color3);
            }


            // draw keys
            for (int p = first; p < next; p++)
            {
                var _p = p - g_clip.SongOff;
                var px = x - _f * pw + _p * pw;

                if (g_clip.Piano) DrawPianoRoll(sprites, px, py, pw, ph, g_clip, p, 1, false, g_nSteps);
                else         DrawPattern  (sprites, px, py, pw, ph, g_clip, p, 1, false);

                if (g_clip.ParamKeys)
                {
                    FillRect     (sprites, px, py+ph+ph/5, pw, 1,    color3);
                    DrawParamKeys(sprites, px, py + ph,    pw, ph/5, g_clip, p, g_clip.CurChan);
                }
                else if (g_clip.ParamAuto)
                {
                    FillRect(sprites, px, py + ph + ph/5, pw, 1, color3);
                }
            }

            if (g_clip.ParamAuto)
            { 
                var fx   = x - _f * pw;
                var wMax = Math.Min(g_clip.Patterns.Count * pw, w * 2);

                DrawParamAuto(sprites, fx, py + ph, pw, ph/5, wMax, g_clip, 0, g_clip.CurChan);
            }


            // draw current pattern box
            DrawRect(sprites, pxCur + 1, py, wt * g_nSteps - 2, ph, color6, 2);


            // draw play position
            if (OK(g_clip.PlayStep))
            {
                var pl    = x  - pw * (nDsp * 4 + g_clip.SongOff);
                var xTick = pl + wt * (int)g_clip.PlayStep;

                FillRect(sprites, xTick, py, wt, ph, color6);

                if (g_clip.Piano) DrawPianoNeg(sprites, pl, py, pw, ph, g_clip, g_clip.PlayPat, (int)g_clip.PlayStep, false);
                else         DrawPatNeg  (sprites, pl, py, pw, ph, g_clip, g_clip.PlayPat, (int)g_clip.PlayStep, false);
            }


            if (g_clip.Patterns.Count > maxDspPats)
            {
                var bw = (w * 2) / (float)g_clip.Patterns.Count;
                var sh = 29;

                var px = x - nDsp * 4 * pw;
                var by = y + h - sh;

                for (int p = 0; p < g_clip.Patterns.Count; p++)
                {
                    FillRect(sprites, px + bw * p + 1, by, 1, sh, color4);

                    var m = Array.FindIndex(g_clip.Mems, _m => _m == p);
                    if (m > -1) DrawString(sprites, S((char)(65 + m)), px + 5, by - 30, 0.7f, color4);
                }

                foreach (var b in g_clip.Blocks)
                {
                    var bx = px + bw * b.First + 1;
                    var sw = bw * b.Len - 2;

                    FillRect(sprites, bx, by, sw, sh, b == curBlock ? color3 : color2);

                    for (int i = 1; i < b.Len; i++)
                        FillRect(sprites, bx + bw * i, by, 1, sh, color5);

                    DrawLeftBracket (sprites, bx, by, 16, sh, 1);
                    DrawRightBracket(sprites, bx + sw, by, 16, sh, 1);
                }

                FillRect(sprites, px + bw * g_clip.CurPat, by, bw, sh, color4);

                if (OK(g_clip.PlayStep))
                    FillRect(sprites, px + bw / g_nSteps * g_clip.PlayStep, by, 4, sh, color6);
            }


            for (int p = first; p < next; p++)
            {
                var _p = p - g_clip.SongOff;

                var px = x - _f * pw + _p * pw;

                var b = g_clip.GetBlock(p);
                var c = b != null && b == curBlock ? color5 : color4;

                DrawString(sprites, S(p + 1), px + 8, py - 28, 0.8f, c);

                var m = Array.FindIndex(g_clip.Mems, _m => _m == p);
                if (m > -1) DrawString(sprites, S((char)(65 + m)), px + 8, py - 68, 1, color4);
            }


            DrawSongFuncButtons(sprites, w, h, nDsp);


            dsp.Draw(sprites);
        }


        void DrawSongFuncButtons(List<MySprite> sprites, float w, float h, int nDsp)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (nDsp == 0)
            {
                DrawFuncButton(sprites, strDel, 0, w, h, false, false, g_clipPressed.Contains(0));
                DrawFuncButton(sprites, "Dup",  1, w, h, false, false, g_clipPressed.Contains(1));
                DrawFuncButton(sprites, "New",  2, w, h, false, false, g_clipPressed.Contains(2));

                DrawFuncButton(sprites, "Cue",  4, w, h, false, false, g_clip.CueNext > -1);
                DrawFuncButton(sprites, "◄",    5, w, h, false, false, g_clipPressed.Contains(5) ^ g_clip.MovePat); }
         else { DrawFuncButton(sprites, "►",    0, w, h, false, false, g_clipPressed.Contains(6) ^ g_clip.MovePat);
                DrawFuncButton(sprites, "◄►",   1, w, h, false, false, g_clip.MovePat);
                                                
                DrawFuncButton(sprites, "[",    3, w, h, false, false, g_clip.In);
                DrawFuncButton(sprites, "]",    4, w, h, false, false, g_clip.Out);
                DrawFuncButton(sprites, "X",    5, w, h, false, false, g_clipPressed.Contains(11));
            }
        }


        void DrawBrackets(List<MySprite> sprites, int p, float x, float y, float w, float h, float bw, float bh)
        {
            if (g_clip.Blocks.Find(b => p == b.First) != null) DrawLeftBracket (sprites, x,     y, bw, h, bh);
            if (g_clip.Blocks.Find(b => p == b.Last ) != null) DrawRightBracket(sprites, x + w, y, bw, h, bh);
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
