using System;
using System.Collections.Generic;


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


            public float GetVolume(int i, long gTime, long lTime, long sTime, int noteLen, Note note, int src, List<TriggerValue> triggerValues)
            {

                if (i == 0)
                    return Dry.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues);

                else
                { 
                    var dc = Count.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues) - 1; // -1 because 0 is the source sound
                    var dl = Level.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues);
                    var dp = Power.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues);

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


            public override void Randomize()
            {
                Dry  .Randomize();
                Count.Randomize();
                Time .Randomize();
                Level.Randomize();
                Power.Randomize();
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.MoveIndicator.Z != 0) prog.AdjustFromController(song, Count, -g_remote.MoveIndicator.Z/ControlSensitivity);
                if (g_remote.MoveIndicator.X != 0) prog.AdjustFromController(song, Time,   g_remote.MoveIndicator.X/ControlSensitivity);

                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(song, Level, -g_remote.RotationIndicator.X/ControlSensitivity);
                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(song, Power,  g_remote.RotationIndicator.Y/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Dry":  return Dry   ?? (Dry   = (Parameter)NewSettingFromTag("Dry",  this));
                    case "Cnt":  return Count ?? (Count = (Parameter)NewSettingFromTag("Cnt",  this));
                    case "Time": return Time  ?? (Time  = (Parameter)NewSettingFromTag("Time", this));
                    case "Lvl":  return Level ?? (Level = (Parameter)NewSettingFromTag("Lvl",  this));
                    case "Pow":  return Power ?? (Power = (Parameter)NewSettingFromTag("Pow",  this));
                }

                return null;
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

                del.Dry   = Parameter.Load(data, ref i, inst, iSrc, del);
                del.Count = Parameter.Load(data, ref i, inst, iSrc, del);
                del.Time  = Parameter.Load(data, ref i, inst, iSrc, del);
                del.Level = Parameter.Load(data, ref i, inst, iSrc, del);
                del.Power = Parameter.Load(data, ref i, inst, iSrc, del);

                return del;
            }
        }
    }
}
