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

            lblOctave     = new Label(GetLabel("Octave"),  null, null, UpdateOctaveLabel);
            lblShuffle    = new Label(GetLabel("Shuffle"), null, null, UpdateShuffleLabel);
            lblOctaveUp   = new Label(GetLabel("Octave Up"));
            lblOctaveDown = new Label(GetLabel("Octave Down"));
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
                lbl.SetText(" ");

            else
            { 
                switch (lbl.Data)
                { 
                case 0: lbl.SetText("Clr");  break;
                case 1: lbl.SetText("Rnd");  break;
                                                        
                case 2: lbl.SetText("Inst"); break;
                case 3: lbl.SetText("All Ch", 7.6f, 19.5f); break;
                case 4: lbl.SetText("Pick"); break;
                                                        
                case 5: lbl.SetText("◄∙∙");  break;
                case 6: lbl.SetText("∙∙►");  break;
                                                        
                case 7: lbl.SetText("1/4");  break;
                case 8: lbl.SetText("1/8");  break;
                case 9: lbl.SetText("Flip"); break;
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
                ?    -lbl.Data == 15 && EditedClip.HalfSharp
                  || NoteIsBright(LowToNote(-lbl.Data))
                : StepIsBright(lbl);
        }


        bool PianoLowIsDim(Label lbl)
        {
            return 
                   ShowPiano 
                && NoteIsDim(LowToNote(-lbl.Data));
        }


        void UpdatePianoLow(Label lbl)
        {
            if (ShowPiano)
            {
                if (-lbl.Data == 15) lbl.SetText("‡", 8, 17); 
                else                 lbl.SetText(" ");
            } 
            else                     lbl.SetText(" ");
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

            else if (EditedClip.Chord > -1
                  && EditedClip.ChordEdit)
                return EditedClip.Chords[EditedClip.Chord].Contains(noteNum);

            else if (IsPlaying 
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

                    if (   IsPlaying
                        && OK(PlayPattern.Channels[ch].Notes.Find(n => NoteIsPlaying(noteNum, n))))
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
            if (   note.SongStep >= EditedClip.EditPos 
                && note.SongStep <  EditedClip.EditPos + EditedClip.EditStepLength)
                return T;

            return F;
        }
        
        
        bool NoteIsTriggered(int noteNum, Note note)
        {
            var timeStep = IsPlaying ? PlayStep : TimeStep;

            return
                   noteNum == note.Number
                && timeStep >= note.Step
                && timeStep <  note.Step + note.StepLength;
        }


        bool ToggleIsBright(Label lbl)
        {
            return
                   lbl.Data == 2 && EditedClip.RndInst
                || lbl.Data == 3 && EditedClip.AllChan
                || lbl.Data == 4 && EditedClip.Pick;
        }


        void UpdatePianoToggle(Label lbl)
        {
            if (ShowPiano)
            {
                lbl.SetText(
                    //     ║  ███  ║       ║  ███
                    //     ║       ║       ║     
                    //═════╬═══════╬═══════╬═════
                    //     ║       ║       ║     
                    // ███ ║  ███  ║  ███  ║  ███
                    lblHigh[10].Panel.CustomData,
                    1.7f,
                    17);
            }
            else
            {
                lbl.SetText(
                    //█ █ ██ █ █ █
                    //█▄█▄██▄█▄█▄█
                    //▀▀▀▀▀▀▀▀▀▀▀▀
                    lblShuffle.Panel.CustomData,
                    3.7f,
                    10);
            }
        }


        bool StepIsBright(Label lbl)
        {
            var patStep  = -lbl.Data;
            var songStep =  CurPat * g_patSteps + patStep;

            var on = OK(CurChannel.Notes.Find(n => 
                   n.Step >= patStep
                && n.Step <  patStep+1));

            if (   IsPlaying
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

                 if (EditedClip.Spread) val = EditedClip.ChordSpread;
            else if (ShowPiano)      val = CurChannel.Transpose;
            else                     val = CurChannel.Shuffle;

            lbl.SetText((val == 15 ? "+" : " ") + S(val));
        }


        void UpdateShuffleLabel(Label lbl)
        {
            if (EditedClip.Spread)
                lbl.SetText("Sprd");

            else if (ShowPiano)
                lbl.SetText(
                    //█ █ ██ █ █ █
                    //█▄█▄██▄█▄█▄█
                    //▀▀▀▀▀▀▀▀▀▀▀▀
                    lblShuffle.Panel.CustomData,
                    3.7f,
                    10);

            else
                lbl.SetText("Shuf");
        }
    }
}
