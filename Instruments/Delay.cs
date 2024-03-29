﻿using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        public class Delay : Setting
        {
            public Parameter Dry,
                             Count,
                             Time,
                             Level,
                             Power; // convert to int when applying



            public Delay(Instrument inst, Source src) 
                : base(strDel, Setting_null, Setting_null, True, inst, src)
            {
                Dry   = (Parameter)NewSettingFromTag(strDry,  this, inst, src);
                Count = (Parameter)NewSettingFromTag(strCnt,  this, inst, src);
                Time  = (Parameter)NewSettingFromTag(strTime, this, inst, src);
                Level = (Parameter)NewSettingFromTag(strLvl,  this, inst, src);
                Power = (Parameter)NewSettingFromTag(strPow,  this, inst, src);
            }



            public Delay(Delay del) 
                : base(del.Tag, Setting_null, del, del.On, del.Instrument, del.Source)
            {
                Dry   = new Parameter(del.Dry,   this);
                Count = new Parameter(del.Count, this);
                Time  = new Parameter(del.Time,  this);
                Level = new Parameter(del.Level, this);
                Power = new Parameter(del.Power, this);
            }



            public Delay Copy()
            {
                return new Delay(this);
            }



            public float GetVolume(int i, TimeParams tp)
            {
                if (tp.Program.TooComplex) return 0;

                float val;

                if (i == 0)
                { 
                    val = Dry.UpdateValue(tp);
                }
                else
                { 
                    var dc = Count.UpdateValue(tp)-1; // -1 because 0 is the source sound
                    var dl = Level.UpdateValue(tp);
                    var dp = Power.UpdateValue(tp);

                    val = 
                        dc != 0
                        ? dl * (float)Math.Pow(((int)dc - (i-1)) / dc, 1/dp)
                        : 0;
                }

                return Math.Max(0, val);
            }



            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Dry  .HasDeepParams(chan, src)
                    || Count.HasDeepParams(chan, src)
                    || Time .HasDeepParams(chan, src)
                    || Level.HasDeepParams(chan, src)
                    || Power.HasDeepParams(chan, src);
            }



            public override void Clear()
            {
                Dry  .Clear();
                Count.Clear();
                Time .Clear();
                Level.Clear();
                Power.Clear();
            }



            public override void Reset()
            {
                base.Reset();

                Dry  .Reset();
                Count.Reset();
                Time .Reset();
                Level.Reset();
                Power.Reset();
            }



            public override void Randomize()
            {
                if (TooComplex) return;

                Dry  .Randomize();
                Count.Randomize();
                Time .Randomize();
                Level.Randomize();
                Power.Randomize();
            }



            public override void AdjustFromController(Clip clip)
            {
                Program.AdjustFromController(clip, Count, -g_remote.MoveIndicator    .Z*ControlSensitivity);
                Program.AdjustFromController(clip, Time,   g_remote.MoveIndicator    .X*ControlSensitivity);

                Program.AdjustFromController(clip, Level, -g_remote.RotationIndicator.X*ControlSensitivity);
                Program.AdjustFromController(clip, Power,  g_remote.RotationIndicator.Y*ControlSensitivity);
            }



            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strDry:  return GetOrAddParamFromTag(Dry,   tag);
                    case strCnt:  return GetOrAddParamFromTag(Count, tag);
                    case strTime: return GetOrAddParamFromTag(Time,  tag);
                    case strLvl:  return GetOrAddParamFromTag(Level, tag);
                    case strPow:  return GetOrAddParamFromTag(Power, tag);
                }

                return Setting_null;
            }



            public void Delete(int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Count.Delete(iSrc);
                Time .Delete(iSrc);
                Level.Delete(iSrc);
                Power.Delete(iSrc);
            }



            public override string Save()
            {
                return
                      Tag

                    + PS(SaveToggles())

                    + P (Dry  .Save())
                    + P (Count.Save())
                    + P (Time .Save())
                    + P (Level.Save())
                    + P (Power.Save());
            }



            uint SaveToggles()
            {
                uint f = 0;
                var  d = 0;

                WriteBit(ref f, On, d++);

                return f;
            }



            public static Delay Load(string[] data, ref int d, Instrument inst, int iSrc)
            {
                var tag = data[d++];
 
                
                var del = new Delay(
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);

                del.LoadToggles(data[d++]);


                del.Dry   = Parameter.Load(data, ref d, inst, iSrc, del, del.Dry  );
                del.Count = Parameter.Load(data, ref d, inst, iSrc, del, del.Count);
                del.Time  = Parameter.Load(data, ref d, inst, iSrc, del, del.Time );
                del.Level = Parameter.Load(data, ref d, inst, iSrc, del, del.Level);
                del.Power = Parameter.Load(data, ref d, inst, iSrc, del, del.Power);


                return del;
            }



            bool LoadToggles(string toggles)
            {
                uint f;
                if (!uint.TryParse(toggles, out f)) return False;

                var d = 0;

                On = ReadBit(f, d++);

                return True;
            }



            public override string GetLabel(out float width)
            {
                width = 176;

                return
                      PrintValue(Count.Value, 0, True, 0).PadLeft(2) + "  "
                    + PrintValue(Time .Value, 2, True, 0).PadLeft(4) + "  "
                    + PrintValue(Level.Value, 2, True, 0).PadLeft(4) + "  "
                    + PrintValue(Power.Value, 2, True, 0).PadLeft(4);
            }



            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                if (!_dp.Program.TooComplex)
                {
                    base.DrawLabels(sprites, x, y, dp);

                    if (Count.HasDeepParams(CurChannel, CurSrc)) Count.DrawLabels(sprites, x, y, dp);
                    if (Time .HasDeepParams(CurChannel, CurSrc)) Time .DrawLabels(sprites, x, y, dp);
                    if (Level.HasDeepParams(CurChannel, CurSrc)) Level.DrawLabels(sprites, x, y, dp);
                    if (Power.HasDeepParams(CurChannel, CurSrc)) Power.DrawLabels(sprites, x, y, dp);
                }

                _dp.Next(dp);
            }



            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams _dp)
            {
                var b = 18;


                var w0 = 240f;
                var h0 = 120f;

                var x0 = x + w/2 - w0/2;
                var y0 = y + h/2 - h0/2;


                FillRect(sprites, x0, y0 + h0 - b - 1, w0, 2, color3);

                var dd = Dry  .CurValue;
                var dc = Count.CurValue;
                var dt = Time .CurValue * 100;
                var dl = Level.CurValue;
                var dp = Power.CurValue;


                var fs = 0.5f;
                var dx = 0f;


                var tpSet = new TimeParams(g_time, 0, Note_null, EditedClip.EditLength, CurSrc, _triggerDummy, EditedClip, _dp.Program);

                for (int i = 0; i < (int)dc && dx < w - dt; i++)
                {
                    dx = i * dt;

                    FillRect(sprites, 
                        x0 + dx + (i > 0 ? 2 : 0), 
                        y0 + h0 - b, 
                        i == 0 ? 8 : 4, 
                        -(h0 - b*2) * GetVolume(i, tpSet),
                        color4);
                }


                // dry
                DrawString(
                    sprites,
                    S00(dd), // -1 because 0 is the source sound
                    x0,
                    y0 - b + 8,
                    fs,
                    IsCurParam(strDry) ? color6 : color3);


                // count
                DrawString(
                    sprites, 
                    S(Math.Round(dc-1)), // -1 because 0 is the source sound
                    x0,
                    y0 + h0 - b + 8, 
                    fs,
                    IsCurParam(strCnt) ? color6 : color3);


                if (dc-1 > 0)
                { 
                    // level
                    var lx = x0 + dt + 15;

                    DrawString(
                        sprites, 
                        S00(dl), 
                        lx, 
                        y0 + h0 - b - (h0 - b*2) * dl - 24, 
                        fs,
                        IsCurParam(strLvl) ? color6 : color3, 
                        TA_CENTER);


                    // time
                    DrawString(
                        sprites, 
                        S0(Math.Round(dt*10)) + " ms", 
                        x0 + 60, 
                        y0 + h0 - b + 8, 
                        fs,
                        IsCurParam(strTime) ? color6 : color3, 
                        TA_CENTER);


                    // power
                    var px  = x0 + MinMax(90, dt*(dc-1)/2, w0);
                    var dim = dc > 1 && Math.Abs(px - lx) > 20 ? color6 : color3;

                    var tp  = new TimeParams(0, 0, Note_null, EditedClip.EditLength, CurSrc, _triggerDummy, EditedClip, _dp.Program);
                    var vol = GetVolume(Math.Max(0, (int)dc/2 - 1), tp);

                    DrawString(
                        sprites, 
                        S00(dp),
                        px,
                        y0 + h0 - b - (h0 - b*2) * vol - 24,
                        fs,
                        IsCurParam(strPow) ? color6 : color3,
                        TA_CENTER);
                }
            }



            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                DrawFuncButton(sprites, strDry,  0, w, h, True, Dry  .HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strCnt,  1, w, h, True, Count.HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strTime, 2, w, h, True, Time .HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strLvl,  3, w, h, True, Level.HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, strPow,  4, w, h, True, Power.HasDeepParams(chan, CurSrc));
            }



            public override void Func(int func)
            {
                switch (func)
                {
                    case 0: AddNextSetting(strDry);  break;
                    case 1: AddNextSetting(strCnt);  break;
                    case 2: AddNextSetting(strTime); break;
                    case 3: AddNextSetting(strLvl);  break;
                    case 4: AddNextSetting(strPow);  break;
                }
            }



            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}
