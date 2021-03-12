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
        public class Key
        {
            public int       SourceIndex;

            public Parameter Parameter;

            public float     Value,
                             StepTime;

            public Channel   Channel;

            public string Path { get { return Parameter.GetPath(SourceIndex); } }

            public Key(int srcIndex, Parameter param, float val, float stepTime, Channel chan = null)
            {
                SourceIndex = srcIndex;
                Parameter   = param;
                Value       = val;
                StepTime    = stepTime;
                Channel     = chan;
            }

            public Key(Key key)
            {
                SourceIndex = key.SourceIndex;
                Parameter   = key.Parameter;
                Value       = key.Value;
                StepTime    = key.StepTime;
                Channel     = key.Channel;
            }
        }
    }
}
