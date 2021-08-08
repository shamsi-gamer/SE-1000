using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class Bias : Setting
        {
            public Parameter LowNote,
                             HighNote,
                             Amount,
                             Power;

            public float     CurValue;


            public Bias(Setting parent, Instrument inst, Source src) 
                : base(strBias, parent, Setting_null, inst, src)
            {
                LowNote  = (Parameter)NewSettingFromTag(strLow,  this, inst, src);
                HighNote = (Parameter)NewSettingFromTag(strHigh, this, inst, src);
                Amount   = (Parameter)NewSettingFromTag(strAmt,  this, inst, src);
                Power    = (Parameter)NewSettingFromTag(strPow,  this, inst, src);
            }



            public Bias(Bias bias, Setting parent, Instrument inst, Source src) 
                : base(bias.Tag, parent, bias.Prototype, inst, src)
            {
                LowNote  = new Parameter(bias.LowNote,  this);
                HighNote = new Parameter(bias.HighNote, this);
                Amount   = new Parameter(bias.Amount,   this);
                Power    = new Parameter(bias.Power,    this);
            }



            public Bias Copy(Setting parent) 
            {
                return new Bias(this, parent, Instrument, Source);
            }



            public float UpdateValue(TimeParams tp)
            {
                if (tp.Program.TooComplex) 
                    return 0;

                var lowNum  = LowNote .UpdateValue(tp);
                var highNum = HighNote.UpdateValue(tp);
                var amt     = Amount  .UpdateValue(tp);
                var pow     = Power   .UpdateValue(tp);
                
                var num  = tp.Note.Number / (float)NoteScale;
                var f    = Math.Min(Math.Max(0, (num - lowNum) / (highNum - lowNum)), 1);
                CurValue = GetValue(f);

                m_valid  = True;
                return CurValue;
            }



            public float GetValue(float f)
            {
                var amt = Amount.CurValue;
                var pow = Power .CurValue;

                var low  = Math.Min(Math.Max(0, 1 - amt), 1);
                var high = Math.Min(Math.Max(0, 1 + amt), 1);

                return 
                    amt >= 0
                    ? low  + (float)Math.Pow(  f, 1/pow)*(high-low)
                    : high - (float)Math.Pow(1-f, 1/pow)*(high-low);
            }



            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       LowNote .HasDeepParams(chan, src)
                    || HighNote.HasDeepParams(chan, src)
                    || Amount  .HasDeepParams(chan, src)
                    || Power   .HasDeepParams(chan, src);
            }



            public override void Clear()
            {
                LowNote .Clear();
                HighNote.Clear();
                Amount  .Clear();
                Power   .Clear();
            }



            public override void Reset()
            {
                base.Reset();

                LowNote .Reset();
                HighNote.Reset();
                Amount  .Reset();
                Power   .Reset();
            }



            public override void Randomize()
            {
                LowNote .Randomize();
                HighNote.Randomize();
                Amount  .Randomize();
                Power   .Randomize();
            }



            public override void AdjustFromController(Clip clip)
            {
                var mi = g_remote.MoveIndicator;
                var ri = g_remote.RotationIndicator;

                Program.AdjustFromController(clip, LowNote, -mi.Z*ControlSensitivity);
                Program.AdjustFromController(clip, Amount,   mi.X*ControlSensitivity);

                Program.AdjustFromController(clip, Power,   -ri.X*ControlSensitivity);
                Program.AdjustFromController(clip, HighNote, ri.Y*ControlSensitivity);
            }



            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strLow:  return GetOrAddParamFromTag(LowNote,  tag);
                    case strHigh: return GetOrAddParamFromTag(HighNote, tag);
                    case strAmt:  return GetOrAddParamFromTag(Amount,   tag);
                    case strPow:  return GetOrAddParamFromTag(Power,    tag);
                }

                return Setting_null;
            }



            public void Delete(int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                LowNote .Delete(iSrc);
                HighNote.Delete(iSrc);
                Amount  .Delete(iSrc);
                Power   .Delete(iSrc);
            }



            public override string Save()
            {
                var save =
                      Tag
                    + P(LowNote .Save())
                    + P(HighNote.Save())
                    + P(Amount  .Save())
                    + P(Power   .Save());

                return save;
            }



            public static Bias Load(string[] data, ref int d, Instrument inst, int iSrc, Setting parent)
            {
                var tag = data[d++];

                var bias = new Bias(
                    parent, 
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);

                bias.LowNote  = Parameter.Load(data, ref d, inst, iSrc, bias, bias.LowNote );
                bias.HighNote = Parameter.Load(data, ref d, inst, iSrc, bias, bias.HighNote);
                bias.Amount   = Parameter.Load(data, ref d, inst, iSrc, bias, bias.Amount  );
                bias.Power    = Parameter.Load(data, ref d, inst, iSrc, bias, bias.Power   );

                return bias;
            }



            public override string GetLabel(out float width)
            {
                width = 185;

                return
                      PrintValue(LowNote .Value, 0, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(HighNote.Value, 0, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Amount  .Value, 2, True, 0).PadLeft(5) + strEmpty
                    + PrintValue(Power   .Value, 2, True, 0).PadLeft(4);
            }



            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                if (!_dp.Program.TooComplex)
                {
                    base.DrawLabels(sprites, x, y, dp);

                    if (Amount  .HasDeepParams(CurChannel, CurSrc)) Amount  .DrawLabels(sprites, x, y, dp);
                    if (Power   .HasDeepParams(CurChannel, CurSrc)) Power   .DrawLabels(sprites, x, y, dp);
                    if (LowNote .HasDeepParams(CurChannel, CurSrc)) LowNote .DrawLabels(sprites, x, y, dp);
                    if (HighNote.HasDeepParams(CurChannel, CurSrc)) HighNote.DrawLabels(sprites, x, y, dp);
                }

                _dp.Next(dp);
            }



            public void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, Channel chan, Program prog)
            {
                FillRect(sprites, x, y, w, h, color0);

                var  rh = h - 90;
                var irh = h - 50;


                var dp = new DrawParams(False, prog);

                if (OK(CurSrc)) SelSource    .DrawLabels(sprites, x+5, y+10, dp);
                else            SelInstrument.DrawLabels(sprites, x+5, y+10, dp);


                var isLow  = IsCurParam(strLow);
                var isHigh = IsCurParam(strHigh);
                var isAmt  = IsCurParam(strAmt);
                var isPow  = IsCurParam(strPow);


                var minNote  = 36;
                var maxNote  = 119;

                var lowNote  = (int)LowNote .Value;
                var highNote = (int)HighNote.Value;


                var pw = w - 200; // piano width
                var ow = pw/7;    // octave width

                var ym = 330;

                for (int i = 0; i < 7; i++)
                {
                    DrawOctave(
                        sprites,
                        (w-pw)/2 + i*ow,
                        ym + 10,
                        ow,
                        60,
                        i,
                        minNote,
                        lowNote,
                        highNote);
                }


                var px     = (w-pw)/2 + 4;

                var spread = highNote-lowNote;
                var kw     = pw/(maxNote-minNote+1);
                           
                var amt    = Amount.Value;
                var pow    = Power .Value;
                           
                var low    = Math.Min(Math.Max(0, 1 - amt), 1);
                var high   = Math.Min(Math.Max(0, 1 + amt), 1);
                

                DrawMarker(sprites, px+1 + (lowNote -minNote)*kw, ym, low *100, isLow,  isAmt);
                DrawMarker(sprites, px+1 + (highNote-minNote)*kw, ym, high*100, isHigh, isAmt);


                var strName = strBias;

                     if (isLow ) strName = strLow;
                else if (isHigh) strName = strHigh;
                else if (isAmt ) strName = strAmt;
                else if (isPow ) strName = strPow;

                DrawString(
                    sprites,
                    FullNameFromTag(strName), 
                    px + pw/2, 
                    ym - 200, 
                    1.5f, 
                    color6,
                    TA_CENTER);


                var powColor = isPow ? color6 : color4;
                var powWidth = isPow ? 4 : 2;

                var amtColor = isAmt ? color6 : color4;
                var amtWidth = isAmt ? 4 : 2;

                DrawCurve(
                    sprites, 
                    GetValue,
                    px+1 + (lowNote-minNote)*kw, 
                    ym - 100,
                    spread*kw,
                    100,
                    powColor,
                    powWidth);

                FillRect(sprites, px+1,                         ym - low *100, (lowNote-minNote )*kw, amtWidth, amtColor);
                FillRect(sprites, px+1 + (highNote-minNote)*kw, ym - high*100, (maxNote-highNote)*kw, amtWidth, amtColor);


                DrawString(
                    sprites, 
                    S00(amt), 
                    px + (lowNote-minNote)*kw + (1+amt)*spread*kw/2, 
                    ym - 100 - 47, 
                    0.8f, 
                    isAmt ? color6 : color4,
                    TA_CENTER);

                DrawString(
                    sprites, 
                    S00(pow), 
                    px + (lowNote-minNote)*kw + (1+amt)*spread*kw/2, 
                    ym - 100 + 40, 
                    0.8f, 
                    isPow ? color6 : color4,
                    TA_CENTER);


                DrawFuncButtons(sprites, w, h, chan);
            }



            void DrawOctave(List<MySprite> sprites, float x, float y, float w, float h, int octave, int firstNote, int lowNote, int highNote)
            {
                var kw = w/7;
                var bw = w/12;
                var bh = h*3/5f;


                for (int i = 0, j = 0; i < 7; i++, j++)
                {
                    var isNote = 
                           octave*12 + j == lowNote  - firstNote
                        || octave*12 + j == highNote - firstNote;

                    var kx    = x + i*kw;
                    var color = isNote ? color6 : color4;

                    FillRect(sprites, kx+1, y, kw-2, h, color);

                    if (   i != 2
                        && i != 6) 
                        j++;
                }


                for (int i = 0; i < 12; i++)
                {
                    var isNote = 
                           octave*12 + i == lowNote  - firstNote
                        || octave*12 + i == highNote - firstNote;

                    var color = isNote ? color6 : color1;

                    if (   i ==  1 
                        || i ==  3
                        || i ==  6
                        || i ==  8
                        || i == 10)
                        FillRect(sprites, x+1.3f + i*bw, y, bw, bh, color);
                }
            }



            void DrawMarker(List<MySprite> sprites, float x, float y, float h, bool isNote, bool isAmt)
            {
                DrawLine(
                    sprites, 
                    x, y, 
                    x, y-h, 
                    isNote ? color6 : color4, 
                    isNote ? 3 : 1);

                FillCircle(sprites, x, y-h, 5, isAmt ? color6 : color4);

                //DrawString(
                //    sprites, 
                //    S(num), 
                //    x, 
                //    y + 150, 
                //    0.8f, 
                //    color6,
                //    TA_CENTER);
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                if (CurSetting.Parent.GetType() == typeof(Bias))
                    EditedClip.CurSetting.DrawFuncButtons(sprites, w, y, chan);

                else
                { 
                    DrawFuncButton(sprites, strAmt,  1, w, y, True, Amount  .HasDeepParams(chan, CurSrc));
                    DrawFuncButton(sprites, strPow,  2, w, y, True, Power   .HasDeepParams(chan, CurSrc));
                    DrawFuncButton(sprites, strLow,  3, w, y, True, LowNote .HasDeepParams(chan, CurSrc));
                    DrawFuncButton(sprites, strHigh, 4, w, y, True, HighNote.HasDeepParams(chan, CurSrc));
                }
            }



            public override void Func(int func)
            {
                switch (func)
                {
                    case 1: AddNextSetting(strAmt);  break;
                    case 2: AddNextSetting(strPow);  break;
                    case 3: AddNextSetting(strLow);  break;
                    case 4: AddNextSetting(strHigh); break;
                }
            }



            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}