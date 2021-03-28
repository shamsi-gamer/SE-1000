using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Song
        {
            public string        Name;

            public Arpeggio      Arpeggio; // indicates that this song is an arpeggio

            public List<Pattern> Patterns;
            public List<Block>   Blocks;

            public List<Key>[]   ChannelAutoKeys = new List<Key>[g_nChans];


            public int           Length;

            public float         EditPos,
                                 LastEditPos;
            
            public List<Note>    EditNotes;
            public Note          Inter;


            public Song(string name = "Song 1")
            {
                Name     = name;

                Arpeggio = null;
                Length   = -1;

                Patterns = new List<Pattern>();
                Blocks   = new List<Block>();

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>();

                EditNotes = new List<Note>();
                
                ResetState();
            }


            public Song(Song song)
            {
                Name     = song.Name;

                Arpeggio = song.Arpeggio;
                Length   = song.Length;

                Patterns = new List<Pattern>();
                foreach (var pat in song.Patterns)
                { 
                    Patterns.Add(new Pattern(pat));
                    Patterns.Last().Song = this;
                }

                Blocks = new List<Block>();
                foreach (var b in song.Blocks)
                    Blocks.Add(new Block(b));

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>(song.ChannelAutoKeys[i]);

                EditNotes = new List<Note>();
                
                ResetState();
            }


            public void ClearAudoKeys()
            {
                foreach (var keys in ChannelAutoKeys)
                    keys.Clear();
            }


            public void UpdateAutoKeys()
            {
                for (int ch = 0; ch < g_nChans; ch++)
                { 
                    var chanKeys = ChannelAutoKeys[ch];

                    chanKeys.Clear();

                    for (int p = 0; p < Patterns.Count; p++)
                    {
                        var keys = Patterns[p].Channels[ch].AutoKeys;

                        for (int k = 0; k < keys.Count; k++)
                        { 
                            chanKeys.Add(new Key(
                                keys[k].SourceIndex,
                                keys[k].Parameter,
                                keys[k].Value, 
                                keys[k].StepTime + p*nSteps,
                                keys[k].Channel));
                        }
                    }

                    chanKeys.Sort((a, b) => a.StepTime.CompareTo(b.StepTime));
                }
            }


            public void Clear()
            {
                Name = "";

                Patterns.Clear();
                Blocks.Clear();

                foreach (var keys in ChannelAutoKeys)
                    keys.Clear();

                ResetState();
            }


            void ResetState()
            {
                EditPos     = fN;
                LastEditPos = fN;

                Inter       = null;
                EditNotes.Clear();
            }


            public int   GetNotePat(Note note) { return Patterns.FindIndex(p => p.Channels.Contains(note.Channel)); }
            public float GetStep   (Note note) { return GetNotePat(note) * nSteps + note.PatStep; }

            public int   GetKeyPat (Key key)   { return Patterns.FindIndex(p => Array.Find(p.Channels, c => c.AutoKeys.Contains(key)) != null); }
            public float GetStep   (Key key)   { return GetKeyPat(key) * nSteps + key.StepTime; }


            public Block GetBlock(int pat)
            {
                return Blocks.Find(b =>
                       pat >= b.First
                    && pat <= b.Last);
            }


            public string Save()
            {
                return
                      N(Name.Replace("\n", "\u0085"))
                    + N(SavePatterns())
                    + SaveBlocks();
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
                var save = S(g_song.Blocks.Count);

                foreach (var b in g_song.Blocks)
                {
                    save +=
                      ";" + S(b.First)
                    + ";" + S(b.Last);
                }

                return save;
            }


            public static Song Load(string[] lines, ref int line)
            { 
                if (lines.Length < 3)
                    return null;

                var song = new Song();

                song.Name = lines[line++].Replace("\u0085", "\n");

                if (!song.LoadPatterns(lines, ref line)) return null;
                if (!song.LoadBlocks(lines[line++]))     return null;

                song.UpdateAutoKeys();
                //if (!Finalize(insts)) return F;
                //if (!LoadEdit(lines, ref line)) return F;

                return song;
            }


            bool LoadPatterns(string[] lines, ref int line)
            {
                int nPats = int.Parse(lines[line++]);

                for (int p = 0; p < nPats; p++)
                {
                    int i = 0;
                    var pat = Pattern.Load(lines[line++].Split(';'), ref i);
                    if (pat == null) return false;

                    pat.Song = this;

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
        }


        void ClearSong()
        {
            g_sm.StopAll();

            g_notes.Clear();
            g_sounds.Clear();

            g_song.Clear();
        }


        void ToggleMovePattern()
        {
            g_movePat = !g_movePat;

            UpdateLight(lblPrevPat, g_movePat);
            UpdateLight(lblNextPat, g_movePat);
            UpdateLight(lblMovePat, g_movePat);

            if (g_movePat)
                DisableBlock();
        }
    }
}
