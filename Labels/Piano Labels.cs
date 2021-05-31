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

            lblOctave     = new Label(GetLabel("Octave"),  CF_null, CF_null, UpdateOctaveLabel);
            lblShuffle    = new Label(GetLabel("Shuffle"), CF_null, CF_null, UpdateShuffleLabel);
            lblOctaveUp   = new Label(GetLabel("Octave Up"));
            lblOctaveDown = new Label(GetLabel("Octave Down"));
        }


        void InitPianoLabelsHigh()
        {
            lblHigh = new List<Label>();

            var high = new List<IMyTextPanel>();
            Get(high, l => l.CustomName.Length >= 11 && l.CustomName.Substring(0, 11) == "Label High ");
            high = high.OrderBy(l => int_Parse(l.CustomName.Substring(11))).ToList();

            for (int h = 0; h < 10; h++)
            { 
                lblHigh.Add(new Label(
                    high[h], 
                    PianoHighIsBright, 
                    PianoHighIsDim, 
                    UpdatePianoHigh, 
                    UpdatePianoHighColor, 
                    h,
                    True));
            }

            lblHigh.Add(new Label(high[10],
                lbl => IsPressed(lbl),
                CF_null,
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
                lbl.SetText(strEmpty);

            else
            { 
                switch (lbl.Data)
                { 
                case 0: lbl.SetText("Clr");          break;
                case 1: lbl.SetText("Rnd");          break;
                                                                
                case 2: lbl.SetText("Inst");         break;
                case 3: lbl.SetText("All Ch", 7.6f, 19.5f); break;
                case 4: lbl.SetText("Pick");         break;
                                                        
                case 5: lbl.SetText("Copy",  9, 14); break;
                case 6: lbl.SetText("Paste", 9, 14); break;
                                                        
                case 7: lbl.SetText("1/4");          break;
                case 8: lbl.SetText("1/8");          break;
                case 9: lbl.SetText("Flip");         break;
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
            low = low.OrderBy(l => int_Parse(l.CustomName.Substring(10))).ToList();

            for (int l = 0; l < low.Count; l++)
            { 
                lblLow.Add(new Label(
                    low[l], 
                    PianoLowIsBright, 
                    PianoLowIsDim, 
                    UpdatePianoLow, 
                    UpdatePianoLowColor, 
                    -l,
                    True));
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
                else                 lbl.SetText(strEmpty);
            } 
            else                     lbl.SetText(strEmpty);
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
                    OK(CurSrc)
                    ? SelSource    .Tune
                    : SelInstrument.Tune;

                return tune.Chord.Contains(noteNum);
            }

            else if (OK(EditedClip.Chord)
                  && EditedClip.ChordEdit)
                return EditedClip.Chords[EditedClip.Chord].Contains(noteNum);

            else if (Playing 
                  && OK(EditedClip.Track.PlayPat)
                  && OK(PlayChannel.Notes.FindIndex(n => NoteIsPlaying(noteNum, n))))
                return True; // note is in the currently played channel

            else if (OK(g_notes.FindIndex(n => NoteIsTriggered(noteNum, n))))
                return True; // note is being played on the piano

            return False;
        }


        bool NoteIsDim(int noteNum)
        {
            if (IsCurParam(strTune))
            {
                var tune =
                    OK(CurSrc)
                    ? SelSource    .Tune
                    : SelInstrument.Tune;

                return tune.FinalChord.Contains(noteNum);
            }
            else if (OK(EditedClip.Track.PlayPat))
            { 
                for (int ch = 0; ch < g_nChans; ch++)
                {
                    if (ch == CurChan)
                        continue;

                    if (   Playing
                        && OK(PlayPattern.Channels[ch].Notes.Find(n => NoteIsPlaying(noteNum, n))))
                        return True;
                }
            }

            return False;
        }


        bool NoteIsPlaying(int noteNum, Note note)
        {
            if (  !EditedClipIsPlaying
                || noteNum != note.Number)
                return False;

            var track = EditedClip.Track;

            // note is at the playback position
            if (   track.PlayStep >= note.SongStep + note.ShOffset
                && track.PlayStep <  note.SongStep + note.ShOffset + note.StepLength)
                return True;

            // note is at edit position
            if (   note.SongStep >= EditedClip.EditPos 
                && note.SongStep <  EditedClip.EditPos + EditedClip.EditStepLength)
                return True;

            return False;
        }
        
        
        bool NoteIsTriggered(int noteNum, Note note)
        {
            var timeStep = Playing ? EditedClip.Track.PlayStep : TimeStep;

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

            var track = EditedClip.Track;

            if (   Playing
                && EditedClipIsPlaying
                && (int)track.PlayStep  == songStep
                && CurPat == track.PlayPat)
                return !on;
            else if (on)
                return True;

            return False;
        }


        void UpdateOctaveLabel(Label lbl)
        {
            if (TooComplex) return;

            int val;

                 if (EditedClip.Spread) val = EditedClip.ChordSpread;
            else if (ShowPiano)      val = CurChannel.Transpose;
            else                     val = CurChannel.Shuffle;

            lbl.SetText((val == 15 ? "+" : strEmpty) + S(val));
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
