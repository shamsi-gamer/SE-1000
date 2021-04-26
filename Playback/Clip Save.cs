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
        public partial class Clip
        {
            public string Save()
            {
                return
                      N(Name.Replace("\n", "\u0085"))
                    + N(SaveConfig())
                    + N(SaveChords())
                    + N(SaveMems())
                    + N(SavePatterns())
                    +   SaveBlocks();
            }


            uint SaveToggles()
            {
                uint f = 0;
                var  i = 0;

                WriteBit(ref f, Loop,       i++);
                WriteBit(ref f, Block,      i++);
                WriteBit(ref f, AllPats,    i++);
                WriteBit(ref f, Follow,     i++);
                WriteBit(ref f, AutoCue,    i++);

                WriteBit(ref f, MovePat,    i++);

                WriteBit(ref f, In,         i++);
                WriteBit(ref f, Out,        i++);

                WriteBit(ref f, AllChan,    i++);
                WriteBit(ref f, RndInst,    i++);

                WriteBit(ref f, Piano,      i++);

                WriteBit(ref f, Transpose,  i++);
                WriteBit(ref f, Spread,     i++);

                WriteBit(ref f, Shift,      i++);
                WriteBit(ref f, MixerShift, i++);

                WriteBit(ref f, Hold,       i++);
                WriteBit(ref f, Pick,       i++);

                WriteBit(ref f, ChordMode,  i++);
                WriteBit(ref f, ChordEdit,  i++);
                WriteBit(ref f, ChordAll,   i++);

                WriteBit(ref f, HalfSharp,  i++);

                WriteBit(ref f, ParamKeys,  i++);
                WriteBit(ref f, ParamAuto,  i++);

                WriteBit(ref f, MemSet,     i++);

                return f;
            }


            string SaveConfig()
            {
                return
                      WS(SaveToggles())

                    + WS(PlayTime)
                    + WS(PlayPat)
                    + WS(CueNext)

                    + WS(CurPat)         
                    + WS(CurChan)        

                    + WS(SelChan)        
                    + WS(CurSrc)

                    + W (CurSet > -1 ? CurSetting.GetPath(CurSrc) : "")

                    + WS(EditStep)
                    + WS(EditLength)     

                    + WS(CurNote)      

                    + WS(Chord)        
                    + WS(ChordSpread)
                
                    + WS(SongOff)        
                    + WS(InstOff)        
                    + WS(SrcOff )

                    + WS(Solo)

                    + WS(Volume)

                    + W (ModDestConnecting != null ? ModDestConnecting.GetPath(ModDestSrcIndex) : "")
                    + WS(ModDestSrcIndex)
                    + WS(ModDestChannel != null ? Patterns.IndexOf(ModDestChannel.Pattern) : -1)
                    + WS(ModDestChannel != null ? ModDestChannel.Pattern.Channels.IndexOf(ModDestChannel) : -1)

                    +  S(ColorIndex);
            }


            string SaveMems()
            {
                var mems = "";

                for (int m = 0; m < nMems; m++)
                    mems += S(Mems[m]) + (m < nMems-1 ? ";" : "");

                return mems;
            }


            string SaveChords()
            {
                var chords = "";

                for (int c = 0; c < Chords.Length; c++)
                {
                    var chord = Chords[c];

                    for (int k = 0; k < chord.Count; k++)
                        chords += chord[k] + (k < chord.Count - 1 ? "," : "");

                    if (c < Chords.Length - 1)
                        chords += ";";
                }

                return chords;
            }


            string SavePatterns()
            {
                var save = "";

                save += S(Patterns.Count);

                foreach (var pat in Patterns)
                    save += "\n" + pat.Save();

                return save;
            }


            string SaveBlocks()
            {
                var save = S(Blocks.Count);

                foreach (var b in Blocks)
                {
                    save +=
                      PS(b.First)
                    + PS(b.Last);
                }

                return save;
            }
        }
    }
}
