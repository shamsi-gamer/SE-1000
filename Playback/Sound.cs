﻿using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Sound
        {
            public string             Sample;

            public List<Speaker>      Speakers;
                                      
            public Channel            Channel;
            public int                iChan;
                                      
            public long               Time; // in ticks
            public int                Length;
            public int                ReleaseLength;
                                      
            public long               ElapsedTime; // in ticks
                                      
            public float              TriggerVolume;//,
                                      //DisplayVolume;

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


            public Sound(string sample, Channel chan, int ch, long frameTime, int frameLen, int releaseLen, float vol, Instrument inst, int iSrc, Note note, List<TriggerValue> triggerValues, bool isEcho, Sound echoSrc, float echoVol, Parameter harmonic = null, Sound hrmSound = null, float hrmPos = fN)
            {
                Speakers      = new List<Speaker>();
                Sample        = sample;
                              
                Channel       = chan;
                iChan         = ch;
                              
                Time          = frameTime;

                Length        = frameLen;
                ReleaseLength = releaseLen;

                ElapsedTime   = 0;

                TriggerVolume = vol;
                //DisplayVolume = vol;

                TriggerValues = new List<TriggerValue>();
                foreach (var val in triggerValues)
                    TriggerValues.Add(new TriggerValue(val));
                    
                Instrument    = inst;

                SourceIndex   = iSrc;
                Source        = Instrument.Sources[iSrc];
                              
                Note          = note;
                              
                Harmonic      = harmonic;
                HrmSound      = hrmSound;
                HrmPos        = hrmPos;

                IsEcho        = isEcho;
                EchoSource    = echoSrc;
                EchoVolume    = echoVol;

                Cache         = IsEcho ? null : new float[Length + ReleaseLength];
            }


            public Sound(Sound snd, bool isEcho, Sound echoSrc, float echoVol)
            {
                Speakers      = new List<Speaker>();
                Sample        = snd.Sample;

                Channel       = snd.Channel;
                iChan         = snd.iChan;

                Time     = snd.Time;

                Length   = snd.Length;
                ReleaseLength = snd.ReleaseLength;

                ElapsedTime   = snd.ElapsedTime;

                TriggerVolume = snd.TriggerVolume;
                //DisplayVolume = snd.DisplayVolume;

                TriggerValues = new List<TriggerValue>();
                foreach (var val in snd.TriggerValues)
                    TriggerValues.Add(new TriggerValue(val));
                    
                Instrument    = snd.Instrument;

                SourceIndex   = snd.SourceIndex;
                Source        = snd.Source;

                Note          = snd.Note;

                Harmonic      = snd.Harmonic;
                HrmSound      = snd.HrmSound;
                HrmPos        = snd.HrmPos;

                IsEcho        = isEcho;
                EchoSource    = echoSrc;
                EchoVolume    = echoVol;

                Cache         = IsEcho ? null : new float[Length + ReleaseLength];
            }


            public float GetVolume(long gTime, long sTime, Program prog)
            {
                if (prog.TooComplex) return 0;

                var lTime = gTime - Time; // local time

                var tpInst = new TimeParams(gTime, lTime, sTime, Note, Length, -1,           TriggerValues, prog);
                var tpSrc  = new TimeParams(gTime, lTime, sTime, Note, Length, Source.Index, TriggerValues, prog);

                var vol = 
                      Instrument.Volume.UpdateValue(tpInst)
                    * Source    .Volume.UpdateValue(tpSrc);

                return MinMax(0, vol, 2);
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


                var lTime = g_time - Time;
                var sTime = g_time - g_song.StartTime;

                var tp = new TimeParams(g_time, lTime, sTime, Note, Length, SourceIndex, TriggerValues, prog);

                float vol = 0;

                if (Cache != null) // not echo
                {
                    var updateVol = 
                           g_song.PlayTime < Time + Length + ReleaseLength
                        && !prog.TooComplex
                        ? GetVolume(g_time, g_song.StartTime, prog)
                        : 1;

                    vol = 
                          TriggerVolume
                        * updateVol
                        * Channel.Volume
                        * g_volume;

                    var inst = Source.Instrument;

                    if (    Harmonic != null
                        && !prog.TooComplex)
                    {
                        Harmonic.UpdateValue(tp);

                        Harmonic.CurValue = ApplyFilter(Harmonic.CurValue, Source, HrmPos, tp);

                        vol = MinMax(
                            Harmonic.Min, 
                            vol * Harmonic.CurValue, 
                            Harmonic.Max);

                        inst.DisplayVolume = 
                            OK(inst.DisplayVolume)
                            ? sndAdd(inst.DisplayVolume, Harmonic.CurValue)
                            : Harmonic.CurValue;
                    }


                    if (Source.Oscillator == OscClick)
                        vol = 0;
                    else if (Source.Oscillator == OscCrunch)
                        vol /= 2;


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

                UpdateSpeakers(vol, prog);

                ElapsedTime = g_time - Time;
            }


            void UpdateSpeakers(float vol, Program prog)
            {
                if (prog.TooComplex) return;


                var v = (float)Math.Pow(vol, 2);

                if (Speakers.Count == 0)
                {
                    while (v-- > 0)
                    { 
                        var spk = g_sm.GetSpeaker();

                        if (spk != null)
                        { 
                            spk.Block.SelectedSound = Sample;
                            spk.Block.LoopPeriod    = 60;
                            spk.Block.Play();

                            Speakers.Add(spk);
                        }
                    }
                }


                v = (float)Math.Pow(vol, 2);

                foreach (var spk in Speakers)
                {
                    spk.Block.Volume = Math.Min(v--, 1);

                    // if sample is ending, restart it //TODO make this smooth
                    if (   ElapsedTime >= (Source.Oscillator.Length - 0.1f) * FPS
                        && Source.Oscillator != OscClick)
                    {
                        spk.Block.Stop();
                        spk.Block.Play();
                    }
                }        
            }

        }
    }
}