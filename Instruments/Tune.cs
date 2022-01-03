using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;



namespace IngameScript
{
    partial class Program
    {
        public class Tune : Parameter
        {
            public TuneChord Chord;



            public Tune(Instrument inst, Source src) 
                : base(strTune, -240, 240, -12, 12, 0.5f, 12, 0, False, Setting_null, inst, src)
            {
                Chord = TuneChord_null;
            }



            public Tune(Tune tune) 
                : base(tune, Setting_null)
            {
                Chord = OK(tune.Chord) ? new TuneChord(tune.Chord) : TuneChord_null;
            }



            public Tune Copy()
            {
                return new Tune(this);
            }



            public override float UpdateValue(TimeParams tp)
            {
                if (OK(Chord))
                    Chord.UpdateValue(tp);

                var noteNum = tp.Note.Number;

                var val = base.UpdateValue(tp);

                tp.Note.Number += noteNum;

                return val;
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                if (tag == strChord) return Chord ?? (Chord = new TuneChord(this, Instrument, Source));
                else                 return base.GetOrAddSettingFromTag(tag);
            }



            public override bool HasDeepParams(Channel chan, int src)
            {
                return 
                       base.HasDeepParams(chan, src)
                    || OK(Chord);
            }



            public override void DeleteSetting(Setting setting)
            {
                if (setting == Chord) Chord = TuneChord_null;
                else                  base.DeleteSetting(setting);
            }



            public override void Clear()
            {
                Chord?.Clear();
                Chord = TuneChord_null;

                base.Clear();
            }



            public override void Reset()
            {
                Chord?.Reset();
                base  .Reset();
            }



            public override void Randomize()
            {
                m_value = NormalMin + RND * (NormalMax - NormalMin);


                if (RND > 1/3f) m_value = (int)(m_value/ 7)* 7;
                else            m_value = (int)(m_value/12)*12;                


                // TODO randomize Chord


                if (   !TooComplex
                    && RND > 0.8f)
                {
                    Lfo = new LFO(this, Instrument, Source);
                    Lfo.Randomize();
                }
                else
                { 
                    if (OK(Lfo))
                        g_lfo.Remove(Lfo);

                    Lfo = LFO_null;
                }
            }



            public override void Delete(int iSrc)
            {
                base  .Delete(iSrc);
                Chord?.Delete(iSrc);
            }



            public override string GetLabel(out float width)
            {
                width = 90f; 
                return PrintValue(Value, 2, True, 1).PadLeft(5);
            }



            public override string Save()
            {
                var tune = base.Save();

                var nSettings = OK(Chord) ? 1 : 0;

                tune += PS(nSettings);

                if (OK(Chord))
                    tune += SaveSetting(Chord);

                return tune;
            }



            public static Tune Load(string[] data, ref int d, Instrument inst, int iSrc)
            {
                var tune = new Tune(
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);


                Parameter.Load(data, ref d, inst, iSrc, Setting_null, tune);


                var nSettings = int_Parse(data[d++]);

                while (nSettings-- > 0)
                {
                    switch (data[d])
                    { 
                        case strChord: tune.Chord = TuneChord.Load(data, ref d, inst, iSrc, tune); break;
                    }
                }

                return tune;
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                base.DrawFuncButtons(sprites, w, h, chan);

                DrawFuncButton(sprites, strChord, 1, w, h, True, OK(Chord));
            }



            public override void Func(int func)
            {
                if (func == 1) AddNextSetting(strChord);
                else           base.Func(func);
            }



            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}
