using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
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


            g_lightPiston = Get("Label Piston")   as IMyPistonBase;
            g_lightHinge1 = Get("Label Hinge 1")  as IMyMotorBase;
            g_lightHinge2 = Get("Label Hinge 2")  as IMyMotorBase;

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
                SetLabelColor(g_session.CurClip.ColorIndex);
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


        void ProcessArg(string arg)
        {
            arg = arg.ToLower().Trim();

            int val;

            switch (arg)
            {
                case "load":       LoadSongExt();                   break;
                case "save":       SaveSongExt();                   break;

                case "load all":   Load();                          break;
                case "save all":   Save();                          break;
                                                                    
                //case "import":     ImportInstruments();             break;
                                                                    
                case "play":       Play();                          break;
                case "stop":       Stop();                          break;
                                                                        
                case "bpm up":     SetStepLength(-1);               break;
                case "bpm down":   SetStepLength(1);                break;
                                                                        
                case "del pat":    g_session.CurClip.DeletePattern();          break;
                case "dup pat":    g_session.CurClip.DuplicatePattern();       break;
                case "new pat":    g_session.CurClip.NewPattern();             break;
                case "move pat":   g_session.CurClip.ToggleMovePattern();      break;
                case "prev pat":   g_session.CurClip.PrevPattern(g_session.CurClip.MovePat); break;
                case "next pat":   g_session.CurClip.NextPattern(g_session.CurClip.MovePat); break;
                                                                    
                case "loop":       g_session.CurClip.ToogleLoop();             break;
                case "block":      g_session.CurClip.ToggleBlock();            break;
                case "all pat":    g_session.CurClip.ToggleAllPatterns();      break;
                case "auto cue":   g_session.CurClip.ToggleAutoCue();          break;
                case "follow":     g_session.CurClip.ToggleFollow();           break;
                                                                        
                case "new":        New();                           break;
                case "dup":        Duplicate();                     break;
                case "del":        Delete();                        break;
                case "move":       g_session.CurClip.ToggleMove();             break;
                case "prev":       Move(-1);                        break;
                case "next":       Move( 1);                        break;
                                                                    
                case "out":        BackOut();                       break;
                case "back":       Back();                          break;
                case "enter":      Enter();                         break;
                                                                    
                case "f1":         SetFunc(0);                      break;
                case "f2":         SetFunc(1);                      break;
                case "f3":         SetFunc(2);                      break;
                case "f4":         SetFunc(3);                      break;
                case "f5":         SetFunc(4);                      break;
                case "f6":         SetFunc(5);                      break;
                                                                    
                case "cmd1":       Command1();                      break;
                case "cmd2":       Command2();                      break;
                case "up":         Adjust(g_session.CurClip, CurSetting,  1);  break;
                case "down":       Adjust(g_session.CurClip, CurSetting, -1);  break;
                case "shift":      Shift();                         break;
                case "cmd3":       Command3();                      break;
                                                                    
                case "tr up":      SetTranspose(g_session.CurClip,  1);        break;
                case "tr down":    SetTranspose(g_session.CurClip, -1);        break;
                                                                    
                case "spread":     Spread();                        break;

                case "rnd snd":    /*RandomSound(g_session.Instruments.IndexOf(g_session.CurClip.CurrentInstrument));*/ break;
                                       
                case "up all":     SetVolumeAll( 1);                break;
                case "down all":   SetVolumeAll(-1);                break;
                                                                    
                case "solo all":   EnableChannels(true);            break;
                case "mute all":   EnableChannels(false);           break;

                case "m shift":    MixerShift();                    break;

                case "clips":      Clips();                         break;
                                                                    
                case "edit":       Edit();                          break;
                                                                    
                case "chord":      ToggleChordMode();               break;
                case "chord 1":    Chord(1);                        break;
                case "chord 2":    Chord(2);                        break;
                case "chord 3":    Chord(3);                        break;
                case "chord 4":    Chord(4);                        break;
                case "chord edit": ToggleChordEdit();               break;
                                                                    
                case "edit step":  ChangeEditStep();                break;
                case "edit len":   ChangeEditLength();              break;
                                                                    
                case "step":       Step(g_session.CurClip, g_session.CurClip.CurChan);    break;
                case "hold":       Hold(g_session.CurClip);                    break;
                                                                     
                case "left":       Left(g_session.CurClip);                    break;
                case "right":      Right(g_session.CurClip);                   break;
                                                             
                case "random":     Random();                        break;
                                                                    
                case "lock":       Lock();                          break;
                case "auto lock":  AutoLock();                      break;
                                                                        
                case "gyro":       Gyro();                          break;
                case "noise":      NoiseEmitters();                 break;
                                                                    
                case "sb":         g_session.CurClip.StartBlock(); g_session.CurClip.MovePatternOff(); break;
                case "eb":         g_session.CurClip.EndBlock();                            break;
                case "cb":         g_session.CurClip.ClearBlock(); g_session.CurClip.MovePatternOff(); break;
                                                                        
                case "rl":         SetLabelColor(0);                break;
                case "ol":         SetLabelColor(1);                break;
                case "yl":         SetLabelColor(2);                break;
                case "gl":         SetLabelColor(3);                break;
                case "bl":         SetLabelColor(4);                break;
                case "ml":         SetLabelColor(5);                break;
                case "wl":         SetLabelColor(6);                break;
                                                                    
                case "light":      ToggleLabel();                   break;
                case "fold":       ToggleFold();                    break;
                                                                    
                case "cue":        g_session.CurClip.Cue();                    break;
                case "mem":        g_session.CurClip.Mem();                    break;
                                                                   

                default:
                         if (arg.Length > 5 && arg.Substring(0, 5) == "high ") { int h; if (int.TryParse(arg.Substring(5), out h)) High(h); }
                    else if (arg.Length > 4 && arg.Substring(0, 4) == "low " ) { int l; if (int.TryParse(arg.Substring(4), out l)) Low (l); }

                    else if ((val = GetInt(arg, "up "  )) > -1) SetVolume(val,  1);
                    else if ((val = GetInt(arg, "down ")) > -1) SetVolume(val, -1);

                    else if ((val = GetInt(arg, "solo ")) > -1) Solo(val);
                    else if ((val = GetInt(arg, "mute ")) > -1) Mute(val);

                    else if ((val = GetInt(arg, "mem " )) > -1) g_session.CurClip.SetMem(val);

                    break;
            }
        }
    }
}
