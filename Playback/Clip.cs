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

            public List<Pattern> Patterns;
            public List<Block>   Blocks;

            public List<Key>[]   ChannelAutoKeys = new List<Key>[g_nChans];


            public float         EditPos,
                                 LastEditPos;
            
            public List<Note>    EditNotes;
            public Note          Inter;


            public bool[]        ChanOn = new bool[g_nChans];


            public bool          Loop,
                                 Block,
                                 AllPats,
                                 Follow,
                                 AutoCue;

            public bool          AllChan,
                                 RndInst,
                                 
                                 Accent,
                                 Piano,
                                 
                                 Transpose = False,
                                 Strum    = False,
                                        
                                 Shift     = False,
                                 Move      = False,
                                 
                                 Hold,
                                 Pick,
                                 
                                 ChordMode,
                                 ChordEdit,
                                 ChordAll,
                                 HalfSharp,
                                 
                                 ParamKeys,
                                 ParamAuto,
                                 
                                 SetMemPat = False;
                                 
                                 
            public int           ChordStrum;
                                 
            public bool          In, 
                                 Out,
                                 
                                 MovePat;
                                 
            public float         Volume;
                           

            public int           EditPat,
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

            public List<Setting> Settings;


            public int           Index          => Track.Clips.IndexOf(this);

            public Pattern       EditPattern    => Patterns[EditPat];
            public Channel       CurChannel     => EditPattern.Channels[CurChan];
            public Instrument    CurInstrument  => CurChannel.Instrument;
            public Channel       SelChannel     => OK(SelChan) ? EditPattern.Channels[SelChan] : Channel_null;
            public Instrument    SelInstrument  => SelChannel?.Instrument ?? Instrument_null;
            public Source        SelSource      => OK(CurSrc) ? SelInstrument.Sources[CurSrc] : Source_null;

            public float         EditStep       => g_steps[EditStepIndex  ];
            public float         EditStepLength => g_steps[EditLengthIndex];
            public int           EditLength     => (int)(EditStepLength * TicksPerStep);


            public int           StepLength     => Patterns.Count * g_patSteps;


            public Program       Program;



            public Clip(Track track, Program prog, string name = strClip)
            {
                Track       = track;
                Name        = name;
                            
                Settings    = new List<Setting>();
                Patterns    = new List<Pattern>();
                Blocks      = new List<Block>();

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>();

                EditNotes       = new List<Note>();
                                
                Recording       = 
                Loop            =
                Block           =
                AllPats         =
                Follow          =
                AutoCue         = False;
                                
                MovePat         = 
                                
                In              = 
                Out             = 
                                
                AllChan         = 
                RndInst         = 
                                
                Accent          =
                Piano           = 
                                
                Shift           = 
                MixerShift      =
                SetOrPat        =
                SetMemSet       =
                                
                Move            =

                Hold            = 
                Pick            = 
                                
                ChordMode       =
                ChordEdit       =
                ChordAll        = 
                                
                HalfSharp       =
                                
                ParamKeys       = 
                ParamAuto       =
                                
                SetMemPat       = False;
                                
                EditPat         =  
                CurChan         = 0;
                                
                SelChan         = 
                CurSrc          = 
                CurSet          = -1;
                           
                EditStepIndex   =  
                EditLengthIndex = 2;
                         
                CurNote         = 69 * NoteScale;
                                
                Chord           = -1;
                ChordStrum      =  
                                
                SongOff         =  
                InstOff         =  
                SrcOff          = 0;
                                
                Solo            = -1;
                                
                Volume          = 1;
                                
                ColorIndex      = 4;


                for (int m = 0; m < nMems; m++)
                    Mems[m] = -1;


                Chords = new List<int>[4];

                for (int i = 0; i < Chords.Length; i++)
                    Chords[i] = new List<int>();


                Program = prog;

                ResetState();
            }



            public Clip(Clip clip, Track track, Program prog)
            {
                Name     = clip.Name;
                Track    = track;

                Settings = new List<Setting>();

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
                                
                Accent          = clip.Accent;
                Piano           = clip.Piano;
                                
                Shift           = clip.Shift;
                                
                Move            = clip.Move;

                Hold            = clip.Hold;
                Pick            = clip.Pick;
                                
                ChordMode       = clip.ChordMode;
                ChordEdit       = clip.ChordEdit;
                ChordAll        = clip.ChordAll;
                                
                HalfSharp       = clip.HalfSharp;
                                
                ParamKeys       = clip.ParamKeys;
                ParamAuto       = clip.ParamAuto;
                                
                SetMemPat       = clip.SetMemPat;
                                
                EditPat          = clip.EditPat;
                CurChan         = clip.CurChan;
                SelChan         = clip.SelChan;
                CurSrc          = clip.CurSrc;
                CurSet          = -1;//clip.CurSet;

                EditStepIndex   = clip.EditStepIndex;
                EditLengthIndex = clip.EditLengthIndex;

                CurNote         = clip.CurNote;
                                
                Chord           = clip.Chord;
                ChordStrum      = clip.ChordStrum;
                                
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


                Program = prog;

                ResetState();
            }



            public void ClearAutoKeys()
            {
                foreach (var keys in ChannelAutoKeys)
                    keys.Clear();
            }



            public void UpdateAutoKeys()
            {
                for (int ch = 0; ch < g_nChans; ch++)
                { 
                    if (Program.TooComplex) return;

                    var chanKeys = ChannelAutoKeys[ch];

                    chanKeys.Clear();

                    for (int p = 0; p < Patterns.Count; p++)
                    {
                        if (Program.TooComplex) return;

                        var keys = Patterns[p].Channels[ch].AutoKeys;

                        for (int k = 0; k < keys.Count; k++)
                        { 
                            chanKeys.Add(new Key(
                                keys[k].SourceIndex,
                                keys[k].Parameter,
                                keys[k].Value, 
                                keys[k].Step + p*g_patSteps,
                                keys[k].Channel));
                        }
                    }

                    chanKeys.Sort((a, b) => a.Step.CompareTo(b.Step));
                }
            }



            public static Clip Create(Track track, Program prog)
            {
                var clip = new Clip(track, prog);
                clip.Patterns.Add(new Pattern(Instruments[0], clip));
                GetNewClipName(clip, track.Clips);
                return clip;
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
                EditPos     = float_NaN;
                LastEditPos = float_NaN;

                Inter       = Note_null;
                EditNotes.Clear();
            }



            public int   GetKeyPat(Key key) { return Patterns.FindIndex(p => OK(Array.Find(p.Channels, c => c.AutoKeys.Contains(key)))); }
            public float GetStep  (Key key) { return GetKeyPat(key) * g_patSteps + key.Step; }



            public Block GetBlock(int pat)
            {
                return Blocks.Find(b =>
                       pat >= b.First
                    && pat <= b.Last);
            }



            public void SetCue()
            {
                Track.NextPat = Track.NextPat == EditPat ? -1 : EditPat;
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
                var b = GetBlock(p);

                if (Loop && Block && OK(b))
                {
                    f = b.First;
                    l = b.Last;
                }
                else if (Loop)
                {
                    f = p;
                    l = p;
                }
                else
                {
                    f = 0;
                    l = Patterns.Count-1;
                }
            }



            public void TrimCurrentNotes(int ch = -1)
            {
                var timeStep = Playing ? Track.PlayStep : TimeStep;

                foreach (var note in g_notes)
                {
                    if (   ch < 0
                        || note.iChan == ch)
                    { 
                        var noteStep = Playing ? note.ClipStep : note.Step;
                        note.UpdateStepLength(timeStep - noteStep);
                    }
                }
            }



            public void WrapCurrentNotes(int nWrapSteps)
            {
                var timeStep = Playing ? Track.PlayStep : TimeStep;

                foreach (var note in g_notes)
                {
                    var noteStep = Playing ? note.ClipStep : note.Step;

                    if (   timeStep >= noteStep
                        && timeStep <  noteStep + note.StepLength)
                        note.UpdateStepTime(-nWrapSteps);
                }
            }



            public void StartBlock()
            {
                var b = GetBlock(EditPat);

                if (!OK(b))
                {
                    Blocks.Add(new Block(EditPat));

                    In     = True;
                    Follow = False;
                }
                else
                {
                    In = !In;

                    if (In)
                    {
                        Out    = False;
                        Follow = False;
                    }
                }

                MovePatternOff();
            }



            public void EndBlock()
            {
                var b = GetBlock(EditPat);

                if (!OK(b))
                {
                    Blocks.Add(new Block(EditPat));

                    Out    = True;
                    Follow = False;
                }
                else
                {
                    Out = !Out;

                    if (Out)
                    {
                        In     = False;
                        Follow = False;
                    }
                }
            }
        


            public void ClearBlock()
            {
                Blocks.Remove(GetBlock(EditPat));
                DisableBlock();
                MovePatternOff(); 
                g_lcdPressed.Add(lcdClip + 11);
            }



            public void DisableBlock()
            {
                In  = False;
                Out = False;
            }



            public void MovePatternOff()
            {
                MovePat = False;
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
                    AutoCue = False;
            }



            public void ToggleAutoCue()
            {
                AutoCue = !AutoCue;

                if (AutoCue)
                {
                    SetCue();
                    Follow = False;
                }
            }



            public void Cue()
            {
                SetCue();

                g_lcdPressed.Add(lcdClip + 4);
            }



            public void GetCurPatterns(out int first, out int last)
            {
                GetPatterns(EditPat, out first, out last);
            }



            public void GetPatterns(int pat, out int first, out int last)
            {
                var b = GetBlock(pat);

                if (   Block
                    && OK(b))
                {
                    first = b.First;
                    last  = b.Last;
                }
                else if (AllPats)
                {
                    first = 0;
                    last  = Patterns.Count-1;
                }
                else
                {
                    first = pat;
                    last  = pat;
                }
            }



            public List<Channel> GetCurChannels(int p)
            {
                var pat   = Patterns[p];
                var chans = new List<Channel>();

                if (AllChan)
                {
                    foreach (var ch in pat.Channels)
                        chans.Add(ch);
                }
                else if (RndInst)
                {
                    foreach (var ch in pat.Channels)
                        if (ch.Instrument == CurInstrument)
                            chans.Add(ch);
                }
                else
                    chans.Add(pat.Channels[CurChan]);

                return chans;
            }



            public void StopEdit()
            {
                if (EditNotes.Count > 0)
                    Hold = False;

                EditNotes.Clear();
            }



            public void LimitRecPosition()
            {
                int st, nx;
                GetPosLimits(EditPat, out st, out nx);

                     if (EditPos >= nx) EditPos -= nx - st;
                else if (EditPos <  st) EditPos += nx - st;

                var cp = (int)(EditPos / g_patSteps);
                if (cp != EditPat) SetEditPattern(cp);
            }



            public Setting   LastSetting => Settings.Count > 0 ? Settings.Last()  : Setting_null;
            public Setting   CurSetting  => OK(CurSet)         ? Settings[CurSet] : Setting_null;

            public Parameter CurParam    => (Parameter)CurSetting;
            public Modulate  CurModulate => (Modulate) CurSetting;

            public Bias CurBias => (Bias)CurSetting;
            public Bias CurOrParentBias =>
                IsCurSetting(typeof(Bias))
                ? CurBias
                : (Bias)CurSetting.Parent;

            public Harmonics CurHarmonics => (Harmonics)CurSetting;
            public Harmonics CurOrParentHarmonics =>
                IsCurSetting(typeof(Harmonics))
                ? CurHarmonics
                : (Harmonics)CurSetting.Parent;
        }
    }
}
