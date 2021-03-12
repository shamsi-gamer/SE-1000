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
        public bool inputValid = true;


        void UpdateInst()
        {
            if (   inputValid
                && g_song.SelChan > -1
                && g_song.CurSrc < 0)
            {
                var sb = new StringBuilder();
                dspMain.Surface.ReadText(sb, false);

                CurrentInstrument(g_song).Name = sb.ToString().Trim();
            }

            inputValid = true;
        }

        void UpdateSongName()
        {
            var sb = new StringBuilder();
            dspInfo.Surface.ReadText(sb, false);

            g_song.Name = sb.ToString().Trim();
        }

        void UpdateSongDsp()
        {
            dspInfo.Surface.WriteText(g_song.Name.Replace("\u0085", "\n"));
        }
    }
}
