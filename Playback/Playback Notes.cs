using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void ToggleNotes(Clip clip, List<int> notes, int ch)
        {
            var pat = 
                Playing 
                ? OK(clip.EditPos) ? clip.EditPat : clip.Track.PlayPat 
                : clip.EditPat;

            var chan = clip.Patterns[pat].Channels[ch]; 


            var _found = False;

            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];

                int found;
                do
                { 
                    found = chan.Notes.FindIndex(n => 
                           note == n.Number
                        && clip.EditPos == clip.EditPat*g_patSteps + n.Step + ChordStrum(i));

                    if (OK(found)) 
                    {
                        chan.Notes.RemoveAt(found);
                        _found = True;
                    }
                } 
                while (OK(found));
            }


            if (!_found)
                AddChannelNotes(clip, notes, ch, clip.EditPos);
        }



        void PlayNote(Clip clip, int num, int ch)
        {
            if (TooComplex) return;

            clip.TrimCurrentNotes(ch);
            g_lastNotes.Clear();


            var notes = GetChordNotes(num);


            if (   OK(clip.EditPos)
                || Recording)
            {
                if (   clip.ChordMode
                    && clip.Chord < 0) ToggleNotes(clip, notes, ch);
                else                     PlayNotes(clip, notes, ch);

                if (!Recording)
                { 
                    if (    (  !clip.ChordMode
                             || OK(clip.Chord))
                        && !(   clip.ChordEdit 
                             && OK(clip.Chord)))
                        MoveEdit(clip, 1, True);
                }
            }
            else
            {
                var chanNotes = g_notes.Where(n => n.iChan == ch).ToList();

                var notFound = OK(notes.FindIndex(n => 
                    chanNotes.FindIndex(_n => _n.Number == n) < 0));

                if (notFound)
                    clip.TrimCurrentNotes(ch);

                for (int i = 0; i < notes.Count; i++)
                    TriggerNote(clip, notes[i], ch, EditedClip.EditStepLength, ChordStrum(i));
            }


            clip.CurNote = num;
        }


        void PlayNotes(Clip clip, List<int> notes, int ch)
        {
            var pat = 
                Playing 
                ? OK(clip.EditPos) ? clip.EditPat : clip.Track.PlayPat 
                : clip.EditPat;

            var chan = clip.Patterns[pat].Channels[ch];


            for (int i = 0; i < notes.Count; i++)
            {
                if (TooComplex) return;

                var note = notes[i];
                int found;

                do
                { 
                    found = chan.Notes.FindIndex(n => 
                        clip.EditPos == pat*g_patSteps + n.Step + ChordStrum(i));

                    if (OK(found)) 
                        chan.Notes.RemoveAt(found);
                } 
                while (OK(found));
            }


            var step = 
                Recording 
                ? clip.Track.PlayStep
                : clip.EditPos;

            AddChannelNotes(clip, notes, ch, step);
        }



        void AddChannelNotes(Clip clip, List<int> notes, int ch, float step)
        {
            var pat =
                Playing
                ? (OK(clip.EditPos) ? clip.EditPat : clip.Track.PlayPat)
                : clip.EditPat;

            var chan = clip.Patterns[pat].Channels[ch];


            for (int i = 0; i < notes.Count; i++)
            {
                if (TooComplex) return;

                var note = notes[i];

                if (!(   clip.ChordEdit
                      && OK(clip.Chord)))
                {
                    //var editStep = 1/2f;

                    //var noteStep = (int)((step % g_patSteps + ChordStrum(i)) / editStep) * editStep;
                    var noteStep = step % g_patSteps + ChordStrum(i);
                    var lastNote = new Note(chan, ch, 1, False, note, noteStep, EditedClip.EditStepLength);
                    
                    g_lastNotes.Add(lastNote);
                    chan.AddNote(lastNote);
                }

                TriggerNote(clip, note, ch, EditedClip.EditStepLength, ChordStrum(i));
            }
        }



        void TriggerNote(Clip clip, int num, int ch, float len, float chordStrumOffset)
        {
            var chan  = clip.EditPattern.Channels[ch];
            var track = clip.Track;

            var patStep = TimeStep
                  //(OK(track.PlayStep)
                  // ?    track.StartStep
                  //   +  track.PlayPat * g_patSteps 
                  //   + (track.PlayStep % g_patSteps) 
                  // : TimeStep) 
                + chordStrumOffset;

            AddNoteAndSounds(new Note(chan, ch, 1, False, num, patStep, len));
        }



        void AddPlaybackNotes(Clip clip)
        {
            if (TooComplex)
                return;

            var pat = clip.Patterns[clip.Track.PlayPat];

            for (int ch = 0; ch < g_nChans; ch++)
            {
                var chan = pat.Channels[ch];
                if (!chan.On) continue;

                var sh    = (int)clip.Track.PlayStep % 2 != 0 ? chan.Shuffle : 0;
                var notes = chan.Notes.FindAll(n => n.ClipTime == clip.Track.PlayTime);

                foreach (var n in notes)
                {
                    var note = new Note(n);
                    note.Step = TimeStep + (float)sh / TicksPerStep;
                    AddNoteAndSounds(note);
                }
            }
        }



        void AddNoteAndSounds(Note note)
        {
            var inst = note.Instrument;
            note.Sounds.Clear();


            var track = note.Clip.Track;

            var sh = (int)track.PlayStep % 2 != 0 ? note.Channel.Shuffle : 0;


            var found =
                g_notes.Find(n => 
                       track.PlayStep >= n.Step 
                    && track.PlayStep <  n.Step + n.StepLength);

            if (   OK(found)
                && found.Number     == note.Number
                && found.StepLength == float_Inf)
                return;


            foreach (var src in inst.Sources)
            {
                if (TooComplex) return;

                if (src.On)
                    src.CreateSounds(note.Sounds, note, this);
            }


            g_notes .Add     (note);
            g_sounds.AddRange(note.Sounds);
        }



        void StopNotes()
        {
            var delete = new List<int>();

            for (int i = 0; i < g_notes.Count; i++)
            {
                var note = g_notes[i];

                if (TimeStep >= note.Step + note.StepLength)
                    delete.Add(i);
            }

            for (int i = delete.Count - 1; i >= 0; i--)
                g_notes.RemoveAt(delete[i]);
        }



        static int AdjustNoteNumber(Note note, Source src, int sndLen, Program prog)
        {
            //add glide notes when notes are actually added ("pressed"),
            //then here check for them and move them to the target 
            //    according to speed (find min and max on both sides, then adjust the middle accordingly)
            //when a note is removed as it ends, remove also the glide note

            var inst = src.Instrument;


            float _noteNum = note.Number;

            var tp = new TimeParams(
                g_time, 
                0, 
                note, 
                sndLen, 
                src.Index, 
                _triggerDummy, 
                note.Clip,
                prog);


            _noteNum += inst.Tune?.UpdateValue(tp) * NoteScale ?? 0;

            float noteNumInst = MinMax(12 * NoteScale, (int)Math.Round(_noteNum), 150 * NoteScale);

            if (   OK(inst.Tune)
                && inst.Tune.On
                && OK(inst.Tune.Chord)
                && inst.Tune.Chord.On
                && inst.Tune.Chord.CurFinalChord.Count > 0)
                noteNumInst = LimitNoteToChord((int)Math.Round(noteNumInst), inst.Tune.Chord.CurFinalChord);


            noteNumInst += src.Tune?.UpdateValue(tp) * NoteScale ?? 0;

            var noteNumSrc = MinMax(12*NoteScale, (int)Math.Round(noteNumInst), 150*NoteScale);

            if (   OK(src.Tune)
                && src.Tune.On
                && OK(src.Tune.Chord)
                && src.Tune.Chord.On
                && src.Tune.Chord.CurFinalChord.Count > 0)
                noteNumSrc = LimitNoteToChord(noteNumSrc, src.Tune.Chord.CurFinalChord);


            return noteNumSrc;
        }
    }
}
