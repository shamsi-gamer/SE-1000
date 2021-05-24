﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static string GetChordName(List<int> chord, string other)
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

            if (    EditClip.Chord > -1
                && !IsCurParam(strTune))
            {
                var chord = EditClip.Chords[EditClip.Chord];

                chord.Sort();

                for (int i = 0; i < chord.Count; i++)
                    chordNotes.Add(note + chord[i] - chord[0]);
            }
            else
                chordNotes.Add(note);

            return chordNotes;
        }


        static bool IsChord(List<int> chord, int n2, int n3)
        {
            var c = new List<int>(chord);

            for (int i = 0; i < c.Count; i++)
                while (c[i] >= 12*NoteScale) c[i] %= 12*NoteScale;
                
            c = c.Select(n => n).Distinct().ToList();
            if (c.Count != 3) return F;
            c.Sort();

            if (      c[1]-c[0] ==  n2          * NoteScale
                   && c[2]-c[0] ==  n3          * NoteScale
                ||    c[1]-c[0] == (n3-n2)      * NoteScale
                   && c[2]-c[0] == (12-n2)      * NoteScale
                ||    c[1]-c[0] == (12-n3)      * NoteScale
                   && c[2]-c[0] == (12-(n3-n2)) * NoteScale)
                return T;
         
            return F;
        }


        static bool IsChord(List<int> chord, int n2, int n3, int n4)
        {
            var c = new List<int>(chord);

            for (int i = 0; i < c.Count; i++)
                while (c[i] >= 12*NoteScale) c[i] %= 12*NoteScale;
                
            c = c.Select(n => n).Distinct().ToList();
            if (c.Count != 4) return F;
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
                return T;
         
            return F;
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
            if (EditClip.ChordEdit)
                return;

            if (IsCurParam(strTune))
            {
                var tune = SelSource    ?.Tune
                        ?? SelInstrument?.Tune;

                tune.UseChord = !tune.UseChord;
            }
            else
            {
                EditClip.ChordMode = !EditClip.ChordMode;

                if (!EditClip.ChordMode)
                    EditClip.Chord = -1;
            }
        }


        // return steps
        float ChordSpread(int n)
        {
            return (float)(EditClip.ChordSpread*Math.Pow(n, 1.33) / g_session.TicksPerStep);        
        }


        void Chord(int chord)
        {
            var tune = SelSource    ?.Tune
                    ?? SelInstrument?.Tune;


            if (   IsCurParam(strTune)
                && tune.UseChord)
            {
                var _chord = EditClip.Chords[chord-1];

                var tc = tune.Chord;

                bool add = F;
                foreach (var _note in _chord)
                {
                    if (tc.FindIndex(n => n == _note) < 0)
                    {
                        add = T;
                        break;
                    }
                }

                if (add)
                {
                    foreach (var n in _chord)
                        if (!tc.Contains(n)) tc.Add(n);
                }
                else
                {
                    foreach (var n in _chord)
                        if (tc.Contains(n)) tc.Remove(n);
                }

                var inst = SelInstrument;
                var src  = CurSrc > -1 ? inst.Sources[CurSrc] : null;

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);

                //MarkChordLabel(chord);
            }
            else if (EditClip.ChordEdit)
            {
                EditClip.Chord = 
                    EditClip.Chord != chord-1
                    ? chord-1
                    : -1;
            }
            else if (EditClip.ChordMode)
            {
                if (EditClip.Chord != chord-1)
                {
                    if (EditClip.Chords[chord-1].Count > 0)
                    { 
                        EditClip.Chord    = chord-1;
                        EditClip.ChordAll = F;
                    }
                }
                else
                    EditClip.Chord = -1;
            }
            else
            {
                EditClip.Chord = chord-1;
                var _chord = EditClip.Chords[EditClip.Chord];

                if (_chord.Count > 0)
                {
                    _chord.Sort();
                    PlayNote(EditClip, _chord[0], _chord, CurChan);
                }

                //MarkChordLabel(chord);            

                EditClip.Chord = -1;
            }
        }


        void ToggleChordEdit()
        {
            if (IsCurParam(strTune))
            {
                var inst = SelInstrument;
                var src  = SelSource;

                var tune = SelSource    ?.Tune
                        ?? SelInstrument?.Tune;

                if (tune.UseChord)
                { 
                    tune.AllOctaves = !tune.AllOctaves;
                    tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
                }
            }
            else if (EditClip.ChordMode)
            {
                EditClip.ChordAll = !EditClip.ChordAll;
                //if (g_chordAll) EditClip.Chord = -1;
            }
            else
            { 
                EditClip.ChordEdit = !EditClip.ChordEdit;
                if (!EditClip.ChordEdit) EditClip.Chord = -1;
            }
        }


        void EditChord(int noteNum)
        {
            var chord = EditClip.Chords[EditClip.Chord];

            if (chord.Contains(noteNum)) chord.Remove(noteNum);
            else                         chord.Add   (noteNum);

            if (chord.Count > 0)
            {
                chord.Sort();

                var oldIndex = EditClip.EditLengthIndex;
                EditClip.EditLengthIndex = Math.Min(EditClip.EditLengthIndex, g_steps.Length-2);
                PlayNote(EditClip, chord[0], chord, CurChan);
                EditClip.EditLengthIndex = oldIndex;
            }
        }


        void UpdateFinalTuneChord(Tune tune, int noteNum)
        {
            var chord = tune.Chord;

            if (chord.Contains(noteNum)) chord.Remove(noteNum);
            else                         chord.Add   (noteNum);

            tune.FinalChord = UpdateFinalTuneChord(chord, tune.AllOctaves);
        }


        List<int> UpdateFinalTuneChord(List<int> _tuneChord, bool tuneAll)
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
                    while (tuneChord[i] >= firstNote + 24)
                        tuneChord[i] -= 24;
                }

                tuneChord = tuneChord.Distinct().ToList();


                finalTuneChord = new List<int>();

                foreach (var note in tuneChord)
                {
                    finalTuneChord.Add(note);

                    int n = note;
                    while ((n -= 24) >= 24)
                        finalTuneChord.Add(n);

                    n = note;
                    while ((n += 24) < 300)
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
