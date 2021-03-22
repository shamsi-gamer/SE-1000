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
                                      
            public long               FrameTime; // in ticks
            public int                FrameLength;
            public int                ReleaseLength;
                                      
            public long               ElapsedFrameTime;
                                      
            public float              TriggerVolume,
                                      DisplayVolume;

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
                Speakers         = new List<Speaker>();
                Sample           = sample;
                                 
                Channel          = chan;
                iChan            = ch;
                                 
                FrameTime        = frameTime;

                FrameLength      = frameLen;
                ReleaseLength    = releaseLen;

                ElapsedFrameTime = 0;

                TriggerVolume    = vol;
                DisplayVolume    = vol;

                TriggerValues    = new List<TriggerValue>();
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

                Cache            = IsEcho ? null : new float[FrameLength + ReleaseLength];
            }


            public Sound(Sound snd, bool isEcho, Sound echoSrc, float echoVol)
            {
                Speakers         = new List<Speaker>();
                Sample           = snd.Sample;

                Channel          = snd.Channel;
                iChan            = snd.iChan;

                FrameTime        = snd.FrameTime;

                FrameLength      = snd.FrameLength;
                ReleaseLength    = snd.ReleaseLength;

                ElapsedFrameTime = snd.ElapsedFrameTime;

                TriggerVolume    = snd.TriggerVolume;
                DisplayVolume    = snd.DisplayVolume;

                TriggerValues    = new List<TriggerValue>();
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

                Cache            = IsEcho ? null : new float[FrameLength + ReleaseLength];
            }


            public float GetVolume(long gTime, long sTime)
            {
                var lTime = gTime - FrameTime; // local time

                var vol = 
                      Instrument.Volume.GetValue(gTime, lTime, sTime, FrameLength, Note, -1,           TriggerValues)
                    * Source    .Volume.GetValue(gTime, lTime, sTime, FrameLength, Note, Source.Index, TriggerValues);

                return MinMax(0, vol, 2);
            }


            public void Stop()
            {
                foreach (var spk in Speakers)
                    g_sm.FreeSpeaker(spk);

                Speakers.Clear();
            }
        }
    }
}