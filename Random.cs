using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        float Rnd { get { return (float)g_rnd.NextDouble(); } }


        void RandomNotes()
        {
            if (allChan)
            {
                for (int ch = 0; ch < nChans; ch++)
                    RandomNotes(ch);
            }
            else
                RandomNotes(CurChan);
        }


        void RandomNotes(int ch)
        {
            int first, last;
            GetPatterns(g_song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
                RandomNotes(p, ch);
        }


        void RandomNotes(int pat, int ch)
        {
            var chan = g_song.Patterns[pat].Channels[ch];

            const int minNote   = 54 * NoteScale;
            const int maxNote   = 78 * NoteScale;

            const int _noteScale = 32;


            // this is only used for randomizing chords,
            // but must be initialized here
            var chords = new List<List<int>>();
            foreach (var c in g_chords)
                if (c.Count > 0) chords.Add(new List<int>(c));

            var curChord = g_rnd.Next(0, chords.Count);


            var step  =  0f;
            var dStep =  0f;

            var note  = -1;


            while (step < nSteps)
            {
                if (g_rnd.NextDouble() >= 0.5)
                {
                    var found = chan.Notes.Find(n => n.PatStep == step);

                    if (found != null) chan.Notes.Remove(found);
                    else
                    { 
                        var editLength = g_steps[g_rnd.Next(0, g_editLength + 1)];

                        if (note < 0)
                        { 
                            note = ((minNote + (int)(Math.Pow(g_rnd.NextDouble(), 0.25) * (maxNote - minNote))) / NoteScale) * NoteScale;
                            chan.AddNote(new Note(chan, ch, 1, note, step, editLength));
                        }
                        else
                        {
                            var dNoteMax = dStep * _noteScale;// * Math.Pow(g_rnd.NextDouble(), 0.7);
                            var dNote    = dNoteMax * g_rnd.NextDouble();

                            var rndNote = (int)MinMax(minNote, note - dNoteMax/2 + dNote, maxNote);
                            if (!g_halfSharp) rndNote = (rndNote / NoteScale) * NoteScale;

                            if (g_chordMode)
                            {
                                if (g_chord > -1)
                                {
                                    var chord = g_chords[g_chord];

                                    if (g_chordAll)
                                        chord = UpdateFinalTuneChord(chord, true);
                                    
                                    note = chord[g_rnd.Next(0, chord.Count)];
                                    chan.AddNote(new Note(chan, ch, 1, note, step, editLength));
                                }
                                else if (g_chordAll)
                                {
                                    var chord = chords[curChord];
                                    note = chord[g_rnd.Next(0, chord.Count)];
                                    chan.AddNote(new Note(chan, ch, 1, note, step, editLength));                               
                                }
                                else
                                {
                                    note = rndNote;

                                    var chord  = new List<int>{0};
                                    var nNotes = 1 + Math.Pow(g_rnd.NextDouble(), 2) * (3 - 1);

                                    var dist = 0;
                                    for (int i = 0; i < nNotes; i++)
                                    { 
                                        dist += g_rnd.Next(3, 5) * NoteScale;
                                        chord.Add(dist);
                                    }

                                    foreach (var off in chord)
                                    { 
                                        var _note = note + off;
                                        if (!g_halfSharp) _note = (_note / NoteScale) * NoteScale;
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

                dStep = 1 + (int)Math.Round(Math.Pow(g_rnd.NextDouble(), 0.8f) * Math.Max(0, (g_steps[g_editStep] - 1)));
                step += dStep;
            }
        }


        void RandomParamKeys(int pat, int ch)
        {
            var chan = g_song.Patterns[pat].Channels[ch];

            foreach (var note in chan.Notes)
            {
                var param = GetCurrentParam(note.Instrument);
                var index = note.Keys.FindIndex(k => k.Path == param.GetPath(CurSrc));

                var rndValue = (float)(param.NormalMin + g_rnd.NextDouble() * (param.NormalMax - param.NormalMin));

                if (index > -1)
                    note.Keys[index].Value = rndValue;
                else
                    note.Keys.Add(new Key(CurSrc, param, rndValue, note.PatStep, chan));
            }
        }


        void RandomParamAuto(int pat, int ch)
        {
            var chan  = g_song.Patterns[pat].Channels[ch];
            var param = GetCurrentParam(chan.Instrument);

            for (int step = 0; step < nSteps; step++)
            { 
                if (g_rnd.NextDouble() < 0.5)
                    continue;

                var rndValue = (float)(param.NormalMin + g_rnd.NextDouble() * (param.NormalMax - param.NormalMin));

                var index = chan.AutoKeys.FindIndex(k => 
                       k.Path == param.GetPath(CurSrc) 
                    && k.StepTime == step);

                if (index > -1)
                    chan.AutoKeys[index].Value = rndValue;
                else
                    chan.AutoKeys.Add(new Key(CurSrc, param, rndValue, step, chan));
            }

            g_song.UpdateAutoKeys();
        }


        //void RandomChannel(int pat, int ch, bool vol = false)
        //{
        //    if (rndInst) RandomInstrument(pat, ch);
        //    if (vol)     RndVol(pat, ch);

        //    RandomShuffle(pat, ch);
        //    RandomNotes(pat, ch);
        //}


        void RandomShuffle(int pat, int ch)
        {
            var chan = g_song.Patterns[pat].Channels[ch];
            chan.Shuffle = g_rnd.Next(0, g_ticksPerStep - 1);
        }


        void RndVol(int pat, int ch)
        {
            var chan = g_song.Patterns[pat].Channels[ch];
            chan.Volume = (float)g_rnd.NextDouble();
        }


        void RandomInstrument(int pat, int ch)
        {
            var chan = g_song.Patterns[pat].Channels[ch];
            chan.Instrument = g_inst[g_rnd.Next(0, g_inst.Count)];
            UpdateInstOff(ch);
        }

        void RandomSound(int iInst)
        {
            //var inst = g_inst[iInst];

            //inst.Sources.Clear();

            //var nSrc = g_rnd.Next(0, maxSrc) + 1;

            //for (int i = 0; i < nSrc; i++)
            //{
            //    var src = new Source();

            //    src.Oscillator   = (Oscillator)g_rnd.Next((int)Oscillator.Sine, (int)Oscillator.Samples1);
            //    src.Volume       = i == 0 ? 1 : 0.5f + Rnd / 2;
            //    src.LfoAmplitude = Rnd;
            //    src.LfoFrequency = Rnd * 20;
            //    src.Attack       = Rnd / 4;
            //    src.Decay        = Rnd / 3;
            //    src.Sustain      = Rnd;
            //    src.Release      = Rnd / 2;
            //    src.Transpose    = i == 0 ? 0 : g_rnd.Next(-2, 3) * 12;
            //    src.Offset       = g_rnd.Next(-2, 3);

            //    inst.Sources.Add(src);
            //}

            //inst.DelayCount = g_rnd.Next(0, 4);
            //inst.DelayTime = Math.Max(0.01f, Rnd) / 2;
            //inst.DelayLevel = Rnd;
            //inst.DelayPower = Math.Max(0.01f, Rnd * 5);

            //if (g_song.CurSrc > -1)
            //    UpdateSrcOff(inst, g_song.CurSrc);

            //TriggerNote(curNote > 1 ? curNote : 69, g_song.CurChan, stepLen);
        }
    }
}
