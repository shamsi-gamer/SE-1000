﻿using Sandbox.Game.EntityComponents;
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
        public class Speaker
        {
            bool m_used;

            public IMySoundBlock Block;

            public bool IsUsed => m_used;

            public Speaker(IMySoundBlock sb)
            {
                Block = sb;
                m_used = F;
            }

            public void SetUsed() { m_used = T; }
            public void Free() { m_used = F; }
        }
    }
}
