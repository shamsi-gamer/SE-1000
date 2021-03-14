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
        void MovePatternOff()
        {
            movePat = false;
            UpdateLight(lblMovePat, false);
        }


        void ToogleLoop()
        {
            loopPat = !loopPat;
            UpdateLight(lblLoop, loopPat);
        }


        void ToggleMove()
        {
            if (curSet > -1) return;

            g_move = !g_move;

            UpdateLight(lblMove, g_move ^ (CurSrc > -1), SelChan > -1 && !g_move);
            UpdateLight(lblPrev, g_move || CurSrc > -1,  SelChan > -1);
            UpdateLight(lblNext, g_move || CurSrc > -1,  SelChan > -1);
        }


        void ToggleBlock()
        {
            g_block = !g_block;
            UpdateLight(lblBlock, g_block);
        }


        void ToggleAllPatterns()
        {
            allPats = !allPats;
            UpdateLight(lblAllPatterns, allPats);
        }


        void ToggleFollow()
        {
            g_follow = !g_follow;
            UpdateLight(lblFollow, g_follow);

            if (g_follow)
            {
                g_autoCue = false;
                UpdateLight(lblAutoCue, false);
            }
        }


        void ToggleAutoCue()
        {
            g_autoCue = !g_autoCue;
            UpdateLight(lblAutoCue, g_autoCue);

            if (g_autoCue)
            {
                Cue();

                g_follow = false;
                UpdateLight(lblFollow, false);
            }
        }
    }
}
