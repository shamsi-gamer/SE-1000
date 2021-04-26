using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        static float RND { get { return (float)g_rnd.NextDouble(); } }


        void Random()
        {
                 if (g_clip.SelChan < 0)  RandomPatternNotes();
            else if (g_clip.ParamKeys
                  || g_clip.ParamAuto)    RandomValues(g_clip.CurChan);
            else if (g_clip.CurSet  > -1) CurSetting               .Randomize(this);
            else if (g_clip.CurSrc  > -1) g_clip.SelectedSource    .Randomize(new List<Oscillator>(), this);
            else if (g_clip.SelChan > -1) g_clip.SelectedInstrument.Randomize(this);

            MarkLight(lblRandom);
        }


        void RandomValues(int ch)
        {
            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                     if (g_clip.ParamKeys) RandomParamKeys(p, ch);
                else if (g_clip.ParamAuto) RandomParamAuto(p, ch);
            }
        }


        void RandomPatternNotes()
        {
            var nChannels = g_rnd.Next(1, g_nChans/2);

            var rndInst = new List<Instrument>();

            for (int i = 0; i < nChannels; i++)
            {
                var ch = g_rnd.Next(0, g_nChans);
                RandomNotes(ch, rndInst);
            }
        }


        void RandomChannelNotes()
        {
            if (g_clip.AllChan)
            {
                for (int ch = 0; ch < g_nChans; ch++)
                    RandomNotes(ch, null);
            }
            else
                RandomNotes(g_clip.CurChan, null);
        }


        void RandomNotes(int ch, List<Instrument> rndInst)
        {
            if (TooComplex) return;

            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var chan = g_clip.Patterns[p].Channels[ch];
                var inst = g_inst[g_rnd.Next(0, g_inst.Count)];

                if (rndInst != null)
                { 
                    if (rndInst.Contains(inst))
                        return;

                    rndInst.Add(inst);
                }

                if (   rndInst != null
                    || g_clip.RndInst)
                    chan.Instrument = inst;


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


            var chan = g_clip.Patterns[pat].Channels[ch];


            const int minNote   = 54 * NoteScale;
            const int maxNote   = 78 * NoteScale;

            const int _noteScale = 32;


            // this is only used for randomizing chords,
            // but must be initialized here
            var chords = new List<List<int>>();
            foreach (var c in g_clip.Chords)
                if (c.Count > 0) chords.Add(new List<int>(c));

            var curChord = g_rnd.Next(0, chords.Count);


            var step  =  0f;
            var dStep =  0f;

            var note  = -1;


            while (step < g_nSteps)
            {
                if (TooComplex) return;

                if (RND >= 0.5)
                {
                    var found = chan.Notes.Find(n => n.PatStep == step);

                    if (found != null) chan.Notes.Remove(found);
                    else
                    { 
                        var editLength = g_steps[g_rnd.Next(0, g_clip.EditLength + 1)];

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
                            if (!g_clip.HalfSharp) rndNote = (rndNote / NoteScale) * NoteScale;

                            if (g_clip.ChordMode)
                            {
                                if (g_clip.Chord > -1)
                                {
                                    var chord = g_clip.Chords[g_clip.Chord];

                                    if (g_clip.ChordAll)
                                        chord = UpdateFinalTuneChord(chord, true);
                                    
                                    note = chord[g_rnd.Next(0, chord.Count)];
                                    chan.AddNote(new Note(chan, ch, 1, note, step, editLength));
                                }
                                else if (g_clip.ChordAll)
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
                                        if (!g_clip.HalfSharp) _note = (_note / NoteScale) * NoteScale;
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

                dStep = 1 + (int)Math.Round(Math.Pow(RND, 0.8f) * Math.Max(0, (g_steps[g_clip.EditStep] - 1)));
                step += dStep;
            }
        }


        void RandomParamKeys(int pat, int ch)
        {
            var chan = g_clip.Patterns[pat].Channels[ch];

            foreach (var note in chan.Notes)
            {
                var param = GetCurrentParam(note.Instrument);
                var index = note.Keys.FindIndex(k => k.Path == param.GetPath(g_clip.CurSrc));

                var rndValue = (float)(param.NormalMin + RND * (param.NormalMax - param.NormalMin));

                if (index > -1)
                    note.Keys[index].Value = rndValue;
                else
                    note.Keys.Add(new Key(g_clip.CurSrc, param, rndValue, note.PatStep, chan));
            }
        }


        void RandomParamAuto(int pat, int ch)
        {
            var chan  = g_clip.Patterns[pat].Channels[ch];
            var param = GetCurrentParam(chan.Instrument);

            for (int step = 0; step < g_nSteps; step++)
            { 
                if (RND < 0.5)
                    continue;

                var rndValue = (float)(param.NormalMin + RND * (param.NormalMax - param.NormalMin));

                var index = chan.AutoKeys.FindIndex(k => 
                       k.Path == param.GetPath(g_clip.CurSrc) 
                    && k.StepTime == step);

                if (index > -1)
                    chan.AutoKeys[index].Value = rndValue;
                else
                    chan.AutoKeys.Add(new Key(g_clip.CurSrc, param, rndValue, step, chan));
            }

            g_clip.UpdateAutoKeys();
        }


        //void RandomChannel(int pat, int ch, bool vol = F)
        //{
        //    if (rndInst) RandomInstrument(pat, ch);
        //    if (vol)     RndVol(pat, ch);

        //    RandomShuffle(pat, ch);
        //    RandomNotes(pat, ch);
        //}


        void RandomShuffle(int pat, int ch)
        {
            var chan = g_clip.Patterns[pat].Channels[ch];
            chan.Shuffle = g_rnd.Next(0, g_ticksPerStep - 1);
        }


        void RndVol(int pat, int ch)
        {
            var chan = g_clip.Patterns[pat].Channels[ch];
            chan.Volume = RND;
        }


        void RandomInstrument(int pat, int ch)
        {
            var chan = g_clip.Patterns[pat].Channels[ch];
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

            //if (g_song.g_clip.CurSrc > -1)
            //    UpdateSrcOff(inst, g_song.g_clip.CurSrc);

            //TriggerNote(curNote > 1 ? curNote : 69, g_song.CurChan, stepLen);
        }
    }
}
