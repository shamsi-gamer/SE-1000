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

            var chan  = clip.CurrentPattern.Channels[ch];
            var notes = GetChordNotes(num);


            if (OK(clip.EditPos))
            {
                if (   clip.ChordMode
                    && clip.Chord < 0)
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
                                && clip.EditPos == clip.CurPat*g_nSteps + n.PatStep + ChordSpread(i));

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

                            if (!(   clip.ChordEdit
                                  && clip.Chord > -1))
                            {
                                var noteStep = clip.EditPos % g_nSteps + ChordSpread(i);
                                var lastNote = new Note(chan, ch, 1, note, noteStep, CurClip.EditStepLength);
                    
                                lastNotes.Add(lastNote);
                                chan.AddNote(lastNote);
                            }

                            TriggerNote(clip, note, ch, CurClip.EditStepLength, ChordSpread(i));
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
                                clip.EditPos == clip.CurPat*g_nSteps + n.PatStep + ChordSpread(i));

                            if (found > -1) 
                                chan.Notes.RemoveAt(found);
                        } 
                        while (found > -1);
                    }


                    for (int i = 0; i < notes.Count; i++)
                    {
                        if (TooComplex) return;

                        var note = notes[i];

                        if (!(   clip.ChordEdit
                              && clip.Chord > -1))
                        {
                            var noteStep = clip.EditPos % g_nSteps + ChordSpread(i);
                            var lastNote = new Note(chan, ch, 1, note, noteStep, CurClip.EditStepLength);
                    
                            lastNotes.Add(lastNote);
                            chan.AddNote(lastNote);
                        }

                        TriggerNote(clip, note, ch, CurClip.EditStepLength, ChordSpread(i));
                    }
                }

                
                if (    (  !clip.ChordMode
                         || clip.Chord > -1)
                    && !(   clip.ChordEdit 
                         && clip.Chord > -1))
                    MoveEdit(clip, 1, true);
            }
            else
            {
                var chanNotes = g_notes.Where(n => n.iChan == ch).ToList();

                var notFound = notes.FindIndex(n => 
                    chanNotes.FindIndex(_n => _n.Number == n) < 0) 
                    > -1;

                if (notFound)
                    clip.TrimCurrentNotes(ch);

                for (int i = 0; i < notes.Count; i++)
                    TriggerNote(clip, notes[i], ch, CurClip.EditStepLength, ChordSpread(i));
            }


            clip.CurNote = num;
        }


        void TriggerNote(Clip clip, int num, int ch, float len, float chordSpread)
        {
            var chan = clip.CurrentPattern.Channels[ch];

            var patStep = 
                  (g_playing ? (clip.PlayPat - clip.CurPat) * g_nSteps + (clip.PlayStep % g_nSteps) : 0) 
                + chordSpread;

            var found = g_notes.Find(n =>
                   n.iChan  == ch
                && n.Number == num);

            AddNoteAndSounds(new Note(chan, ch, 1, num, patStep, len));

            //if (clip.Piano)
            //    GetLabelFromNote(num).Mark();
        }


        void AddPlaybackNotes(Clip clip)
        {
            if (TooComplex)
                return;

            var pat = clip.Patterns[clip.PlayPat];

            for (int ch = 0; ch < g_nChans; ch++)
            {
                var chan = pat.Channels[ch];
                if (!chan.On) continue;

                var sh    = (int)clip.PlayStep % 2 != 0 ? chan.Shuffle : 0;
                var notes = chan.Notes.FindAll(n => n.SongTime == clip.PlayTime);

                foreach (var n in notes)
                {
                    var note = new Note(n);

                    note.PatStep += (float)sh / g_session.TicksPerStep;

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

            var clip = note.Channel.Pattern.Clip;


            var sh = (int)clip.PlayStep % 2 != 0 ? note.Channel.Shuffle : 0;

            if (note.Instrument.Arpeggio != null)
            {
                var notes = note.Channel.Notes.FindAll(n =>
                          n.Instrument.Arpeggio != null
                       && clip.PlayTime >= clip.PlayPat*g_nSteps*g_session.TicksPerStep + n.PatTime
                       && clip.PlayTime <  clip.PlayPat*g_nSteps*g_session.TicksPerStep + n.PatTime + n.FrameLength);

                foreach (var n in notes)
                {
                    if (TooComplex) return;

                    var arp = n.Instrument.Arpeggio;

                    var arpNotes = arp.Clip.Patterns[0].Channels[0].Notes.FindAll(_n =>
                        clip.PlayTime == (n.PatStep + sh)*g_session.TicksPerStep + _n.ArpPlayTime);

                    foreach (var nn in arpNotes)
                    {
                        nn.PatStep = n.PatStep + (n.ArpPlayTime + sh) / g_session.TicksPerStep;

                        g_notes.Add(nn);
                        g_sounds.AddRange(nn.Sounds);
                    }
                }
            }
            else // normal note
            {
                var found =
                    g_notes.Find(n => 
                           clip.PlayStep >= n.PatStep 
                        && clip.PlayStep <  n.PatStep + n.StepLength);
                
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

                if (clip.PlayTime < 0)
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


        //void StopNote(Clip clip, Note note)
        //{
        //    var step = g_playing ? PlayStep : TimeStep;
        //    //note.UpdateStepLength(step - note.PatStep, g_session.TicksPerStep);
        //}


        static int AdjustNoteNumber(Note note, Source src, int sndLen, Program prog)
        {
            var inst = src.Instrument;

            float _noteNum = note.Number;

            var tp = new TimeParams(
                g_time, 
                0, 
                g_time - note.Channel.Pattern.Clip.StartTime, 
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
