using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const int                 NoteScale          = 2;
        const float               ControlSensitivity = 12;                                
                                  
                                  
        static List<string>       g_samples          = new List<string>();
                                                     
        static List<Note>         g_notes            = new List<Note>();
        static List<Sound>        g_sounds           = new List<Sound>();
                                                     
        static List<LFO>          g_lfo              = new List<LFO>();
        static List<Modulate>     g_mod              = new List<Modulate>();
                                                     
                                                     
        float[]                   g_dspVol           = new float[g_nChans];
                                  
                                  
        static Display            dspMixer1, dspMixer2,
                                  dspVol1, dspVol2, dspVol3,
                                  dspInfo,
                                  dspIO,
                                  dspClip1, dspClip2,
                                  dspMain;
                                  
        static IMyTextPanel       pnlInfoLog,

                                  pnlStorageState,
                                  pnlStorageSession,
                                  pnlStorageInstruments,
                                  pnlStorageTracks;


        static IMyRemoteControl   g_remote;
        List<IMyLandingGear>      g_locks          = new List<IMyLandingGear>();
                                                   
        List<IMyGyro>             g_gyros          = new List<IMyGyro>();
        List<IMyTimerBlock>       g_timers         = new List<IMyTimerBlock>();

        IMyPistonBase             g_lightPiston;
        IMyMotorBase              g_lightHinge1, g_lightHinge2;


        static List<Note>         lastNotes        = new List<Note>();

        Channel                   copyChan         = null;


        static List<TriggerValue> _triggerDummy    = new List<TriggerValue>();

                                                   
        public Program()
        {
            pnlInfoLog = Get("Info Display") as IMyTextPanel;
            pnlInfoLog.CustomData = ""; // init log storage

            pnlStorageState       = Get("Storage State")       as IMyTextPanel;
            pnlStorageSession     = Get("Storage Session")     as IMyTextPanel;
            pnlStorageInstruments = Get("Storage Instruments") as IMyTextPanel;
            pnlStorageTracks      = Get("Storage Tracks")      as IMyTextPanel;

            dspMain = new Display(Dsp("Main"));


            InitSpeakers();
            g_sm.Speakers[0].Block.GetSounds(g_samples);


            if (!IsModPresent())
            {
                DrawMissingMod();
                return;
            }


            Runtime.UpdateFrequency =
                  UpdateFrequency.Update1
                | UpdateFrequency.Update10;


            for (int i = 0; i < g_random.Length; i++)
                g_random[i] = RND;


            InitDisplays();
            InitLabels();
            InitFuncButtons(); 


            for (int i = 0; i < g_nChans; i++)
                g_dspVol[i] = 0;


            Get(g_locks, b => !b.CustomName.Contains("Fold"));

            Get(g_gyros);
            Get(g_timers);


            g_lightPiston = Get("Light Piston")   as IMyPistonBase;
            g_lightHinge1 = Get("Light Hinge 1")  as IMyMotorBase;
            g_lightHinge2 = Get("Light Hinge 2")  as IMyMotorBase;

            g_remote      = Get("Remote Control") as IMyRemoteControl;


            g_init = true;
        }


        void FinishStartup()
        {
            // load oscillators one by one to avoid complexity hang
            if (_loadStep < 10)
                LoadOscillatorSamples(_loadStep++);

            if (_loadStep == 10)
            {
                _loadStep++;

                Load();

                //UpdateLabels();
                SetLabelColor(CurClip.ColorIndex);
            }
        }


        void InitDisplays()
        {
            dspIO     = new Display(Dsp("IO"      ));
            dspInfo   = new Display(Dsp("Info"    ));
            dspClip1  = new Display(Dsp("Clip",  1));
            dspClip2  = new Display(Dsp("Clip",  2));
            dspMixer1 = new Display(Dsp("Mixer", 1));
            dspMixer2 = new Display(Dsp("Mixer", 2));
            dspVol1   = new Display(Dsp(strVol,  1));
            dspVol2   = new Display(Dsp(strVol,  2));
            dspVol3   = new Display(Dsp(strVol,  3));
        }
    }
}
