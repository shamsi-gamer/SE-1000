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

            public List<Key>[]   ChannelAutoKeys = new List<Key>[nChans];


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
                for (int ch = 0; ch < nChans; ch++)
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
                EditPos     = float.NaN;
                LastEditPos = float.NaN;

                Inter       = null;
                EditNotes.Clear();
            }


            public int   GetNotePat(Note note) { return Patterns.FindIndex(p => p.Channels.Contains(note.Channel)); }
            public float GetStep   (Note note) { return GetNotePat(note) * nSteps + note.PatStep; }

            public int   GetKeyPat (Key key)   { return Patterns.FindIndex(p => p.Channels.Find(c => c.AutoKeys.Contains(key)) != null); }
            public float GetStep   (Key key)   { return GetKeyPat(key) * nSteps + key.StepTime; }


            public Block GetBlock(int pat)
            {
                return Blocks.Find(b =>
                       pat >= b.First
                    && pat <= b.Last);
            }
        }


        /////////////////////////////////////////////////////////////////////////////


        //void InitSong()
        //{

        //        //NewSong();
        //}


        //void NewSong()
        //{
        //    ClearSong();

        //    g_volume = 1;

        //    g_ticksPerStep = 7;

        //    g_inst.Clear();

        //    g_inst.Add(new Instrument());
        //    g_inst[0].Sources.Add(new Source(g_inst[0]));
        //    g_song.Patterns.Add(new Pattern(g_song, g_inst[0]));

        //    g_song.Name = "New Song";

        //    UpdateSongDsp();

        //    g_volume      = 1;
        //    g_chord       = -1;
        //    g_chordEdit   = false;
        //    g_chordSpread = 0;
        //    g_editStep    = 2;
        //    g_editLength  = 2;
        //    g_curNote     = 69 * NoteScale;
        //    //g_showNote      = g_curNote;
                         
        //    g_loop       = 
        //    g_block       = 
        //    g_in          = 
        //    g_out         = 
        //    g_movePat       = 
        //    g_allPats       = 
        //    g_autoCue     = 
        //    g_halfSharp   = 
        //    g_follow      = 
        //    g_allChan       = 
        //    g_piano       = 
        //    g_move        = 
        //    g_shift       = 
        //    g_mixerShift  = 
        //    g_hold        = 
        //    g_chordMode   = 
        //    g_chordAll    = 
        //    g_pick        = 
        //    g_rndInst       = 
        //    g_paramKeys   = 
        //    g_paramAuto   = false;


        //    PlayTime    = -1;
        //    PlayPat     = -1;

        //    CurPat      =  0;

        //    CurChan     =  0;
        //    SelChan     = -1;
        //    CurSrc      = -1;
                            
        //    StartTime   = -1;


        //    g_chords = new List<int>[4];

        //    for (int i = 0; i < g_chords.Length; i++)
        //        g_chords[i] = new List<int>();
            

        //    SetLightColor(4);
        //    UpdateLights();
        //}


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
