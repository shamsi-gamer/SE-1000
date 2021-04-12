using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        bool[] g_on = new bool[g_nChans];


        static bool        g_in, 
                           g_out,

                           g_loop,
                           g_block,
                           g_movePat,
                           g_allPats,
                           g_follow,
                           g_autoCue,

                           g_allChan,
                           g_rndInst,
            
                           g_piano,
                           g_move       = false,
            
                           g_transpose  = false,
                           g_spread     = false,
                                        
                           g_shift      = false,
                           g_mixerShift = false,
            
                           g_hold,
                           g_pick,
            
                           g_chordMode,
                           g_chordEdit,
                           g_chordAll,
                           g_halfSharp,

                           g_paramKeys,
                           g_paramAuto,
                           
                           g_setMem = false;
        

        static int         g_chordSpread;
        static float       g_volume;
                           
        static int         g_ticksPerStep = 7;

        public static int  CurPat,
                           CurChan,
                           SelChan = -1,
                           CurSrc  = -1,
                           CurSet  = -1;


        static long        g_time    = -1; // in ticks
 
        static float       TimeStep { get { return (float)g_time / g_ticksPerStep; } }


        static float[]     g_steps = { 0.25f, 0.5f, 1, 2, 4, 8, 16, 65536f };

        static int         g_editStep   = 2;
        static int         g_editLength = 2;


        static float       EditStep       { get { return g_steps[g_editStep  ]; } }
        static float       EditStepLength { get { return g_steps[g_editLength]; } }
        static int         EditLength     { get { return (int)(EditStepLength * g_ticksPerStep); } }



        static List<int>[] g_chords; // = new List<int>[4];

        static int         g_chord,
                           g_curNote,
                              
                           g_songOff,
                           g_instOff,
                           g_srcOff,

        //int                g_showNote;

                           g_solo = -1;


        int                g_iCol;
                           

        int[]              g_mem  = new int[nMems];



        void SetDefaultMachineState()
        {
            ClearSong();

            g_movePat    = 

            g_in         = 
            g_out        = 

            g_loop       = 
            g_block      = 
            g_allPats    = 
            g_follow     = 
            g_autoCue    = 

            g_allChan    = 
            g_rndInst    = 

            g_piano      = 
            g_move       = 

            g_shift      = 
            g_mixerShift = 

            g_hold       = 
            g_pick       = 

            g_chordMode  =
            g_chordEdit  =
            g_chordAll   = 

            g_halfSharp  =

            g_paramKeys  = 
            g_paramAuto  =
            
            g_setMem     = false;


            g_ticksPerStep = 7;


            CurPat        =  0;
            CurChan       =  0;
            SelChan       = -1;
            CurSrc        = -1;
            CurSet        = -1;
                          
            g_editStep    =  2;
            g_editLength  =  2;

            g_curNote     = 69 * NoteScale;

            g_chord       = -1;
            g_chordSpread =  0;
                             
            g_songOff     =  0;
            g_instOff     =  0;
            g_srcOff      =  0;

            g_solo        = -1;
                            
            g_volume      =  1;
                            
            g_iCol        =  0;


            for (int m = 0; m < nMems; m++)
                g_mem[m] = -1;


            g_chords = new List<int>[4];

            for (int i = 0; i < g_chords.Length; i++)
                g_chords[i] = new List<int>();


            g_inst.Clear();
            g_inst.Add(new Instrument());
            g_inst[0].Sources.Add(new Source(g_inst[0]));
            g_song.Patterns.Add(new Pattern(g_song, g_inst[0]));
            g_song.Name = "New Song";


            UpdateSongDsp();

            SetLightColor(4);
            UpdateLights();
        }
    }
}

