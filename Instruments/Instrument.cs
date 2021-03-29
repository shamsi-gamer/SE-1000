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


            public void Randomize(Program prog)
            {
                Sources.Clear();
                var nSrc = Math.Max(1, (int)Math.Round(Math.Pow(RND, 1.5) * MaxSources/2));

                var used = new List<Oscillator>();

                for (int i= 0; i < nSrc; i++)
                { 
                    var src = new Source(this);
                 
                    Sources.Add(src);
                    src.Randomize(used, prog);
                }

                Volume.SetValue(1, null, -1);//.Randomize(Program prog);

                //if (RND > 0.7f)
                //{
                //    Tune = new Tune();
                //    Tune.Randomize(Program prog);
                //}
                //else
                    Tune = null;
                
                if (   RND > 0.7f
                    && Sources.Find(s => s.Harmonics != null) != null)
                {
                    Filter = new Filter();
                    Filter.Randomize(prog);
                }
                else
                    Filter = null;

                if (RND > 0.7f)
                {
                    Delay = new Delay();
                    Delay.Randomize(prog);
                }
                else
                    Delay = null;

                //if (RND > 0.7f)
                //{
                //    Arpeggio = new Arpeggio(this);
                //    Arpeggio.Randomize(Program prog);
                //}
                //else
                    Arpeggio = null;
            }


            public Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Vol":  return Volume;
                    case "Tune": return Tune     ?? (Tune     = new Tune());
                    case "Flt":  return Filter   ?? (Filter   = new Filter());
                    case "Del":  return Delay    ?? (Delay    = new Delay());
                    case "Arp":  return Arpeggio ?? (Arpeggio = new Arpeggio((Instrument)null));
                }

                return null;
            }


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

                inst.Volume = Parameter.Load(data, ref i, inst, -1, null);

                while (i < data.Length
                    && (   data[i] == "Tune" 
                        || data[i] == "Flt" 
                        || data[i] == "Del" 
                        || data[i] == "Arp"))
                {
                    switch (data[i])
                    { 
                        case "Tune": inst.Tune     = Tune    .Load(data, ref i, inst, -1); break;
                        case "Flt":  inst.Filter   = Filter  .Load(data, ref i, inst, -1); break;
                        case "Del":  inst.Delay    = Delay   .Load(data, ref i, inst, -1); break;
                        case "Arp":  inst.Arpeggio = Arpeggio.Load(data, ref i, inst, -1); break;
                    }
                }

                for (int j = 0; j < nSources; j++)
                    Source.Load(lines, ref line, inst, j);

                return inst;
            }
        }
    }
}
