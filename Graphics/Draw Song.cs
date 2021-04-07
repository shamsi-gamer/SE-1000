using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawSong()
        {
            if (!TooComplex) DrawSong(dspSong1, 0);
            if (!TooComplex) DrawSong(dspSong2, 1);
        }


        void DrawSong(Display dsp, int nDsp)
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

            var pxCur = x - (nDsp*4 + g_songOff) * pw + CurPat * pw;
            var py = y + h/2 - ph/2;

            var first = nDsp * 4 + g_songOff;
            var next = Math.Min((nDsp + 1) * 4 + g_songOff, g_song.Patterns.Count);

            var curBlock = g_song.GetBlock(CurPat);

            var _f = first - g_songOff;


            for (int p = first; p < next; p++)
            {
                var _p = p - g_songOff;

                var px = x - _f * pw + _p * pw;

                FillRect(sprites, px, py, pw, ph, color1);

                FillRect(sprites, px,      py, 1, ph, color4);
                FillRect(sprites, px + pw, py, 1, ph, color4);

                var bo = 30;

                var block = g_song.GetBlock(p);
                if (block != null)
                {
                    var c = curBlock == block ? color3 : color2;

                    FillRect(sprites, px, py - bo, xt, bo, c);
                    FillRect(sprites, px, py + ph, xt, bo, c);
                }

                DrawBrackets(sprites, p, px, py - bo, pw, ph + bo*2, 40, 2);
            }


            if (!g_piano)
            {
                for (int p = first; p < next; p++)
                {
                    var px = x - (nDsp * 4 + g_songOff) * pw + p * pw;
                    var ch = ph / g_nChans;
                    var cy = py + ph - (CurChan + 1) * ch;

                    FillRect(sprites, px, cy, pw, ch, color3);
                }
            }


            // draw edit position
            if (   g_song.EditPos >= first * g_nSteps
                && g_song.EditPos <  next  * g_nSteps)
            {
                var pl    = x - pw * (nDsp * 4 * pw + CurPat + g_songOff);
                var xTick = wt * g_song.EditPos;

                FillRect(
                    sprites,
                    xTick + 2, 
                    py, 
                    wt - 4,
                    ph + (g_paramKeys || g_paramAuto ? ph/5 : 0),
                    color3);
            }


            // draw keys
            for (int p = first; p < next; p++)
            {
                var _p = p - g_songOff;
                var px = x - _f * pw + _p * pw;

                if (g_piano) DrawPianoRoll(sprites, px, py, pw, ph, g_song, p, 1, false, g_nSteps);
                else         DrawPattern  (sprites, px, py, pw, ph, g_song, p, 1, false);

                if (g_paramKeys)
                {
                    FillRect     (sprites, px, py+ph+ph/5, pw, 1,    color3);
                    DrawParamKeys(sprites, px, py + ph,    pw, ph/5, g_song, p, CurChan);
                }
                else if (g_paramAuto)
                {
                    FillRect(sprites, px, py + ph + ph/5, pw, 1, color3);
                }
            }

            if (g_paramAuto)
            { 
                var fx   = x - _f * pw;
                var wMax = Math.Min(g_song.Patterns.Count * pw, w * 2);

                DrawParamAuto(sprites, fx, py + ph, pw, ph/5, wMax, g_song, 0, CurChan);
            }


            // draw current pattern box
            DrawRect(sprites, pxCur + 1, py, wt * g_nSteps - 2, ph, color6, 2);


            // draw play position
            if (OK(g_song.PlayStep))
            {
                var pl    = x  - pw * (nDsp * 4 + g_songOff);
                var xTick = pl + wt * (int)g_song.PlayStep;

                FillRect(sprites, xTick, py, wt, ph, color6);

                if (g_piano) DrawPianoNeg(sprites, pl, py, pw, ph, g_song, g_song.PlayPat, (int)g_song.PlayStep, false);
                else         DrawPatNeg  (sprites, pl, py, pw, ph, g_song, g_song.PlayPat, (int)g_song.PlayStep, false);
            }


            if (g_song.Patterns.Count > maxDspPats)
            {
                var bw = (w * 2) / (float)g_song.Patterns.Count;
                var sh = 29;

                var px = x - nDsp * 4 * pw;
                var by = y + h - sh;

                for (int p = 0; p < g_song.Patterns.Count; p++)
                {
                    FillRect(sprites, px + bw * p + 1, by, 1, sh, color4);

                    var m = Array.FindIndex(g_mem, _m => _m == p);
                    if (m > -1) DrawString(sprites, S((char)(65 + m)), px + 5, by - 30, 0.7f, color4);
                }

                foreach (var b in g_song.Blocks)
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

                if (OK(g_song.PlayStep))
                    FillRect(sprites, px + bw / g_nSteps * g_song.PlayStep, by, 4, sh, color6);
            }


            for (int p = first; p < next; p++)
            {
                var _p = p - g_songOff;

                var px = x - _f * pw + _p * pw;

                var b = g_song.GetBlock(p);
                var c = b != null && b == curBlock ? color5 : color4;

                DrawString(sprites, S(p + 1), px + 8, py - 28, 0.8f, c);

                var m = Array.FindIndex(g_mem, _m => _m == p);
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
                DrawFuncButton(sprites, "Del",  0, w, h, false, false, songPressed.Contains(0));
                DrawFuncButton(sprites, "Dup",  1, w, h, false, false, songPressed.Contains(1));
                DrawFuncButton(sprites, "New",  2, w, h, false, false, songPressed.Contains(2));
                DrawFuncButton(sprites, "◄",    5, w, h, false, false, songPressed.Contains(5) ^ g_movePat);
            }
            else
            {
                DrawFuncButton(sprites, "►",  0, w, h, false, false, songPressed.Contains( 6) ^ g_movePat);
                DrawFuncButton(sprites, "◄►", 1, w, h, false, false, g_movePat);
                DrawFuncButton(sprites, "[",  3, w, h, false, false, g_in);
                DrawFuncButton(sprites, "]",  4, w, h, false, false, g_out);
                DrawFuncButton(sprites, "X",  5, w, h, false, false, songPressed.Contains(11));
            }
        }


        void DrawBrackets(List<MySprite> sprites, int p, float x, float y, float w, float h, float bw, float bh)
        {
            if (g_song.Blocks.Find(b => p == b.First) != null) DrawLeftBracket (sprites, x,     y, bw, h, bh);
            if (g_song.Blocks.Find(b => p == b.Last ) != null) DrawRightBracket(sprites, x + w, y, bw, h, bh);
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
