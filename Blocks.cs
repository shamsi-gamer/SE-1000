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
        public class Block
        {
            public int First,
                       Last;

            public Block(int first)
            {
                First = first;
                Last  = First;
            }

            public Block(int first, int last)
            {
                First = first;
                Last  = last;
            }

            public Block(Block block)
            {
                First = block.First;
                Last  = block.Last;
            }

            public int Len { get { return Last - First + 1; } }
        }


        //////////////////////////////////////////////////////////////////////////////////////


        bool g_block;
        bool g_in, g_out;


        void StartBlock()
        {
            var b = g_song.GetBlock(CurPat);

            if (b == null)
            {
                g_song.Blocks.Add(new Block(CurPat));

                g_in     = true;
                g_follow = false;

                UpdateLight(lblFollow, false);
            }
            else
            {
                g_in = !g_in;

                if (g_in)
                {
                    g_out    = false;
                    g_follow = false;

                    UpdateLight(lblFollow, false);
                }
            }

            MovePatternOff();
        }


        void EndBlock()
        {
            var b = g_song.GetBlock(CurPat);

            if (b == null)
            {
                g_song.Blocks.Add(new Block(CurPat));

                g_out    = true;
                g_follow = false;

                UpdateLight(lblFollow, false);
            }
            else
            {
                g_out = !g_out;

                if (g_out)
                {
                    g_in     = false;
                    g_follow = false;

                    UpdateLight(lblFollow, false);
                }

                //g_blocks[b].Next = currentPattern + 1;

                //if (g_blocks[b].Next == g_blocks[b].Start)
                // g_blocks[b].Next = g_blocks[b].Start + 1;
                //else if (g_blocks[b].Next < g_blocks[b].Start)
                // Swap(ref g_blocks[b].Next, ref g_blocks[b].Start);
            }
        }
        

        void ClearBlock()
        {
            g_song.Blocks.Remove(g_song.GetBlock(CurPat));

            DisableBlock();
            MovePatternOff();

            songPressed.Add(11);
        }


        void DisableBlock()
        {
            g_in  = false;
            g_out = false;
        }
    }
}
