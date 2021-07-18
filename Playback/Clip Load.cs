using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public partial class Clip
        {
            public static Clip Load(string[] lines, ref int line, Track track, Program prog)//, out string curPath)
            { 
                //curPath     = "";

                if (lines.Length < 3)
                    return Clip_null;

                var clip = new Clip(track, prog);

                var cfg = lines[line++].Split(';');

                if (   !clip.LoadConfig  (cfg) //, out curPath)) return Clip_null;
                    || !clip.LoadChords  (lines[line++])
                    || !clip.LoadMems    (lines[line++])
                    || !clip.LoadPatterns(lines, ref line)
                    || !clip.LoadBlocks  (lines[line++]))
                    return Clip_null;

                clip.UpdateAutoKeys();

                return clip;
            }


            bool LoadToggles(string toggles)
            {
                uint f;
                if (!uint.TryParse(toggles, out f)) return False;

                var i = 0;

                Loop        = ReadBit(f, i++);
                Block       = ReadBit(f, i++);
                AllPats     = ReadBit(f, i++);
                Follow      = ReadBit(f, i++);
                AutoCue     = ReadBit(f, i++);
                            
                MovePat     = ReadBit(f, i++);
                            
                In          = ReadBit(f, i++);
                Out         = ReadBit(f, i++);
                            
                AllChan     = ReadBit(f, i++);
                RndInst     = ReadBit(f, i++);
                            
                Piano       = ReadBit(f, i++);
                            
                Transpose   = ReadBit(f, i++);
                Strum      = ReadBit(f, i++);
                            
                Shift       = ReadBit(f, i++);
                MixerShift  = ReadBit(f, i++);
                            
                Hold        = ReadBit(f, i++);
                Pick        = ReadBit(f, i++);
                            
                ChordMode   = ReadBit(f, i++);
                ChordEdit   = ReadBit(f, i++);
                ChordAll    = ReadBit(f, i++);
                            
                HalfSharp   = ReadBit(f, i++);
                            
                ParamKeys   = ReadBit(f, i++);
                ParamAuto   = ReadBit(f, i++);
                            
                SetMemPat   = ReadBit(f, i++);
                            
                Move        = ReadBit(f, i++);
                SetOrPat    = ReadBit(f, i++);
                SetMemSet   = ReadBit(f, i++);

                return True;
            }



            bool LoadConfig(string[] cfg)//, out string curPath)
            {
                //curPath = "";

                var c = 0;

                Name = cfg[c++].Replace("\u0085", "\n");

                LoadToggles(cfg[c++]);

                return
                       int_TryParse  (cfg[c++], out EditPat        )
                    && int_TryParse  (cfg[c++], out CurChan        )
                                                                    
                    && int_TryParse  (cfg[c++], out SelChan        )
                    && int_TryParse  (cfg[c++], out CurSrc         )

                    //Path = cfg[c++];                             

                    && int_TryParse  (cfg[c++], out EditStepIndex  )
                    && int_TryParse  (cfg[c++], out EditLengthIndex)
                                                                    
                    && int_TryParse  (cfg[c++], out CurNote        )
                                                                    
                    && int_TryParse  (cfg[c++], out Chord          )
                    && int_TryParse  (cfg[c++], out ChordStrum    )
                                                                    
                    && int_TryParse  (cfg[c++], out SongOff        )
                    && int_TryParse  (cfg[c++], out InstOff        )
                    && int_TryParse  (cfg[c++], out SrcOff         )
                                                                    
                    && int_TryParse  (cfg[c++], out Solo           )
                                                                    
                    && float.TryParse(cfg[c++], out Volume         )
                                                                    
                    && int_TryParse  (cfg[c++], out ColorIndex     );
            }


            bool LoadPatterns(string[] lines, ref int line)
            {
                int nPats = int_Parse(lines[line++]);

                for (int p = 0; p < nPats; p++)
                {
                    int i = 0;
                    var pat = Pattern.Load(lines[line++].Split(';'), ref i);
                    if (!OK(pat)) return False;

                    pat.Clip = this;

                    Patterns.Add(pat);
                }

                return True;
            }


            bool LoadBlocks(string line)
            {
                var data = line.Split(';');
                var i    = 0;

                Blocks.Clear();

                int nBlocks = int_Parse(data[i++]);

                for (int b = 0; b < nBlocks; b++)
                {
                    int first = int_Parse(data[i++]);
                    int last  = int_Parse(data[i++]);

                    Blocks.Add(new Block(first, last));
                }

                return True;
            }


            //bool LoadEdit(string[] lines, ref int line)
            //{
            //    g_song.EditNotes.Clear();

            //    var cfg = lines[line++].Split(';');
            //    if (cfg.Length == 0) return F;

            //    var c = 0;

            //    int nNotes;
            //    if (!int_TryParse(cfg[c++], out nNotes)) return F;

            //    for (int i = 0; i < nNotes; i++)
            //    {
            //        int p, ch, n;
            //        if (!int_TryParse(cfg[c++], out p )) return F;
            //        if (!int_TryParse(cfg[c++], out ch)) return F;
            //        if (!int_TryParse(cfg[c++], out n )) return F;

            //        g_song.EditNotes.Add(g_song.Patterns[p].Channels[ch].Notes[n]);
            //    }

            //    return T;
            //}


            bool LoadMems(string line)
            {
                var mems = line.Split(';');

                for (int m = 0; m < nMems; m++)
                    if (!int_TryParse(mems[m], out Mems[m])) return False;

                return True;
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
                        if (!int_TryParse(k, out key)) return False;
                        Chords[_c].Add(key);
                    }
                }


                return True;
            }
        }
    }
}
