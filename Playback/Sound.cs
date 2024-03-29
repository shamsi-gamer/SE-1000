﻿using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Sound
        {
            public int                NoteNumber;

            public string             Sample;

            public List<Speaker>      Speakers;
                                      
            public Channel            Channel;
            public int                iChan,
                                      
                                      Length,
                                      ReleaseLength;
                                      
            public long               Time,        // in ticks
                                      ElapsedTime; // in ticks
                                      
            public float              TriggerVolume;

            public List<TriggerValue> TriggerValues;

            public Instrument         Instrument;
                                      
            public int                SourceIndex;
            public Source             Source;
                                      
            public Note               Note;
                                      
            public Parameter          Harmonic;
            public Sound              HrmSound;
            public float              HrmPos;
                                      
            public float[]            Cache;
            public Sound              EchoSource;
            public float              EchoVolume;
            public bool               IsEcho;

            public Sample             NoteSample =>
                                             Note.Number >= 12*NoteScale
                                          && Note.Number-12*NoteScale < Source.Oscillator.Samples.Count
                                          ? Source.Oscillator.Samples[Note.Number-12*NoteScale]
                                          : null;



            public Sound(int noteNum, string sample, Channel chan, int ch, long frameTime, int frameLen, int releaseLen, float vol, Instrument inst, int iSrc, Note note, List<TriggerValue> triggerValues, bool isEcho, Sound echoSrc, float echoVol, Parameter harmonic = Parameter_null, Sound hrmSound = Sound_null, float hrmPos = float_NaN)
            {
                Speakers         = new List<Speaker>();
                                 
                NoteNumber       = noteNum;

                Sample           = sample;
                                 
                Channel          = chan;
                iChan            = ch;
                                 
                Time             = frameTime;
                                 
                Length           = frameLen;
                ReleaseLength    = releaseLen;
                                 
                ElapsedTime      = 0;
                                 
                TriggerVolume    = vol;

                TriggerValues = new List<TriggerValue>();
                foreach (var val in triggerValues)
                    TriggerValues.Add(new TriggerValue(val));
                    
                Instrument       = inst;
                                 
                SourceIndex      = iSrc;
                Source           = Instrument.Sources[iSrc];
                                 
                Note             = note;
                                 
                Harmonic         = harmonic;
                HrmSound         = hrmSound;
                HrmPos           = hrmPos;
                                 
                IsEcho           = isEcho;
                EchoSource       = echoSrc;
                EchoVolume       = echoVol;
                                 
                Cache            = IsEcho ? null : new float[Length + ReleaseLength];
            }



            public Sound(Sound snd, bool isEcho, Sound echoSrc, float echoVol)
            {
                Speakers         = new List<Speaker>();
                                 
                NoteNumber       = snd.NoteNumber;

                Sample           = snd.Sample;
                                 
                Channel          = snd.Channel;
                iChan            = snd.iChan;
                                 
                Time             = snd.Time;
                                 
                Length           = snd.Length;
                ReleaseLength    = snd.ReleaseLength;
                                 
                ElapsedTime      = snd.ElapsedTime;
                                 
                TriggerVolume    = snd.TriggerVolume;
                //DisplayVolume  = snd.DisplayVolume;
                
                TriggerValues = new List<TriggerValue>();
                foreach (var val in snd.TriggerValues)
                    TriggerValues.Add(new TriggerValue(val));
                    
                Instrument       = snd.Instrument;
                                 
                SourceIndex      = snd.SourceIndex;
                Source           = snd.Source;
                                 
                Note             = snd.Note;
                                 
                Harmonic         = snd.Harmonic;
                HrmSound         = snd.HrmSound;
                HrmPos           = snd.HrmPos;
                                 
                IsEcho           = isEcho;
                EchoSource       = echoSrc;
                EchoVolume       = echoVol;
                                 
                Cache            = IsEcho ? null : new float[Length + ReleaseLength];
            }



            public float GetVolume(long gTime, Program prog)
            {
                if (prog.TooComplex) return 0;

                var lTime = gTime - Time; // local time

                var tpInst = new TimeParams(gTime, lTime, Note, Length, -1,           TriggerValues, Note.Clip, prog);
                var tpSrc  = new TimeParams(gTime, lTime, Note, Length, Source.Index, TriggerValues, Note.Clip, prog);

                var vol = 
                      Instrument.Volume.UpdateValue(tpInst)
                    * Source    .Volume.UpdateValue(tpSrc);

                return MinMax(0, vol, 2);
            }



            public float GetTune(long gTime, Program prog)
            {
                if (prog.TooComplex) return 0;

                var lTime = gTime - Time; // local time

                var tpInst = new TimeParams(gTime, lTime, Note, Length, -1, TriggerValues, Note.Clip, prog);
                var tpSrc  = new TimeParams(gTime, lTime, Note, Length, Source.Index, TriggerValues, Note.Clip, prog);

                var tune =
                      Instrument.Tune.UpdateValue(tpInst)
                    * Source    .Tune.UpdateValue(tpSrc);

                return tune;
            }



            public void Stop()
            {
                foreach (var spk in Speakers)
                    g_sm.FreeSpeaker(spk);

                Speakers.Clear();
            }



            public void Update(Program prog)
            {
                if (prog.TooComplex)
                {
                    ElapsedTime = g_time - Time;
                    return;
                }


                var track = Note.Clip.Track;


                var lTime = g_time - Time;
                var sTime = g_time - track.StartTime;

                var tp = new TimeParams(g_time, lTime, Note, Length, SourceIndex, TriggerValues, Note.Clip, prog);

                float vol = 0;

                if (OK(Cache)) // not echo
                {
                    var updateVol = 
                        track.PlayTime < Time + Length + ReleaseLength
                        ? GetVolume(g_time, prog)
                        : 0;

                    vol = 
                          TriggerVolume
                        * updateVol
                        * Channel.Volume
                        * Note.Clip.Volume;

                    if (Note.Accent)
                        vol *= Note.Channel.AccentScale;


                    // this is for the fake "current volume"
                    if (   (   Source.Oscillator == OscSlowSweepDown
                            || Source.Oscillator == OscFastSweepDown
                            || Source.Oscillator == OscSlowSweepUp
                            || Source.Oscillator == OscFastSweepUp)
                        && !OK(NoteSample)
                        && lTime > NoteSample.Length * FPS)
                        vol = 0;
                    else if (Source.Oscillator == OscPulse)
                        vol /= 2;


                    var inst = Source.Instrument;

                    var srcVol = 0f;

                    if (    OK(Harmonic)
                        && !prog.TooComplex)
                    {
                        Harmonic.UpdateValue(tp);

                        Harmonic.CurValue = ApplyFilter(Harmonic.CurValue, Source, HrmPos, tp);

                        vol = MinMax(
                            Harmonic.Min, 
                            vol * Harmonic.CurValue, 
                            Harmonic.Max);

                        srcVol = Math.Max(srcVol, vol);

                        inst.DisplayVolume = 
                            OK(inst.DisplayVolume)
                            ? sndAdd(inst.DisplayVolume, vol)
                            : vol;
                    }

                    Source.CurVolume = Math.Max(srcVol, vol);

                    inst.DisplayVolume = 
                        OK(inst.DisplayVolume)
                        ? sndAdd(inst.DisplayVolume, vol)
                        : vol;


                    if (lTime < Cache.Length)
                        Cache[lTime] = vol;
                }
                else if (lTime < EchoSource.Cache.Length)
                {
                    vol = EchoSource.Cache[lTime]
                        * EchoVolume;
                }

                
                if (!prog.TooComplex)
                {
                    if (!OK(vol))
                        vol = 0;

                    var noteNum = AdjustNoteNumber(Note, Source, Note.FrameLength, prog);

                    UpdateSpeakers(vol, noteNum);
                }


                ElapsedTime = g_time - Time;
            }



            void UpdateSpeakers(float vol, int noteNum)
            {
                var v = PrepareVolume(vol);

                if (Speakers.Count == 0)
                {
                    var loopable = OscIsLoopable(Source.Oscillator);

                    while ( loopable && v-- >  0
                        || !loopable && v-- >= 0)
                    {
                        var spk = g_sm.GetSpeaker();

                        if (OK(spk))
                        { 
                            spk.Block.SelectedSound = Sample;
                            spk.Block.LoopPeriod    = 60;
                            spk.Block.Play();

                            Speakers.Add(spk);
                        }
                    }
                }


                v = PrepareVolume(vol);

                foreach (var spk in Speakers)
                {
                    spk.Block.Volume = Math.Min(v--, 1);

                    // if sample is ending,
                    // or tne note number is changing,
                    // restart it

                    if (      (  !OK(NoteSample) 
                               || ElapsedTime >= (NoteSample.Length - 0.1f) * FPS)
                           && OscIsLoopable(Source.Oscillator)
                        || NoteNumber != noteNum)
                    {
                        // TODO make this smooth
                        //spk.Block.Volume = 0;
                        spk.Block.Stop();

                        if (NoteNumber != noteNum)
                        {
                            NoteNumber = noteNum;
                            Sample                  = Source.GetSample(NoteNumber);
                            spk.Block.SelectedSound = Sample;
                        }

                        spk.Block.Play();
                    }
                }        
            }



            bool OscIsLoopable(Oscillator osc)
            {
                return 
                       osc != OscSlowSweepDown
                    && osc != OscSlowSweepUp
                    && osc != OscFastSweepDown
                    && osc != OscFastSweepUp
                    && osc != OscSample;
            }



            float PrepareVolume(float vol)
            {
                return (float)Math.Pow(vol, 3);
                //return (float)(1 - Math.Cos(Tau/4 * vol));
            }
        }
    }
}