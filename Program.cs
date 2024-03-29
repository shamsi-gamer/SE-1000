﻿using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const int                     OscCount           = 14,
                                      
                                      NoteScale          = 2;
        const float                   ControlSensitivity = 0.01f;

        const float                   AccentStep         = 0.05f;

                                         
                                         
        static  List<string>          g_samples = new List<string>();
                                      
        static  List<Note>            g_notes   = new List<Note>();
        static  List<Sound>           g_sounds  = new List<Sound>();
                                      
        static  List<LFO>             g_lfo     = new List<LFO>();
        static  List<Modulate>        g_mod     = new List<Modulate>();
                                                            

        static Display                dspMixer1, dspMixer2,
                                      dspVol1, dspVol2, dspVol3,
                                      dspInfo,
                                      dspIO,
                                      dspClip1, dspClip2,
                                      dspMain;
                                      
        static IMyTextPanel           pnlInfoLog,
                                      
                                      pnlStorageState,
                                      pnlStorageInstruments,
                                      pnlStorageTracks; 
        
        static IMyRemoteControl       g_remote;
                                      
        List<IMyTimerBlock>           g_timers = new List<IMyTimerBlock>();
        List<IMyGyro>                 g_gyros  = new List<IMyGyro>();
        List<IMyArtificialMassBlock>  g_mass   = new List<IMyArtificialMassBlock>();


        IMyPistonBase                 g_lightPiston;
         IMyMotorBase                 g_lightHinge1, 
                                      g_lightHinge2, 
                                      g_hingeL, 
                                      g_hingeR;


        static List<Note>             g_lastNotes = new List<Note>();
        static List<Channel>          g_copyChans = new List<Channel>();

        static float                  g_noteScaleExp, // scale = 2 ^ g_noteScaleExp
                                      g_noteScaleOrigin;


        static List<TriggerValue>    _triggerDummy = new List<TriggerValue>();




                                                   
        public Program()
        {
            Runtime.UpdateFrequency =
                  UpdateFrequency.Update1
                | UpdateFrequency.Update10;


            pnlInfoLog = Get("Info Display") as IMyTextPanel;
            
            ResetLog();


            pnlStorageState       = GetLcd(strStorage + " State");
            pnlStorageInstruments = GetLcd(strStorage + " Instruments");
            pnlStorageTracks      = GetLcd(strStorage + " Tracks");


            InitSpeakers();
            g_sm.Speakers[0].Block.GetSounds(g_samples);


            for (int i = 0; i < g_random.Length; i++)
                g_random[i] = RND;


            ResetIO();


            InitDisplays();
            InitLabels();
            InitFuncButtons(); 


            Get(g_timers);
            Get(g_gyros);
            Get(g_mass);


            g_lightPiston = Get("Piston " + strLight) as IMyPistonBase;
            g_lightHinge1 = GetHinge(strLight + " 1");
            g_lightHinge2 = GetHinge(strLight + " 2");
            g_hingeL      = GetHinge("L");
            g_hingeR      = GetHinge("R");

            g_remote      = Get("Remote Control") as IMyRemoteControl;


            g_init = True;
        }



        void FinishStartup()
        {
            // load oscillators one by one to avoid complexity hang

            if (_loadStep < OscCount)
                LoadOscillatorSamples(_loadStep++);

            if (_loadStep == OscCount)
            {
                _loadStep++;

                if (pnlStorageState.GetText().Trim() == "")
                { 
                    Load("", pnlStorageInstruments.GetText(), "");
                    SetLabelColor(EditedClip.ColorIndex);
                    return;
                }
                else
                { 
                    Load(
                        pnlStorageState      .GetText(),
                        pnlStorageInstruments.GetText(),
                        pnlStorageTracks     .GetText());
                }
            }
        }



        void InitDisplays()
        {
            dspMain   = new Display(GetDisplay("Main"));

            dspIO     = new Display(GetDisplay("IO"));
            dspInfo   = new Display(GetDisplay("Info"));
            
            dspClip1  = new Display(GetDisplay(strClip, 1));
            dspClip2  = new Display(GetDisplay(strClip, 2));
            
            dspMixer1 = new Display(GetDisplay(strMixer, 1));
            dspMixer2 = new Display(GetDisplay(strMixer, 2));

            dspVol1   = new Display(GetDisplay(strVol, 1));
            dspVol2   = new Display(GetDisplay(strVol, 2));
            dspVol3   = new Display(GetDisplay(strVol, 3));
        }
    }
}
