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


            public Tune(Instrument inst, Source src) 
                : base(strTune, -240, 240, -12, 12, 0.5f, 12, 0, null, inst, src)
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


            public override void Randomize(Program prog)
            {
                m_value = NormalMin + RND * (NormalMax - NormalMin);

                if (RND > 1/3f) m_value = (int)(m_value/ 7)* 7;
                else            m_value = (int)(m_value/12)*12;                


                if (   !prog.TooComplex
                    && !AnyParentIsEnvelope
                    && (  !IsDigit(Tag[0]) && RND > 0.5f
                        || IsDigit(Tag[0]) && RND > 0.9f))
                {
                    Envelope = new Envelope(this, Instrument, Source);
                    Envelope.Randomize(prog);
                }
                else 
                    Envelope = null;


                if (   !prog.TooComplex
                    && RND > 0.8f)
                {
                    Lfo = new LFO(this, Instrument, Source);
                    Lfo.Randomize(prog);
                }
                else
                { 
                    if (Lfo != null)
                        g_lfo.Remove(Lfo);

                    Lfo = null;
                }
            }


            public override string GetLabel(out float width)
            {
                width = 90f; 
                return printValue(Value, 2, true, 1).PadLeft(5);
            }


            public override string Save()
            {
                var tune =
                      W(base.Save())
                    + WB(UseChord)
                    + WB(AllOctaves);

                tune += S(Chord.Count);

                for (int i = 0; i < Chord.Count; i++)
                    tune += PS(Chord[i]);

                return tune;
            }


            public static Tune Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tune = new Tune(
                    inst, 
                    iSrc > -1 ? inst.Sources[iSrc] : null);

                Parameter.Load(data, ref i, inst, iSrc, null, tune);

                tune.UseChord   = data[i++] == "1";
                tune.AllOctaves = data[i++] == "1";

                var nChords = int.Parse(data[i++]);

                for (int j = 0; j < nChords; j++)
                    tune.Chord.Add(int.Parse(data[i++]));

                return tune;
            }


            public override bool CanDelete()
            {
                return true;
            }
        }
    }
}
