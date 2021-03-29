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
        public class TriggerValue
        {
            public string Path;
            public float  Value;


            public TriggerValue(string path, float value)
            {
                Path  = path;
                Value = value;
            }


            public TriggerValue(TriggerValue val)
            {
                Path  = val.Path;
                Value = val.Value;
            }


            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(TriggerValue))
                    return false;

                var trig = (TriggerValue)obj;

                return Path  == trig.Path;
            }


            public override int GetHashCode()
            {
                return Path.GetHashCode() * 17;
            }
        }
    }
}
