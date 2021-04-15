using System;
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


            public Delay() : base("Del", null)
            {
                Dry   = (Parameter)NewSettingFromTag("Dry",  this);
                Count = (Parameter)NewSettingFromTag("Cnt",  this);
                Time  = (Parameter)NewSettingFromTag("Time", this);
                Level = (Parameter)NewSettingFromTag("Lvl",  this);
                Power = (Parameter)NewSettingFromTag("Pow",  this);
            }


            public Delay(Delay del) : base(del.Tag, null, del)
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

                if (i == 0)
                { 
                    return Dry.UpdateValue(tp);
                }
                else
                { 
                    var dc = Count.UpdateValue(tp) - 1; // -1 because 0 is the source sound
                    var dl = Level.UpdateValue(tp);
                    var dp = Power.UpdateValue(tp);

                    return 
                        dc != 0
                        ? dl * (float)Math.Pow(((int)dc - (i-1)) / dc, 1/dp)
                        : 0;
                }
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


            public override void Remove(Setting setting)
            {
                     if (setting == Dry  ) Dry   = null;
                else if (setting == Count) Count = null;
                else if (setting == Time ) Time  = null;
                else if (setting == Level) Level = null;
                else if (setting == Power) Power = null;
            }


            public override void Clear()
            {
                Dry  .Clear();
                Count.Clear();
                Time .Clear();
                Level.Clear();
                Power.Clear();
            }


            public override void Randomize(Program prog)
            {
                if (prog.TooComplex) return;

                Dry  .Randomize(prog);
                Count.Randomize(prog);
                Time .Randomize(prog);
                Level.Randomize(prog);
                Power.Randomize(prog);
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.MoveIndicator    .Z != 0) prog.AdjustFromController(song, Count, -g_remote.MoveIndicator    .Z/ControlSensitivity);
                if (g_remote.MoveIndicator    .X != 0) prog.AdjustFromController(song, Time,   g_remote.MoveIndicator    .X/ControlSensitivity);

                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(song, Level, -g_remote.RotationIndicator.X/ControlSensitivity);
                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(song, Power,  g_remote.RotationIndicator.Y/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Dry":  return GetOrAddParamFromTag(Dry,   tag);
                    case "Cnt":  return GetOrAddParamFromTag(Count, tag);
                    case "Time": return GetOrAddParamFromTag(Time,  tag);
                    case "Lvl":  return GetOrAddParamFromTag(Level, tag);
                    case "Pow":  return GetOrAddParamFromTag(Power, tag);
                }

                return null;
            }


            public void Delete(Song song, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Count.Delete(song, iSrc);
                Time .Delete(song, iSrc);
                Level.Delete(song, iSrc);
                Power.Delete(song, iSrc);
            }


            public override string Save()
            {
                return
                      W(Tag)

                    + W(Dry  .Save())
                    + W(Count.Save())
                    + W(Time .Save())
                    + W(Level.Save())
                    +   Power.Save();
            }


            public static Delay Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tag = data[i++];
 
                var del = new Delay();

                del.Dry   = Parameter.Load(data, ref i, inst, iSrc, del, del.Dry  );
                del.Count = Parameter.Load(data, ref i, inst, iSrc, del, del.Count);
                del.Time  = Parameter.Load(data, ref i, inst, iSrc, del, del.Time );
                del.Level = Parameter.Load(data, ref i, inst, iSrc, del, del.Level);
                del.Power = Parameter.Load(data, ref i, inst, iSrc, del, del.Power);

                return del;
            }


            public override string GetLabel(out float width)
            {
                width = 176;

                return
                      printValue(Count.CurValue, 0, true, 0).PadLeft(2) + "  "
                    + printValue(Time .CurValue, 2, true, 0).PadLeft(4) + "  "
                    + printValue(Level.CurValue, 2, true, 0).PadLeft(4) + "  "
                    + printValue(Power.CurValue, 2, true, 0).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Count.HasDeepParams(CurrentChannel, CurSrc)) Count.DrawLabels(sprites, x, y, dp);
                if (Time .HasDeepParams(CurrentChannel, CurSrc)) Time .DrawLabels(sprites, x, y, dp);
                if (Level.HasDeepParams(CurrentChannel, CurSrc)) Level.DrawLabels(sprites, x, y, dp);
                if (Power.HasDeepParams(CurrentChannel, CurSrc)) Power.DrawLabels(sprites, x, y, dp);

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


                var tpSet = new TimeParams(g_time, 0, 0, null, EditLength, CurSrc, _triggerDummy, _dp.Program);

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
                    IsCurParam("Dry") ? color6 : color3);


                // count
                DrawString(
                    sprites, 
                    S(Math.Round(dc-1)), // -1 because 0 is the source sound
                    x0,
                    y0 + h0 - b + 8, 
                    fs, 
                    IsCurParam("Cnt") ? color6 : color3);


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
                        IsCurParam("Lvl") ? color6 : color3, 
                        TaC);


                    // time
                    DrawString(
                        sprites, 
                        S0(Math.Round(dt*10)) + " ms", 
                        x0 + 60, 
                        y0 + h0 - b + 8, 
                        fs,
                        IsCurParam("Time") ? color6 : color3, 
                        TaC);


                    // power
                    var px  = x0 + MinMax(90, dt*(dc-1)/2, w0);
                    var dim = dc > 1 && Math.Abs(px - lx) > 20 ? color6 : color3;

                    var tp  = new TimeParams(0, 0, 0, null, EditLength, CurSrc, _triggerDummy, _dp.Program);
                    var vol = GetVolume(Math.Max(0, (int)dc / 2 - 1), tp);

                    DrawString(
                        sprites, 
                        S00(dp),
                        px,
                        y0 + h0 - b - (h0 - b*2) * vol - 24,
                        fs,
                        IsCurParam("Pow") ? color6 : color3,
                        TaC);
                }
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                DrawFuncButton(sprites, "Dry",  0, w, h, true, Dry  .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Cnt",  1, w, h, true, Count.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Time", 2, w, h, true, Time .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Lvl",  3, w, h, true, Level.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Pow",  4, w, h, true, Power.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "X",    5, w, h, false, false, mainPressed.Contains(5));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                    case 0: AddNextSetting("Dry");  break;
                    case 1: AddNextSetting("Cnt");  break;
                    case 2: AddNextSetting("Time"); break;
                    case 3: AddNextSetting("Lvl");  break;
                    case 4: AddNextSetting("Pow");  break;
                    case 5: RemoveSetting(this);    break;
                }
            }
        }
    }
}
