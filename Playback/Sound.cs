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
                Speakers      = new List<Speaker>();
                Sample        = sample;
                              
                Channel       = chan;
                iChan         = ch;
                              
                Time     = frameTime;

                Length   = frameLen;
                ReleaseLength = releaseLen;

                ElapsedTime   = 0;

                TriggerVolume = vol;
                DisplayVolume = vol;

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
                DisplayVolume = snd.DisplayVolume;

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
                      Instrument.Volume.GetValue(tpInst)
                    * Source    .Volume.GetValue(tpSrc);

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