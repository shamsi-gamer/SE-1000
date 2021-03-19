using System.Collections.Generic;
using System;


namespace IngameScript
{
    partial class Program
    {
        public class Harmonics : Setting
        {
            public enum Preset { Sine, Saw4, Saw8, Saw16, Saw24, Square4, Square8, Square16, Square24, Flat4, Flat8, Flat16, Flat24, Random4, Random8, Random16, Random24 };

            public Parameter[] Tones = new Parameter[24];

            public Preset CurPreset;
            public int    CurTone;


            public Harmonics() : base("Hrm", null)
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i] = NewHarmonicParam(i, this);

                CurPreset = Preset.Sine;
                CurTone   = 0;
            }


            public Harmonics(Harmonics hrm) : base(hrm.Tag, null, hrm)
            {
                for (int i = 0; i < hrm.Tones.Length; i++)
                    Tones[i] = new Parameter(hrm.Tones[i], this);

                CurPreset = hrm.CurPreset;
                CurTone   = hrm.CurTone;
            }


            public Harmonics Copy()
            {
                return new Harmonics(this);
            }


            public void SetPreset(Preset preset)
            {
                switch (preset)
                {
                case Preset.Sine:     FillSine();            break;
                                                                 
                case Preset.Saw4:     FillSaw( 4);           break;
                case Preset.Saw8:     FillSaw( 8);           break;
                case Preset.Saw16:    FillSaw(16);           break;
                case Preset.Saw24:    FillSaw(24);           break;
                                                                 
                case Preset.Square4:  FillSquare( 4);        break;
                case Preset.Square8:  FillSquare( 8);        break;
                case Preset.Square16: FillSquare(16);        break;
                case Preset.Square24: FillSquare(24);        break;
                                                                 
                case Preset.Flat4:    FillFlat( 4);          break;
                case Preset.Flat8:    FillFlat( 8);          break;
                case Preset.Flat16:   FillFlat(16);          break;
                case Preset.Flat24:   FillFlat(24);          break;

                case Preset.Random4:  FillRandom( 4, g_rnd); break;
                case Preset.Random8:  FillRandom( 8, g_rnd); break;
                case Preset.Random16: FillRandom(16, g_rnd); break;
                case Preset.Random24: FillRandom(24, g_rnd); break;
                }
            }


            public void FillSine()
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i == 0 ? 1 : 0, null, -1);
            }


            public void FillSaw(int nTones)
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i >= nTones ? 0 : 1f/(i+1), null, -1);
            }


            public void FillSquare(int nTones)
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i >= nTones || i%2 != 0 ? 0 : 1f/(i+1), null, -1);
            }


            public void FillFlat(int nTones)
            {
                var div = (float)Math.Log(nTones, 2);

                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i >= nTones ? 0 : 1f/div, null, -1);
            }


            public void FillRandom(int nTones, Random rnd)
            {
                var prev = 0f;

                for (int i = 0; i < Tones.Length; i++)
                { 
                    var val = (float)rnd.NextDouble();
                    prev = (prev + val)/2;
                    Tones[i].SetValue(i >= nTones || rnd.NextDouble() < 0.5 ? 0 : val, null, -1);
                }
            }


            public void CreateSounds(List<Sound> sounds, Source src, Note note, int noteNum, long sndTime, int sndLen, int relLen, float vol, List<TriggerValue> triggerValues)
            {
                var inst = src.Instrument;
                var iSrc = inst.Sources.IndexOf(src);

                Sound snd0 = null;

                for (int i = 0; i < Tones.Length; i++)
                {
                    var _noteNum = freq2note(note2freq(noteNum) * (i+1));

                    if (   _noteNum <  12*NoteScale
                        || _noteNum > 150*NoteScale)
                        continue;

                    var sample = src.GetSample(_noteNum);
                    var hrmPos = i / (float)Tones.Length;

                    var snd = new Sound(
                        sample, 
                        note.Channel,
                        note.iChan,
                        sndTime,
                        sndLen,
                        relLen,
                        0, // is set below
                        inst,
                        iSrc,
                        note,
                        triggerValues,
                        false,
                        null,
                        0,
                        Tones[i],
                        snd0,
                        hrmPos);

                    if (i == 0) snd0 = snd;

                    var lTime = g_time - sndTime;
                    var sTime = StartTime > -1 ? g_time - StartTime : lTime;

                    Tones[i].CurValue = Tones[i].GetValue(g_time, lTime, sTime, sndLen, note, iSrc, snd.TriggerValues) * vol;
                    Tones[i].CurValue = ApplyFilter(Tones[i].CurValue, src, hrmPos, g_time, lTime, sTime, sndLen, note, iSrc, snd.TriggerValues); 

                    snd.TriggerVolume = Tones[i].CurValue;

                    sounds.Add(snd);
                }
            }


            public override void Clear()
            {
                foreach (var tone in Tones)
                    tone.Clear();
            }


            public override void Randomize()
            {
                SetPreset((Preset)g_rnd.Next(0, 17));

                if (g_rnd.NextDouble() > 0.5)
                    Smooth();

                for (int i = 0; i < Tones.Length; i++)
                {
                    var tone = Tones[i];

                    if (g_rnd.NextDouble() > 0.9f) tone.Randomize();
                    else                           tone.Clear();
                }
            }


            public void Smooth()
            { 
                var values = new float[Tones.Length];

                for (var i = 0; i < values.Length; i++)
                { 
                         if (i == 0)               values[i] = (Tones[i  ].Value + Tones[i+1].Value)/2;
                    else if (i == values.Length-1) values[i] = (Tones[i-1].Value + Tones[i  ].Value)/2;
                    else                           values[i] = 0.25f*Tones[i-1].Value + 0.5f*Tones[i].Value + 0.25f*Tones[i+1].Value;          
                }

                for (var i = 0; i < values.Length; i++)
                    Tones[i].SetValue(values[i], null, -1);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                return Tones[int.Parse(tag)];
            }


            public override string Save()
            {
                var hrm = W(Tag);

                for (int i = 0; i < Tones.Length; i++)
                    hrm += W(Tones[i].Save());

                hrm += WS((int)CurPreset);
                hrm +=  S(CurTone);
                
                return hrm;
            }


            public static Harmonics Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tag = data[i++];

                var hrm = new Harmonics();

                for (int j = 0; j < hrm.Tones.Length; j++)
                    hrm.Tones[j] = Parameter.Load(data, ref i, inst, iSrc, hrm);
                
                hrm.CurPreset = (Preset)int.Parse(data[i++]);
                hrm.CurTone   = int.Parse(data[i++]);

                return hrm;
            }
        }
    }
}
