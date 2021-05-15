﻿using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public partial class Clip
        {
            public string        Name;
            public Track         Track;

            public Arpeggio      Arpeggio; // indicates that this clip is an arpeggio

            public List<Pattern> Patterns;
            public List<Block>   Blocks;

            public List<Key>[]   ChannelAutoKeys = new List<Key>[g_nChans];


            public int           Length;

            public float         EditPos,
                                 LastEditPos;
            
            public List<Note>    EditNotes;
            public Note          Inter;


            public bool[]        ChanOn = new bool[g_nChans];


            public int           CueNext;

            public bool          Recording;

            public bool          Loop,
                                 Block,
                                 AllPats,
                                 Follow,
                                 AutoCue;

            public bool          AllChan,
                                 RndInst,
                                 
                                 Piano,
                                 
                                 Transpose  = F,
                                 Spread     = F,
                                        
                                 Shift      = F,
                                 MixerShift = F,
                                 
                                 Hold,
                                 Pick,
                                 
                                 ChordMode,
                                 ChordEdit,
                                 ChordAll,
                                 HalfSharp,
                                 
                                 ParamKeys,
                                 ParamAuto,
                                 
                                 MemSet = F;
                                 
                                 
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
                                 
            public int           EditStepIndex   = 2;
            public int           EditLengthIndex = 2;
                                 
                                 
            public List<int>[]   Chords; // = new List<int>[4];
                                 
            public int           Chord,
                                 CurNote,
                                 
                                 SongOff,
                                 InstOff,
                                 SrcOff,
                                 
                                 Solo = -1;
                                 
                                 
            public int           ColorIndex;
                                 
                                 
            public int[]         Mems = new int[nMems];
                                 

            public Pattern       CurPattern     { get { return Patterns[CurPat]; } }
            public Channel       CurChannel     { get { return CurPattern.Channels[CurChan]; } }
            public Instrument    CurInstrument  { get { return CurChannel.Instrument; } }
            public Channel       SelChannel    { get { return SelChan > -1 ? CurPattern.Channels[SelChan] : null; } }
            public Instrument    SelInstrument { get { return SelChannel?.Instrument ?? null; } }
            public Source        SelSource     { get { return CurSrc > -1 ? SelInstrument.Sources[CurSrc] : null; } }


            public float         EditStep       { get { return g_steps[EditStepIndex  ]; } }
            public float         EditStepLength { get { return g_steps[EditLengthIndex]; } }
            public int           EditLength     { get { return (int)(EditStepLength * g_session.TicksPerStep); } }


            public Clip(Track track, string name = "Clip 1")
            {
                Name        = name;
                Track       = track;
                            
                Arpeggio    = null;
                Length      = -1;
                            
                Patterns    = new List<Pattern>();

                Blocks      = new List<Block>();

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>();

                EditNotes   = new List<Note>();

                CueNext     = -1;
                            
                Recording   = 
                Loop        =
                Block       =
                AllPats     =
                Follow      =
                AutoCue     = F;

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
                         
                MemSet      = F;

                CurPat      =  
                CurChan     = 0;
                SelChan     = 
                CurSrc      = 
                CurSet      = -1;
                           
                EditStepIndex    =  
                EditLengthIndex  = 2;
                         
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

                EditNotes       = new List<Note>();
                                
                CueNext         = clip.CueNext;
                                
                Loop            = clip.Loop;
                Block           = clip.Block;
                AllPats         = clip.AllPats;
                Follow          = clip.Follow;
                AutoCue         = clip.AutoCue;
                                
                MovePat         = clip.MovePat;
                                
                In              = clip.In;
                Out             = clip.Out;
                                
                AllChan         = clip.AllChan;
                RndInst         = clip.RndInst;
                                
                Piano           = clip.Piano;
                                
                Shift           = clip.Shift;
                MixerShift      = clip.MixerShift;
                                
                Hold            = clip.Hold;
                Pick            = clip.Pick;
                                
                ChordMode       = clip.ChordMode;
                ChordEdit       = clip.ChordEdit;
                ChordAll        = clip.ChordAll;
                                
                HalfSharp       = clip.HalfSharp;
                                
                ParamKeys       = clip.ParamKeys;
                ParamAuto       = clip.ParamAuto;
                                
                MemSet          = clip.MemSet;
                                
                CurPat          = clip.CurPat;
                CurChan         = clip.CurChan;
                SelChan         = clip.SelChan;
                CurSrc          = clip.CurSrc;
                CurSet          = clip.CurSet;

                EditStepIndex   = clip.EditStepIndex;
                EditLengthIndex = clip.EditLengthIndex;

                CurNote         = clip.CurNote;
                                
                Chord           = clip.Chord;
                ChordSpread     = clip.ChordSpread;
                                
                SongOff         = clip.SongOff;
                InstOff         = clip.InstOff;
                SrcOff          = clip.SrcOff;
                                
                Solo            = clip.Solo;
                                
                Volume          = clip.Volume;
                                
                ColorIndex      = clip.ColorIndex;


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
                                keys[k].StepTime + p*g_patSteps,
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
            public float GetStep  (Key key) { return GetKeyPat(key) * g_patSteps + key.StepTime; }


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


            public void GetPosLimits(int pat, out int start, out int end)
            {
                int first, last;
                GetPlayPatterns(pat, out first, out last);

                start =  first     * g_patSteps;
                end   = (last + 1) * g_patSteps;
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
                var timeStep = g_playing ? PlayStep : TimeStep;

                foreach (var note in g_notes)
                {
                    if (   ch < 0
                        || note.iChan == ch)
                    { 
                        var noteStep = g_playing ? note.SongStep : note.PatStep;
                        note.UpdateStepLength(timeStep - noteStep);
                    }
                }
            }


            public void WrapCurrentNotes(int nWrapSteps)
            {
                var timeStep = g_playing ? PlayStep : TimeStep;

                foreach (var note in g_notes)
                {
                    var noteStep = g_playing ? note.SongStep : note.PatStep;

                    if (   timeStep >= noteStep
                        && timeStep <  noteStep + note.StepLength)
                        note.UpdateStepTime(-nWrapSteps);
                }
            }


            public void StartBlock()
            {
                var b = GetBlock(CurPat);

                if (b == null)
                {
                    Blocks.Add(new Block(CurPat));

                    In     = T;
                    Follow = F;
                }
                else
                {
                    In = !In;

                    if (In)
                    {
                        Out    = F;
                        Follow = F;
                    }
                }

                MovePatternOff();
            }


            public void EndBlock()
            {
                var b = GetBlock(CurPat);

                if (b == null)
                {
                    Blocks.Add(new Block(CurPat));

                    Out    = T;
                    Follow = F;
                }
                else
                {
                    Out = !Out;

                    if (Out)
                    {
                        In     = F;
                        Follow = F;
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
                MovePatternOff(); 
                g_clipPressed.Add(11);
            }


            public void DisableBlock()
            {
                In  = F;
                Out = F;
            }


            public void MovePatternOff()
            {
                MovePat = F;
            }


            public void ToogleLoop()
            {
                Loop = !Loop;
            }


            public void ToggleMovePattern()
            {
                MovePat = !MovePat;

                if (MovePat)
                    DisableBlock();
            }


            public void ToggleBlock()
            {
                Block = !Block;
            }


            public void ToggleAllPatterns()
            {
                AllPats = !AllPats;
            }


            public void ToggleFollow()
            {
                Follow = !Follow;

                if (Follow)
                {
                    AutoCue = F;
                }
            }


            public void ToggleAutoCue()
            {
                AutoCue = !AutoCue;

                if (AutoCue)
                {
                    Cue();
                    Follow = F;
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
                    Hold = F;

                EditNotes.Clear();

                //UpdateHoldLabel();
            }


            public void LimitRecPosition()
            {
                int st, nx;
                GetPosLimits(CurPat, out st, out nx);

                     if (EditPos >= nx) EditPos -= nx - st;
                else if (EditPos <  st) EditPos += nx - st;

                var cp = (int)(EditPos / g_patSteps);
                if (cp != CurPat) SetCurrentPattern(cp);
            }
        }
    }
}
