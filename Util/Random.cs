﻿using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        static float RND { get { return (float)g_rnd.NextDouble(); } }


        void Random()
        {
                 if (g_session.CurClip.SelChan < 0)  RandomPatternNotes();
            else if (g_session.CurClip.ParamKeys
                  || g_session.CurClip.ParamAuto)    RandomValues(g_session.CurClip.CurChan);
            else if (g_session.CurClip.CurSet  > -1) CurSetting               .Randomize(this);
            else if (g_session.CurClip.CurSrc  > -1) g_session.CurClip.SelectedSource    .Randomize(new List<Oscillator>(), this);
            else if (g_session.CurClip.SelChan > -1) g_session.CurClip.SelectedInstrument.Randomize(this);

            //MarkLabel(lblRandom);
        }


        void RandomValues(int ch)
        {
            int first, last;
            g_session.CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                     if (g_session.CurClip.ParamKeys) RandomParamKeys(p, ch);
                else if (g_session.CurClip.ParamAuto) RandomParamAuto(p, ch);
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
            if (g_session.CurClip.AllChan) Random();
            else                           RandomNotes(g_session.CurClip.CurChan, null);
        }


        void RandomNotes(int ch, List<Instrument> rndInst)
        {
            if (TooComplex) return;

            int first, last;
            g_session.CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var chan = g_session.CurClip.Patterns[p].Channels[ch];
                var inst = g_session.Instruments[g_rnd.Next(0, g_session.Instruments.Count)];

                if (rndInst != null)
                { 
                    if (rndInst.Contains(inst))
                        return;

                    rndInst.Add(inst);
                }

                if (   rndInst != null
                    || g_session.CurClip.RndInst)
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


            var chan = g_session.CurClip.Patterns[pat].Channels[ch];


            const int minNote   = 54 * NoteScale;
            const int maxNote   = 78 * NoteScale;

            const int _noteScale = 32;


            // this is only used for randomizing chords,
            // but must be initialized here
            var chords = new List<List<int>>();
            foreach (var c in g_session.CurClip.Chords)
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
                        var editLength = g_steps[g_rnd.Next(0, g_session.CurClip.EditLength + 1)];

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
                            if (!g_session.CurClip.HalfSharp) rndNote = (rndNote / NoteScale) * NoteScale;

                            if (g_session.CurClip.ChordMode)
                            {
                                if (g_session.CurClip.Chord > -1)
                                {
                                    var chord = g_session.CurClip.Chords[g_session.CurClip.Chord];

                                    if (g_session.CurClip.ChordAll)
                                        chord = UpdateFinalTuneChord(chord, true);
                                    
                                    note = chord[g_rnd.Next(0, chord.Count)];
                                    chan.AddNote(new Note(chan, ch, 1, note, step, editLength));
                                }
                                else if (g_session.CurClip.ChordAll)
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
                                        if (!g_session.CurClip.HalfSharp) _note = (_note / NoteScale) * NoteScale;
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

                dStep = 1 + (int)Math.Round(Math.Pow(RND, 0.8f) * Math.Max(0, (g_steps[g_session.CurClip.EditStep] - 1)));
                step += dStep;
            }
        }


        void RandomParamKeys(int pat, int ch)
        {
            var chan = g_session.CurClip.Patterns[pat].Channels[ch];

            foreach (var note in chan.Notes)
            {
                var param = GetCurrentParam(note.Instrument);
                var index = note.Keys.FindIndex(k => k.Path == param.GetPath(g_session.CurClip.CurSrc));

                var rndValue = (float)(param.NormalMin + RND * (param.NormalMax - param.NormalMin));

                if (index > -1)
                    note.Keys[index].Value = rndValue;
                else
                    note.Keys.Add(new Key(g_session.CurClip.CurSrc, param, rndValue, note.PatStep, chan));
            }
        }


        void RandomParamAuto(int pat, int ch)
        {
            var chan  = g_session.CurClip.Patterns[pat].Channels[ch];
            var param = GetCurrentParam(chan.Instrument);

            for (int step = 0; step < g_nSteps; step++)
            { 
                if (RND < 0.5)
                    continue;

                var rndValue = (float)(param.NormalMin + RND * (param.NormalMax - param.NormalMin));

                var index = chan.AutoKeys.FindIndex(k => 
                       k.Path == param.GetPath(g_session.CurClip.CurSrc) 
                    && k.StepTime == step);

                if (index > -1)
                    chan.AutoKeys[index].Value = rndValue;
                else
                    chan.AutoKeys.Add(new Key(g_session.CurClip.CurSrc, param, rndValue, step, chan));
            }

            g_session.CurClip.UpdateAutoKeys();
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
            var chan = g_session.CurClip.Patterns[pat].Channels[ch];
            chan.Shuffle = g_rnd.Next(0, g_session.TicksPerStep - 1);
        }


        void RndVol(int pat, int ch)
        {
            var chan = g_session.CurClip.Patterns[pat].Channels[ch];
            chan.Volume = RND;
        }


        void RandomInstrument(int pat, int ch)
        {
            var chan = g_session.CurClip.Patterns[pat].Channels[ch];
            chan.Instrument = g_session.Instruments[g_rnd.Next(0, g_session.Instruments.Count)];
            UpdateInstOff(ch);
        }

        void RandomSound(int iInst)
        {
            //var inst = g_session.Instruments[iInst];

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

            //if (g_song.g_session.CurClip.CurSrc > -1)
            //    UpdateSrcOff(inst, g_song.g_session.CurClip.CurSrc);

            //TriggerNote(curNote > 1 ? curNote : 69, g_song.CurChan, stepLen);
        }
    }
}