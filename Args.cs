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
                case "load":       LoadSongExt();                        break;
                case "save":       SaveSongExt();                        break;
                                                                         
                case "load all":   Load();                               break;
                case "save all":   Save();                               break;
                                                                         
                case "import inst": g_session.ImportInstruments();       break;
                                                                         
                case "play":       Play();                               break;
                case "stop":       Stop();                               break;
                                                                             
                case "bpm up":     SetStepLength(-1);                    break;
                case "bpm down":   SetStepLength(1);                     break;
                                                                        
                case "del pat":    CurClip.DeletePattern();              break;
                case "dup pat":    CurClip.DuplicatePattern();           break;
                case "new pat":    CurClip.NewPattern();                 break;
                case "move pat":   CurClip.ToggleMovePattern();          break;
                case "prev pat":   CurClip.PrevPattern(CurClip.MovePat); break;
                case "next pat":   CurClip.NextPattern(CurClip.MovePat); break;
                                                                    
                case "loop":       CurClip.ToogleLoop();                 break;
                case "block":      CurClip.ToggleBlock();                break;
                case "all pat":    CurClip.ToggleAllPatterns();          break;
                case "auto cue":   CurClip.ToggleAutoCue();              break;
                case "follow":     CurClip.ToggleFollow();               break;
                                                                        
                case "new":        New();                                break;
                case "dup":        Duplicate();                          break;
                case "del":        Delete();                             break;
                case "move":       ToggleMove();                         break;
                case "prev":       Move(-1);                             break;
                case "next":       Move( 1);                             break;
                                                                         
                case "out":        BackOut();                            break;
                case "back":       Back();                               break;
                case "enter":      Enter();                              break;
                                                                         
                case "f1":         SetFunc(0);                           break;
                case "f2":         SetFunc(1);                           break;
                case "f3":         SetFunc(2);                           break;
                case "f4":         SetFunc(3);                           break;
                case "f5":         SetFunc(4);                           break;
                case "f6":         SetFunc(5);                           break;
                                                                         
                case "cmd1":       Command1();                           break;
                case "cmd2":       Command2();                           break;
                case "up":         Adjust(CurClip, CurSetting,  1);      break;
                case "down":       Adjust(CurClip, CurSetting, -1);      break;
                case "shift":      Shift();                              break;
                case "cmd3":       Command3();                           break;
                                                                         
                case "tr up":      SetTranspose(CurClip,  1);            break;
                case "tr down":    SetTranspose(CurClip, -1);            break;
                                                                         
                case "spread":     Spread();                             break;
                                                                      
                case "rnd snd":    /*RandomSound(g_session.Instruments.IndexOf(CurClip.CurrentInstrument));*/ break;
                                       
                case "up all":     SetVolumeAll( 1);                     break;
                case "down all":   SetVolumeAll(-1);                     break;
                                                                          
                case "solo all":   EnableChannels(true);                 break;
                case "mute all":   EnableChannels(false);                break;
                                                                         
                case "m shift":    MixerShift();                         break;
                                                                         
                case "clips":      Clips();                              break;
                                                                         
                case "edit":       Edit();                               break;
                case "rec":        Record();                             break;
                                                                         
                case "chord":      ToggleChordMode();                    break;
                case "chord 1":    Chord(1);                             break;
                case "chord 2":    Chord(2);                             break;
                case "chord 3":    Chord(3);                             break;
                case "chord 4":    Chord(4);                             break;
                case "chord edit": ToggleChordEdit();                    break;
                                                                         
                case "edit step":  ChangeEditStep();                     break;
                case "edit len":   ChangeEditLength();                   break;
                                                                          
                case "step":       Step(CurClip, CurChan);               break;
                case "hold":       Hold(CurClip);                        break;
                                                                          
                case "left":       Left (CurClip);                       break;
                case "right":      Right(CurClip);                       break;
                                                                         
                                                                         
                case "lock":       Lock();                               break;
                case "auto lock":  AutoLock();                           break;
                                                                              
                case "gyro":       Gyro();                               break;
                case "noise":      NoiseEmitters();                      break;
                                                                    
                case "sb":         CurClip.StartBlock();                 break;
                case "eb":         CurClip.EndBlock();                   break;
                case "cb":         CurClip.ClearBlock();                 break;
                                                                        
                case "rl":         SetLabelColor(0);                     break;
                case "ol":         SetLabelColor(1);                     break;
                case "yl":         SetLabelColor(2);                     break;
                case "gl":         SetLabelColor(3);                     break;
                case "bl":         SetLabelColor(4);                     break;
                case "ml":         SetLabelColor(5);                     break;
                case "wl":         SetLabelColor(6);                     break;
                                                                         
                case "light":      ToggleLabel();                        break;
                case "fold":       ToggleFold();                         break;
                                                                         
                case "cue":        CurClip.Cue();                        break;
                case "mem":        CurClip.Mem();                        break;
                                                                   

                default:
                         if (arg.Length > 5 && arg.Substring(0, 5) == "high ") { int h; if (int.TryParse(arg.Substring(5), out h)) High(h); }
                    else if (arg.Length > 4 && arg.Substring(0, 4) == "low " ) { int l; if (int.TryParse(arg.Substring(4), out l)) Low (l); }

                    else if ((val = GetInt(arg, "up "  )) > -1) SetVolume(val,  1);
                    else if ((val = GetInt(arg, "down ")) > -1) SetVolume(val, -1);

                    else if ((val = GetInt(arg, "solo ")) > -1) Solo(val);
                    else if ((val = GetInt(arg, "mute ")) > -1) Mute(val);

                    else if ((val = GetInt(arg, "mem " )) > -1) CurClip.SetMem(val);

                    break;
            }
        }
    }
}
