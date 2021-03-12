using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Instrument
        {
            public string       Name;

            public List<Source> Sources;

            public Parameter    Volume;

            public Tune         Tune;
            public Filter       Filter;

            public Delay        Delay;
            public Arpeggio     Arpeggio;


            public Instrument()
            {
                Name     = "New Sound";
                         
                Sources  = new List<Source>();
                         
                Volume   = new Parameter("Volume", "Vol", 0, 2, 0.5f, 1, 0.01f, 0.1f, 1);
                Volume.Envelope = new Envelope();
                Volume.Envelope.Parent = Volume;

                Tune     = null;
                Filter   = null;
                
                Delay    = null;
                Arpeggio = null;
            }


            public Instrument(string name) : this()
            {
                Name = name;
            }


            public Instrument(Instrument inst)
            {
                Name = inst.Name;

                Sources = new List<Source>();
                foreach (var src in inst.Sources)
                    Sources.Add(new Source(src, this));

                Volume   = new Parameter(inst.Volume);

                Tune     = inst.Tune     != null ? new Tune    (inst.Tune    ) : null;
                Filter   = inst.Filter   != null ? new Filter  (inst.Filter  ) : null;
                
                Delay    = inst.Delay    != null ? new Delay   (inst.Delay   ) : null;
                Arpeggio = inst.Arpeggio != null ? new Arpeggio(inst.Arpeggio) : null;
            }


            public void MakeValid()
            {
                Volume.MakeValid();
            }


            public void Randomize()
            {
                Sources.Clear();
                var nSrc = Math.Max(1, (int)Math.Round(Math.Pow(g_rnd.NextDouble(), 1.5) * MaxSources/2));

                var used = new List<Oscillator>();

                for (int i= 0; i < nSrc; i++)
                { 
                    var src = new Source(this);
                 
                    Sources.Add(src);
                    src.Randomize(used);
                }

                Volume.SetValue(1, null, -1);//.Randomize();

                //if (g_rnd.NextDouble() > 0.7f)
                //{
                //    Tune = new Tune();
                //    Tune.Randomize();
                //}
                //else
                    Tune = null;
                
                if (   g_rnd.NextDouble() > 0.7f
                    && Sources.Find(s => s.Harmonics != null) != null)
                {
                    Filter = new Filter();
                    Filter.Randomize();
                }
                else
                    Filter = null;

                if (g_rnd.NextDouble() > 0.7f)
                {
                    Delay = new Delay();
                    Delay.Randomize();
                }
                else
                    Delay = null;

                //if (g_rnd.NextDouble() > 0.7f)
                //{
                //    Arpeggio = new Arpeggio(this);
                //    Arpeggio.Randomize();
                //}
                //else
                    Arpeggio = null;
            }
        }
    }
}
