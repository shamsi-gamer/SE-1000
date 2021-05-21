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
            if (NO(dsp)) return;

            var v = dsp.Viewport;

            var x = v.X;
            var y = v.Y;
            var w = v.Width;
            var h = v.Height;

            var sprites = new List<MySprite>();


            FillRect(sprites, x, y, w, h, color0);


            if (OK(CurClip))
            {
                var xt = 256f;
                var wt = xt / g_patSteps;
                var ht = wt;

                var pw = wt * g_patSteps;
                var ph = ht * g_nChans;

                var pxCur = x - (nDsp*4 + CurClip.SongOff) * pw + CurPat * pw;
                var py = y + h/2 - ph/2;

                var first = nDsp * 4 + CurClip.SongOff;
                var next = Math.Min((nDsp + 1) * 4 + CurClip.SongOff, CurClip.Patterns.Count);

                var curBlock = CurClip.GetBlock(CurPat);

                var _f = first - CurClip.SongOff;


                for (int p = first; p < next; p++)
                {
                    var _p = p - CurClip.SongOff;

                    var px = x - _f * pw + _p * pw;

                    FillRect(sprites, px, py, pw, ph, color1);

                    FillRect(sprites, px,      py, 1, ph, color4);
                    FillRect(sprites, px + pw, py, 1, ph, color4);

                    var bo = 30;

                    var block = CurClip.GetBlock(p);
                    if (OK(block))
                    {
                        var c = curBlock == block ? color3 : color2;

                        FillRect(sprites, px, py - bo, xt, bo, c);
                        FillRect(sprites, px, py + ph, xt, bo, c);
                    }

                    DrawBrackets(sprites, p, px, py - bo, pw, ph + bo*2, 40, 2);
                }


                if (!CurClip.Piano)
                {
                    for (int p = first; p < next; p++)
                    {
                        var px = x - (nDsp * 4 + CurClip.SongOff) * pw + p * pw;
                        var ch = ph / g_nChans;
                        var cy = py + ph - (CurChan + 1) * ch;

                        FillRect(sprites, px, cy, pw, ch, color3);
                    }
                }


                // draw edit position
                if (   CurClip.EditPos >= first * g_patSteps
                    && CurClip.EditPos <  next  * g_patSteps)
                {
                    var pl    = x - pw * (nDsp * 4 * pw + CurPat + CurClip.SongOff);
                    var xTick = wt * CurClip.EditPos;

                    FillRect(
                        sprites,
                        xTick + 2, 
                        py, 
                        wt - 4,
                        ph + (CurClip.ParamKeys || CurClip.ParamAuto ? ph/5 : 0),
                        color3);
                }


                // draw keys
                for (int p = first; p < next; p++)
                {
                    var _p = p - CurClip.SongOff;
                    var px = x - _f * pw + _p * pw;

                    if (CurClip.Piano) DrawPianoRoll(sprites, px, py, pw, ph, CurClip, p, 1, F, g_patSteps);
                    else         DrawPattern  (sprites, px, py, pw, ph, CurClip, p, 1, F);

                    if (CurClip.ParamKeys)
                    {
                        FillRect     (sprites, px, py+ph+ph/5, pw, 1,    color3);
                        DrawParamKeys(sprites, px, py + ph,    pw, ph/5, CurClip, p, CurChan);
                    }
                    else if (CurClip.ParamAuto)
                    {
                        FillRect(sprites, px, py + ph + ph/5, pw, 1, color3);
                    }
                }

                if (CurClip.ParamAuto)
                { 
                    var fx   = x - _f * pw;
                    var wMax = Math.Min(CurClip.Patterns.Count * pw, w * 2);

                    DrawParamAuto(sprites, fx, py + ph, pw, ph/5, wMax, CurClip, 0, CurChan);
                }


                // draw current pattern box
                DrawRect(sprites, pxCur + 1, py, wt * g_patSteps - 2, ph, color6, 2);


                // draw play position
                if (g_playing)
                {
                    var pl    = x  - pw * (nDsp * 4 + CurClip.SongOff);
                    var xTick = pl + wt * (int)PlayStep;

                    FillRect(sprites, xTick, py, wt, ph, color6);

                    if (CurClip.Piano) DrawPianoNeg(sprites, pl, py, pw, ph, CurClip, PlayPat, (int)PlayStep, F);
                    else                         DrawPatNeg  (sprites, pl, py, pw, ph, CurClip, PlayPat, (int)PlayStep, F);
                }


                if (CurClip.Patterns.Count > maxDspPats)
                {
                    var bw = (w * 2) / (float)CurClip.Patterns.Count;
                    var sh = 29;

                    var px = x - nDsp * 4 * pw;
                    var by = y + h - sh;

                    for (int p = 0; p < CurClip.Patterns.Count; p++)
                    {
                        FillRect(sprites, px + bw * p + 1, by, 1, sh, color4);

                        var m = Array.FindIndex(CurClip.Mems, _m => _m == p);
                        if (m > -1) DrawString(sprites, S((char)(65 + m)), px + 5, by - 30, 0.7f, color4);
                    }

                    foreach (var b in CurClip.Blocks)
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
                    var _p = p - CurClip.SongOff;

                    var px = x - _f * pw + _p * pw;

                    var b = CurClip.GetBlock(p);
                    var c = OK(b) && b == curBlock ? color5 : color4;

                    DrawString(sprites, S(p + 1), px + 8, py - 28, 0.8f, c);

                    var m = Array.FindIndex(CurClip.Mems, _m => _m == p);
                    if (m > -1) DrawString(sprites, S((char)(65 + m)), px + 8, py - 68, 1, color4);
                }


                DrawClipFuncButtons(sprites, w, h, nDsp);
            }

            dsp.Draw(sprites);
        }


        void DrawClipFuncButtons(List<MySprite> sprites, float w, float h, int nDsp)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (nDsp == 0)
            {
                DrawFuncButton(sprites, strDel, 0, w, h, F, F, g_lcdPressed.Contains(lcdClip+ 0));
                DrawFuncButton(sprites, "Dup",  1, w, h, F, F, g_lcdPressed.Contains(lcdClip+ 1));
                DrawFuncButton(sprites, "New",  2, w, h, F, F, g_lcdPressed.Contains(lcdClip+ 2));
                                                                                              
                DrawFuncButton(sprites, "Cue",  4, w, h, F, F, CurClip.Track.NextPat > -1);         
                DrawFuncButton(sprites, "◄",    5, w, h, F, F, g_lcdPressed.Contains(lcdClip+ 5) ^ CurClip.MovePat); }
         else { DrawFuncButton(sprites, "►",    0, w, h, F, F, g_lcdPressed.Contains(lcdClip+ 6) ^ CurClip.MovePat);
                DrawFuncButton(sprites, "◄►",   1, w, h, F, F, CurClip.MovePat);
                                                
                DrawFuncButton(sprites, "[",    3, w, h, F, F, CurClip.In);
                DrawFuncButton(sprites, "]",    4, w, h, F, F, CurClip.Out);
                DrawFuncButton(sprites, "X",    5, w, h, F, F, g_lcdPressed.Contains(lcdClip+11));
            }
        }


        void DrawBrackets(List<MySprite> sprites, int p, float x, float y, float w, float h, float bw, float bh)
        {
            if (OK(CurClip.Blocks.Find(b => p == b.First))) DrawLeftBracket (sprites, x,     y, bw, h, bh);
            if (OK(CurClip.Blocks.Find(b => p == b.Last ))) DrawRightBracket(sprites, x + w, y, bw, h, bh);
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
