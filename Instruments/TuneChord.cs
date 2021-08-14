using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class TuneChord : Setting
        {
            public List<List<int>> Chords;
            public List<bool>      AllOctaves;

            public Parameter       Selected;
                                   
            public List<int>       FinalChord;



            public TuneChord(Setting parent, Instrument inst, Source src) 
                : base(strChord, parent, Setting_null, inst, src)
            {
                Chords     = new List<List<int>>();
                AllOctaves = new List<bool>();

                AddFirstChord();

                Selected   = (Parameter)NewSettingFromTag(strSel, this, inst, src);

                FinalChord = new List<int>();
            }



            public TuneChord(TuneChord chord, Setting parent, Instrument inst, Source src) 
                : base(chord.Tag, parent, chord.Prototype, inst, src)
            {
                Chords     = new List<List<int>>(chord.Chords);
                AllOctaves = new List<bool>(chord.AllOctaves);
                
                Selected   = new Parameter(chord.Selected, this);

                FinalChord = new List<int>(chord.FinalChord);
            }



            public TuneChord Copy(Setting parent) 
            {
                return new TuneChord(this, parent, Instrument, Source);
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
                    || Selected.HasDeepParams(chan, src);
            }



            public override void Clear()
            {
                Chords    .Clear();
                AllOctaves.Clear();
                
                Selected  .Clear();
                
                FinalChord.Clear();
            }



            public override void Reset()
            {
                base.Reset();
                Selected.Reset();
            }



            public override void Randomize()
            {
                // TODO: create random chords
                //AllOctaves = RND > 0.5f;
                Selected.Randomize();
            }



            public override void AdjustFromController(Clip clip)
            {
                Program.AdjustFromController(clip, Selected, -g_remote.RotationIndicator.X*ControlSensitivity);
            }



            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strSel: return GetOrAddParamFromTag(Selected, tag);
                }

                return Setting_null;
            }



            public void Delete(int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Selected.Delete(iSrc);
            }



            public override string Save()
            {
                var save = 
                      Tag
                    + PS(Chords.Count);

                for (var c = 0; c < Chords.Count; c++)
                { 
                    var chord = Chords[c];

                    save += PS(chord.Count);

                    foreach (var note in chord)
                        save += PS(note);

                    save += PS(AllOctaves[c] ? "1" : "0");
                }

                save += P(Selected.Save());

                return save;
            }



            public static TuneChord Load(string[] data, ref int d, Instrument inst, int iSrc, Setting parent)
            {
                var tag = data[d++];

                var chord = new TuneChord(
                    parent, 
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);


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

                chord.Selected = Parameter.Load(data, ref d, inst, iSrc, chord, chord.Selected);


                return chord;
            }



            public override string GetLabel(out float width)
            {
                width = 123;

                return PrintValue(Selected.Value, 2, True, 0).PadLeft(5);
            }



            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                if (!_dp.Program.TooComplex)
                {
                    base.DrawLabels(sprites, x, y, dp);
                    if (Selected.HasDeepParams(CurChannel, CurSrc)) Selected.DrawLabels(sprites, x, y, dp);
                }

                _dp.Next(dp);
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

                var ow = 100; // octave width
                var pw = ow * nOctaves; // piano width


                var minNote = 36;
                var maxNote = 119;

                for (int c = 0; c < Chords.Count; c++)
                {
                    //FillRect(
                    //    sprites, 
                    //    x + w/2 - 100, 
                    //    ry + c*ch,
                    //    200,
                    //    ch - 10,
                    //    (int)Selected.CurValue == c ? color4 : color2);

                    for (int i = 0; i < nOctaves; i++)
                    {
                        DrawOctave(
                            sprites,
                            x + (w-pw)/2 + i*ow,
                            ry + c*ch,
                            ow,
                            ch - 10,
                            i,
                            minNote,
                            minNote,
                            maxNote);
                    }
                }


                //var dp = new DrawParams(False, prog);

                //if (OK(CurSrc)) SelSource    .DrawLabels(sprites, x+5, y+10, dp);
                //else            SelInstrument.DrawLabels(sprites, x+5, y+10, dp);


                //var isLow  = IsCurParam(strLow);
                //var isHigh = IsCurParam(strHigh);
                //var isAmt  = IsCurParam(strAmt);
                //var isPow  = IsCurParam(strPow);


                //var minNote  = 36;
                //var maxNote  = 119;

                //var lowNote  = (int)LowNote .Value;
                //var highNote = (int)HighNote.Value;


                //var pw = w - 200; // piano width
                //var ow = pw/7;    // octave width

                //var ym = 330;

                //for (int i = 0; i < 7; i++)
                //{
                //    DrawOctave(
                //        sprites,
                //        (w-pw)/2 + i*ow,
                //        ym + 10,
                //        ow,
                //        60,
                //        i,
                //        minNote,
                //        lowNote,
                //        highNote);
                //}


                //var px     = (w-pw)/2 + 4;

                //var spread = highNote-lowNote;
                //var kw     = pw/(maxNote-minNote+1);

                //var amt    = Amount.Value;
                //var pow    = Power .Value;

                //var low    = Math.Min(Math.Max(0, 1 - amt), 1);
                //var high   = Math.Min(Math.Max(0, 1 + amt), 1);


                //DrawMarker(sprites, px+1 + (lowNote -minNote)*kw, ym, low *100, isLow,  isAmt);
                //DrawMarker(sprites, px+1 + (highNote-minNote)*kw, ym, high*100, isHigh, isAmt);


                //var strName = strBias;

                //     if (isLow ) strName = strLow;
                //else if (isHigh) strName = strHigh;
                //else if (isAmt ) strName = strAmt;
                //else if (isPow ) strName = strPow;

                //DrawString(
                //    sprites,
                //    FullNameFromTag(strName), 
                //    px + pw/2, 
                //    ym - 200, 
                //    1.5f, 
                //    color6,
                //    TA_CENTER);


                //var powColor = isPow ? color6 : color4;
                //var powWidth = isPow ? 4 : 2;

                //var amtColor = isAmt ? color6 : color4;
                //var amtWidth = isAmt ? 4 : 2;

                //DrawCurve(
                //    sprites, 
                //    GetValue,
                //    px+1 + (lowNote-minNote)*kw, 
                //    ym - 100,
                //    spread*kw,
                //    100,
                //    powColor,
                //    powWidth);

                //FillRect(sprites, px+1,                         ym - low *100, (lowNote-minNote )*kw, amtWidth, amtColor);
                //FillRect(sprites, px+1 + (highNote-minNote)*kw, ym - high*100, (maxNote-highNote)*kw, amtWidth, amtColor);


                //DrawString(
                //    sprites, 
                //    S00(amt), 
                //    px + (lowNote-minNote)*kw + (1+amt)*spread*kw/2, 
                //    ym - 100 - 47, 
                //    0.8f, 
                //    isAmt ? color6 : color4,
                //    TA_CENTER);

                //DrawString(
                //    sprites, 
                //    S00(pow), 
                //    px + (lowNote-minNote)*kw + (1+amt)*spread*kw/2, 
                //    ym - 100 + 40, 
                //    0.8f, 
                //    isPow ? color6 : color4,
                //    TA_CENTER);


                // bottom func separator
                FillRect(sprites, x, y + instHeight, w, 1, color6);

                DrawFuncButtons(sprites, w, h, chan);
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                DrawFuncButton(sprites, strSel, 1, w, y, True, Selected.HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, "┼",  5, w, y, False, False);
            }



            public override void Func(int func)
            {
                switch (func)
                {
                    case 1: AddNextSetting(strSel); break;
                    case 5: AddNewChord();          break;
                }
            }



            void AddNewChord()
            {
                if (Chords.Count >= 8)
                    return;

                Chords    .Add(new List<int>());
                AllOctaves.Add(False);
            }



            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}