using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void DrawPatternDisplay(List<MySprite> sprites, float x, float y, float w, float h, Song song, int pat, bool isolated)
        {
            FillRect(sprites, x, y, w, h, color0);

            var rh = h - 90;


            DrawChannelList(sprites, x, y, 340, rh, song);

            var xt = 340;
            var wt = (w - xt) / nSteps;

            DrawGrid(sprites, x + xt, y, w - xt, rh, song.CurPat);


            var ch = rh / nChans;
            var cy = y + rh - (song.CurChan + 1) * ch;

            FillRect(sprites, xt, cy, w - xt, ch, color3);

            for (int t = 1; t < nSteps; t++)
                FillRect(sprites, xt + t * wt, cy, 1, ch, color4);


            // edit position
            if (   song.EditPos >= song.CurPat      * nSteps
                && song.EditPos < (song.CurPat + 1) * nSteps)
            {
                FillRect(
                    sprites, 
                    x + xt + wt * (song.EditPos % nSteps), 
                    y, 
                    wt,
                    g_paramKeys || g_paramAuto ? h : rh,
                    color3);
            }


            DrawPattern(sprites, x + xt, y, w - xt, rh, song, pat, 2, isolated);

            if (   OK(song.PlayStep)
                && song.PlayPat == pat)
            {
                FillRect(sprites, x + xt + wt * ((int)song.PlayStep % nSteps), y, wt, rh, color6);
                DrawPatNeg(sprites, x + xt, y, w - xt, rh, song, pat, (int)song.PlayStep, isolated);
            }


            FillRect(sprites, x, y + rh, w, 1, color6);

            if (IsCurParam())
                DrawParamValues(sprites, CurParam, x, y, w, h, xt, rh, song, pat);

            if (song.SelChan < 0)
                DrawFuncButtons(sprites, w, h, song);
        }


        void DrawChannelList(List<MySprite> sprites, float x, float y, float w, float h, Song song)
        {
            var ch = h / nChans;

            FillRect(sprites, x, y + h - song.CurChan * ch - 35, w, ch, color6);

            for (int c = 0; c < nChans; c++)
            {
                var yLine = y + h - c * ch - 40;

                var pat  = CurrentPattern(song);
                var chan = pat.Channels[c];

                DrawString(sprites, 
                     (c + 1).ToString().PadLeft(2)
                    + " "
                    + chan.Instrument.Name,
                    6,
                    yLine + 6,
                    1,
                    c == song.CurChan ? color0 : (g_piano ? color2 : (chan.Notes.Count > 0 ? color6 : color2)));
            }
        }


        void DrawGrid(List<MySprite> sprites, float x, float y, float w, float h, int pattern, int patSteps = nSteps)
        {
            var wt = w / nSteps;
            var ht = h / nChans;

            for (int t = 0; t < patSteps; t += 4)
                FillRect(sprites, x + t * wt, y, wt, h, color1);

            for (int ch = 0; ch < nChans; ch += 2)
                FillRect(sprites, x, y + h - (ch + 1) * ht, wt*patSteps, ht, color2);

            for (int t = 0; t < patSteps; t++)
                FillRect(sprites, x + t * wt, y, 1, h, color3);
        }


        void DrawPattern(List<MySprite> sprites, float x, float y, float w, float h, Song song, int pat, int gs, bool isolated, int songSteps = nSteps)
        {
            var wt = w/nSteps;
            var ht = h/nChans;

            for (int ch = 0; ch < nChans; ch++)
            {
                var yLine = y + h - (ch+1) * h/nChans;

                var bh = h/80;
                var th = ht - bh*2;


                for (int p = 0; p <= pat; p++)
                {
                    var patStart = pat * nSteps;
                    var patEnd   = patStart + songSteps;//(pat+1)*nSteps;

                    var chan = song.Patterns[p].Channels[ch];


                    foreach (var n in chan.Notes)
                    {
                        var noteStart = song.GetStep(n) + n.ShOffset;
                        var noteEnd   = noteStart + n.StepLength;

                        if (   noteEnd   <= patStart
                            || noteStart >= patEnd)
                            continue;

                        noteStart = Math.Max(noteStart, patStart);
                        noteEnd   = Math.Min(noteEnd,   patEnd);


                        var pt = new Vector2(
                            x + wt * (noteStart-patStart),
                            yLine + ht/2);

                        var tw = (float)Math.Floor(wt * (noteEnd-noteStart)) - gs*2;

                        var colTick = chan.On ? color6 : color3;


                        FillRect(
                            sprites, 
                            pt.X + gs,
                            pt.Y - th/2,
                            tw,
                            th,
                            colTick);
                    }
                }
            }
        }


        void DrawPatNeg(List<MySprite> sprites, float x, float y, float w, float h, Song song, int pat, float step, bool isolated)
        {
            var wt = w/nSteps;
            var ht = h/nChans;

            var _step = step % nSteps;

            var bt = w/76;
            var th = ht - bt*2;

            for (int ch = 0; ch < nChans; ch++)
            {
                var yLine = y + h - (ch + 1) * h/nChans;

                for (int p = 0; p <= pat; p++)
                {
                    var patStart =  pat   *nSteps;
                    var patEnd   = (pat+1)*nSteps;

                    var chan = song.Patterns[p].Channels[ch];

                    foreach (var n in chan.Notes)
                    {
                        var noteStart = song.GetStep(n) + n.ShOffset;
                        var noteEnd   = noteStart + n.StepLength;

                        if (   noteEnd   <= patStart
                            || noteStart >= patEnd)
                            continue;

                        var pt = new Vector2(
                            x + wt * (isolated ? _step : step),
                            yLine + ht/2);

                        if (   step >= noteStart
                            && step <  noteEnd)
                        {
                            var tw = (float)Math.Floor(wt * (noteEnd - noteStart));

                            FillRect(
                                sprites, 
                                pt.X + 1,
                                pt.Y - th/2,
                                Math.Min(tw, wt) - 2,
                                th,
                                color2);
                        }
                    }
                }
            }
        }
    }
}
