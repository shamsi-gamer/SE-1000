namespace IngameScript
{
    partial class Program
    {
        void ProcessArg(string arg)
        {
            arg = arg.ToLower().Trim();

            int val;

            switch (arg)
            {
                case "load":        LoadSongExt();                  break;
                case "save":        SaveSongExt();                  break;
                                                            
                case "load all":    Load();                         break;
                case "save all":    Save();                         break;
                                                                   
                case "import inst": g_session.ImportInstruments();  break;
                                                                   
                case "play":        Play();                         break;
                                                                          
                case "bpm up":      SetStepLength(-1);              break;
                case "bpm down":    SetStepLength( 1);              break;

                case "del pat":     EditClip?.DeletePattern();       break;
                case "dup pat":     EditClip?.DuplicatePattern();    break;
                case "new pat":     EditClip?.NewPattern();          break;
                case "move pat":    EditClip?.ToggleMovePattern();   break;
                case "prev pat":    EditClip?.PrevPattern();         break;
                case "next pat":    EditClip?.NextPattern();         break;

                case "loop":        EditClip?.ToogleLoop();          break;
                case "block":       EditClip?.ToggleBlock();         break;
                case "all pat":     EditClip?.ToggleAllPatterns();   break;
                case "auto cue":    EditClip?.ToggleAutoCue();       break;
                case "follow":      EditClip?.ToggleFollow();        break;
                                                                         
                case "new":         New();                                break;
                case "dup":         Duplicate();                          break;
                case "del":         Delete();                             break;
                case "move":        ToggleMove();                         break;
                case "prev":        Move(-1);                             break;
                case "next":        Move( 1);                             break;
                                                                          
                case "out":         BackOut();                            break;
                case "back":        Back();                               break;
                case "enter":       Enter();                              break;
                                                                          
                case "f1":          SetFunc(0);                           break;
                case "f2":          SetFunc(1);                           break;
                case "f3":          SetFunc(2);                           break;
                case "f4":          SetFunc(3);                           break;
                case "f5":          SetFunc(4);                           break;
                case "f6":          SetFunc(5);                           break;
                                                                          
                case "cmd1":        Command1();                           break;
                case "cmd2":        Command2();                           break;
                case "up":          Adjust(EditClip, CurSetting,  1);      break;
                case "down":        Adjust(EditClip, CurSetting, -1);      break;
                case "shift":       Shift();                              break;
                case "cmd3":        Command3();                           break;
                                                                          
                case "tr up":       SetTranspose( 1);                     break;
                case "tr down":     SetTranspose(-1);                     break;
                                                                          
                case "spread":      Spread();                             break;
                                                                       
                case "up all":      SetVolumeAll( 1);                     break;
                case "down all":    SetVolumeAll(-1);                     break;
                                                                           
                case "solo all":    EnableChannels(T);                    break;
                case "mute all":    EnableChannels(F);                    break;
                                                                          
                case "m shift":     MixerShift();                         break;
                                                                          
                case "session":     ToggleSession();                      break;
                                                                          
                case "edit":        Edit();                               break;
                case "rec":         Record();                             break;
                                                                          
                case "chord":       ToggleChordMode();                    break;
                case "chord 1":     Chord(1);                             break;
                case "chord 2":     Chord(2);                             break;
                case "chord 3":     Chord(3);                             break;
                case "chord 4":     Chord(4);                             break;
                case "chord edit":  ToggleChordEdit();                    break;
                                                                          
                case "edit step":   ChangeEditStep();                     break;
                case "edit len":    ChangeEditLength();                   break;
                                                                           
                case "step":        Step(EditClip, CurChan);               break;
                case "hold":        Hold(EditClip);                        break;
                                                                           
                case "left":        Left (EditClip);                       break;
                case "right":       Right(EditClip);                       break;
                                                                          
                                                                          
                case "lock":        Lock();                               break;
                case "auto lock":   AutoLock();                           break;

                case "sb":          EditClip.StartBlock();                 break;
                case "eb":          EditClip.EndBlock();                   break;
                case "cb":          EditClip.ClearBlock();                 break;
                                                                         
                case "rl":          SetLabelColor(0);                     break;
                case "ol":          SetLabelColor(1);                     break;
                case "yl":          SetLabelColor(2);                     break;
                case "gl":          SetLabelColor(3);                     break;
                case "bl":          SetLabelColor(4);                     break;
                case "ml":          SetLabelColor(5);                     break;
                case "wl":          SetLabelColor(6);                     break;
                                                                          
                case "light":       ToggleLabel();                        break;

                case "fold":        ToggleFold();                         break;
                case "timers":      NoiseEmitters();                      break;
                                                                     
                case "gyro":        Gyro();                               break;
                                                                          
                case "cue":         EditClip.Cue();                        break;

                case "mem":         EditClip.Mem();                        break;
                                                                    

                default:
                         if (arg.Length > 5 && arg.Substring(0, 5) == "high ") { int h; if (int.TryParse(arg.Substring(5), out h)) High(h); }
                    else if (arg.Length > 4 && arg.Substring(0, 4) == "low " ) { int l; if (int.TryParse(arg.Substring(4), out l)) Low (l); }

                    else if ((val = GetInt(arg, "up "  )) > -1) SetVolume(val,  1);
                    else if ((val = GetInt(arg, "down ")) > -1) SetVolume(val, -1);

                    else if ((val = GetInt(arg, "solo ")) > -1) Solo(val);
                    else if ((val = GetInt(arg, "mute ")) > -1) Mute(val);

                    else if ((val = GetInt(arg, "mem " )) > -1) EditClip.SetMem(val);

                    break;
            }
        }
    }
}
