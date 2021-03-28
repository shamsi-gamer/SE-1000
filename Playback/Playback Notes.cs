using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void PlayNote(Song song, int num, List<int> chord, int ch)
        {
            StopCurrentNotes(song, ch);
            lastNotes.Clear();

            var chan  = CurrentPattern.Channels[ch];
            var notes = GetChordNotes(num);


            if (OK(song.EditPos))
            {
                if (   g_chordMode
                    && g_chord < 0)
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
                                && song.EditPos == CurPat*nSteps + n.PatStep + ChordSpread(i));

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

                            if (!(   g_chordEdit
                                  && g_chord > -1))
                            {
                                var noteStep = song.EditPos % nSteps + ChordSpread(i);
                                var lastNote = new Note(chan, ch, 1, note, noteStep, EditLength);
                    
                                lastNotes.Add(lastNote);
                                chan.AddNote(lastNote);
                            }

                            TriggerNote(note, ch, EditLength, ChordSpread(i));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < notes.Count; i++)
                    {
                        var note = notes[i];
                        int found;

                        do
                        { 
                            found = chan.Notes.FindIndex(n => 
                                song.EditPos == CurPat*nSteps + n.PatStep + ChordSpread(i));

                            if (found > -1) 
                                chan.Notes.RemoveAt(found);
                        } 
                        while (found > -1);
                    }


                    for (int i = 0; i < notes.Count; i++)
                    {
                        var note = notes[i];

                        if (!(   g_chordEdit
                              && g_chord > -1))
                        {
                            var noteStep = song.EditPos % nSteps + ChordSpread(i);
                            var lastNote = new Note(chan, ch, 1, note, noteStep, EditLength);
                    
                            lastNotes.Add(lastNote);
                            chan.AddNote(lastNote);
                        }

                        TriggerNote(note, ch, EditLength, ChordSpread(i));
                    }
                }

                
                if (    (  !g_chordMode
                         || g_chord > -1)
                    && !(   g_chordEdit 
                         && g_chord > -1))
                    MoveEdit(song, 1, true);
            }
            else
            {
                var chanNotes = g_notes.Where(n => n.iChan == ch).ToList();

                var notFound = notes.FindIndex(n => 
                    chanNotes.FindIndex(_n => _n.Number == n) < 0) 
                    > -1;

                if (notFound)
                    StopCurrentNotes(song, ch);

                for (int i = 0; i < notes.Count; i++)
                    TriggerNote(notes[i], ch, EditLength, ChordSpread(i));
            }


            g_curNote = num;
        }


        void TriggerNote(int num, int ch, float len, float chordSpread)
        {
            var chan = CurrentPattern.Channels[ch];

            var patStep = 
                 (PlayTime > -1 ? PlayStep : 0) 
                + chordSpread;

            var found = g_notes.Find(n =>
                   n.iChan  == ch
                && n.Number == num);

            AddNoteAndSounds(new Note(chan, ch, 1, num, patStep, len));

            if (g_piano)
                MarkLight(GetLightFromNote(num));
        }


        void AddPlaybackNotes()
        {
            var pat = g_song.Patterns[PlayPat];

            for (int ch = 0; ch < g_nChans; ch++)
            {
                var chan = pat.Channels[ch];
                if (!chan.On) continue;


                var sh    = (int)PlayStep % 2 != 0 ? chan.Shuffle : 0;
                var notes = chan.Notes.FindAll(n => n.SongTime == PlayTime);

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

            var sh = (int)PlayStep % 2 != 0 ? note.Channel.Shuffle : 0;

            if (note.Instrument.Arpeggio != null)
            {
                var notes = note.Channel.Notes.FindAll(n =>
                          n.Instrument.Arpeggio != null
                       && PlayTime >= PlayPat*nSteps*g_ticksPerStep + n.PatTime
                       && PlayTime <  PlayPat*nSteps*g_ticksPerStep + n.PatTime + n.FrameLength);

                foreach (var n in notes)
                {
                    var arp = n.Instrument.Arpeggio;

                    var arpNotes = arp.Song.Patterns[0].Channels[0].Notes.FindAll(_n =>
                        PlayTime == (n.PatStep + sh)*g_ticksPerStep + _n.ArpPlayTime);

                    foreach (var nn in arpNotes)
                    {
                        nn.PatStep = n.PatStep + (n.ArpPlayTime + sh) / g_ticksPerStep;

                        g_notes.Add(nn);
                        g_sounds.AddRange(nn.Sounds);
                    }
                }
            }
            else
            {
                foreach (var src in inst.Sources)
                { 
                    if (src.On)
                        src.CreateSounds(note.Sounds, note, this);
                }

                if (PlayTime < 0)
                    note.PatStep = TimeStep;

                g_notes.Add(note);
                g_sounds.AddRange(note.Sounds);
            }
        }


        void StopNotes(float time)
        {
            var delete = new List<int>();

            for (int i = 0; i < g_notes.Count; i++)
            {
                var note = g_notes[i];

                if (time >= note.PatStep + note.StepLength)
                    delete.Add(i);
            }

            for (int i = delete.Count - 1; i >= 0; i--)
                g_notes.RemoveAt(delete[i]);
        }


        //void StopNote(Song song, Note note)
        //{
        //    var step = PlayTime > -1 ? PlayStep : TimeStep;
        //    //note.UpdateStepLength(step - note.PatStep, g_ticksPerStep);
        //}


        void StopCurrentNotes(Song song, int ch = -1)
        {
            var timeStep = PlayTime > -1 ? PlayStep      : TimeStep;

            foreach (var note in g_notes)
            {
                if (   ch < 0
                    || note.iChan == ch)
                { 
                    var noteStep = PlayTime > -1 ? note.SongStep : note.PatStep;
                    note.UpdateStepLength(timeStep - noteStep);
                }
            }
        }


        static int AdjustNoteNumber(Note note, Source src, int sndLen, Program prog)
        {
            var inst = src.Instrument;

            float _noteNum = note.Number;

            _noteNum += inst.Tune?.GetValue(g_time, 0, StartTime, sndLen, note, src.Index, _triggerDummy, prog) * NoteScale ?? 0;
            _noteNum += src .Tune?.GetValue(g_time, 0, StartTime, sndLen, note, src.Index, _triggerDummy, prog) * NoteScale ?? 0;

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
