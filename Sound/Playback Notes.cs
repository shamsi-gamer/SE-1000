using Sandbox.Game.EntityComponents;
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
        bool movePat;
        bool loopPat;
        bool allChan;
        bool allPats;
        bool g_autoCue;
        bool rndInst;
             
        // display offsets
        int  songOff;
        int  instOff;
        int  srcOff;



        void PlayNote(Song song, int num, List<int> chord, int ch)
        {
            StopCurrentNotes(song, ch);
            lastNotes.Clear();

            var _chan = CurrentPattern(g_song).Channels[ch];
            var  chan = CurrentPattern(song)  .Channels[ch];

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
                                && song.EditPos == song.CurPat*nSteps + n.PatStepTime + ChordSpread(i));

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
                                var lastNote = new Note(
                                    chan, 
                                    ch, 
                                    1, 
                                    note, 
                                    _chan.Instrument, 
                                    song.EditPos % nSteps + ChordSpread(i), 
                                    EditLength);
                    
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
                                song.EditPos == song.CurPat*nSteps + n.PatStepTime + ChordSpread(i));

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
                            var lastNote = new Note(chan, ch, 1, note, _chan.Instrument, song.EditPos % nSteps + ChordSpread(i), EditLength);
                    
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
            showNote  = g_curNote;

            g_sampleValid = false;
        }


        void TriggerNote(int num, int ch, float len, float chordSpread)
        {
            var  chan = CurrentPattern(g_song).Channels[ch];

            var noteTime = 
                  (OK(g_song.PlayStep) ? g_song.PlayStep : TimeStep) 
                + chordSpread;

            var found = g_notes.Find(n =>
                   n.iChan  == ch
                && n.Number == num);


            if (found != null)
                StopNote(g_song, found);
            else
            { 
                AddNoteAndSounds(new Note(
                    chan,
                    ch, 
                    1, 
                    num, 
                    chan.Instrument, 
                    noteTime, 
                    len));
            }


            if (g_piano)
                MarkLight(GetLightFromNote(num));
        }


        void AddPlaybackNotes()
        {
            var pat = g_song.Patterns[g_song.PlayPat];

            for (int ch = 0; ch < nChans; ch++)
            {
                var chan = pat.Channels[ch];
                var sh   = (int)g_song.PlayStep % 2 != 0 ? chan.Shuffle : 0;


                if (chan.On)
                {
                    var notes = chan.Notes.FindAll(n => 
                           g_song.PlayTime
                        == g_song.PlayPat*nSteps*g_ticksPerStep + n.PatTime);

                    foreach (var n in notes)
                    {
                        var note = new Note(n);

                        note.PatStepTime = g_song.PlayStep + (float)sh / g_ticksPerStep;

                        if (note.Instrument.Arpeggio != null)
                            note.ArpPlayTime = 0;

                        AddNoteAndSounds(note);
                    }
                }
            }
        }


        void AddNoteAndSounds(Note note)
        {
            var inst = note.Instrument;
            note.Sounds.Clear();

            var sh = (int)g_song.PlayStep % 2 != 0 ? note.Channel.Shuffle : 0;

            if (note.Instrument.Arpeggio != null)
            {
                var notes = note.Channel.Notes.FindAll(n =>
                          n.Instrument.Arpeggio != null
                       && g_song.PlayTime >= g_song.PlayPat*nSteps*g_ticksPerStep + n.PatTime
                       && g_song.PlayTime <  g_song.PlayPat*nSteps*g_ticksPerStep + n.PatTime + n.FrameLength);

                foreach (var n in notes)
                {
                    var arp = n.Instrument.Arpeggio;

                    var arpNotes = arp.Song.Patterns[0].Channels[0].Notes.FindAll(_n =>
                        g_song.PlayTime == (n.PatStepTime + sh)*g_ticksPerStep + _n.ArpPlayTime);

                    foreach (var nn in arpNotes)
                    {
                        nn.PatStepTime = n.PatStepTime + (n.ArpPlayTime + sh) / g_ticksPerStep;

                        g_notes.Add(nn);
                        g_sounds.AddRange(nn.Sounds);
                    }
                }
            }
            else
            {
                foreach (var src in inst.Sources)
                    AddSourceSounds(note.Sounds, src, note);
 
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

                if (time >= note.PatStepTime + note.StepLength)
                    delete.Add(i);
            }

            for (int i = delete.Count - 1; i >= 0; i--)
                g_notes.RemoveAt(delete[i]);
        }


        void StopNote(Song song, Note note)
        {
            var step = OK(song.PlayStep) ? song.PlayStep : TimeStep;
            note.SetLength(step - note.PatStepTime, g_ticksPerStep);
        }


        void StopCurrentNotes(Song song, int ch = -1)
        {
            var step = OK(song.PlayStep) ? song.PlayStep : TimeStep;

            foreach (var note in g_notes)
            {
                if (   ch < 0
                    || note.iChan == ch)
                    note.SetLength(step - note.PatStepTime, g_ticksPerStep);
            }
        }


        static int AdjustNoteNumber(Note note, Source src, int sndLen)
        {
            var inst = src.Instrument;

            float _noteNum = note.Number;

            _noteNum += inst.Tune?.GetValue(g_time, 0, g_song.StartTime, sndLen, note, src.Index, _triggerDummy)*NoteScale ?? 0;
            _noteNum += src .Tune?.GetValue(g_time, 0, g_song.StartTime, sndLen, note, src.Index, _triggerDummy)*NoteScale ?? 0;

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
