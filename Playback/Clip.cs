using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public partial class Clip
        {
            public string        Name;
            public Track         Track;

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

            public float         PlayStep { get { return OK(PlayTime) ? PlayTime / (float)g_ticksPerStep : fN; } }


            public int           CueNext;

            public bool          Loop,
                                 Block,
                                 AllPats,
                                 Follow,
                                 AutoCue;

            public bool          AllChan,
                                 RndInst,
                                 
                                 Piano,
                                 
                                 Transpose  = false,
                                 Spread     = false,
                                        
                                 Shift      = false,
                                 MixerShift = false,
                                 
                                 Hold,
                                 Pick,
                                 
                                 ChordMode,
                                 ChordEdit,
                                 ChordAll,
                                 HalfSharp,
                                 
                                 ParamKeys,
                                 ParamAuto,
                                 
                                 MemSet = false;
                                 
                                 
            public int           ChordSpread;
                                 
            public bool          In, 
                                 Out,
                                 
                                 MovePat;
                                 
            public float         Volume;
                           

            public int           CurPat,
                                 CurChan,
                                 SelChan = -1,
                                 CurSrc  = -1,
                                 CurSet  = -1;
                                 
            public int           EditStep   = 2;
            public int           EditLength = 2;
                                 
                                 
            public List<int>[]   Chords; // = new List<int>[4];
                                 
            public int           Chord,
                                 CurNote,
                                 
                                 SongOff,
                                 InstOff,
                                 SrcOff,
                                 
                                 Solo = -1;
                                 
                                 
            public int           ColorIndex;
                                 
                                 
            public int[]         Mems = new int[nMems];
                                 

            public Pattern       CurrentPattern     { get { return Patterns[CurPat]; } }
            public Channel       CurrentChannel     { get { return CurrentPattern.Channels[CurChan]; } }
            public Instrument    CurrentInstrument  { get { return CurrentChannel.Instrument; } }
            public Channel       SelectedChannel    { get { return SelChan > -1 ? CurrentPattern.Channels[SelChan] : null; } }
            public Instrument    SelectedInstrument { get { return SelectedChannel?.Instrument ?? null; } }
            public Source        SelectedSource     { get { return CurSrc > -1 ? SelectedInstrument.Sources[CurSrc] : null; } }


            public Clip(Track track, string name = "Clip 1")
            {
                Name        = name;
                Track       = track;
                            
                Arpeggio    = null;
                Length      = -1;
                            
                Patterns = new List<Pattern>();

                Blocks      = new List<Block>();

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>();

                EditNotes   = new List<Note>();
                            
                PlayTime    = long_NaN;
                StartTime   = long_NaN;
                            
                PlayPat     = -1;
                            
                CueNext     = -1;
                            
                Loop        =
                Block       =
                AllPats     =
                Follow      =
                AutoCue     = false;

                MovePat     = 
                         
                In          = 
                Out         = 
                         
                AllChan     = 
                RndInst     = 
                         
                Piano       = 
                         
                Shift       = 
                MixerShift  = 
                         
                Hold        = 
                Pick        = 
                         
                ChordMode   =
                ChordEdit   =
                ChordAll    = 
                         
                HalfSharp   =
                         
                ParamKeys   = 
                ParamAuto   =
                         
                MemSet      = false;

                CurPat      =  
                CurChan     = 0;
                SelChan     = 
                CurSrc      = 
                CurSet      = -1;
                           
                EditStep    =  
                EditLength  = 2;
                         
                CurNote     = 69 * NoteScale;
                         
                Chord       = -1;
                ChordSpread =  
                            
                SongOff     =  
                InstOff     =  
                SrcOff      = 0;
                         
                Solo        = -1;
                           
                Volume      = 1;
                           
                ColorIndex  = 4;


                for (int m = 0; m < nMems; m++)
                    Mems[m] = -1;


                Chords = new List<int>[4];

                for (int i = 0; i < Chords.Length; i++)
                    Chords[i] = new List<int>();


                ResetState();
            }


            public Clip(Clip clip)
            {
                Name     = clip.Name;
                Track    = clip.Track;

                Arpeggio = clip.Arpeggio;
                Length   = clip.Length;

                Patterns = new List<Pattern>();
                foreach (var pat in clip.Patterns)
                { 
                    Patterns.Add(new Pattern(pat));
                    Patterns.Last().Clip = this;
                }

                Blocks = new List<Block>();
                foreach (var b in clip.Blocks)
                    Blocks.Add(new Block(b));

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>(clip.ChannelAutoKeys[i]);

                EditNotes   = new List<Note>();
                            
                PlayTime    = clip.PlayTime;
                StartTime   = clip.StartTime;
                            
                PlayPat     = clip.PlayPat;
                            
                CueNext     = clip.CueNext;
                            
                Loop        = clip.Loop;
                Block       = clip.Block;
                AllPats     = clip.AllPats;
                Follow      = clip.Follow;
                AutoCue     = clip.AutoCue;

                MovePat     = clip.MovePat;

                In          = clip.In;
                Out         = clip.Out;

                AllChan     = clip.AllChan;
                RndInst     = clip.RndInst;

                Piano       = clip.Piano;

                Shift       = clip.Shift;
                MixerShift  = clip.MixerShift;

                Hold        = clip.Hold;
                Pick        = clip.Pick;

                ChordMode   = clip.ChordMode;
                ChordEdit   = clip.ChordEdit;
                ChordAll    = clip.ChordAll;

                HalfSharp   = clip.HalfSharp;

                ParamKeys   = clip.ParamKeys;
                ParamAuto   = clip.ParamAuto;

                MemSet      = clip.MemSet;

                CurPat      = clip.CurPat;
                CurChan     = clip.CurChan;
                SelChan     = clip.SelChan;
                CurSrc      = clip.CurSrc;
                CurSet      = clip.CurSet;

                EditStep    = clip.EditStep;
                EditLength  = clip.EditLength;

                CurNote     = clip.CurNote;

                Chord       = clip.Chord;
                ChordSpread = clip.ChordSpread;

                SongOff     = clip.SongOff;
                InstOff     = clip.InstOff;
                SrcOff      = clip.SrcOff;

                Solo        = clip.Solo;

                Volume      = clip.Volume;

                ColorIndex  = clip.ColorIndex;


                for (int m = 0; m < nMems; m++)
                    Mems[m] = -1;


                Chords = new List<int>[4];

                for (int i = 0; i < Chords.Length; i++)
                    Chords[i] = new List<int>();


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
                CueNext = CueNext == CurPat ? -1 : CurPat;
            }


            public void CueNextPattern()
            {
                Length = Patterns.Count * g_nSteps;


                if (CueNext > -1)
                {
                    var b = GetBlock(PlayPat);

                    if (Block && b != null)
                        PlayPat = b.Last;
                }


                if (PlayStep >= (PlayPat + 1) * g_nSteps)
                { 
                    int start, end;
                    GetPosLimits(PlayPat, out start, out end);
                    end = start + Math.Min(end - start, Length);

                    if (CueNext > -1)
                    {
                        var b = GetBlock(CueNext);
                        if (Block && b != null)
                            CueNext = b.First;

                        PlayTime  = GetPatTime(CueNext);
                        StartTime = g_time - PlayTime;

                        CueNext = -1;
                    }
                    else if (PlayStep >= end)
                    {
                        WrapCurrentNotes(end - start);

                        PlayTime  -= (end - start) * g_ticksPerStep;
                        StartTime += (end - start) * g_ticksPerStep;
                    }
                }


                PlayPat =
                    OK(PlayTime)
                    ? (int)(PlayStep / g_nSteps)
                    : -1;
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
                if (Loop)
                {
                    f = p;
                    l = p;

                    var b = GetBlock(p);

                    if (   Block
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


            public void TrimCurrentNotes(int ch = -1)
            {
                var timeStep = OK(PlayTime) ? PlayStep : TimeStep;

                foreach (var note in g_notes)
                {
                    if (   ch < 0
                        || note.iChan == ch)
                    { 
                        var noteStep = OK(PlayTime) ? note.SongStep : note.PatStep;
                        note.UpdateStepLength(timeStep - noteStep);
                    }
                }
            }


            public void WrapCurrentNotes(int nWrapSteps)
            {
                var timeStep = OK(PlayTime) ? PlayStep : TimeStep;

                foreach (var note in g_notes)
                {
                    var noteStep = OK(PlayTime) ? note.SongStep : note.PatStep;

                    if (   timeStep >= noteStep
                        && timeStep <  noteStep + note.StepLength)
                        note.UpdateStepTime(-nWrapSteps);
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


                if (OK(PlayTime))
                    PlayTime++;
            }


            public void StartBlock()
            {
                var b = GetBlock(CurPat);

                if (b == null)
                {
                    Blocks.Add(new Block(CurPat));

                    In     = true;
                    Follow = false;

                    UpdateLight(lblFollow, false);
                }
                else
                {
                    In = !In;

                    if (In)
                    {
                        Out    = false;
                        Follow = false;

                        UpdateLight(lblFollow, false);
                    }
                }
            }


            public void EndBlock()
            {
                var b = GetBlock(CurPat);

                if (b == null)
                {
                    Blocks.Add(new Block(CurPat));

                    Out    = true;
                    Follow = false;

                    UpdateLight(lblFollow, false);
                }
                else
                {
                    Out = !Out;

                    if (Out)
                    {
                        In     = false;
                        Follow = false;

                        UpdateLight(lblFollow, false);
                    }

                    //g_blocks[b].Next = currentPattern + 1;

                    //if (g_blocks[b].Next == g_blocks[b].Start)
                    // g_blocks[b].Next = g_blocks[b].Start + 1;
                    //else if (g_blocks[b].Next < g_blocks[b].Start)
                    // Swap(ref g_blocks[b].Next, ref g_blocks[b].Start);
                }
            }
        

            public void ClearBlock()
            {
                Blocks.Remove(GetBlock(CurPat));
                DisableBlock();
                g_clipPressed.Add(11);
            }


            public void DisableBlock()
            {
                In  = false;
                Out = false;
            }


            public void MovePatternOff()
            {
                MovePat = false;
                UpdateLight(lblMovePat, false);
            }


            public void ToogleLoop()
            {
                Loop = !Loop;
                UpdateLight(lblLoop, Loop);
            }


            public void ToggleMove()
            {
                if (CurSet > -1) return;

                g_move = !g_move;

                UpdateLight(lblMove, g_move ^ (CurSrc > -1), SelChan > -1 && !g_move);
                UpdateLight(lblPrev, g_move || CurSrc > -1,  SelChan > -1);
                UpdateLight(lblNext, g_move || CurSrc > -1,  SelChan > -1);
            }


            public void ToggleMovePattern()
            {
                MovePat = !MovePat;

                UpdateLight(lblPrevPat, MovePat);
                UpdateLight(lblNextPat, MovePat);
                UpdateLight(lblMovePat, MovePat);

                if (MovePat)
                    DisableBlock();
            }


            public void ToggleBlock()
            {
                Block = !Block;
                UpdateLight(lblBlock, Block);
            }


            public void ToggleAllPatterns()
            {
                AllPats = !AllPats;
                UpdateLight(lblAllPatterns, AllPats);
            }


            public void ToggleFollow()
            {
                Follow = !Follow;
                UpdateLight(lblFollow, Follow);

                if (Follow)
                {
                    AutoCue = false;
                    UpdateLight(lblAutoCue, false);
                }
            }


            public void ToggleAutoCue()
            {
                AutoCue = !AutoCue;
                UpdateLight(lblAutoCue, AutoCue);

                if (AutoCue)
                {
                    Cue();

                    Follow = false;
                    UpdateLight(lblFollow, false);
                }
            }


            public void GetCurPatterns(out int first, out int last)
            {
                GetPatterns(CurPat, out first, out last);
            }


            public void GetPatterns(int pat, out int first, out int last)
            {
                first = pat;
                last  = pat;

                if (AllPats)
                {
                    var b = GetBlock(pat);

                    if (   Block
                        && b != null)
                    {
                        first = b.First;
                        last  = b.Last;
                    }
                    else
                    {
                        first = 0;
                        last  = Patterns.Count-1;
                    }
                }
            }


            public void StopEdit()
            {
                if (EditNotes.Count > 0)
                    Hold = false;

                EditNotes.Clear();

                UpdateHoldLight();
            }


            public void LimitRecPosition()
            {
                int st, nx;
                GetPosLimits(CurPat, out st, out nx);

                     if (EditPos >= nx) EditPos -= nx - st;
                else if (EditPos <  st) EditPos += nx - st;

                var cp = (int)(EditPos / g_nSteps);
                if (cp != CurPat) SetCurrentPattern(cp);
            }
        }


        void ClearClips()
        {
            g_sm    .StopAll();

            g_notes .Clear();
            g_sounds.Clear();

            foreach (var track in g_tracks)
            {
                track.Clips  .Clear();
                track.Indices.Clear();
                track.CurIndex = -1;
            }
        }
    }
}
