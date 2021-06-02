using System.Collections.Generic;
using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public class Pattern
        {
            public Clip      Clip;

            public Channel[] Channels = new Channel[g_nChans];


            public Pattern(Instrument defInst, Clip clip = Clip_null)
            {
                Clip = clip;
                
                for (int i = 0; i < g_nChans; i++)
                    Channels[i] = new Channel(this, defInst);
            }


            public Pattern(Pattern pat)
            {
                Clip = pat.Clip;
                
                for (int i = 0; i < g_nChans; i++)
                    Channels[i] = new Channel(pat.Channels[i], this);
            }


            public void Clear()
            {
                for (int ch = 0; ch < g_nChans; ch++)
                { 
                    Channels[ch].Notes.Clear();
                    Channels[ch].AutoKeys.Clear();
                }
            }
            

            public string Save()
            {
                var nUsed = 0;

                foreach (var chan in Channels)
                    if (!chan.IsDefault) nUsed++;

                var save = S(nUsed);

                foreach (var chan in Channels)
                { 
                    if (!chan.IsDefault)
                        save += P(chan.Save());
                }

                return save;
            }


            public static Pattern Load(string[] data, ref int i)
            {
                var pat = new Pattern(Instruments[0]);

                foreach (var chan in pat.Channels)
                    chan.Instrument = Instruments[0];

                var nChans = int_Parse(data[i++]);

                for (int ch = 0; ch < nChans; ch++)
                { 
                    int iChan;
                    var chan = Channel.Load(data, ref i, out iChan, pat);
                    pat.Channels[iChan] = chan;
                }

                return pat;
            }
        }
    }
}
