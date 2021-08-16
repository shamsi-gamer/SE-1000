using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void DrawPianoDisplay(List<MySprite> sprites, float x, float y, float w, float h, Clip clip, int pat, bool isolated)
        {
            FillRect(sprites, x, y, w, h, color0);

            var  rh = h - 90;
            var irh = h - 50;

            var xt = 310;
            var pw = 25;

            DrawChannelList(sprites, x, y, xt-pw, rh);


            var _dummy = new List<TriggerValue>();

            var tp = new TimeParams(g_time, 0, Note_null, EditedClip.EditLength, CurSrc, _dummy, clip, this);


            var wt = (float)(w - xt) / g_patSteps;

            var isEditing = 
                   OK(clip.EditPos)
                && clip.EditPos >= EditPat      * g_patSteps
                && clip.EditPos < (EditPat + 1) * g_patSteps;

            // draw edit position
            if (isEditing)
            { 
                FillRect(
                    sprites, 
                    x + xt + wt * (clip.EditPos % g_patSteps), 
                    y, 
                    wt * EditedClip.EditStep - 9,
                    EditedClip.ParamKeys || EditedClip.ParamAuto ? h : rh, 
                    color1);
            }


            DrawPianoGrid(sprites, x + xt,        y, w - xt, rh);


            // draw edit position
            if (isEditing)
            { 
                FillRect(
                    sprites, 
                    x + xt + wt * (clip.EditPos % g_patSteps), 
                    y, 
                    wt / (EditedClip.EditStep == 0.5f ? 2 : 1),
                    EditedClip.ParamKeys || EditedClip.ParamAuto ? h : rh, 
                    color3);
            }


            DrawPianoRoll(sprites, x + xt, y, w - xt, rh, clip, pat, 2);


            var track = EditedClip.Track;

            // draw position line/s
            if (   Playing
                && track.PlayPat == pat
                && EditedClipIsPlaying)
            {
                FillRect(sprites, x + xt + wt * ((int)track.PlayStep % g_patSteps), y, wt, rh, color6);
                DrawPianoNeg(sprites, x + xt, y, w - xt, rh, EditedClip, pat, (int)track.PlayStep, True);
            }


            FillRect(sprites, x, y + rh, w, 1, color6);

            if (IsCurParam())
                DrawKeysAndAuto(sprites, EditedClip.CurParam, x, y, w, h, xt, rh, clip, pat);

            if (SelChan < 0)
                DrawFuncButtons(sprites, w, h);
        }



        void DrawPianoGrid(List<MySprite> sprites, float x, float y, float w, float h)
        {
            var wt = w/g_patSteps;

            //for (int t = 0; t < g_patSteps; t += 4)
            //    FillRect(sprites, x + t * wt, y, wt, h, color1);

            DrawBeats(sprites, x, y, w, h);
            DrawPianoKeys(sprites, x, y, wt * g_patSteps, h, color2);

            for (int t = 0; t < g_patSteps; t++)
                FillRect(sprites, x + t * wt, y, 1, h, color3);
        }



        void DrawPianoKeys(List<MySprite> sprites, float x, float y, float w, float h, Color white)
        {
            int[] black = { 1, 3, 6, 8, 10, 13, 15, 18, 20, 22 };

            var ht = h/25;

            for (int n = 0; n < 25; n++)
            {
                if (!black.Contains(n))
                    FillRect(sprites, x, y + h - (n+1)*ht + 1, w, ht-2, white);
            }
        }



        void DrawPianoRoll(List<MySprite> sprites, float x, float y, float w, float h, Clip clip, int p, int gs)
        {
            for (int ch = 0; ch < g_nChans; ch++)
            {
                if (   ch != CurChan
                    && (  !clip.RndInst
                        || clip.Patterns[p].Channels[ch].Instrument == clip.Patterns[p].Channels[CurChan].Instrument))
                    DrawChanNotes(sprites, x, y, w, h, clip, p, gs, ch, color3);
            }

            DrawChanNotes(sprites, x, y, w, h, clip, p, gs, CurChan, color6);
        }



        void DrawChanNotes(List<MySprite> sprites, float x, float y, float w, float h, Clip clip, int p, int gs, int ch, Color col)
        {
            var wt = w/g_patSteps;
            var ht = h/ 25;
                     
            var bw = w/ 76;
            var bh = h/200;
            var th = ht - bh*2;

            for (int _p = 0; _p <= p; _p++)
            {
                var patStart = p * g_patSteps;
                var patEnd   = patStart + g_patSteps;

                var pat  = clip.Patterns[_p];
                var chan = pat.Channels[ch];

                var min  = (60 + chan.Transpose*12) * NoteScale;
                var max  = (84 + chan.Transpose*12) * NoteScale;

                foreach (var n in chan.Notes)
                {
                    if (   n.Number < min
                        || n.Number > max)
                        continue;

                    var noteStart = n.ClipStep + n.ShOffset;
                    var noteEnd   = noteStart + n.StepLength;

                    if (   noteEnd   <= patStart
                        || noteStart >= patEnd)
                        continue;

                    noteStart = Math.Max(noteStart, patStart);
                    noteEnd   = Math.Min(noteEnd,   patEnd);

                    var yLine = y + h - ((float)n.Number/NoteScale - chan.Transpose * 12 - 60 + 1) * ht;

                    var pt = new Vector2(
                        x + wt * (noteStart-patStart),
                        yLine + ht/2);

                    var tw = (float)Math.Floor(wt * (noteEnd-noteStart)) - gs*2;

                    FillRect(sprites, 
                        pt.X + gs,
                        pt.Y - th/2,
                        tw,
                        th,
                        col);
                }
            }
        }



        void DrawPianoNeg(List<MySprite> sprites, float x, float y, float w, float h, Clip clip, int pat, float step, bool isolated)
        {
            for (int ch = 0; ch < g_nChans; ch++)
            {
                var wt = w/g_patSteps;
                var ht = h/25;

                var _step = step % g_patSteps;

                var bh = h/200;
                var th = ht - bh*2;

                var col =
                    ch == CurChan
                    ? color2
                    : color5;


                for (int _p = 0; _p <= pat; _p++)
                {
                    var patStart =  pat   *g_patSteps;
                    var patEnd   = (pat+1)*g_patSteps;

                    var chan = clip.Patterns[_p].Channels[ch];


                    var min = (60 + chan.Transpose*12) * NoteScale;
                    var max = (84 + chan.Transpose*12) * NoteScale;


                    foreach (var n in chan.Notes)
                    {
                        if (   n.Number < min
                            || n.Number > max)
                            continue;

                        var noteStart = n.ClipStep + n.ShOffset;
                        var noteEnd   = noteStart + n.StepLength;

                        if (   noteEnd   <= patStart
                            || noteStart >= patEnd)
                            continue;

                        var yLine = y + h - ((float)n.Number/NoteScale - chan.Transpose * 12 - 60 + 1) * ht;

                        var pt = new Vector2(
                            x + wt * (isolated ? _step : step),
                            yLine + ht/2);

                        if (   step >= noteStart
                            && step <  noteEnd)
                        {
                            var tw = (float)Math.Floor(wt * (noteEnd - noteStart));

                            FillRect(sprites, 
                                pt.X + 1,
                                pt.Y - th/2,
                                Math.Min(tw, wt) - 2,
                                th,
                                col);
                        }
                    }
                }
            }
        }



        static void DrawOctave(List<MySprite> sprites, float x, float y, float w, float h, int octave, int firstNote, int lowNote, int highNote)
        {
            DrawOctave(
                sprites, 
                x, y, w, h, 
                octave, 
                new int[] {
                    lowNote, 
                    highNote },
                firstNote,
                color1,
                color4,
                color6);
        }



        static void DrawOctave(List<MySprite> sprites, float x, float y, float w, float h, int octave, int[] brightNotes, int firstNote, Color colBlack, Color colWhite, Color colBright)
        {
            var kw = w/7;
            var bw = w/12;
            var bh = h*3/5f;

            var minNote = 36;


            for (int i = 0, j = 0; i < 7; i++, j++)
            {
                var isNote = brightNotes.Contains(/*minNote +*/ octave*12 + j);

                var kx    = x + i*kw;
                var color = isNote ? colBright : colWhite;

                FillRect(sprites, kx+1, y, kw-2, h, color);

                if (   i != 2
                    && i != 6) 
                    j++;
            }


            for (int i = 0; i < 12; i++)
            {
                var isNote = brightNotes.Contains(/*minNote +*/ octave*12 + i);

                var color = isNote ? colBright : colBlack;

                if (   i ==  1 
                    || i ==  3
                    || i ==  6
                    || i ==  8
                    || i == 10)
                    FillRect(sprites, x+1.3f + i*bw, y, bw, bh, color);
            }
        }
    }
}
