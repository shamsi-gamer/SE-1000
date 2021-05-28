using System;
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
            public int                iChan,
                                      
                                      Length,
                                      ReleaseLength;
                                      
            public long               Time, // in ticks
                                      ElapsedTime; // in ticks

                                      
                                      //DisplayVolume;
            public float              TriggerVolume;//,

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

            public Sample             NoteSample => Source.Oscillator.Samples[Note.Number-24*NoteScale];


            public Sound(string sample, Channel chan, int ch, long frameTime, int frameLen, int releaseLen, float vol, Instrument inst, int iSrc, Note note, List<TriggerValue> triggerValues, bool isEcho, Sound echoSrc, float echoVol, Parameter harmonic = Parameter_null, Sound hrmSound = Sound_null, float hrmPos = float_NaN)
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

                Time          = snd.Time;
                              
                Length        = snd.Length;
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
                var sTime = g_time - EditedClip.Track.StartTime;

                var tp = new TimeParams(g_time, lTime, sTime, Note, Length, SourceIndex, TriggerValues, prog);

                float vol = 0;

                if (OK(Cache)) // not echo
                {
                    var updateVol = 
                        EditedClip.Track.PlayTime < Time + Length + ReleaseLength
                        //&& !prog.TooComplex
                        ? GetVolume(g_time, EditedClip.Track.StartTime, prog)
                        : 0;

                    vol = 
                          TriggerVolume
                        * updateVol
                        * Channel.Volume
                        * EditedClip.Volume;

                    // this is for the fake "current volume"
                    if (   (   Source.Oscillator == OscSlowSweepDown
                            || Source.Oscillator == OscFastSweepDown
                            || Source.Oscillator == OscSlowSweepUp
                            || Source.Oscillator == OscFastSweepUp)
                        && lTime > NoteSample.Length * FPS)
                        vol = 0;
                    else if (Source.Oscillator == OscCrunch)
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

                        srcVol = Math.Max(srcVol, Harmonic.CurValue);

                        inst.DisplayVolume = 
                            OK(inst.DisplayVolume)
                            ? sndAdd(inst.DisplayVolume, Harmonic.CurValue)
                            : Harmonic.CurValue;
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

                        if (OK(spk))
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

                    // if sample is ending, restart it 
                    if (   ElapsedTime >= (NoteSample.Length - 0.1f) * FPS
                        && Source.Oscillator != OscSlowSweepDown
                        && Source.Oscillator != OscSlowSweepUp
                        && Source.Oscillator != OscFastSweepDown
                        && Source.Oscillator != OscFastSweepUp)
                    {
                        // TODO make this smooth
                        spk.Block.Stop();
                        spk.Block.Play();
                    }
                }        
            }
        }
    }
}