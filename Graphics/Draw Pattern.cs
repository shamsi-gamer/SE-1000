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
            var wt = (w - xt) / g_nSteps;

            DrawGrid(sprites, x + xt, y, w - xt, rh, CurPat);


            var ch = rh / g_nChans;
            var cy = y + rh - (CurChan + 1) * ch;

            FillRect(sprites, xt, cy, w - xt, ch, color3);

            for (int t = 1; t < g_nSteps; t++)
                FillRect(sprites, xt + t * wt, cy, 1, ch, color4);


            // edit position
            if (   song.EditPos >= CurPat      * g_nSteps
                && song.EditPos < (CurPat + 1) * g_nSteps)
            {
                FillRect(
                    sprites, 
                    x + xt + wt * (song.EditPos % g_nSteps), 
                    y, 
                    wt,
                    g_paramKeys || g_paramAuto ? h : rh,
                    color3);
            }


            DrawPattern(sprites, x + xt, y, w - xt, rh, song, pat, 2, isolated);

            if (   OK(g_song.PlayTime)
                && g_song.PlayPat == pat)
            {
                FillRect(sprites, x + xt + wt * ((int)g_song.PlayStep % g_nSteps), y, wt, rh, color6);
                DrawPatNeg(sprites, x + xt, y, w - xt, rh, song, pat, (int)g_song.PlayStep, isolated);
            }


            FillRect(sprites, x, y + rh, w, 1, color6);

            if (IsCurParam())
                DrawValueLegend(sprites, CurParam, x, y, w, h, xt, rh, song, pat);

            if (SelChan < 0)
                DrawFuncButtons(sprites, w, h, song);
        }


        void DrawChannelList(List<MySprite> sprites, float x, float y, float w, float h, Song song)
        {
            var ch = h / g_nChans;

            FillRect(sprites, x, y + h - CurChan * ch - 35, w, ch, CurrentChannel.On ? color6 : color3);

            for (int c = 0; c < g_nChans; c++)
            {
                var yLine = y + h - c * ch - 40;
                var chan  = CurrentPattern.Channels[c];

                DrawString(sprites, 
                     S(c + 1).PadLeft(2)
                    + " "
                    + chan.Instrument.Name,
                    6,
                    yLine + 6,
                    1,
                    c == CurChan ? color0 : (chan.Notes.Count > 0 ? (chan.On ? color6 : color3) : color2));
            }
        }


        void DrawGrid(List<MySprite> sprites, float x, float y, float w, float h, int pattern, int patSteps = g_nSteps)
        {
            var wt = w / g_nSteps;
            var ht = h / g_nChans;

            for (int t = 0; t < patSteps; t += 4)
                FillRect(sprites, x + t * wt, y, wt, h, color1);

            for (int ch = 0; ch < g_nChans; ch += 2)
                FillRect(sprites, x, y + h - (ch + 1) * ht, wt*patSteps, ht, color2);

            for (int t = 0; t < patSteps; t++)
                FillRect(sprites, x + t * wt, y, 1, h, color3);
        }


        void DrawPattern(List<MySprite> sprites, float x, float y, float w, float h, Song song, int pat, int gs, bool isolated, int songSteps = g_nSteps)
        {
            var wt = w/g_nSteps;
            var ht = h/g_nChans;

            for (int ch = 0; ch < g_nChans; ch++)
            {
                var yLine = y + h - (ch+1) * h/g_nChans;

                var bh = h/80;
                var th = ht - bh*2;


                for (int p = 0; p <= pat; p++)
                {
                    var patStart = pat * g_nSteps;
                    var patEnd   = patStart + songSteps;

                    var chan = song.Patterns[p].Channels[ch];


                    foreach (var n in chan.Notes)
                    {
                        var noteStart = n.SongStep + n.ShOffset;
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
            var wt = w/g_nSteps;
            var ht = h/g_nChans;

            var _step = step % g_nSteps;

            var bt = w/76;
            var th = ht - bt*2;

            for (int ch = 0; ch < g_nChans; ch++)
            {
                var yLine = y + h - (ch + 1) * h/g_nChans;

                for (int p = 0; p <= pat; p++)
                {
                    var patStart =  pat   *g_nSteps;
                    var patEnd   = (pat+1)*g_nSteps;

                    var chan = song.Patterns[p].Channels[ch];

                    foreach (var n in chan.Notes)
                    {
                        var noteStart = n.SongStep + n.ShOffset;
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
