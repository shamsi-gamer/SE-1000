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

            if (    g_clip.Chord > -1
                && !IsCurParam(strTune))
            {
                var chord = g_clip.Chords[g_clip.Chord];

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
            if (c.Count != 3) return false;
            c.Sort();

            if (      c[1]-c[0] ==  n2          * NoteScale
                   && c[2]-c[0] ==  n3          * NoteScale
                ||    c[1]-c[0] == (n3-n2)      * NoteScale
                   && c[2]-c[0] == (12-n2)      * NoteScale
                ||    c[1]-c[0] == (12-n3)      * NoteScale
                   && c[2]-c[0] == (12-(n3-n2)) * NoteScale)
                return true;
         
            return false;
        }


        static bool IsChord(List<int> chord, int n2, int n3, int n4)
        {
            var c = new List<int>(chord);

            for (int i = 0; i < c.Count; i++)
                while (c[i] >= 12*NoteScale) c[i] %= 12*NoteScale;
                
            c = c.Select(n => n).Distinct().ToList();
            if (c.Count != 4) return false;
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
                return true;
         
            return false;
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
            if (g_clip.ChordEdit)
                return;

            if (IsCurParam(strTune))
            {
                var tune = g_clip.SelectedSource    ?.Tune
                        ?? g_clip.SelectedInstrument?.Tune;

                tune.UseChord = !tune.UseChord;
            }
            else
            {
                g_clip.ChordMode = !g_clip.ChordMode;

                if (!g_clip.ChordMode)
                    g_clip.Chord = -1;
            }
            
            UpdateChordLights();
            UpdateKeyLights();
            UpdateShuffleLight();
            UpdateOctaveLight();
        }


        // return steps
        float ChordSpread(int n)
        {
            return (float)(g_clip.ChordSpread*Math.Pow(n, 1.33) / g_ticksPerStep);        
        }


        void Chord(int chord)
        {
            var tune = g_clip.SelectedSource    ?.Tune
                    ?? g_clip.SelectedInstrument?.Tune;


            if (   IsCurParam(strTune)
                && tune.UseChord)
            {
                var _chord = g_clip.Chords[chord-1];

                var tc = tune.Chord;

                bool add = false;
                foreach (var _note in _chord)
                {
                    if (tc.FindIndex(n => n == _note) < 0)
                    {
                        add = true;
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

                var inst = g_clip.SelectedInstrument;
                var src  = g_clip.CurSrc > -1 ? inst.Sources[g_clip.CurSrc] : null;

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);

                MarkChordLight(chord);
            }
            else if (g_clip.ChordEdit)
            {
                g_clip.Chord = 
                    g_clip.Chord != chord-1
                    ? chord-1
                    : -1;
            }
            else if (g_clip.ChordMode)
            {
                if (g_clip.Chord != chord-1)
                {
                    if (g_clip.Chords[chord-1].Count > 0)
                    { 
                        g_clip.Chord    = chord-1;
                        g_clip.ChordAll = false;
                    }
                }
                else
                    g_clip.Chord = -1;
            }
            else
            {
                g_clip.Chord = chord-1;
                var _chord = g_clip.Chords[g_clip.Chord];

                if (_chord.Count > 0)
                {
                    _chord.Sort();
                    PlayNote(g_clip, _chord[0], _chord, g_clip.CurChan);
                }

                MarkChordLight(chord);            

                g_clip.Chord = -1;
            }

            UpdateChordLights();
            UpdateShuffleLight();
        }


        void ToggleChordEdit()
        {
            if (IsCurParam(strTune))
            {
                var inst = g_clip.SelectedInstrument;
                var src  = g_clip.SelectedSource;

                var tune = g_clip.SelectedSource    ?.Tune
                        ?? g_clip.SelectedInstrument?.Tune;

                if (tune.UseChord)
                { 
                    tune.AllOctaves = !tune.AllOctaves;
                    tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
                }
            }
            else if (g_clip.ChordMode)
            {
                g_clip.ChordAll = !g_clip.ChordAll;
                //if (g_chordAll) g_clip.Chord = -1;
            }
            else
            { 
                g_clip.ChordEdit = !g_clip.ChordEdit;
                if (!g_clip.ChordEdit) g_clip.Chord = -1;
            }

            UpdateChordLights();
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
