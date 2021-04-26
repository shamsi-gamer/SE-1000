using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        static Display
            dspMixer1, dspMixer2,
            dspVol1, dspVol2, dspVol3,
            dspInfo,
            dspIO,
            dspClip1, dspClip2,
            dspMain;


        static IMyTextPanel     pnlInfoLog;


        List<IMyLandingGear>    g_locks  = new List<IMyLandingGear>();

        List<IMyGyro>           g_gyros  = new List<IMyGyro>();
        List<IMyTimerBlock>     g_timers = new List<IMyTimerBlock>();

        IMyPistonBase           g_lightPiston;
        IMyMotorBase            g_lightHinge1, g_lightHinge2;

        static IMyRemoteControl g_remote;
                                
        static List<string>     g_samples = new List<string>();

        static List<Instrument> g_inst   = new List<Instrument>();

        static List<Note>       g_notes  = new List<Note>();
        static List<Sound>      g_sounds = new List<Sound>();

        float[]                 g_dspVol = new float[g_nChans];

        const float             ControlSensitivity = 12;                                
        const int               NoteScale          = 2;


        static List<TriggerValue> _triggerDummy = new List<TriggerValue>();
        
        static bool             g_started = false;
        static bool             g_init    = false;

        static int              g_curRuntimeTick = 0;
        static float[]          g_runtimeMs      = new float[6];
        static float            g_maxRuntimeMs   = 0;
                                              
        static List<LFO>        g_lfo = new List<LFO>();
        static List<Modulate>   g_mod = new List<Modulate>();
                                
        static Clip             g_clip = new Clip();
                                
                                
        
        Key                     g_editKey = null;
                                
        List<Note>              lastNotes = new List<Note>();
                                             
        Channel                 copyChan  = null;

                                

        float
            g_instCount = 0,
            g_dspCount  = 0;



        public Program()
        {
            pnlInfoLog = Get("Info Display") as IMyTextPanel;
            pnlInfoLog.CustomData = ""; // init log storage


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
            InitLights();
            
            
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


            //SetTranspose(g_song, g_song.CurChan, 0);


            g_init = true;
        }


        void FinishStartup()
        {
            // load oscillators one by one to avoid complexity hang
            if (_loadStep < 10)
                LoadOscillatorSamples(_loadStep++);

            if (_loadStep == 10)
            {
                Load();

                //UpdateLights();
                SetLightColor(g_clip.ColorIndex);

                _loadStep++;
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
            dspVol1   = new Display(Dsp(strVol,   1));
            dspVol2   = new Display(Dsp(strVol,   2));
            dspVol3   = new Display(Dsp(strVol,   3));
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
                                                                    
                case "import":     ImportInstruments();             break;
                                                                    
                case "play":       Play();                          break;
                case "stop":       Stop();                          break;
                                                                        
                case "bpm up":     SetStepLength(-1);               break;
                case "bpm down":   SetStepLength(1);                break;
                                                                        
                case "del pat":    g_clip.DeletePattern();          break;
                case "dup pat":    g_clip.DuplicatePattern();       break;
                case "new pat":    g_clip.NewPattern();             break;
                case "move pat":   g_clip.ToggleMovePattern();      break;
                case "prev pat":   g_clip.PrevPattern(g_clip.MovePat); break;
                case "next pat":   g_clip.NextPattern(g_clip.MovePat); break;
                                                                    
                case "loop":       g_clip.ToogleLoop();             break;
                case "block":      g_clip.ToggleBlock();            break;
                case "all pat":    g_clip.ToggleAllPatterns();      break;
                case "auto cue":   g_clip.ToggleAutoCue();          break;
                case "follow":     g_clip.ToggleFollow();           break;
                                                                        
                case "new":        New();                           break;
                case "dup":        Duplicate();                     break;
                case "del":        Delete();                        break;
                case "move":       g_clip.ToggleMove();             break;
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
                case "up":         Adjust(g_clip, CurSetting,  1);  break;
                case "down":       Adjust(g_clip, CurSetting, -1);  break;
                case "shift":      Shift();                         break;
                case "cmd3":       Command3();                      break;
                                                                    
                case "tr up":      SetTranspose(g_clip,  1);        break;
                case "tr down":    SetTranspose(g_clip, -1);        break;
                                                                    
                case "spread":     Spread();                        break;

                case "rnd snd":    RandomSound(g_inst.IndexOf(g_clip.CurrentInstrument)); break;
                                       
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
                                                                    
                case "step":       Step(g_clip, g_clip.CurChan);    break;
                case "hold":       Hold(g_clip);                    break;
                                                                     
                case "left":       Left(g_clip);                    break;
                case "right":      Right(g_clip);                   break;
                                                             
                case "random":     Random();                        break;
                                                                    
                case "lock":       Lock();                          break;
                case "auto lock":  AutoLock();                      break;
                                                                        
                case "gyro":       Gyro();                          break;
                case "noise":      NoiseEmitters();                 break;
                                                                    
                case "sb":         g_clip.StartBlock(); g_clip.MovePatternOff(); break;
                case "eb":         g_clip.EndBlock();                            break;
                case "cb":         g_clip.ClearBlock(); g_clip.MovePatternOff(); break;
                                                                        
                case "rl":         SetLightColor(0);                break;
                case "ol":         SetLightColor(1);                break;
                case "yl":         SetLightColor(2);                break;
                case "gl":         SetLightColor(3);                break;
                case "bl":         SetLightColor(4);                break;
                case "ml":         SetLightColor(5);                break;
                case "wl":         SetLightColor(6);                break;
                                                                    
                case "light":      ToggleLight();                   break;
                case "fold":       ToggleFold();                    break;
                                                                    
                case "cue":        g_clip.Cue();                    break;
                case "mem":        g_clip.Mem();                    break;
                                                                   

                default:
                         if (arg.Length > 5 && arg.Substring(0, 5) == "high ") { int h; if (int.TryParse(arg.Substring(5), out h)) High(h); }
                    else if (arg.Length > 4 && arg.Substring(0, 4) == "low " ) { int l; if (int.TryParse(arg.Substring(4), out l)) Low (l); }

                    else if ((val = GetInt(arg, "up "  )) > -1) g_clip.SetVolume(val,  1);
                    else if ((val = GetInt(arg, "down ")) > -1) g_clip.SetVolume(val, -1);

                    else if ((val = GetInt(arg, "solo ")) > -1) Solo(val);
                    else if ((val = GetInt(arg, "mute ")) > -1) Mute(val);

                    else if ((val = GetInt(arg, "mem " )) > -1) g_clip.SetMem(val);

                    break;
            }
        }
    }
}
