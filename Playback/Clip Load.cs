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
            public static Clip Load(string[] lines, ref int line, out string curPath, out string modConnPath, out int modPat, out int modChan)
            { 
                curPath     = "";
                modConnPath = "";
                modPat      = -1;
                modChan     = -1;

                if (lines.Length < 3)
                    return null;

                var clip = new Clip();

                clip.Name = lines[line++].Replace("\u0085", "\n");

                var cfg = lines[line++].Split(';');
                if (!clip.LoadConfig  (cfg, out curPath, out modConnPath, out modPat, out modChan))   return null;
                if (!clip.LoadChords  (lines[line++]))   return null;
                if (!clip.LoadMems    (lines[line++]))   return null;
                if (!clip.LoadPatterns(lines, ref line)) return null;
                if (!clip.LoadBlocks  (lines[line++]))   return null;

                clip.UpdateAutoKeys();

                return clip;
            }


            bool LoadToggles(string toggles)
            {
                uint f;
                if (!uint.TryParse(toggles, out f)) return false;

                var i = 0;

                Loop       = ReadBit(f, i++);
                Block      = ReadBit(f, i++);
                AllPats    = ReadBit(f, i++);
                Follow     = ReadBit(f, i++);
                AutoCue    = ReadBit(f, i++);

                MovePat    = ReadBit(f, i++);

                In         = ReadBit(f, i++);
                Out        = ReadBit(f, i++);
            
                AllChan    = ReadBit(f, i++);
                RndInst    = ReadBit(f, i++);
            
                Piano      = ReadBit(f, i++);
            
                Transpose  = ReadBit(f, i++);
                Spread     = ReadBit(f, i++);

                Shift      = ReadBit(f, i++);
                MixerShift = ReadBit(f, i++);
            
                Hold       = ReadBit(f, i++);
                Pick       = ReadBit(f, i++);

                ChordMode  = ReadBit(f, i++);
                ChordEdit  = ReadBit(f, i++);
                ChordAll   = ReadBit(f, i++);

                HalfSharp  = ReadBit(f, i++);

                ParamKeys  = ReadBit(f, i++);
                ParamAuto  = ReadBit(f, i++);
                       
                MemSet     = ReadBit(f, i++);

                return true;
            }


            bool LoadConfig(string[] cfg, out string curPath, out string modConnPath, out int modPat, out int modChan)
            {
                curPath     = "";
                modConnPath = "";
                modPat      = -1;
                modChan     = -1;

                int c = 0;

                LoadToggles(cfg[c++]);

                if (!long .TryParse(cfg[c++], out PlayTime   )) return false;
                if (!int  .TryParse(cfg[c++], out PlayPat    )) return false;
                if (!int  .TryParse(cfg[c++], out CueNext    )) return false;

                if (!int  .TryParse(cfg[c++], out CurPat     )) return false;
                if (!int  .TryParse(cfg[c++], out CurChan    )) return false;
                                                             
                if (!int  .TryParse(cfg[c++], out SelChan    )) return false;
                if (!int  .TryParse(cfg[c++], out CurSrc     )) return false;
                                                             
                curPath = cfg[c++];                              
                                                             
                if (!int  .TryParse(cfg[c++], out EditStep   )) return false;
                if (!int  .TryParse(cfg[c++], out EditLength )) return false;
                                                           
                if (!int  .TryParse(cfg[c++], out CurNote    )) return false;
                                                             
                if (!int  .TryParse(cfg[c++], out Chord      )) return false;
                if (!int  .TryParse(cfg[c++], out ChordSpread)) return false;
                                                           
                if (!int  .TryParse(cfg[c++], out SongOff    )) return false;
                if (!int  .TryParse(cfg[c++], out InstOff    )) return false;
                if (!int  .TryParse(cfg[c++], out SrcOff     )) return false;
                                                             
                if (!int  .TryParse(cfg[c++], out Solo       )) return false;
                                                           
                if (!float.TryParse(cfg[c++], out Volume     )) return false;

                modConnPath = cfg[c++];

                if (!int  .TryParse(cfg[c++], out ModDestSrcIndex)) return false;
                if (!int  .TryParse(cfg[c++], out modPat     )) return false;
                if (!int  .TryParse(cfg[c++], out modChan    )) return false;
                                                             
                if (!int  .TryParse(cfg[c++], out ColorIndex )) return false;

                return true;
            }


            bool LoadPatterns(string[] lines, ref int line)
            {
                int nPats = int.Parse(lines[line++]);

                for (int p = 0; p < nPats; p++)
                {
                    int i = 0;
                    var pat = Pattern.Load(lines[line++].Split(';'), ref i);
                    if (pat == null) return false;

                    pat.Clip = this;

                    Patterns.Add(pat);
                }

                return true;
            }


            bool LoadBlocks(string line)
            {
                var data = line.Split(';');
                var i    = 0;

                Blocks.Clear();

                int nBlocks = int.Parse(data[i++]);

                for (int b = 0; b < nBlocks; b++)
                {
                    int first = int.Parse(data[i++]);
                    int last  = int.Parse(data[i++]);

                    Blocks.Add(new Block(first, last));
                }

                return true;
            }


            bool LoadMems(string line)
            {
                var mems = line.Split(';');

                for (int m = 0; m < nMems; m++)
                    if (!int.TryParse(mems[m], out Mems[m])) return false;

                return true;
            }


            bool LoadChords(string strChords)
            {
                Chords = new List<int>[4];

                for (int _c = 0; _c < Chords.Length; _c++)
                    Chords[_c] = new List<int>();


                var chords = strChords.Split(';');

                for (int _c = 0; _c < chords.Length; _c++)
                { 
                    var _keys = chords[_c].Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

                    Chords[_c] = new List<int>();

                    int key;
                    foreach (var k in _keys)
                    {
                        if (!int.TryParse(k, out key)) return false;
                        Chords[_c].Add(key);
                    }
                }


                return true;
            }
        }
    }
}
