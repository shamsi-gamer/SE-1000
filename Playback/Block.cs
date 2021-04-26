using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
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
    }
}
