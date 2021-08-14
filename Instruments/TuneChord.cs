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



            //public float UpdateValue(TimeParams tp)
            //{
            //    if (tp.Program.TooComplex) 
            //        return 0;

            //    //var param = (Parameter)Parent;


            //    // get connected value

            //    var val = 0f;

            //    for (int i = 0; i < ModSettings.Count; i++)
            //    {
            //        var set  = ModSettings   [i];
            //        var src  = ModSources    [i];
            //        var inst = ModInstruments[i];

            //        if (OK(set))
            //        {
            //                 if (set.GetType() == typeof(Parameter)) val = Math.Max(val, ((Parameter)set).CurValue);
            //            else if (set.GetType() == typeof(LFO      )) val = Math.Max(val, ((LFO      )set).CurValue);
            //            else if (set.GetType() == typeof(Envelope )) val = Math.Max(val, ((Envelope )set).CurValue);
            //            else if (set.GetType() == typeof(Modulate )) val = Math.Max(val, ((Modulate )set).CurValue);
            //            // TODO add more that have CurValue
            //        }

            //        else if (OK(src))  val = Math.Max(val, src .CurVolume);
            //        else if (OK(inst)) val = Math.Max(val, inst.CurVolume);
            //        else               val = 0;
            //    }


            //    if (Op == ModOp.Set)
            //    {
            //        // replace value with connected

            //        CurValue = val;
            //    }
            //    else
            //    { 
            //        // modify value with connected
                
            //        var amt = Amount .UpdateValue(tp);
            //        var att = Attack .UpdateValue(tp);
            //        var rel = Release.UpdateValue(tp);
                
            //        if (att == 0) att = 0.000001f;
            //        if (rel == 0) rel = 0.000001f;

            //        var  cv  = Math.Abs(CurValue);
            //        var _amt = Math.Abs(amt);

            //        var a = Math.Min(   cv + val*_amt/FPS/att, _amt);
            //        var r = Math.Max(0, cv -     _amt/FPS/rel);

            //        CurValue = Math.Sign(amt) * (r + (a - r) * val);
            //    }

            //    m_valid  = True;
            //    return CurValue;
            //}



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



            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                //var isAmt = IsCurParam(strAmt);
                //var isAtt = IsCurParam(strAtt);
                //var isRel = IsCurParam(strRel);


                //var w0 = 240f;
                //var h0 = 120f;

                //var x0 = x + w/2 - w0/2;
                //var y0 = y + h/2 - h0/4;

                //Vector2 p0, p1, p2;

                //GetEnvelopeCoords(x0, y0, w0, h0, False, out p0, out p1, out p2);
                //DrawEnvelopeSupports(sprites, p0, w0, y0, h0);


                //FillRect(sprites, p0.X, y0 + h0/2, w0, -CurValue*h/4, color3);


                //GetEnvelopeCoords(x0, y0, w0, h0, True, out p0, out p1, out p2);
                //DrawEnvelope(sprites, p0, p1, p2, color3, False, False, False);

                //GetEnvelopeCoords(x0, y0, w0, h0, False, out p0, out p1, out p2);
                //DrawEnvelope(sprites, p0, p1, p2, color5, isAmt, isAtt, isRel);


                //var strFrom = "from\n";

                //if (ModSettings.Count == 0)
                //    strFrom += "...";

                //else
                //{
                //    for (int i = 0; i < ModSettings.Count; i++)
                //    {
                //        var set  = ModSettings   [i];
                //        var src  = ModSources    [i];
                //        var inst = ModInstruments[i];
                        
                //        strFrom += "\n";

                //             if (OK(set))  strFrom += set.Path;
                //        else if (OK(src))  strFrom += inst.Name + "/" + src.Index;
                //        else if (OK(inst)) strFrom += inst.Name;
                //    }
                //}

                //DrawString(sprites, strFrom, x0 + w0/2, y + h/2 - h0/2 - 80, 0.5f, color5, TA_CENTER);
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                DrawFuncButton(sprites, strSel, 1, w, y, True, Selected.HasDeepParams(chan, CurSrc));
            }



            public override void Func(int func)
            {
                switch (func)
                {
                    case 1: AddNextSetting(strSel); break;
                }
            }



            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}