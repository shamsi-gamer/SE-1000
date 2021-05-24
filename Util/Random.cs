using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        static float RND { get { return (float)g_rnd.NextDouble(); } }


        void Random()
        {
                 if (   SelChan < 0
                     || !EditClip.RndInst) RandomChannelNotes();
            else if (EditClip.ParamKeys
                  || EditClip.ParamAuto)   RandomValues(CurChan);
            else if (CurSet  > -1)        CurSetting   .Randomize(this);
            else if (CurSrc  > -1)        SelSource    .Randomize(new List<Oscillator>(), this);
            else if (SelChan > -1)        SelInstrument.Randomize(this);
        }


        void RandomValues(int ch)
        {
            int first, last;
            EditClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                     if (EditClip.ParamKeys) RandomParamKeys(p, ch);
                else if (EditClip.ParamAuto) RandomParamAuto(p, ch);
            }
        }

            
        //void RandomPatternNotes()
        //{
        //    var nChannels = g_rnd.Next(1, g_nChans/2);

        //    var rndInst = new List<Instrument>();

        //    for (int i = 0; i < nChannels; i++)
        //    {
        //        var ch = g_rnd.Next(0, g_nChans);
        //        RandomNotes(ch, rndInst);
        //    }

        //    UpdateInstOff(EditClip.CurChan);
        //}


        void RandomChannelNotes()
        {
            if (EditClip.AllChan) Random();
            else                 RandomNotes(CurChan, null);
        }


        void RandomNotes(int ch, List<Instrument> rndInst)
        {
            if (TooComplex) return;

            int first, last;
            EditClip.GetCurPatterns(out first, out last);

            var inst = g_session.Instruments[g_rnd.Next(0, g_session.Instruments.Count)];

            if (OK(rndInst))
            { 
                if (rndInst.Contains(inst))
                    return;

                rndInst.Add(inst);
            }

            for (int p = first; p <= last; p++)
            {
                if (   OK(rndInst)
                    || EditClip.RndInst)
                    EditClip.Patterns[p].Channels[ch].Instrument = inst;

                if (RND > 0.6  ) Flip(p, ch,  1);
                if (RND > 0.8  ) Flip(p, ch,  2);
                if (RND > 0.9  ) Flip(p, ch,  4);
                if (RND > 0.925) Flip(p, ch,  8);
                if (RND > 0.95 ) Flip(p, ch, 16);

                RandomNotes(p, ch);
            }
        }


        void RandomNotes(int pat, int ch)
        {
            if (TooComplex) return;


            var chan = EditClip.Patterns[pat].Channels[ch];


            const int minNote   = 54 * NoteScale;
            const int maxNote   = 78 * NoteScale;

            const int _noteScale = 32;


            // this is only used for randomizing chords,
            // but must be initialized here
            var chords = new List<List<int>>();
            foreach (var c in EditClip.Chords)
                if (c.Count > 0) chords.Add(new List<int>(c));

            var curChord = g_rnd.Next(0, chords.Count);


            var step  =  0f;
            var dStep =  0f;

            var note  = -1;


            while (step < g_patSteps)
            {
                if (TooComplex) return;

                if (RND >= 0.5)
                {
                    var found = chan.Notes.Find(n => n.Step == step);

                    if (OK(found)) chan.Notes.Remove(found);
                    else
                    { 
                        var editLength = g_steps[g_rnd.Next(0, EditClip.EditLengthIndex + 1)];

                        if (note < 0)
                        { 
                            note = ((minNote + (int)(Math.Pow(RND, 0.25) * (maxNote - minNote))) / NoteScale) * NoteScale;
                            chan.AddNote(new Note(chan, ch, 1, note, step, editLength));
                        }
                        else
                        {
                            var dNoteMax = dStep * _noteScale;// * Math.Pow(RND, 0.7);
                            var dNote    = dNoteMax * RND;

                            var rndNote = (int)MinMax(minNote, note - dNoteMax/2 + dNote, maxNote);
                            if (!EditClip.HalfSharp) rndNote = (rndNote / NoteScale) * NoteScale;

                            if (EditClip.ChordMode)
                            {
                                if (EditClip.Chord > -1)
                                {
                                    var chord = EditClip.Chords[EditClip.Chord];

                                    if (EditClip.ChordAll)
                                        chord = UpdateFinalTuneChord(chord, T);
                                    
                                    note = chord[g_rnd.Next(0, chord.Count)];
                                    chan.AddNote(new Note(chan, ch, 1, note, step, editLength));
                                }
                                else if (EditClip.ChordAll)
                                {
                                    var chord = chords[curChord];
                                    note = chord[g_rnd.Next(0, chord.Count)];
                                    chan.AddNote(new Note(chan, ch, 1, note, step, editLength));                               
                                }
                                else
                                {
                                    note = rndNote;

                                    var chord  = new List<int>{0};
                                    var nNotes = 1 + Math.Pow(RND, 2) * (3 - 1);

                                    var dist = 0;
                                    for (int i = 0; i < nNotes; i++)
                                    { 
                                        dist += g_rnd.Next(3, 5) * NoteScale;
                                        chord.Add(dist);
                                    }

                                    foreach (var off in chord)
                                    { 
                                        var _note = note + off;
                                        if (!EditClip.HalfSharp) _note = (_note / NoteScale) * NoteScale;
                                        chan.AddNote(new Note(chan, ch, 1, _note, step, editLength));                               
                                    }
                                }
                            }
                            else
                            { 
                                note = rndNote;
                                chan.AddNote(new Note(chan, ch, 1, note, step, editLength));
                            }
                        }
                    }
                }

                dStep = 1 + (int)Math.Round(Math.Pow(RND, 0.8f) * Math.Max(0, (g_steps[EditClip.EditStepIndex] - 1)));
                step += dStep;
            }
        }


        void RandomParamKeys(int pat, int ch)
        {
            var chan = EditClip.Patterns[pat].Channels[ch];

            foreach (var note in chan.Notes)
            {
                var param = GetCurrentParam(note.Instrument);
                var index = note.Keys.FindIndex(k => k.Path == param.GetPath(CurSrc));

                var rndValue = (float)(param.NormalMin + RND * (param.NormalMax - param.NormalMin));

                if (index > -1)
                    note.Keys[index].Value = rndValue;
                else
                    note.Keys.Add(new Key(CurSrc, param, rndValue, note.Step, chan));
            }
        }


        void RandomParamAuto(int pat, int ch)
        {
            var chan  = EditClip.Patterns[pat].Channels[ch];
            var param = GetCurrentParam(chan.Instrument);

            for (int step = 0; step < g_patSteps; step++)
            { 
                if (RND < 0.5)
                    continue;

                var rndValue = (float)(param.NormalMin + RND * (param.NormalMax - param.NormalMin));

                var index = chan.AutoKeys.FindIndex(k => 
                       k.Path == param.GetPath(CurSrc) 
                    && k.StepTime == step);

                if (index > -1)
                    chan.AutoKeys[index].Value = rndValue;
                else
                    chan.AutoKeys.Add(new Key(CurSrc, param, rndValue, step, chan));
            }

            EditClip.UpdateAutoKeys();
        }
    }
}
