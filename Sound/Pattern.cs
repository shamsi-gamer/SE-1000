using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Pattern
        {
            public Song          Song;

            public List<Channel> Channels;


            public Pattern(Song song = null)
            {
                Song = song;
                
                Channels = new List<Channel>();
                for (int i = 0; i < nChans; i++)
                    Channels.Add(new Channel(this));
            }


            public Pattern(Pattern pat)
            {
                Song = pat.Song;
                
                Channels = new List<Channel>();
                for (int i = 0; i < nChans; i++)
                { 
                    Channels.Add(new Channel(pat.Channels[i]));
                    Channels[i].Pattern = this;
                }
            }


            public Pattern(Song song, Instrument inst) : this(song)
            {
                foreach (var c in Channels)
                    c.Instrument = inst;
            }


            public void Clear()
            {
                for (int ch = 0; ch < nChans; ch++)
                { 
                    Channels[ch].Notes.Clear();
                    Channels[ch].AutoKeys.Clear();
                }
            }
        }
    }
}
