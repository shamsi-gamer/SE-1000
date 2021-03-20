using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // pattern random
        // timing issues when moving blocks etc.
        // timing issues with playing
        // when editLength is long, editing the chord doesn't play the current chord properly when a note is added/deleted
        // fix volume display

        // mixer channel volumes aren't saved
        // save machine state after instrument editing is done
        // add power to envelope decay and release to have a more gentle slope at the end
        // keys should affect trigger, auto should affect volume
        // add Modulate to param (Level, Attack, Release)
        // side chain compression (Modulate on all params, with delay and +/-)
        // plug any param into any param (connect button)
        // hold doesn't seem to work properly when entering long notes
        // fix filter
        // volume not shown correctly for harmonics & filters
        // fix Note-mode moving of notes with block/allpat enabled
        // when shuffle is too strong, notes in other channels become too long
        // BUG: holding a chord and then pressing another chord with some of the same keys will disable
        //   those keys until the second press
        // harmony - across selected patterns, copy existing notes and shift them up or down
        // record keys/chords and mouse control of parameters as automation
        // bring back default samples
        // improve randomizer

        // save (song saves modified copies of instruments)


        // blocks


        // add delay pitch (every next tap goes +-1 or whatever, +-5 should give thirds which sound nice)
        // add delay dampiing


        // need to reduce code complexity
        // move playback engine into separate PB
        // minify TextAlignment.CENTER, etc.
        // use as much Linq instead of loops as possible
        // refactor .GetValue(.......) into passing a single time/note object


        // lfo song start (maybe sync value)


        // ARPEGGIATOR
        // show playback in arp relative to arp
        // faintly show outlying notes in arpeggio
        // scale
        // make it so arpeggio length can't be deleted


        // note button
        // fin button (finalize notes to octave)


        // copy/paste


        // BUG volume display doesn't work for harmonics


        // source offset before note time (add playback notes before)

        // make long note loop breaks smooth (blend across 1 frame)
        // real time tune

        // deal with loops properly
        // when randoming with random instrument, the new instrument's name is set to the old one

        // allow loading without mod

        // replace Pick with Note


        // pattern length





        #region Variables
        Display
            dspMixer1, dspMixer2,
            dspVol1, dspVol2, dspVol3,
            dspInfo,
            dspIO,
            dspSong1, dspSong2,
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

        List<Note>              g_notes  = new List<Note>();
        List<Sound>             g_sounds = new List<Sound>();

        float[]                 g_vol = new float[nChans];

        const float             ControlSensitivity = 8;                                
        const int               NoteScale          = 2;


        static List<TriggerValue> _triggerDummy = new List<TriggerValue>();
        
        static bool             g_started = false;
        static bool             g_init    = false;

        static int              g_curRuntimeTick = 0;
        static float[]          g_runtimeMs      = new float[6];
        static float            g_maxRuntimeMs   = 0;
                                              

                                
        static Song             g_song = new Song();
                                
                                
        
        Key                     g_editKey = null;
                                
        List<Note>              lastNotes    = new List<Note>();
                                             
                                
        Channel                 copyChan = null;
                                
                                
        //Oscillator              copyW = Oscillator.Sine;

        //Parameter copyV, copyA, copyD, copyS, copyR;
        //Parameter copyDC, copyDT, copyDL, copyDP;

        //bool copyLFx;

        //int copyTr, copyOff;


        float
            instCount = 0,
            dspCount  = 0;

        #endregion



        IMyTerminalBlock Get   (string s)             { return GridTerminalSystem.GetBlockWithName(s); }
        IMyTextPanel     GetLcd(string s)             { return Get(s) as IMyTextPanel; }
        IMyTextPanel     Lbl   (string s)             { return GetLcd("Label " + s); }
        IMyTextPanel     Dsp   (string s, int i = -1) { return GetLcd(s + " Display" + (i > -1 ? " " + S(i) : "")); }



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
                g_random[i] = (float)g_rnd.NextDouble();


            InitDisplays();
            InitLights();
            
            
            InitFuncButtons(); 


            for (int i = 0; i < nChans; i++)
                g_vol[i] = 0;


            //copyV.Value =
            //copyA.Value =
            //copyD.Value =
            //copyS.Value =
            //copyR.Value = fN;

            //copyTr  =
            //copyOff = 0;


            GridTerminalSystem.GetBlocksOfType(g_locks, b => !b.Name.Contains("Fold"));

            GridTerminalSystem.GetBlocksOfType(g_gyros);
            GridTerminalSystem.GetBlocksOfType(g_timers);

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
            if (_nextToLoad < 10)
                LoadOscillatorSamples(_nextToLoad++);

            if (_nextToLoad == 10)
            {
                if (!Load(Me.CustomData))
                    SetDefaultMachineState();

                _nextToLoad++;
            }
        }


        void InitDisplays()
        {
            dspIO     = new Display(Dsp("IO"      ));
            dspInfo   = new Display(Dsp("Info"    ));
            dspSong1  = new Display(Dsp("Song",  1));
            dspSong2  = new Display(Dsp("Song",  2));
            dspMixer1 = new Display(Dsp("Mixer", 1));
            dspMixer2 = new Display(Dsp("Mixer", 2));
            dspVol1   = new Display(Dsp("Vol",   1));
            dspVol2   = new Display(Dsp("Vol",   2));
            dspVol3   = new Display(Dsp("Vol",   3));
        }


        void ProcessArg(string arg)
        {
            arg = arg.ToLower().Trim();

            int val;

            switch (arg)
            {
                case "load":       LoadSongExt();            break;
                case "save":       SaveSongExt();            break;
                                                                
                case "play":       Play();                   break;
                case "stop":       Stop();                   break;
                                                                 
                case "bpm up":     SetStepLength(-1);        break;
                case "bpm down":   SetStepLength(1);         break;
                                                                 
                case "del pat":    DeletePattern();          break;
                case "dup pat":    DuplicatePattern();       break;
                case "new pat":    NewPattern();             break;
                case "move pat":   ToggleMovePattern();      break;
                case "prev pat":   PrevPattern(g_movePat);     break;
                case "next pat":   NextPattern(g_movePat);     break;
                                                             
                case "loop":       ToogleLoop();             break;
                case "block":      ToggleBlock();            break;
                case "all pat":    ToggleAllPatterns();      break;
                case "auto cue":   ToggleAutoCue();          break;
                case "follow":     ToggleFollow();           break;
                                                                 
                case "new":        New();                    break;
                case "dup":        Duplicate();              break;
                case "del":        Delete();                 break;
                case "move":       ToggleMove();             break;
                case "prev":       Move(-1);                 break;
                case "next":       Move( 1);                 break;
                                                             
                case "out":        BackOut();                break;
                case "back":       Back();                   break;
                case "enter":      Enter();                  break;
                                                             
                case "f1":         SetFunc(0);               break;
                case "f2":         SetFunc(1);               break;
                case "f3":         SetFunc(2);               break;
                case "f4":         SetFunc(3);               break;
                case "f5":         SetFunc(4);               break;
                case "f6":         SetFunc(5);               break;
                                                             
                case "cmd1":       Command1();               break;
                case "cmd2":       Command2();               break;
                case "up":         Adjust(g_song,  1);       break;
                case "down":       Adjust(g_song, -1);       break;
                case "shift":      Shift();                  break;
                case "cmd3":       Command3();               break;

                case "tr up":      SetTranspose(g_song,  1); break;
                case "tr down":    SetTranspose(g_song, -1); break;
                                       
                case "spread":     Spread();                 break;

                case "rnd snd":    RandomSound(g_inst.IndexOf(CurrentInstrument)); break;
                                       
                case "up all":     SetVolumeAll( 1);      break;
                case "down all":   SetVolumeAll(-1);      break;
                                                          
                case "solo all":   EnableChannels(true);  break;
                case "mute all":   EnableChannels(false); break;
                case "m shift":    MixerShift();          break;
                                                          
                case "edit":       Edit();                break;
                                                          
                case "chord":      ToggleChordMode();     break;
                case "chord 1":    Chord(1);              break;
                case "chord 2":    Chord(2);              break;
                case "chord 3":    Chord(3);              break;
                case "chord 4":    Chord(4);              break;
                case "chord edit": ToggleChordEdit();     break;
                                                          
                case "edit step":  ChangeEditStep();      break;
                case "edit len":   ChangeEditLength();    break;
                                   
                case "step":       Step(CurSong, CurChan); break;
                case "hold":       Hold(CurSong);         break;
                                                          
                case "left":       Left(CurSong);         break;
                case "right":      Right(CurSong);        break;

                case "random":     Random();              break;
                    
                case "lock":       Lock();                break;
                case "auto lock":  AutoLock();            break;
                                                              
                case "gyro":       Gyro();                break;
                case "noise":      NoiseEmitters();       break;
                                                          
                case "sb":         StartBlock();    break;
                case "eb":         EndBlock();      break;
                case "cb":         ClearBlock();    break;
                                                              
                case "rl":         SetLightColor(0);      break;
                case "ol":         SetLightColor(1);      break;
                case "yl":         SetLightColor(2);      break;
                case "gl":         SetLightColor(3);      break;
                case "bl":         SetLightColor(4);      break;
                case "ml":         SetLightColor(5);      break;
                case "wl":         SetLightColor(6);      break;
                                                          
                case "light":      ToggleLight();         break;
                case "fold":       ToggleFold();          break;
                                                          
                case "cue":        Cue();                 break;
                case "mem":        Mem();                 break;
                                                                   

                default:
                         if (arg.Length > 5 && arg.Substring(0, 5) == "high ") { int h; if (int.TryParse(arg.Substring(5), out h)) High(h); }
                    else if (arg.Length > 4 && arg.Substring(0, 4) == "low " ) { int l; if (int.TryParse(arg.Substring(4), out l)) Low (l); }

                    else if ((val = GetInt(arg, "up "  )) > -1) SetVolume(g_song, val,  1);
                    else if ((val = GetInt(arg, "down ")) > -1) SetVolume(g_song, val, -1);

                    else if ((val = GetInt(arg, "solo ")) > -1) Solo(val);
                    else if ((val = GetInt(arg, "mute ")) > -1) Mute(val);

                    else if ((val = GetInt(arg, "mem " )) > -1) Mem(val);

                    break;
            }
        }


        public void Save()
        {
            Me.CustomData = SaveMachineState();
        }
    }
}
