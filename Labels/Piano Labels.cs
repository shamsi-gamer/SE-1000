using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void InitPianoLabels()
        {
            InitPianoLabelsHigh();
            InitPianoLabelsLow();

            lblOctave     = new Label(Lbl("Octave"),  null, null, UpdateOctaveLabel);
            lblShuffle    = new Label(Lbl("Shuffle"), null, null, UpdateShuffleLabel);
            lblOctaveUp   = new Label(Lbl("Octave Up"));
            lblOctaveDown = new Label(Lbl("Octave Down"));
        }


        void InitPianoLabelsHigh()
        {
            lblHigh = new List<Label>();

            var high = new List<IMyTextPanel>();
            Get(high, l => l.CustomName.Length >= 11 && l.CustomName.Substring(0, 11) == "Label High ");
            high = high.OrderBy(l => int.Parse(l.CustomName.Substring(11))).ToList();

            for (int h = 0; h < 10; h++)
            { 
                lblHigh.Add(new Label(
                    high[h], 
                    PianoHighIsBright, 
                    PianoHighIsDim, 
                    UpdatePianoHigh, 
                    UpdatePianoHighColor, 
                    h,
                    T));
            }

            lblHigh.Add(new Label(high[10],
                lbl => IsPressed(lbl),
                null,
                UpdatePianoToggle));
        }


        bool PianoHighIsBright(Label lbl)
        {
            return
                ShowPiano
                ? NoteIsBright(HighToNote(lbl.Data))
                : ToggleIsBright(lbl);
        }


        bool PianoHighIsDim(Label lbl)
        {
            return
                   ShowPiano 
                && NoteIsDim(HighToNote(lbl.Data));
        }


        void UpdatePianoHigh(Label lbl)
        {
            if (ShowPiano)
                lbl.SetText(" ");//HighNoteName(lbl.Data, CurClip.HalfSharp)); 

            else
            { 
                switch (lbl.Data)
                { 
                case 0: lbl.SetText("◄∙∙");                 break;
                case 1: lbl.SetText("∙∙►");                 break;
                                                                       
                case 2: lbl.SetText("Pick");                break;
                case 3: lbl.SetText("All Ch", 7.6f, 19.5f); break;
                case 4: lbl.SetText("Inst");                break;
                                                                       
                case 5: lbl.SetText("Rnd");                 break;
                case 6: lbl.SetText("Clr");                 break;
                                                                       
                case 7: lbl.SetText("1/4");                 break;
                case 8: lbl.SetText("1/8");                 break;
                case 9: lbl.SetText("Flip");                break;
                }
            }
        }


        void UpdatePianoHighColor(Label lbl)
        {
            lbl.BackColor = ShowPiano ? color1 : color0;
        }


        void InitPianoLabelsLow()
        {
            lblLow = new List<Label>();

            var low = new List<IMyTextPanel>();
            Get(low, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Low ");
            low = low.OrderBy(l => int.Parse(l.CustomName.Substring(10))).ToList();

            for (int l = 0; l < low.Count; l++)
            { 
                lblLow.Add(new Label(
                    low[l], 
                    PianoLowIsBright, 
                    PianoLowIsDim, 
                    UpdatePianoLow, 
                    UpdatePianoLowColor, 
                    -l,
                    T));
            }
        }


        bool PianoLowIsBright(Label lbl)
        {
            return
                ShowPiano
                ?    -lbl.Data == 15 && CurClip.HalfSharp
                  || NoteIsBright(LowToNote(-lbl.Data))
                : StepIsBright(lbl);
        }


        bool PianoLowIsDim(Label lbl)
        {
            return
                ShowPiano
                ? NoteIsDim(LowToNote(-lbl.Data))
                : F;
        }


        void UpdatePianoLow(Label lbl)
        {
            if (ShowPiano)
            {
                if (-lbl.Data < 15) lbl.SetText(" ");//LowNoteName(-lbl.Data, CurClip.HalfSharp));
                else                lbl.SetText("‡", 8, 17);
            }
            else                    lbl.SetText(" ");
        }


        void UpdatePianoLowColor(Label lbl)
        {
            if (ShowPiano)
                lbl.BackColor = -lbl.Data < 15 ? color1 : color0;

            else
                lbl.BackColor = 
                    -lbl.Data % 4 == 0 
                    ? color2 
                    : color0;
        }


        bool NoteIsBright(int noteNum)
        { 
            if (IsCurParam(strTune))
            {
                var tune =
                    CurSrc > -1
                    ? SelSource    .Tune
                    : SelInstrument.Tune;

                return tune.Chord.Contains(noteNum);
            }

            else if (CurClip.Chord > -1
                  && CurClip.ChordEdit)
                return CurClip.Chords[CurClip.Chord].Contains(noteNum);

            else if (g_playing 
                  && PlayChannel.Notes.FindIndex(n => NoteIsPlaying(noteNum, n)) > -1)
                return T; // note is in the currently played channel

            else if (g_notes.FindIndex(n => NoteIsTriggered(noteNum, n)) > -1)
                return T; // note is being played on the piano

            return F;
        }


        bool NoteIsDim(int noteNum)
        {
            if (IsCurParam(strTune))
            {
                var tune =
                    CurSrc > -1
                    ? SelSource    .Tune
                    : SelInstrument.Tune;

                return tune.FinalChord.Contains(noteNum);
            }
            else
            { 
                for (int ch = 0; ch < g_nChans; ch++)
                {
                    if (ch == CurChan)
                        continue;

                    if (   g_playing
                        && PlayPattern.Channels[ch].Notes.FindIndex(n => NoteIsPlaying(noteNum, n)) > -1)
                        return T;
                }
            }

            return F;
        }


        bool NoteIsPlaying(int noteNum, Note note)
        {
            if (noteNum != note.Number)
                return F;

            // note is at the playback position
            if (   PlayStep >= note.SongStep + note.ShOffset
                && PlayStep <  note.SongStep + note.ShOffset + note.StepLength)
                return T;

            // note is at edit position
            if (   note.SongStep >= CurClip.EditPos 
                && note.SongStep <  CurClip.EditPos + CurClip.EditStepLength)
                return T;

            return F;
        }
        
        
        bool NoteIsTriggered(int noteNum, Note note)
        {
            var timeStep = g_playing ? PlayStep : TimeStep;

            return
                   noteNum == note.Number
                && timeStep >= note.PatStep
                && timeStep <  note.PatStep + note.StepLength;
        }


        bool ToggleIsBright(Label lbl)
        {
            return 
                   lbl.Data == 2 && CurClip.Pick
                || lbl.Data == 3 && CurClip.AllChan
                || lbl.Data == 4 && CurClip.RndInst;
        }


        bool StepIsBright(Label lbl)
        {
            var patStep  = -lbl.Data;
            var songStep =  CurPat * g_patSteps + patStep;

            var on = CurChannel.Notes.Find(n => 
                   n.PatStep >= patStep
                && n.PatStep <  patStep+1) != null;

            if (   g_playing
                && (int)PlayStep  == songStep
                && CurPat == PlayPat)
                return !on;
            else if (on)
                return T;

            return F;
        }


        void UpdateOctaveLabel(Label lbl)
        {
            if (TooComplex) return;

            int val;

                 if (CurClip.Spread) val = CurClip.ChordSpread;
            else if (ShowPiano)      val = CurChannel.Transpose;
            else                     val = CurChannel.Shuffle;

            lbl.SetText((val > 0 ? "+" : "") + S(val));
        }


        void UpdateShuffleLabel(Label lbl)
        {
            if (CurClip.Spread)
                lbl.SetText("Sprd");

            else if (ShowPiano)
                lbl.SetText(
                    " ▄█   █ █ ██ █ █ █   █▄ \n" +
                   " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +
                     " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ ",
                    2,
                    32);

            else
                lbl.SetText("Shuf");
        }
    }
}
