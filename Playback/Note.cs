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

            public bool        Accent;

            public float       Step,
                               StepLength;
            
            public Clip        Clip        => Channel.Pattern.Clip;
            public int         PatIndex    => Clip.Patterns.IndexOf(Channel.Pattern);

            public float       ClipStep    => PatIndex * g_patSteps + Step;

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
                Channel =  Channel_null;
                iChan   = -1;
                        
                Step    = -1;
                            
                Sounds  = new List<Sound>();
                Keys    = new List<Key>();

                Reset();
            }



            public Note(Note note, Channel chan = Channel_null, int chanIndex = -1)
            {
                Channel    = chan ?? note.Channel;
                iChan      = OK(chanIndex) ? chanIndex : note.iChan;
                Number     = note.Number;
                Volume     = note.Volume;
                Accent     = note.Accent;
                Step       = note.Step;
                StepLength = note.StepLength;

                Sounds = new List<Sound>();

                Keys = new List<Key>();
                foreach (var key in note.Keys)
                    Keys.Add(new Key(key));
            }



            public Note(Channel chan, int ch, float vol, bool acc, int num, float time, float len)
            {
                Channel     = chan;
                iChan       = ch;
                Number      = num;
                Volume      = vol;
                Accent      = acc;
                Step        = time;
                StepLength  = len;

                ArpPlayTime = float_NaN;

                Sounds      = new List<Sound>();
                Keys        = new List<Key>();
            }



            public void Reset()
            {
                Number      = 69;
                Volume      = 0;
                Accent      = False;
                StepLength  = 0;

                ArpPlayTime = float_NaN;

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
                    + WS(SaveToggles())
                    +  S(Keys.Count);

                foreach (var key in Keys)
                    save += P(key.Save());
                
                return save;
            }



            uint SaveToggles()
            {
                uint f = 0;
                var  i = 0;

                WriteBit(ref f, Accent, i++);

                return f;
            }



            public static Note Load(string[] data, ref int i, Instrument inst)
            {
                var note = new Note();

                note.Number     = int  .Parse(data[i++]);
                note.Step       = float.Parse(data[i++]);
                note.StepLength = float.Parse(data[i++]);
                note.Volume     = float.Parse(data[i++]);

                note.LoadToggles(data[i++]);

                var nKeys = int_Parse(data[i++]);

                for (int k = 0; k < nKeys; k++)
                    note.Keys.Add(Key.Load(data, ref i));

                return note;
            }



            bool LoadToggles(string toggles)
            {
                uint f;
                if (!uint.TryParse(toggles, out f)) return False;

                var i = 0;

                Accent = ReadBit(f, i++);

                return True;
            }
        }
    }
}
