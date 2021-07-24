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

            lblOctave     = new Label(1, GetLabel("Octave"),      CF_null, CF_null, UpdateOctaveLabel);
            lblShuffle    = new Label(1, GetLabel("Shuffle"),     CF_null, CF_null, UpdateShuffleLabel);
            lblOctaveUp   = new Label(1, GetLabel("Octave Up"),   CF_null, CF_null, lbl => lbl.SetText("►"));
            lblOctaveDown = new Label(1, GetLabel("Octave Down"), CF_null, CF_null, lbl => lbl.SetText("◄"));
        }



        void InitPianoLabelsHigh()
        {
            lblHigh = new List<Label>();

            var high = new List<IMyTextPanel>();
            Get(high, l => l.CustomName.Length >= 11 && l.CustomName.Substring(0, 11) == "Label High ");
            high = high.OrderBy(l => int_Parse(l.CustomName.Substring(11))).ToList();

            for (int h = 0; h < 10; h++)
            { 
                lblHigh.Add(new Label(3, high[h], 
                    PianoHighIsBright, 
                    PianoHighIsDim, 
                    UpdatePianoHigh, 
                    UpdatePianoHighColor, 
                    h,
                    True));
            }

            lblHigh.Add(new Label(3, high[10],
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
                   && NoteIsDim(HighToNote(lbl.Data))
                ||   !ShowPiano
                   && g_copyChans.Count > 0
                   && (lbl == lblHigh[0] || lbl == lblHigh[1]);
        }



        void UpdatePianoHigh(Label lbl)
        {
            if (ShowPiano)
                lbl.SetText(strEmpty);

            else
            { 
                switch (lbl.Data)
                { 
                case 0: lbl.SetText("Copy",  9, 14); break;
                case 1: lbl.SetText("Paste", 9, 14); break;
                                                        
                case 2: lbl.SetText("Pick");         break;
                case 3: lbl.SetText("All Ch", 7.6f, 19.5f); break;
                case 4: lbl.SetText("Inst");         break;
                                                        
                case 5: lbl.SetText("Clr");          break;
                case 6: lbl.SetText("Rnd");          break;
                                                                
                case 7: lbl.SetText(ShowPianoView ? " "   : "/1"); break;
                case 8: lbl.SetText(ShowPianoView ? "Rev" : "/2"); break;
                case 9: lbl.SetText("Flip");         break;
                }
            }
        }


        void UpdatePianoHighColor(Label lbl)
        {
            lbl.BackColor = 
                ShowPiano 
                ? color1 
                : color0;
        }



        void InitPianoLabelsLow()
        {
            lblLow = new List<Label>();

            var low = new List<IMyTextPanel>();
            Get(low, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Low ");
            low = low.OrderBy(l => int_Parse(l.CustomName.Substring(10))).ToList();

            for (int l = 0; l < low.Count; l++)
            { 
                lblLow.Add(new Label(3, low[l], 
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
                ?      -lbl.Data == 15 
                     && EditedClip.HalfSharp
                  ||   -lbl.Data < 15
                     && NoteIsBright(LowToNote(-lbl.Data))
                : StepIsBright(lbl);
        }



        bool PianoLowIsDim(Label lbl)
        {
            return 
                   ShowPiano 
                && -lbl.Data < 15
                && NoteIsDim(LowToNote(-lbl.Data));
        }



        void UpdatePianoLow(Label lbl)
        {
            if (    ShowPiano
                && -lbl.Data == 15) lbl.SetText("‡", 8, 17); 
            else                    lbl.SetText(strEmpty);
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

            else if (OK(EditedClip.EditPos)
                  && OK(CurChannel.Notes.FindIndex(n => NoteIsEdited(n, noteNum))))
                return True; // note is being edited

            else if (OK(g_notes.FindIndex(n => 
                           NoteIsTriggered(noteNum, n)
                        && n.Clip  == EditedClip
                        && n.iChan == CurChan)))
                return True; // note is being played

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
            else if (OK(g_notes.FindIndex(n => NoteIsTriggered(noteNum, n))))
                return True; // note is being played

            //else if (OK(EditedClip.Track.PlayPat))
            //{ 
            //    for (int ch = 0; ch < g_nChans; ch++)
            //    {
            //        if (ch == CurChan)
            //            continue;

            //        if (   Playing
            //            && OK(PlayPattern.Channels[ch].Notes.Find(n => NoteIsEdited(noteNum, n))))
            //            return True;
            //    }
            //}

            return False;
        }



        bool NoteIsEdited(Note note, int noteNum)
        {
            return note.Number == noteNum
                && note.ClipStep >= EditedClip.EditPos
                && note.ClipStep <  EditedClip.EditPos + EditedClip.EditStepLength;
        }



        bool NoteIsTriggered(int noteNum, Note note)
        {
            var timeStep = TimeStep;
                //Playing 
                //? EditedClip.Track.StartStep + EditedClip.Track.PlayStep
                //: TimeStep;

            return
                   noteNum == note.Number
                && timeStep >= note.Step
                && timeStep <  note.Step + note.StepLength;
        }



        bool ToggleIsBright(Label lbl)
        {
            return
                   lbl.Data == 2 && EditedClip.Pick
                || lbl.Data == 3 && EditedClip.AllChan
                || lbl.Data == 4 && EditedClip.RndInst;
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
            var clipStep =  EditPat * g_patSteps + patStep;

            var on = OK(CurChannel.Notes.Find(n => 
                   n.Step >= patStep
                && n.Step <  patStep+1));

            var track = EditedClip.Track;

            if (   Playing
                && EditedClipIsPlaying
                && (int)track.PlayStep  == clipStep
                && EditPat == track.PlayPat)
                return !on;
            else if (on)
                return True;

            return False;
        }



        void UpdateOctaveLabel(Label lbl)
        {
            int val;

                 if (EditedClip.Strum) val = EditedClip.ChordStrum;
            else if (ShowPianoView)    val = CurChannel.Transpose;
            else                       val = CurChannel.Shuffle;

            lbl.SetText((val > 0 ? "+" : "") + S(val));
        }



        void UpdateShuffleLabel(Label lbl)
        {
            if (EditedClip.Strum)
                lbl.SetText("Strum", 9, 14);

            else if (ShowPianoView)
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
