using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void PlayNote(Clip clip, int num, List<int> chord, int ch)
        {
            if (TooComplex) return;

            clip.TrimCurrentNotes(ch);
            lastNotes.Clear();

            //var chan = 
            //    Playing
            //    ? clip.Patterns[clip.Track.PlayPat].Channels[ch]
            //    : CurChannel;

            var notes = GetChordNotes(num);


            if (   OK(clip.EditPos)
                || Recording)
            {
                if (   clip.ChordMode
                    && clip.Chord < 0) PlayChord(clip, notes, ch);
                else                   PlayNote (clip, notes, ch);

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
                    TriggerNote(clip, notes[i], ch, EditedClip.EditStepLength, ChordSpread(i));
            }


            clip.CurNote = num;
        }


        void PlayChord(Clip clip, List<int> notes, int ch)
        {
            var chan = 
                Playing
                ? clip.Patterns[clip.Track.PlayPat].Channels[ch]
                : CurChannel;

            var _found = False;

            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];

                int found;
                do
                { 
                    found = chan.Notes.FindIndex(n => 
                            note == n.Number
                        && clip.EditPos == clip.CurPat*g_patSteps + n.Step + ChordSpread(i));

                    if (OK(found)) 
                    {
                        chan.Notes.RemoveAt(found);
                        _found = True;
                    }
                } 
                while (OK(found));
            }


            if (!_found)
            {
                for (int i = 0; i < notes.Count; i++)
                {
                    var note = notes[i];

                    if (!(   clip.ChordEdit
                          && OK(clip.Chord)))
                    {
                        var noteStep = clip.EditPos % g_patSteps + ChordSpread(i);
                        var lastNote = new Note(chan, ch, 1, note, noteStep, EditedClip.EditStepLength);
                    
                        lastNotes.Add(lastNote);
                        chan.AddNote(lastNote);
                    }

                    TriggerNote(clip, note, ch, EditedClip.EditStepLength, ChordSpread(i));
                }
            }
        }


        void PlayNote(Clip clip, List<int> notes, int ch)
        {
            var pat = 
                Playing 
                ? clip.Track.PlayPat 
                : clip.CurPat;

            var chan = 
                Playing
                ? clip.Patterns[clip.Track.PlayPat].Channels[ch]
                : clip.CurPattern.Channels[ch];

            for (int i = 0; i < notes.Count; i++)
            {
                if (TooComplex) return;

                var note = notes[i];
                int found;

                do
                { 
                    found = chan.Notes.FindIndex(n => 
                        clip.EditPos == pat*g_patSteps + n.Step + ChordSpread(i));

                    if (OK(found)) 
                        chan.Notes.RemoveAt(found);
                } 
                while (OK(found));
            }


            for (int i = 0; i < notes.Count; i++)
            {
                if (TooComplex) return;

                var note = notes[i];

                var step = 
                    Recording 
                    ? clip.Track.PlayStep
                    : clip.EditPos;

                if (!(   clip.ChordEdit
                      && OK(clip.Chord)))
                {
                    var noteStep = step % g_patSteps + ChordSpread(i);
                    var lastNote = new Note(chan, ch, 1, note, noteStep, EditedClip.EditStepLength);
                    
                    lastNotes.Add(lastNote);
                    chan.AddNote(lastNote);
                }

                TriggerNote(clip, note, ch, EditedClip.EditStepLength, ChordSpread(i));
            }
        }


        void TriggerNote(Clip clip, int num, int ch, float len, float chordSpreadOffset)
        {
            var chan  = clip.CurPattern.Channels[ch];
            var track = clip.Track;

            var patStep = 
                  (Playing
                   ?    track.StartStep
                     +  track.PlayPat * g_patSteps 
                     + (track.PlayStep % g_patSteps) 
                   : TimeStep) 
                + chordSpreadOffset;

            AddNoteAndSounds(new Note(chan, ch, 1, num, patStep, len));
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
                var notes = chan.Notes.FindAll(n => n.SongTime == clip.Track.PlayTime);

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
            _noteNum += src .Tune?.UpdateValue(tp) * NoteScale ?? 0;

            var noteNum = MinMax(12*NoteScale, (int)Math.Round(_noteNum), 150*NoteScale);

            if (   OK(src.Tune)
                && src.Tune.UseChord
                && src.Tune.FinalChord.Count > 0)
                noteNum = LimitNoteToChord(noteNum, src.Tune.FinalChord);

            if (   OK(inst.Tune)
                && inst.Tune.UseChord
                && inst.Tune.FinalChord.Count > 0)
                noteNum = LimitNoteToChord(noteNum, inst.Tune.FinalChord);

            return noteNum;
        }
    }
}
