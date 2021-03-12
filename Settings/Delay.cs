using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Delay : Setting
        {
            public Parameter Count,
                             Time,
                             Level,
                             Power; // convert to int when applying


            public Delay() : base("Delay", "Del")
            {
                Count = new Parameter("Count", "Cnt",  0,         100, 1,    16, 1,     10,    4   );
                Time  = new Parameter("Time",  "Time", 0.000001f,  10, 0.01f, 1, 0.01f,  0.1f, 0.2f);
                Level = new Parameter("Level", "Lvl",  0,           1, 0.01f, 1, 0.01f,  0.1f, 0.5f);
                Power = new Parameter("Power", "Pow",  0.000001f,   1, 0.01f, 1, 0.01f,  0.1f, 1);

                Count.Parent = 
                Time .Parent = 
                Level.Parent = 
                Power.Parent = this;
            }


            public Delay(Delay del) : base(del.Name, del.Tag, del)
            {
                Count = new Parameter(del.Count);
                Time  = new Parameter(del.Time);
                Level = new Parameter(del.Level);
                Power = new Parameter(del.Power);

                Count.Parent = 
                Time .Parent = 
                Level.Parent = 
                Power.Parent = this;
            }


            public float GetVolume(int i, long gTime, long lTime, long sTime, int noteLen, Note note, int src, List<TriggerValue> triggerValues)
            {
                var dl = Level?.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues) ?? 0;
                var dc = Count?.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues) ?? 0;
                var dp = Power?.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues) ?? 1;

                return 
                    dc != 0
                    ? dl * (float)Math.Pow(((int)dc - i) / dc, 1/dp)
                    : 0;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Count.HasDeepParams(chan, src)
                    || Time .HasDeepParams(chan, src)
                    || Level.HasDeepParams(chan, src)
                    || Power.HasDeepParams(chan, src);
            }


            public override void Remove(Setting setting)
            {
                     if (setting == Count) Count = null;
                else if (setting == Time)  Time  = null;
                else if (setting == Level) Level = null;
                else if (setting == Power) Power = null;
            }


            public override void Clear()
            {
                Count.Clear();
                Time .Clear();
                Level.Clear();
                Power.Clear();
            }


            public override void Randomize()
            {
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
        }
    }
}
