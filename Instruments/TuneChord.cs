using System;
using System.Linq;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class TuneChord : Parameter
        {
            public List<List<int>> Chords;
            public List<bool>      AllOctaves;

            public List<int>       FinalChord;


            public int             SelectedIndex      => (int)Math.Round(CurValue);
            public List<int>       SelectedChord      => Chords    [SelectedIndex];
            public bool            SelectedAllOctaves => AllOctaves[SelectedIndex];


            public TuneChord(Setting parent, Instrument inst, Source src) 
                : base(strChord, 0, 0, 0, 0, -1, -1, 0, False, parent, inst, src)
            {
                IntBias    = 1; // needed for Mod and LFO as CurValue is used as an int

                Chords     = new List<List<int>>();
                AllOctaves = new List<bool>();

                AddFirstChord();

                FinalChord = new List<int>();
            }



            public TuneChord(TuneChord chord)
                : base(chord, Setting_null)
            {
                Chords     = new List<List<int>>(chord.Chords);
                AllOctaves = new List<bool>(chord.AllOctaves);
                
                FinalChord = new List<int>(chord.FinalChord);
            }



            public TuneChord Copy() 
            {
                return new TuneChord(this);
            }



            public override void SetValue(float val, Note note)
            {
                base.SetValue(val, note);
                UpdateFinalChord();
            }



            public override float UpdateValue(TimeParams tp)
            {
                if (!On)
                    return 0;

                var oldVal = (int)Math.Round(CurValue);
                var val    = base.UpdateValue(tp);

                if ((int)Math.Round(val) != oldVal)
                    UpdateFinalChord();

                return val;
            }



            public void AddFirstChord()
            {
                Chords    .Add(new List<int>());
                AllOctaves.Add(False);
            }



            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Chords.Count > 0
                    || base.HasDeepParams(chan, src);
            }



            public override void Clear()
            {
                Chords    .Clear();
                AllOctaves.Clear();
                
                FinalChord.Clear();
            }



            public override void Randomize()
            {
                base.Randomize();

                // TODO: create random chords
                //AllOctaves = RND > 0.5f;
            }



            public override string Save()
            {
                var save = 
                      base.Save()
                    + PS(Chords.Count);

                for (var c = 0; c < Chords.Count; c++)
                { 
                    var chord = Chords[c];

                    save += PS(chord.Count);

                    foreach (var note in chord)
                        save += PS(note);

                    save += PS(AllOctaves[c] ? "1" : "0");
                }

                return save;
            }



            public static TuneChord Load(string[] data, ref int d, Instrument inst, int iSrc, Setting parent)
            {
                var chord = new TuneChord(
                    parent, 
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);

                chord.Clear();


                Parameter.Load(data, ref d, inst, iSrc, Setting_null, chord);


                int nChords;
                if (!int_TryParse(data[d++], out nChords)) return TuneChord_null;

                for (int c = 0; c < nChords; c++)
                {
                    int nNotes;
                    if (!int_TryParse(data[d++], out nNotes)) return TuneChord_null;

                    var _chord = new List<int>();

                    for (int n = 0; n < nNotes; n++)
                        _chord.Add(int.Parse(data[d++]));

                    chord.Chords.Add(_chord);

                    int allOctaves;
                    if (!int_TryParse(data[d++], out allOctaves)) return TuneChord_null;

                    chord.AllOctaves.Add(allOctaves != 0);
                }


                chord.Max       = 
                chord.NormalMax = chord.Chords.Count-1;

                chord.SetValue(Math.Min(chord.Value, chord.Max), Note_null);


                return chord;
            }



            public override string GetLabel(out float width)
            {
                width = 123;
                return PrintValue(CurValue, 2, True, 0).PadLeft(5);
            }



            public void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, Channel chan, Program prog)
            {
                FillRect(sprites, x, y, w, h, color0);


                var dp = new DrawParams(False, prog);

                if (OK(CurSrc)) SelSource    .DrawLabels(sprites, x+5, y+10, dp);
                else            SelInstrument.DrawLabels(sprites, x+5, y+10, dp);


                var ry = y +  30;
                var rh = h - 110;

                var instHeight = h - 80;

                //var irh = h - 50;
                var ch = rh/8;


                var nOctaves = 2;

                var ow = 100;           // octave width
                var pw = ow * nOctaves; // piano width


                //var minNote   = 36;


                for (int c = 0; c < Chords.Count; c++)
                {
                    for (int i = 0; i < nOctaves; i++)
                    {
                        DrawOctave(
                            sprites,
                            x + (w-pw)/2 + i*ow,
                            ry + c*ch,
                            ow,
                            ch - 10,
                            i + 5,
                            Chords[c].Select(_c => _c/NoteScale).ToArray(),
                            //minNote,
                            color0,
                            SelectedIndex == c ? color3 : color2,
                            SelectedIndex == c ? color6 : color3);
                    }
                }


                // bottom func separator
                FillRect(sprites, x, y + instHeight, w, 1, color6);

                DrawFuncButtons(sprites, w, h, chan);
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                base.DrawFuncButtons(sprites, w, y, chan);
                DrawFuncButton(sprites, /*"┼"*/"+", 1, w, y, False, False);
            }



            public override void Func(int func)
            {
                if (func == 1) AddNewChord();
                else           base.Func(func);
            }



            void AddNewChord()
            {
                if (Chords.Count >= 8)
                    return;

                Chords    .Add(new List<int>());
                AllOctaves.Add(False);

                Max       = 
                NormalMax = Chords.Count-1;

                SetValue(Chords.Count-1, Note_null);
            }



            public void UpdateFinalChord()
            {
                var index = (int)Math.Round(CurValue);

                FinalChord = UpdateFinalTuneChord(
                    Chords    [index], 
                    AllOctaves[index]);
            }



            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}