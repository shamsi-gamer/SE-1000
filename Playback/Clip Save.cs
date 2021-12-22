namespace IngameScript
{
    partial class Program
    {
        public partial class Clip
        {
            public string Save()
            {
                return
                      SaveConfig()

                    + PN(SaveChords())
                    + PN(SaveMems())
                    + PN(SavePatterns())
                    + PN(SaveBlocks());
            }



            uint SaveToggles()
            {
                uint f = 0;
                var  d = 0;

                WriteBit(ref f, Loop,       d++);
                WriteBit(ref f, Block,      d++);
                WriteBit(ref f, AllPats,    d++);
                WriteBit(ref f, Follow,     d++);
                WriteBit(ref f, AutoCue,    d++);
                                            
                WriteBit(ref f, MovePat,    d++);

                WriteBit(ref f, In,         d++);
                WriteBit(ref f, Out,        d++);
                                            
                WriteBit(ref f, AllChan,    d++);
                WriteBit(ref f, UseInst,    d++);

                WriteBit(ref f, Accent,     d++);
                WriteBit(ref f, Pick,       d++);
                WriteBit(ref f, Piano,      d++);
                                            
                WriteBit(ref f, Transpose,  d++);
                WriteBit(ref f, Strum,      d++);
                                            
                WriteBit(ref f, Shift,      d++);
                WriteBit(ref f, MixerShift, d++);
                                            
                WriteBit(ref f, Hold,       d++);
                WriteBit(ref f, Note,       d++);
                                            
                WriteBit(ref f, ChordMode,  d++);
                WriteBit(ref f, ChordEdit,  d++);
                WriteBit(ref f, ChordAll,   d++);
                                            
                WriteBit(ref f, HalfSharp,  d++);
                                            
                WriteBit(ref f, ParamKeys,  d++);
                WriteBit(ref f, ParamAuto,  d++);
                                            
                WriteBit(ref f, SetMemPat,  d++);
                                            
                WriteBit(ref f, Move,       d++);
                WriteBit(ref f, SetOrPat,   d++);
                WriteBit(ref f, SetMemSet,  d++);

                WriteBit(ref f, Scale,      d++);

                return f;
            }



            string SaveConfig()
            {
                return
                      Name.Replace("\n", "\u0085")
                    + PS(SaveToggles())

                    + PS(EditPat)         
                    + PS(CurChan)        

                    + PS(SelChan)        
                    + PS(CurSrc)

                    + PS(EditStepIndex)
                    + PS(EditLengthIndex)     

                    + PS(CurNote)      

                    + PS(Chord)        
                    + PS(ChordStrum)
                
                    + PS(SongOff)        
                    + PS(InstOff)        
                    + PS(SrcOff )

                    + PS(Solo)

                    + PS(Volume)

                    + PS(ColorIndex);
            }



            //String SaveEdit()
            //{
            //    var str = "";
            //    var s   = ";";

            //    str += S(g_song.EditNotes.Count);

            //    foreach (var n in g_song.EditNotes)
            //    {
            //        str +=
            //          s + S(g_song.Patterns.FindIndex(p => p.Channels.Contains(n.Channel)))
            //        + s + S(n.iChan)
            //        + s + S(n.Channel.Notes.IndexOf(n));
            //    }

            //    str += "\n";

            //    return str;
            //}



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
