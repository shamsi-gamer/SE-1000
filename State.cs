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
        bool[] g_on = new bool[nChans];


        bool               g_in, 
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
        

        int                g_chordSpread;
        float              g_volume;
                           
        static int         g_ticksPerStep = 7;

        public static int  CurPat,
                           CurChan,
                           SelChan = -1,
                           CurSrc  = -1,
                           CurSet  = -1,
                            
                           PlayPat;


        static long        g_time    = -1; // in ticks
 
        float              TimeStep { get { return (float)g_time / g_ticksPerStep; } }


        public static long StartTime = -1, // in ticks
                           PlayTime  = -1;

        public float       PlayStep { get { return PlayTime > -1 ? PlayTime / (float)g_ticksPerStep : fN; } }


        float[]            g_steps = { 0.25f, 0.5f, 1, 2, 4, 8, 16, 65536f };

        int                g_editStep   = 2;
        int                g_editLength = 2;

        float              EditStep   { get { return g_steps[g_editStep  ]; } }
        float              EditLength { get { return g_steps[g_editLength]; } }



        List<int>[]        g_chords; // = new List<int>[4];

        int                g_chord,
                           g_curNote,
                              
                           g_songOff,
                           g_instOff,
                           g_srcOff,

        //int                g_showNote;

                           g_cue  = -1,
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
                          
            PlayTime      = -1;
            StartTime     = -1;
            PlayPat       = -1;
            
            g_editStep    =  2;
            g_editLength  =  2;

            g_curNote     = 69 * NoteScale;

            g_chord       = -1;
            g_chordSpread =  0;
                             
            g_songOff     =  0;
            g_instOff     =  0;
            g_srcOff      =  0;

            g_cue         = -1;
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

