using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void PlayNote(Clip song, int num, List<int> chord, int ch)
        {
            if (TooComplex) return;

            song.TrimCurrentNotes(ch);
            lastNotes.Clear();

            var chan  = g_clip.CurrentPattern.Channels[ch];
            var notes = GetChordNotes(num);


            if (OK(song.EditPos))
            {
                if (   g_clip.ChordMode
                    && g_clip.Chord < 0)
                {
                    var _found = false;

                    for (int i = 0; i < notes.Count; i++)
                    {
                        var note = notes[i];

                        int found;
                        do
                        { 
                            found = chan.Notes.FindIndex(n => 
                                   note == n.Number
                                && song.EditPos == g_clip.CurPat*g_nSteps + n.PatStep + ChordSpread(i));

                            if (found > -1) 
                            {
                                chan.Notes.RemoveAt(found);
                                _found = true;
                            }
                        } 
                        while (found > -1);
                    }


                    if (!_found)
                    {
                        for (int i = 0; i < notes.Count; i++)
                        {
                            var note = notes[i];

                            if (!(   g_clip.ChordEdit
                                  && g_clip.Chord > -1))
                            {
                                var noteStep = song.EditPos % g_nSteps + ChordSpread(i);
                                var lastNote = new Note(chan, ch, 1, note, noteStep, EditStepLength);
                    
                                lastNotes.Add(lastNote);
                                chan.AddNote(lastNote);
                            }

                            TriggerNote(note, ch, EditStepLength, ChordSpread(i));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < notes.Count; i++)
                    {
                        if (TooComplex) return;

                        var note = notes[i];
                        int found;

                        do
                        { 
                            found = chan.Notes.FindIndex(n => 
                                song.EditPos == g_clip.CurPat*g_nSteps + n.PatStep + ChordSpread(i));

                            if (found > -1) 
                                chan.Notes.RemoveAt(found);
                        } 
                        while (found > -1);
                    }


                    for (int i = 0; i < notes.Count; i++)
                    {
                        if (TooComplex) return;

                        var note = notes[i];

                        if (!(   g_clip.ChordEdit
                              && g_clip.Chord > -1))
                        {
                            var noteStep = song.EditPos % g_nSteps + ChordSpread(i);
                            var lastNote = new Note(chan, ch, 1, note, noteStep, EditStepLength);
                    
                            lastNotes.Add(lastNote);
                            chan.AddNote(lastNote);
                        }

                        TriggerNote(note, ch, EditStepLength, ChordSpread(i));
                    }
                }

                
                if (    (  !g_clip.ChordMode
                         || g_clip.Chord > -1)
                    && !(   g_clip.ChordEdit 
                         && g_clip.Chord > -1))
                    MoveEdit(song, 1, true);
            }
            else
            {
                var chanNotes = g_notes.Where(n => n.iChan == ch).ToList();

                var notFound = notes.FindIndex(n => 
                    chanNotes.FindIndex(_n => _n.Number == n) < 0) 
                    > -1;

                if (notFound)
                    song.TrimCurrentNotes(ch);

                for (int i = 0; i < notes.Count; i++)
                    TriggerNote(notes[i], ch, EditStepLength, ChordSpread(i));
            }


            g_clip.CurNote = num;
        }


        void TriggerNote(int num, int ch, float len, float chordSpread)
        {
            var chan = g_clip.CurrentPattern.Channels[ch];

            var patStep = 
                 (OK(g_clip.PlayTime) ? (g_clip.PlayPat - g_clip.CurPat) * g_nSteps + (g_clip.PlayStep % g_nSteps) : 0) 
                + chordSpread;

            var found = g_notes.Find(n =>
                   n.iChan  == ch
                && n.Number == num);

            AddNoteAndSounds(new Note(chan, ch, 1, num, patStep, len));

            if (g_clip.Piano)
                MarkLight(GetLightFromNote(num));
        }


        void AddPlaybackNotes()
        {
            if (TooComplex)
                return;


            var pat = g_clip.Patterns[g_clip.PlayPat];

            for (int ch = 0; ch < g_nChans; ch++)
            {
                var chan = pat.Channels[ch];
                if (!chan.On) continue;

                var sh    = (int)g_clip.PlayStep % 2 != 0 ? chan.Shuffle : 0;
                var notes = chan.Notes.FindAll(n => n.SongTime == g_clip.PlayTime);

                foreach (var n in notes)
                {
                    var note = new Note(n);

                    note.PatStep += (float)sh / g_ticksPerStep;

                    if (note.Instrument.Arpeggio != null)
                        note.ArpPlayTime = 0;

                    AddNoteAndSounds(note);
                }
            }
        }


        void AddNoteAndSounds(Note note)
        {
            var inst = note.Instrument;
            note.Sounds.Clear();

            var sh = (int)g_clip.PlayStep % 2 != 0 ? note.Channel.Shuffle : 0;

            if (note.Instrument.Arpeggio != null)
            {
                var notes = note.Channel.Notes.FindAll(n =>
                          n.Instrument.Arpeggio != null
                       && g_clip.PlayTime >= g_clip.PlayPat*g_nSteps*g_ticksPerStep + n.PatTime
                       && g_clip.PlayTime <  g_clip.PlayPat*g_nSteps*g_ticksPerStep + n.PatTime + n.FrameLength);

                foreach (var n in notes)
                {
                    if (TooComplex) return;

                    var arp = n.Instrument.Arpeggio;

                    var arpNotes = arp.Clip.Patterns[0].Channels[0].Notes.FindAll(_n =>
                        g_clip.PlayTime == (n.PatStep + sh)*g_ticksPerStep + _n.ArpPlayTime);

                    foreach (var nn in arpNotes)
                    {
                        nn.PatStep = n.PatStep + (n.ArpPlayTime + sh) / g_ticksPerStep;

                        g_notes.Add(nn);
                        g_sounds.AddRange(nn.Sounds);
                    }
                }
            }
            else // normal note
            {
                var found =
                    g_notes.Find(n => 
                           g_clip.PlayStep >= n.PatStep 
                        && g_clip.PlayStep <  n.PatStep + n.StepLength);
                
                if (   found != null
                    && found.Number == note.Number
                    && found.StepLength == float_Inf)
                    return;


                foreach (var src in inst.Sources)
                {
                    if (TooComplex) return;

                    if (src.On)
                        src.CreateSounds(note.Sounds, note, this);
                }

                if (g_clip.PlayTime < 0)
                    note.PatStep = TimeStep;

                g_notes.Add(note);
                g_sounds.AddRange(note.Sounds);
            }
        }


        void StopNotes(float step)
        {
            var delete = new List<int>();

            for (int i = 0; i < g_notes.Count; i++)
            {
                var note = g_notes[i];

                if (step >= note.PatStep + note.StepLength)
                    delete.Add(i);
            }

            for (int i = delete.Count - 1; i >= 0; i--)
                g_notes.RemoveAt(delete[i]);
        }


        //void StopNote(Clip song, Note note)
        //{
        //    var step = OK(PlayTime) ? PlayStep : TimeStep;
        //    //note.UpdateStepLength(step - note.PatStep, g_ticksPerStep);
        //}


        static int AdjustNoteNumber(Note note, Source src, int sndLen, Program prog)
        {
            var inst = src.Instrument;

            float _noteNum = note.Number;

            var tp = new TimeParams(
                g_time, 
                0, 
                g_time - g_clip.StartTime, 
                note, 
                sndLen, 
                src.Index, 
                _triggerDummy, 
                prog);

            _noteNum += inst.Tune?.UpdateValue(tp) * NoteScale ?? 0;
            _noteNum += src .Tune?.UpdateValue(tp) * NoteScale ?? 0;

            var noteNum = MinMax(12*NoteScale, (int)Math.Round(_noteNum), 150*NoteScale);

            if (   src.Tune != null
                && src.Tune.UseChord
                && src.Tune.FinalChord.Count > 0)
                noteNum = LimitNoteToChord(noteNum, src.Tune.FinalChord);

            if (   inst.Tune != null
                && inst.Tune.UseChord
                && inst.Tune.FinalChord.Count > 0)
                noteNum = LimitNoteToChord(noteNum, inst.Tune.FinalChord);

            return noteNum;
        }
    }
}
