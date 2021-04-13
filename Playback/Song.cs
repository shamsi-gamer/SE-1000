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


            public long          StartTime, // in ticks
                                 PlayTime;

            public int           PlayPat; // this can't be a property because it must sometimes be separate from PlayTime, for queueing

            public float         PlayStep { get { return PlayTime > -1 ? PlayTime / (float)g_ticksPerStep : fN; } }


            public int           Cue;


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
                
                PlayTime  = -1;
                StartTime = -1;

                PlayPat   = -1;

                Cue       = -1;

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
                
                PlayTime  = song.PlayTime;
                StartTime = song.StartTime;

                PlayPat   = song.PlayPat;

                Cue       = song.Cue;

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
                                keys[k].StepTime + p*g_nSteps,
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


            public int   GetKeyPat(Key key) { return Patterns.FindIndex(p => Array.Find(p.Channels, c => c.AutoKeys.Contains(key)) != null); }
            public float GetStep  (Key key) { return GetKeyPat(key) * g_nSteps + key.StepTime; }


            public Block GetBlock(int pat)
            {
                return Blocks.Find(b =>
                       pat >= b.First
                    && pat <= b.Last);
            }


            public void SetCue()
            {
                Cue = Cue == CurPat ? -1 : CurPat;
            }


            public void CueNextPattern()
            {
                //var noteLen = (int)(EditLength * g_ticksPerStep);

                Length = Patterns.Count * g_nSteps;
                //    g_song.Arpeggio != null
                //    ? (int)Math.Round(g_song.Arpeggio.Length.UpdateValue(g_time, 0, g_song.StartTime, noteLen, null, g_song.CurSrc))
                /*:*/


                if (Cue > -1)
                {
                    var b = GetBlock(PlayPat);

                    if (g_block && b != null)
                        PlayPat = b.Last;
                }


                if (PlayStep >= (PlayPat + 1) * g_nSteps)
                { 
                    int start, end;
                    GetPosLimits(PlayPat, out start, out end);
                    end = start + Math.Min(end - start, Length);

                    if (Cue > -1)
                    {
                        var b = GetBlock(Cue);
                        if (g_block && b != null)
                            Cue = b.First;

                        PlayTime  = GetPatTime(Cue);
                        StartTime = g_time - PlayTime;

                        Cue = -1;
                    }
                    else if (PlayStep >= end)
                    {
                        StopCurrentNotes();

                        PlayTime  -= (end - start) * g_ticksPerStep;
                        StartTime += (end - start) * g_ticksPerStep;
                    }
                }


                PlayPat =
                    PlayTime > -1
                    ? (int)(PlayStep / g_nSteps)
                    : -1;


                //if (PlayTime > -1)
                //{
                //         if (CurPat > oldPat) StartTime -= nSteps * g_ticksPerStep;
                //    else if (CurPat < oldPat) StartTime += nSteps * g_ticksPerStep;
                //}
            }


            public void GetPosLimits(int pat, out int start, out int end)
            {
                int first, last;
                GetPlayPatterns(pat, out first, out last);

                start =  first     * g_nSteps;
                end   = (last + 1) * g_nSteps;
            }


            public void GetPlayPatterns(int p, out int f, out int l)
            {
                if (g_loop)
                {
                    f = p;
                    l = p;

                    var b = GetBlock(p);

                    if (   g_block
                        && b != null)
                    {
                        f = b.First;
                        l = b.Last;
                    }
                }
                else
                {
                    f = 0;
                    l = Patterns.Count-1;
                }
            }


            public void StopCurrentNotes(int ch = -1)
            {
                var timeStep = PlayTime > -1 ? PlayStep : TimeStep;

                foreach (var note in g_notes)
                {
                    if (   ch < 0
                        || note.iChan == ch)
                    { 
                        var noteStep = PlayTime > -1 ? note.SongStep : note.PatStep;
                        note.UpdateStepLength(timeStep - noteStep);
                    }
                }
            }


            public void FinalizePlayback()
            {
                //var pat = song.Patterns[song.PlayPat];

                //for (int ch = 0; ch < nChans; ch++)
                //{
                //    var chan = pat.Channels[ch];

                //    var arpNotes = chan.Notes.FindAll(n =>
                //                n.Instrument.Arpeggio != null
                //            && (int)(song.PlayStep * g_ticksPerStep) >= (int)((song.PlayPat * nSteps + n.StepTime               ) * g_ticksPerStep)
                //            && (int)(song.PlayStep * g_ticksPerStep) <  (int)((song.PlayPat * nSteps + n.StepTime + n.StepLength) * g_ticksPerStep));

                //    var noteLen = (int)(EditLength * g_ticksPerStep);

                //    foreach (var n in arpNotes)
                //    {
                //        var arp = n.Instrument.Arpeggio;

                //        n.FramePlayTime += arp.Scale .UpdateValue(g_time, 0, song.StartTime, noteLen, n, -1);
                //        var maxLength    = arp.Length.UpdateValue(g_time, 0, song.StartTime, noteLen, n, -1);

                //        while (n.FramePlayTime >= maxLength * g_ticksPerStep)
                //            n.FramePlayTime -= maxLength * g_ticksPerStep;
                //    }
                //}


                if (PlayTime > -1)
                    PlayTime++;
            }


            public string Save()
            {
                var cfg = 
                      WS(PlayTime)
                    + WS(PlayPat)
                    + S(Cue);

                return
                      N(Name.Replace("\n", "\u0085"))
                    + N(cfg)
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
                var save = S(Blocks.Count);

                foreach (var b in Blocks)
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

                var cfg = lines[line++].Split(';');
                int i   = 0;

                song.PlayTime = long.Parse(cfg[i++]);
                song.PlayPat  = int .Parse(cfg[i++]);
                song.Cue      = int .Parse(cfg[i++]);

                if (!song.LoadPatterns(lines, ref line)) return null;
                if (!song.LoadBlocks(lines[line++]))     return null;

                song.UpdateAutoKeys();

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
