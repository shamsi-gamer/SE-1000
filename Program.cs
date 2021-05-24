using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const int                     NoteScale          = 2;
        const float                   ControlSensitivity = 12;
                                      
        const int                     OscCount           = 13;
                                         
                                         
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
                                      pnlStorageSession,
                                      pnlStorageInstruments,
                                      pnlStorageTracks;
                                          
                                          
        static IMyRemoteControl       g_remote;
         List<IMyLandingGear>         g_locks   = new List<IMyLandingGear>();
                                                    
         List<IMyTimerBlock>          g_timers  = new List<IMyTimerBlock>();
         List<IMyGyro>                g_gyros   = new List<IMyGyro>();
         List<IMyArtificialMassBlock> g_mass    = new List<IMyArtificialMassBlock>();


        IMyPistonBase                 g_lightPiston;
         IMyMotorBase                 g_lightHinge1, g_lightHinge2;
                                      
                                      
        static List<Note>             lastNotes = new List<Note>();
                                                   
        Channel                       copyChan  = null;
                                  
                                  
        static List<TriggerValue>    _triggerDummy = new List<TriggerValue>();

                                                   
        public Program()
        {
            pnlInfoLog = Get("Info Display") as IMyTextPanel;
            pnlInfoLog.CustomData = ""; // init log storage


            pnlStorageState       = Lcd(strStorage + " State");
            pnlStorageSession     = Lcd(strStorage + " Session");
            pnlStorageInstruments = Lcd(strStorage + " Instruments");
            pnlStorageTracks      = Lcd(strStorage + " Tracks");


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


            //foreach (var track in g_session.Tracks)
            //    track.ResetDisplayVolumes();


            Get(g_locks, b => !b.CustomName.Contains("Fold"));

            Get(g_timers);
            Get(g_gyros);
            Get(g_mass);


            g_lightPiston = Get(strLight + " Piston")  as IMyPistonBase;
            g_lightHinge1 = Get(strLight + " Hinge 1") as IMyMotorBase;
            g_lightHinge2 = Get(strLight + " Hinge 2") as IMyMotorBase;

            g_remote      = Get("Remote Control") as IMyRemoteControl;


            g_init = T;
        }


        void FinishStartup()
        {
            // load oscillators one by one to avoid complexity hang
            if (_loadStep < OscCount)
                LoadOscillatorSamples(_loadStep++);

            if (_loadStep == OscCount)
            {
                _loadStep++;

                Load();

                SetLabelColor(EditClip.ColorIndex);
            }
        }


        void InitDisplays()
        {
            dspMain   = new Display(Dsp("Main"));

            dspIO     = new Display(Dsp("IO"));
            dspInfo   = new Display(Dsp("Info"));
            
            dspClip1  = new Display(Dsp(strClip,  1));
            dspClip2  = new Display(Dsp(strClip,  2));
            
            dspMixer1 = new Display(Dsp(strMixer, 1));
            dspMixer2 = new Display(Dsp(strMixer, 2));

            dspVol1   = new Display(Dsp(strVol,   1));
            dspVol2   = new Display(Dsp(strVol,   2));
            dspVol3   = new Display(Dsp(strVol,   3));
        }
    }
}
