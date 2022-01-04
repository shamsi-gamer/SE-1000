using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;



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

            for (int h = 0; h < 11; h++)
            { 
                lblHigh.Add(new Label(3, high[h], 
                    PianoHighIsBright, 
                    PianoHighIsDim, 
                    UpdatePianoHigh, 
                    UpdatePianoHighColor, 
                    h,
                    True));
            }

            lblHigh.Add(new Label(3, high[11],
                lbl => IsPressed(lbl),
                CF_null,
                UpdatePianoToggle));
        }



        bool PianoHighIsBright(Label lbl)
        {
            if (ShowPianoForToggle)
            { 
                if (lbl.Data < 10)
                { 
                    bool halfSharp;
                    var bright = NoteIsBright(HighToNote(lbl.Data, EditedClip.HalfSharp), out halfSharp);

                    lbl.Data2 = halfSharp ? 1 : 0;

                    return bright;
                }
             
                else if (lbl.Data == 10
                      && IsCurSetting(strChord))
                    return ((TuneChord)CurSetting).SelAllOctaves;

                else return False;
            }
            
            else
                return ToggleIsBright(lbl);
        }



        bool PianoHighIsDim(Label lbl)
        {
            return
                      ShowPianoForToggle
                   && (lbl.Data < 10 && NoteIsDim(HighToNote(lbl.Data, EditedClip.HalfSharp)))
                ||   !ShowPianoForToggle
                   && g_copyChans.Count > 0
                   && (lbl == lblHigh[0] || lbl == lblHigh[1]);
        }



        void UpdatePianoHigh(Label lbl)
        {
            if (ShowPianoForToggle)
            {
                if (lbl.Data < 10)
                { 
                    var halfSharp = 
                           !EditedClip.HalfSharp && lbl.Data2 != 0
                        ||  EditedClip.HalfSharp && lbl.Data2 == 0;

                    lbl.SetText(halfSharp ? "‡" : strEmpty, 6, 26);
                }

                else if (lbl.Data == 10
                      && IsCurSetting(strChord)
                      && !(EditedClip.ParamKeys || EditedClip.ParamAuto))
                    lbl.SetText(strAll); 

                else if (lbl.Data == 10)
                    lbl.SetText(strEmpty);
            }
            else
            { 
                switch (lbl.Data)
                { 
                case  0: lbl.SetText("Copy",  9, 14);                   break;
                case  1: lbl.SetText("Paste", 9, 14);                   break;
                                                                           
                case  2: lbl.SetText("Acc");                            break;
                case  3: lbl.SetText("All Ch", 7.6f, 19.5f);            break;
                case  4: lbl.SetText("Inst");                           break;
                                                                           
                case  5: lbl.SetText("Clr");                            break;
                case  6: lbl.SetText("Rnd");                            break;
                                                                 
                case  7: lbl.SetText(ShowPianoView ? strEmpty : "1/4"); break;
                case  8: lbl.SetText(ShowPianoView ? "Rev"    : "1/8"); break;
                case  9: lbl.SetText("Flip");                           break;
                                                                        
                case 10: lbl.SetText("Pick");                           break;
                }
            }
        }



        void UpdatePianoHighColor(Label lbl)
        {
            lbl.BackColor =
                   ShowPianoForToggle
                && lbl.Data < 10
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
            var halfSharp = False;

            var bright =
                ShowPianoForToggle
                ?    -lbl.Data == 15 && EditedClip.HalfSharp
                  || -lbl.Data <  15 && NoteIsBright(LowToNote(-lbl.Data, EditedClip.HalfSharp), out halfSharp)
                : StepIsBright(lbl);

            lbl.Data2 = halfSharp ? 1 : 0;

            return bright;
        }



        bool PianoLowIsDim(Label lbl)
        {
            return
                   ShowPianoForToggle
                && -lbl.Data < 15
                && NoteIsDim(LowToNote(-lbl.Data, EditedClip.HalfSharp));
        }



        void UpdatePianoLow(Label lbl)
        {
            if (    ShowPianoForToggle
                && -lbl.Data == 15)
                lbl.SetText("‡", 8, 17); 

            else if (ShowPianoForToggle
                 && -lbl.Data < 15)
            {
                var _halfSharp = lbl.Data2 != 0;

                var halfSharp = 
                       !EditedClip.HalfSharp &&  _halfSharp
                    ||  EditedClip.HalfSharp && !_halfSharp;

                lbl.SetText(halfSharp ? "‡" : strEmpty, 6, 26);
            }

            else
            { 
                lbl.SetText(
                      !ShowPiano 
                    && StepIsBright(lbl, True) 
                    ? "●" 
                    : strEmpty);
            }
        }



        void UpdatePianoLowColor(Label lbl)
        {
            if (ShowPianoForToggle)
                lbl.BackColor = -lbl.Data < 15 ? color1 : color0;

            else
                lbl.BackColor = 
                    (-lbl.Data/4) % 2 == 0 
                    ? color2 
                    : color0;
        }



        bool NoteIsBright(int noteNum, out bool halfSharp)
        {
            // noteNum can either be half sharp or not, it gets compared 
            // for both either way, and an answer is sent back

            halfSharp = False;

            var note = Note_null;

            if (IsCurParam(strChord))
            {
                var chord = ((TuneChord)CurSetting).SelChord;
                return chord.Contains(noteNum);
            }

            else if (OK(EditedClip.Chord)
                  && EditedClip.ChordEdit)
                return EditedClip.Chords[EditedClip.Chord].Contains(noteNum);

            else if (OK(EditedClip.EditPos)
                  && OK(CurChannel.Notes.FindIndex(n => NoteIsEdited(n, noteNum))))
                return True; // note is being edited

            else if (OK(g_notes.FindIndex(n => 
                           NoteIsTriggered(noteNum, n, out note)
                        && n.Clip  == EditedClip
                        && n.iChan == CurChan)))
            {
                halfSharp = (note.Number % NoteScale > 0) ^ EditedClip.HalfSharp;
                return True; // note is being played
            }


            return False;
        }



        bool NoteIsDim(int noteNum)
        {
            var note = Note_null;

            if (IsCurParam(strChord))
            {
                var tc = (TuneChord)CurSetting;
                return tc.SelFinalChord.Contains(noteNum);
            }
            else if (OK(g_notes.FindIndex(n => NoteIsTriggered(noteNum, n, out note))))
                return True; // note is being played

            return False;
        }



        bool StepIsBright(Label lbl, bool accent = False)
        {
            var patStep  = -lbl.Data;
            var clipStep =  EditPat * g_patSteps + patStep;

            var on = OK(CurChannel.Notes.Find(n => 
                   n.Step >= patStep
                && n.Step <  patStep+1
                && (!accent || n.Accent)));


            var track = EditedClip.Track;

            if (   Playing
                && EditedClipIsPlaying
                && (int)track.PlayStep  == clipStep
                && EditPat == track.PlayPat
                && !accent)
                return !on;
            else if (on)
                return True;

            return False;
        }



        bool NoteIsEdited(Note note, int noteNum)
        {
            return note.Number == noteNum
                && note.ClipStep >= EditedClip.EditPos
                && note.ClipStep <  EditedClip.EditPos + EditedClip.EditStepLength;
        }



        bool NoteIsTriggered(int noteNum, Note note, out Note _note)
        {
            _note = note;

            return
                   noteNum/NoteScale == note.Number/NoteScale
                && TimeStep >= note.Step
                && TimeStep <  note.Step + note.StepLength;
        }



        bool ToggleIsBright(Label lbl)
        {
            return
                   lbl.Data ==  2 && EditedClip.Accent
                || lbl.Data ==  3 && EditedClip.AllChan
                || lbl.Data ==  4 && EditedClip.UseInst
                || lbl.Data == 10 && EditedClip.Pick;
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
                    lblHigh[11].Panel.CustomData,
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



        void UpdateOctaveLabel(Label lbl)
        {
            int val;
            var suf = "";

                 if (EditedClip.Accent) { val = (int)Math.Round((CurChannel.AccentScale - 1) * 100); suf = "%"; }
            else if (EditedClip.Strum)    val = EditedClip.ChordStrum;
            else if (ShowPiano)           val = CurChannel.Transpose;
            else                          val = CurChannel.Shuffle;

            lbl.SetText(
                  (val > 0 ? "+" : "") 
                + S(val) 
                + suf);
        }



        void UpdateShuffleLabel(Label lbl)
        {
            if (EditedClip.Accent)
                lbl.SetText("Acc");

            else if (EditedClip.Strum)
                lbl.SetText("Strum", 9, 14);

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
