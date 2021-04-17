using System;
using System.Text;
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
                               
            public Instrument  Instrument  { get { return Channel.Instrument; } }

            public float       PatStep,
                               StepLength;
            
            public int         PatIndex    { get { return Channel.Pattern.Song.Patterns.IndexOf(Channel.Pattern); } }

            //public Song        Song        { get { return Channel.Pattern.Song; } }
            public float       SongStep    { get { return PatIndex * g_nSteps + PatStep; } }

            public long        PatTime     { get { return (long)(PatStep * g_ticksPerStep); } }
            public long        SongTime    { get { return GetPatTime(PatIndex) + PatTime; } }

            public int         FrameLength { get { return (int)(StepLength * g_ticksPerStep); } }

            public float       Volume;

            public List<Sound> Sounds;

            public List<Key>   Keys;


            // this is in frames, but it's a float because of the
            // variability that needs to be accounted for
            public float       ArpPlayTime; // this gets offset 


            public Note()
            {
                Channel     = null;
                iChan       = -1;
                            
                PatStep     = -1;
                            
                Sounds      = new List<Sound>();
                Keys        = new List<Key>();

                Reset();
            }


            public Note(Note note)
            {
                Channel     = note.Channel;
                iChan       = note.iChan;
                Number      = note.Number;
                PatStep     = note.PatStep;
                StepLength  = note.StepLength;
                Volume      = note.Volume;

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
                PatStep     = time;
                StepLength  = len;
                Volume      = vol;

                ArpPlayTime = fN;

                Sounds      = new List<Sound>();
                            
                Keys        = new List<Key>();
            }


            public void Reset()
            {
                Number      = 69;
                StepLength  = 0;
                Volume      = 0;

                ArpPlayTime = fN;

                Sounds.Clear();
            }


            public float ShOffset
            {
                get
                {
                         if (PatStep % 2 == 1   ) return (float)Channel.Shuffle / g_ticksPerStep;
                    else if (PatStep % 2 == 1.5f) return (float)Channel.Shuffle / g_ticksPerStep;
                    else                          return 0;
                }
            }


            public void UpdateStepTime(float dStep)
            {
                PatStep += dStep;

                foreach (var snd in Sounds)
                    snd.Time += (int)(dStep * g_ticksPerStep);
            }


            public void UpdateStepLength(float len)
            {
                StepLength = len;

                foreach (var snd in Sounds)
                    snd.Length = (int)(StepLength * g_ticksPerStep);
            }


            public string Save()
            {
                var save = 
                      WS(Number)
                    + WS(PatStep)
                    + WS(StepLength)
                    + WS(Volume)
                    +  S(Keys.Count);

                foreach (var key in Keys)
                    save += ";" + key.Save();
                
                return save;
            }


            public static Note Load(string[] data, ref int i, Instrument inst)
            {
                var note = new Note();

                note.Number     = int  .Parse(data[i++]);
                note.PatStep    = float.Parse(data[i++]);
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
