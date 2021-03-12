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
        const float Tau = (float)Math.PI * 2;

        const int FPS = 60;

        const int nChans = 12;
        const int nSteps = 16;


        static Random  g_rnd    = new Random(DateTime.Now.Millisecond);
        static float[] g_random = new float[1000];


        Color
            color6,
            color5,
            color4,
            color3,
            color2,
            color1,
            color0,
            redColor6 = new Color(63, 0, 0),
            redColor0 = new Color(1, 0, 0);


        const int MaxSources = 12;

        // display limits
        const int maxDspInst = 18;
        const int maxDspSrc  = 6;
        const int maxDspPats = 8;

        const int nMems = 8;
    }
}
