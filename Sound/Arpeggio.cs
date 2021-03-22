namespace IngameScript
{
    partial class Program
    {
        public class Arpeggio : Setting
        {
            public Song      Song;

            public Parameter Length,
                             Scale;


            public Arpeggio(Instrument inst) : base("Arp", null)
            {
                Song = new Song("");
                Song.Arpeggio = this;
                Song.Patterns.Add(new Pattern(Song));
                
                SetInstrument(inst);

                Length = (Parameter)NewSettingFromTag("Len", this);
                Scale  = (Parameter)NewSettingFromTag("Scl", this);
            }


            public Arpeggio(Arpeggio arp) : base(arp.Tag, null, arp.Prototype)
            {
                Song   = new Song(arp.Song);

                Length = new Parameter(arp.Length, this);
                Scale  = new Parameter(arp.Scale,  this);

                //if (arp.Song != null)
                //{ 
                //    Notes = new List<Note>();

                //    foreach (var note in arp.Notes)
                //        Notes.Add(note);

                //    if (arp.Length > 0)
                //        Length = arp.Length;

                //    if (arp.Scale != null)
                //    {
                //        Scale = new Parameter(arp.Scale);
                //        Scale.Parent = this;
                //    }
                //    else
                //        Scale = null;
                //}
                //else
                //{
                //    Notes  =  null;
                //    Length = -Math.Abs(Length); // turn off but keep current value
                //    Scale  =  null;
                //}
            }


            public Arpeggio Copy()
            {
                return new Arpeggio(this);
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return Scale != null;
            }


            public override void Remove(Setting setting)
            {
                if (setting == Scale) Scale  = null;
            }


            public void SetInstrument(Instrument inst)
            {
                foreach (var pat in Song.Patterns)
                    pat.Channels[0].Instrument = inst;
            }


            public override void Randomize()
            {
                Length.Randomize();
                Scale .Randomize();
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Len": return Length ?? (Length = (Parameter)NewSettingFromTag("Len", this));
                    case "Scl": return Scale  ?? (Scale  = (Parameter)NewSettingFromTag("Scl", this));
                }

                return null;
            }


            public override string Save()
            {
                var arp = W(Tag);

                // save song here

                arp += W(Length.Save());
                arp +=   Scale .Save();

                return arp;
            }


            public static Arpeggio Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tag = data[i++];
 
                var arp = new Arpeggio(inst);

                arp.Length = Parameter.Load(data, ref i, inst, iSrc, arp);
                arp.Scale  = Parameter.Load(data, ref i, inst, iSrc, arp);

                return arp;
            }
        }
    }
}
