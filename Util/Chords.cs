using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        static string GetChordName(int[] chord, string other)
        {
                 if (IsChord(chord, 4, 8))     return "+";  // augmented
            else if (IsChord(chord, 4, 7))     return "M";  // major
            else if (IsChord(chord, 3, 7))     return "m";  // minor
            else if (IsChord(chord, 3, 6))     return "o";  // diminished

            else if (IsChord(chord, 4, 7, 11)) return "M7"; // major 7th
            else if (IsChord(chord, 4, 7, 10)) return "7";  // dominant 7th
            else if (IsChord(chord, 3, 7, 10)) return "m7"; // minor 7th
            else if (IsChord(chord, 3, 6, 10)) return "ø7"; // half-diminished 7th
            else if (IsChord(chord, 3, 6,  9)) return "o7"; // fully diminished 7th

            else                               return other; 
        }



        List<int> GetChordNotes(int note)
        {
            var chordNotes = new List<int>();

            if (    OK(EditedClip.Chord)
                && !HasTagOrParent(CurSetting, strTune))
            {
                var chord = EditedClip.Chords[EditedClip.Chord];

                chord.Sort();

                for (int i = 0; i < chord.Count; i++)
                    chordNotes.Add(note + chord[i] - chord[0]);
            }
            else
                chordNotes.Add(note);

            return chordNotes;
        }


        static bool IsChord(int[] chord, int n2, int n3)
        {
            var c = new List<int>(chord);

            for (int i = 0; i < c.Count; i++)
                while (c[i] >= 12*NoteScale) c[i] %= 12*NoteScale;
                
            c = c.Select(n => n).Distinct().ToList();
            if (c.Count != 3) return False;
            c.Sort();

            if (      c[1]-c[0] ==  n2          * NoteScale
                   && c[2]-c[0] ==  n3          * NoteScale
                ||    c[1]-c[0] == (n3-n2)      * NoteScale
                   && c[2]-c[0] == (12-n2)      * NoteScale
                ||    c[1]-c[0] == (12-n3)      * NoteScale
                   && c[2]-c[0] == (12-(n3-n2)) * NoteScale)
                return True;
         
            return False;
        }



        static bool IsChord(int[] chord, int n2, int n3, int n4)
        {
            var c = new List<int>(chord);

            for (int i = 0; i < c.Count; i++)
                while (c[i] >= 12*NoteScale) c[i] %= 12*NoteScale;
                
            c = c.Select(n => n).Distinct().ToList();
            if (c.Count != 4) return False;
            c.Sort();

            if (      c[1]-c[0] ==  n2        * NoteScale
                   && c[2]-c[0] ==  n3        * NoteScale
                   && c[3]-c[0] ==  n4        * NoteScale
                ||    c[1]-c[0] == (n3-n2)    * NoteScale
                   && c[2]-c[0] == (n4-n2)    * NoteScale
                   && c[3]-c[0] == (12-n2)    * NoteScale
                ||    c[1]-c[0] == (n4-n3)    * NoteScale
                   && c[2]-c[0] == (12-n3)    * NoteScale
                   && c[3]-c[0] == (12-n2)    * NoteScale
                ||    c[1]-c[0] == (12-n4)    * NoteScale
                   && c[2]-c[0] == (12-n4+n2) * NoteScale
                   && c[3]-c[0] == (12-n4+n3) * NoteScale)
                return True;
         
            return False;
        }



        static int LimitNoteToChord(int note, List<int> chord)
        {
            chord.Sort();

            int index = 0;

            if (note < chord[0])
                index = 0;
            else if (note >= chord.Last())
                index = chord.Count-1;
            else
            {
                for (int i = 0; i < chord.Count-1; i++)
                {
                    if (   note <  chord[i  ]
                        || note >= chord[i+1])
                        continue;

                    if (  Math.Abs(note - chord[i  ]) 
                        < Math.Abs(note - chord[i+1]))
                        index = i;
                    else
                        index = i+1;

                    break;
                }    
            }

            return chord[index];
        }



        void ToggleChordMode()
        {
            if (EditedClip.ChordEdit)
                return;

            EditedClip.ChordMode = !EditedClip.ChordMode;

            if (!EditedClip.ChordMode)
                EditedClip.Chord = -1;
        }



        // return steps
        float ChordStrum(int n)
        {
            return (float)(EditedClip.ChordStrum*Math.Pow(n, 1.33) / TicksPerStep);        
        }



        void Chord(int chord)
        {
            if (IsCurSetting(strChord))
            {
                var tc = (TuneChord)CurSetting;

                var _chord = EditedClip.Chords[chord-1];

                if (_chord.Count > 0)
                {
                    tc.SelChord.Clear();

                    foreach (var n in _chord)
                        tc.SelChord.Add(n);

                    var inst = SelInstrument;
                    var src  = OK(CurSrc) ? inst.Sources[CurSrc] : Source_null;

                    tc.UpdateFinalChord();
                }

                MarkChordLabel(chord-1);
            }
            else if (EditedClip.ChordEdit)
            {
                EditedClip.Chord = 
                    EditedClip.Chord != chord-1
                    ? chord-1
                    : -1;
            }
            else if (EditedClip.ChordMode)
            {
                if (EditedClip.Chord != chord-1)
                {
                    if (EditedClip.Chords[chord-1].Count > 0)
                    { 
                        EditedClip.Chord    = chord-1;
                        EditedClip.ChordAll = False;
                    }
                }
                else
                    EditedClip.Chord = -1;
            }
            else
            {
                EditedClip.Chord = chord-1;
                var _chord = EditedClip.Chords[EditedClip.Chord];

                if (_chord.Count > 0)
                {
                    _chord.Sort();
                    PlayNote(EditedClip, _chord[0], CurChan);
                }

                MarkChordLabel(chord-1);            

                EditedClip.Chord = -1;
            }
        }



        void ToggleChordEdit()
        {
            if (!EditedClip.ChordMode)
            { 
                EditedClip.ChordEdit = !EditedClip.ChordEdit;
                if (!EditedClip.ChordEdit) EditedClip.Chord = -1;
            }
        }



        void EditChord(int noteNum)
        {
            var chord = EditedClip.Chords[EditedClip.Chord];

            if (chord.Contains(noteNum)) chord.Remove(noteNum);
            else                         chord.Add   (noteNum);

            if (chord.Count > 0)
            {
                chord.Sort();

                var oldIndex = EditedClip.EditLengthIndex;
                EditedClip.EditLengthIndex = Math.Min(EditedClip.EditLengthIndex, g_steps.Length-2);
                PlayNote(EditedClip, chord[0], CurChan);
                EditedClip.EditLengthIndex = oldIndex;
            }
        }



        void UpdateTuneChord(TuneChord tc, int noteNum)
        {
            var chord = tc.SelChord;

            if (chord.Contains(noteNum)) chord.Remove(noteNum);
            else                         chord.Add   (noteNum);

            tc.UpdateFinalChord();
        }



        static List<int> UpdateFinalTuneChord(List<int> _tuneChord, bool tuneAll)
        {
            List<int> finalTuneChord;

            if (    tuneAll
                && _tuneChord.Count > 0)
            {
                var tuneChord = new List<int>(_tuneChord);
                tuneChord.Sort();

                var firstNote = tuneChord[0];

                for (int i = 0; i < tuneChord.Count; i++)
                {
                    while (tuneChord[i] >= firstNote + 12*NoteScale)
                        tuneChord[i] -= 12*NoteScale;
                }

                tuneChord = tuneChord.Distinct().ToList();


                finalTuneChord = new List<int>();

                foreach (var note in tuneChord)
                {
                    finalTuneChord.Add(note);

                    int n = note;
                    while ((n -= 12*NoteScale) >= 12*NoteScale)
                        finalTuneChord.Add(n);

                    n = note;
                    while ((n += 12*NoteScale) < 150*NoteScale)
                        finalTuneChord.Add(n);
                }
            }
            else
            {
                finalTuneChord = new List<int>(_tuneChord);
            }

            return finalTuneChord;
        }
    }
}
