using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Note
        {
            public Channel     Channel;
            public int         iChan;
                               
            public int         Number;
                               
            public Instrument  Instrument;

            public float       PatStepTime,
                               StepLength;
                               
            public int         PatIndex     { get { return Channel.Pattern.Song.Patterns.IndexOf(Channel.Pattern); } }

            public long        PatTime      { get { return (long)(PatStepTime * g_ticksPerStep); } }
            public long        SongTime     { get { return PatIndex * nSteps + PatTime; } }

            public int         FrameLength  { get { return (int) (StepLength  * g_ticksPerStep); } }

            public float       Volume;

            public List<Sound> Sounds;

            public List<Key>   Keys;


            // this is in frames, but it's a float because of the
            // variability that needs to be accounted for
            public float       ArpPlayTime; // this gets offset 


            public Note()
            {
                Channel  = null;
                iChan    = -1;

                PatStepTime = -1;

                Sounds   = new List<Sound>();

                Keys     = new List<Key>();

                Reset();
            }


            public Note(Note note)
            {
                Channel    = note.Channel;
                iChan      = note.iChan;
                Number     = note.Number;
                Instrument = note.Instrument;
                PatStepTime   = note.PatStepTime;
                StepLength = note.StepLength;
                Volume     = note.Volume;

                Sounds = new List<Sound>();

                Keys = new List<Key>();
                foreach (var key in note.Keys)
                    Keys.Add(key);
            }


            public Note(Channel chan, int ch, float vol, int num, Instrument inst, float time, float len)
            {
                Channel    = chan;
                iChan      = ch;
                Number     = num;
                Instrument = inst;
                PatStepTime   = time;
                StepLength = len;
                Volume     = vol;

                ArpPlayTime   = float.NaN;

                Sounds     = new List<Sound>();

                Keys       = new List<Key>();
            }


            public void Reset()
            {
                Number     = 69;
                StepLength = 0;
                Volume     = 0;

                ArpPlayTime   = float.NaN;

                Sounds.Clear();
            }


            public float ShOffset
            {
                get
                {
                         if (PatStepTime % 2 == 1   ) return (float)Channel.Shuffle / g_ticksPerStep;
                    else if (PatStepTime % 2 == 1.5f) return (float)Channel.Shuffle / g_ticksPerStep;
                    else return 0;
                }
            }


            public void SetLength(float len, int stepLen)
            {
                StepLength = len;
                UpdateSoundLengths(stepLen);
            }


            void UpdateSoundLengths(int stepLen)
            {
                foreach (var snd in Sounds)
                    snd.FrameLength = (int)(StepLength * stepLen/* - (snd.Envelope?.Offset ?? 0)*/);
            }
        }
    }
}
