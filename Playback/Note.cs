using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Note
        {
            public Channel     Channel;
            public int         iChan;
                               
            public Instrument  Instrument  => Channel.Instrument;

            public int         Number;
            public float       Volume;

            public float       Step,
                               StepLength;
            
            public int         PatIndex    => Channel.Pattern.Clip.Patterns.IndexOf(Channel.Pattern);

            public float       SongStep    => PatIndex * g_patSteps + Step;

            public long        Time        => (long)(Step * TicksPerStep);
            public long        SongTime    => GetPatTime(PatIndex) + Time;

            public int         FrameLength => (int)(StepLength * TicksPerStep);

            public List<Sound> Sounds;

            public List<Key>   Keys;


            // this is in frames, but it's a float because of the
            // variability that needs to be accounted for
            public float       ArpPlayTime; // this gets offset 


            public Note()
            {
                Channel     = null;
                iChan       = -1;
                            
                Step     = -1;
                            
                Sounds      = new List<Sound>();
                Keys        = new List<Key>();

                Reset();
            }


            public Note(Note note)
            {
                Channel     = note.Channel;
                iChan       = note.iChan;
                Number      = note.Number;
                Volume      = note.Volume;
                Step     = note.Step;
                StepLength  = note.StepLength;

                Sounds = new List<Sound>();

                Keys = new List<Key>();
                foreach (var key in note.Keys)
                    Keys.Add(key);
            }


            public Note(Channel chan, int ch, float vol, int num, float time, float len)
            {
                Channel     = chan;
                iChan       = ch;
                Number      = num;
                Volume      = vol;
                Step     = time;
                StepLength  = len;

                ArpPlayTime = fN;

                Sounds      = new List<Sound>();
                Keys        = new List<Key>();
            }


            public void Reset()
            {
                Number      = 69;
                Volume      = 0;
                StepLength  = 0;

                ArpPlayTime = fN;

                Sounds.Clear();
            }


            public float ShOffset
            {
                get
                {
                         if (Step % 2 == 1   ) return (float)Channel.Shuffle / TicksPerStep;
                    else if (Step % 2 == 1.5f) return (float)Channel.Shuffle / TicksPerStep;
                    else                       return 0;
                }
            }


            public void UpdateStepTime(float dStep)
            {
                Step += dStep;

                //foreach (var snd in Sounds)
                //    snd.Time += (int)(dStep * g_session.TicksPerStep);
            }


            public void UpdateStepLength(float len)
            {
                StepLength = len;

                foreach (var snd in Sounds)
                    snd.Length = (int)(StepLength * TicksPerStep);
            }


            public string Save()
            {
                var save = 
                      WS(Number)
                    + WS(Step)
                    + WS(StepLength)
                    + WS(Volume)
                    +  S(Keys.Count);

                foreach (var key in Keys)
                    save += P(key.Save());
                
                return save;
            }


            public static Note Load(string[] data, ref int i, Instrument inst)
            {
                var note = new Note();

                note.Number     = int  .Parse(data[i++]);
                note.Step    = float.Parse(data[i++]);
                note.StepLength = float.Parse(data[i++]);
                note.Volume     = float.Parse(data[i++]);

                var nKeys = int.Parse(data[i++]);

                for (int k = 0; k < nKeys; k++)
                    note.Keys.Add(Key.Load(data, ref i, inst));

                return note;
            }
        }
    }
}
