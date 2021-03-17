using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Instrument
        {
            public string       Name;

            public Parameter    Volume;

            public Tune         Tune;

            public Filter       Filter;

            public Delay        Delay;
            public Arpeggio     Arpeggio;


            public List<Source> Sources;


            public Instrument()
            {
                Name     = "New Sound";
                         
                Volume   = (Parameter)NewSettingFromTag("Vol", null);

                Tune     = null;
                Filter   = null;
                
                Delay    = null;
                Arpeggio = null;
                         
                Sources  = new List<Source>();
            }


            public Instrument(string name) : this()
            {
                Name = name;
            }


            public Instrument(Instrument inst)
            {
                Name = inst.Name;

                Volume   = new Parameter(inst.Volume, null);

                Tune     = inst.Tune    ?.Copy();
                Filter   = inst.Filter  ?.Copy();

                Delay    = inst.Delay   ?.Copy();
                Arpeggio = inst.Arpeggio?.Copy();

                Sources = new List<Source>();
                foreach (var src in inst.Sources)
                    Sources.Add(new Source(src, this));
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


            //public Setting NewSetting(string tag)
            //{
            //    switch (tag)
            //    {
            //        case "Vol":  return Volume;
            //        case "Tune": return Tune;
            //        case "Flt":  return Filter;
            //        case "Del":  return Delay;
            //        case "Arp":  return Arpeggio;
            //    }

            //    return null;
            //}


            public string Save()
            {
                var inst = N(
                      W (Name)
                    + WS(Sources.Count)

                    + Volume.Save()

                    + Program.Save(Tune)
                    + Program.Save(Filter)
                    + Program.Save(Delay)
                    + Program.Save(Arpeggio));

                for (int i = 0; i < Sources.Count; i++)
                    inst += N(Sources[i].Save());

                return inst;
            }


            public static Instrument Load(string[] lines, ref int line)
            {
                var data = lines[line++].Split(';');
                var i    = 0;

                var inst = new Instrument();

                inst.Name = data[i++];

                var nSources = int.Parse(data[i++]);

                inst.Volume = Parameter.Load(data, ref i, null);

                while (i < data.Length
                    && (   data[i] == "Tune" 
                        || data[i] == "Flt" 
                        || data[i] == "Del" 
                        || data[i] == "Arp"))
                {
                    Log("data[" + i + "] = " + data[i]);
                    switch (data[i])
                    { 
                        case "Tune": inst.Tune     = Tune    .Load(data, ref i);       break;
                        case "Flt":  inst.Filter   = Filter  .Load(data, ref i);       break;
                        case "Del":  inst.Delay    = Delay   .Load(data, ref i);       break;
                        case "Arp":  inst.Arpeggio = Arpeggio.Load(data, ref i, inst); break;
                    }
                }

                for (int j = 0; j < nSources; j++)
                    inst.Sources.Add(Source.Load(lines, ref line, inst));

                return inst;
            }
        }
    }
}
