using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Tune : Parameter
        {
            public bool      UseChord,
                             AllOctaves;

            public List<int> Chord,
                             FinalChord;


            public Tune() : base("Tune", -240, 240, -12, 12, 0.5f, 12, 0, null)
            {
                UseChord   = false;
                AllOctaves = false;

                Chord      = new List<int>();
                FinalChord = new List<int>();
            }


            public Tune(Tune tune) : base(tune, null)
            {
                UseChord   = tune.UseChord;
                AllOctaves = tune.AllOctaves;

                Chord = new List<int>();
                foreach (var note in tune.Chord)
                    Chord.Add(note);

                FinalChord = new List<int>();
                foreach (var note in tune.FinalChord)
                    FinalChord.Add(note);
            }


            public Tune Copy()
            {
                return new Tune(this);
            }


            public override string Save()
            {
                var tune =
                      W(base.Save())
                    + WB(UseChord)
                    + WB(AllOctaves);

                tune += S(Chord.Count);

                for (int i = 0; i < Chord.Count; i++)
                    tune += ";" + S(Chord[i]);

                return tune;
            }


            public static Tune Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tune = new Tune();

                Parameter.Load(data, ref i, inst, iSrc, null, tune);

                tune.UseChord   = data[i++] == "1";
                tune.AllOctaves = data[i++] == "1";

                var nChords = int.Parse(data[i++]);

                for (int j = 0; j < nChords; j++)
                    tune.Chord.Add(int.Parse(data[i++]));

                return tune;
            }
        }
    }
}
