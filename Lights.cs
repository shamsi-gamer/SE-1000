using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static IMyTextPanel
            lblOctave, lblShuffle,
            lblMixerVolumeUp, lblMixerVolumeDown, lblMixerAll, lblMixerMuteAll,
            lblPlay, lblStop,
            lblStep, lblHold, 
            lblEdit, 
            lblChord, lblChord1, lblChord2, lblChord3, lblChord4, lblChordEdit,
            lblEditStep, lblEditLength,
            lblLeft, lblRight,
            lblLoop, lblBlock, 
            lblTransposeUp, lblTransposeDown,
            lblAllPatterns, lblMovePat, lblFollow,
            lblMixerShift, lblCue, lblAutoCue, lblMemory,
            lblPrev, lblNext, lblMove, lblEnter, lblBack, lblBackOut,
            lblNew, lblDuplicate, lblDelete,
            lblCmd1, lblCmd2, lblCmd3,
            lblSpread, lblRandom,
            lblUp, lblDown, lblShift, 
            lblPrevPat, lblNextPat,
            lblLock, lblAutoLock,
            lblFold, lblGyro, lblNoise;


        List<IMyTextPanel> lblHigh,
                           lblLow;

        List<IMyTextPanel> lblMem;


        IMyInteriorLight   warningLight;


        static List<int>   mixerPressed   = new List<int>();
        static List<int>   mixerPressed_  = new List<int>();
                                   
        static List<int>   infoPressed    = new List<int>();
        static List<int>   infoPressed_   = new List<int>();

        static List<int>   songPressed    = new List<int>();
        static List<int>   mainPressed    = new List<int>();

        static List<IMyTextPanel> lightsPressed  = new List<IMyTextPanel>();
        static List<IMyTextPanel> lightsPressed_ = new List<IMyTextPanel>();

        IMyReflectorLight frontLight;


        void SetLightColor(int iCol)
        {
            g_iCol = MinMax(0, iCol, 6);

            switch (g_iCol)
            {
                case 0: SetLightColor(new Color(255,   0,   0), 0.35f); break;
                case 1: SetLightColor(new Color(255,  92,   0), 0.35f); break;
                case 2: SetLightColor(new Color(255, 255,   0), 0.4f);  break;
                case 3: SetLightColor(new Color(0,   255,   0), 0.35f); break;
                case 4: SetLightColor(new Color(0,    40, 255));        break;
                case 5: SetLightColor(new Color(128,   0, 255), 0.4f);  break;
                case 6: SetLightColor(new Color(255, 255, 255), 0.35f); break;
            }
        }


        Color MakeColor(Color c, float f)
        {
            return new Color(Color.Multiply(c, f), 1);
        }


        void InitLights()
        {
            lblOctave          = Lbl("Octave");
            lblShuffle         = Lbl("Shuffle");
            lblMixerVolumeUp   = Lbl("M Up R");
            lblMixerVolumeDown = Lbl("M Down R");
            lblMixerAll        = Lbl("M Solo R");
            lblMixerMuteAll    = Lbl("M Mute R");

            lblEdit            = Lbl("Edit");

            lblPlay            = Lbl("Play");
            lblStop            = Lbl("Stop");

            lblPrevPat         = Lbl("Prev Pattern");
            lblNextPat         = Lbl("Next Pattern");

            lblLeft            = Lbl("Left");
            lblRight           = Lbl("Right");
            lblEditStep        = Lbl("Edit Step");
            lblEditLength      = Lbl("Edit Length");

            lblLoop            = Lbl("Loop");
            lblBlock           = Lbl("Block");
            lblAllPatterns     = Lbl("All Patterns");
            lblMovePat         = Lbl("Move Pattern");
            lblAutoCue         = Lbl("Auto Cue");
            lblFollow          = Lbl("Follow");

            lblMixerShift      = Lbl("Mixer Shift");

            lblCue             = Lbl("Cue");
            lblMemory          = Lbl("Mem");
            lblStep            = Lbl("Step");
            lblHold            = Lbl("Hold");
            lblTransposeUp     = Lbl("Transpose Up");
            lblTransposeDown   = Lbl("Transpose Down");

            lblChord           = Lbl("Chord");
            lblChord1          = Lbl("Chord 1");
            lblChord2          = Lbl("Chord 2");
            lblChord3          = Lbl("Chord 3");
            lblChord4          = Lbl("Chord 4");
            lblChordEdit       = Lbl("Chord Edit");

            lblPrev            = Lbl("Prev");
            lblNext            = Lbl("Next");
            lblEnter           = Lbl("Enter");
            lblBack            = Lbl("Back");
            lblBackOut         = Lbl("Back Out");
            lblMove            = Lbl("Move");
            lblNew             = Lbl("New");
            lblDuplicate       = Lbl("Dup");
            lblDelete          = Lbl("Del");

            lblCmd1            = Lbl("Command 1");
            lblCmd2            = Lbl("Command 2");
            lblUp              = Lbl("Up");
            lblDown            = Lbl("Down");
            lblShift           = Lbl("Shift");
            lblCmd3            = Lbl("Command 3");

            lblSpread          = Lbl("Spread");
            lblRandom          = Lbl("Random");

            lblLock            = Lbl("Lock");
            lblAutoLock        = Lbl("Auto Lock");
            lblFold            = Lbl("Fold");
            lblGyro            = Lbl("Gyro");
            lblNoise           = Lbl("Noise");


            for (int m = 0; m < nMems; m++)
                lMems[m] = Lbl("Mem " + S(m));


            lblHigh = new List<IMyTextPanel>();
            lblLow  = new List<IMyTextPanel>();

            Get(lblHigh, l => l.CustomName.Length >= 11 && l.CustomName.Substring(0, 11) == "Label High ");
            lblHigh = lblHigh.OrderBy(l => int.Parse(l.CustomName.Substring(11))).ToList();

            Get(lblLow, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Low ");
            lblLow = lblLow.OrderBy(l => int.Parse(l.CustomName.Substring(10))).ToList();


            lblMem = new List<IMyTextPanel>();
            Get(lblMem, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Mem ");
            lblMem = lblMem.OrderBy(l => int.Parse(l.CustomName.Substring(10))).ToList();


            frontLight = Get("Front Light") as IMyReflectorLight;

            warningLight = Get("Saturation Warning Light") as IMyInteriorLight;
        }


        void SetLightColor(Color c, float f = 1)
        {
            color6 = MakeColor(c, 0.878f * f);
            color5 = MakeColor(c, 0.722f * f);
            color4 = MakeColor(c, 0.353f * f);
            color3 = MakeColor(c, 0.157f * f);
            color2 = MakeColor(c, 0.031f * f);
            color1 = MakeColor(c, 0.020f * f);
            color0 = MakeColor(c, 0.004f * f);


            var labels = new List<IMyTextPanel>();
            Get(labels, l => l.CustomName != "Label Edit");

            foreach (var l in labels)
            {
                l.FontColor = color6;
                l.BackgroundColor = color0;
            }


            var max = Math.Max(Math.Max(color6.R, color6.G), color6.B);

            var lightColor = new Color(
                color6.R / max * 0xFF,
                color6.G / max * 0xFF,
                color6.B / max * 0xFF);


                 if (g_iCol == 1) lightColor = new Color(0xFF, 0x50, 0);
            else if (g_iCol == 5) lightColor = new Color(0xAA, 0, 0xFF);


            var lights = new List<IMyInteriorLight>();

            var group = GridTerminalSystem.GetBlockGroupWithName("Rear Lights");
            if (group != null) group.GetBlocksOfType(lights);

            foreach (var l in lights)
                l.Color = lightColor;


            frontLight.Color = new Color(
                lightColor.R + (int)((0xFF - lightColor.R) * 0.23f),
                lightColor.G + (int)((0xFF - lightColor.G) * 0.23f),
                lightColor.B + (int)((0xFF - lightColor.B) * 0.23f));


            switch (g_iCol)
            {
            case 0: warningLight.Color = new Color(0,    0,    0xFF); break;
            case 1: warningLight.Color = new Color(0,    0,    0xFF); break;
            case 2: warningLight.Color = new Color(0xFF, 0,    0x80); break;
            case 3: warningLight.Color = new Color(0xFF, 0,    0xFF); break;
            case 4: warningLight.Color = new Color(0xFF, 0x40, 0   ); break;
            case 5: warningLight.Color = new Color(0xFF, 0x30, 0   ); break;
            case 6: warningLight.Color = new Color(0xFF, 0,    0   ); break;
            }


            UpdateLights();
        }



        void MarkLight(IMyTextPanel light, bool on = true)
        {
            lightsPressed.Add(light);
            UpdateLight(light, on);
        }


        void UnmarkLight(IMyTextPanel light, bool on = false, bool half = false)
        {
            UpdateLight(light, on, half);
            lightsPressed_.Remove(light);
        }


        void UnmarkAllLights()
        {
            var be = g_song.EditNotes.Count > 0;

            if (lightsPressed_.Contains(lblLeft))      UnmarkLight(lblLeft,  false, be);
            if (lightsPressed_.Contains(lblRight))     UnmarkLight(lblRight, false, be);

            if (lightsPressed_.Contains(lblUp))        UnmarkLight(lblUp,   g_shift);
            if (lightsPressed_.Contains(lblDown))      UnmarkLight(lblDown, g_shift);

            if (lightsPressed_.Contains(lblNextPat))   UnmarkLight(lblNextPat, g_movePat);
            if (lightsPressed_.Contains(lblPrevPat))   UnmarkLight(lblPrevPat, g_movePat);

            if (lightsPressed_.Contains(lblNext))      UnmarkLight(lblNext, g_move || CurSrc > -1, SelChan > -1);
            if (lightsPressed_.Contains(lblPrev))      UnmarkLight(lblPrev, g_move || CurSrc > -1, SelChan > -1);

            if (lightsPressed_.Contains(lblEnter))     UnmarkLight(lblEnter, CurSrc > -1 && CurSet < 0, SelChan > -1 && CurSet < 0);
            if (lightsPressed_.Contains(lblBack))      UnmarkLight(lblBack,  CurSrc > -1, SelChan > -1);

            if (lightsPressed_.Contains(lblNew))       UnmarkLight(lblNew,       CurSrc > -1, SelChan > -1);
            if (lightsPressed_.Contains(lblDuplicate)) UnmarkLight(lblDuplicate, CurSrc > -1, SelChan > -1);
            if (lightsPressed_.Contains(lblDelete))    UnmarkLight(lblDelete,    CurSrc > -1, SelChan > -1);

            if (lightsPressed_.Contains(lblChord1))    UnmarkLight(lblChord1, g_chordEdit && g_chord == 0, g_chords[0].Count > 0);
            if (lightsPressed_.Contains(lblChord2))    UnmarkLight(lblChord2, g_chordEdit && g_chord == 1, g_chords[1].Count > 0);
            if (lightsPressed_.Contains(lblChord3))    UnmarkLight(lblChord3, g_chordEdit && g_chord == 2, g_chords[2].Count > 0);
            if (lightsPressed_.Contains(lblChord4))    UnmarkLight(lblChord4, g_chordEdit && g_chord == 3, g_chords[3].Count > 0);

            if (lightsPressed_.Contains(lblCmd2))      UnmarkLight(lblCmd2, false, copyChan != null);

            foreach (var lbl in lightsPressed_)
                UpdateLight(lbl, false);


            mixerPressed_.Clear();
            infoPressed_  .Clear();
            lightsPressed_.Clear();


            // mark for next cycle and clear pressed list

            mixerPressed_.AddRange(mixerPressed);
            mixerPressed.Clear();

            infoPressed_.AddRange(infoPressed);
            infoPressed.Clear();

            songPressed.Clear();
            mainPressed.Clear();

            lightsPressed_.AddRange(lightsPressed);
            lightsPressed.Clear();
        }


        void UpdateLights()
        {
            if (TooComplex) return;

            UpdateLight(lblFollow,      g_follow);
            UpdateLight(lblLoop,        g_loop);
            UpdateLight(lblBlock,       g_block);
            UpdateLight(lblAllPatterns, g_allPats);
            UpdateLight(lblMovePat,     g_movePat);
            UpdateLight(lblCue,         g_song.Cue > -1);
            UpdateLight(lblAutoCue,     g_autoCue);

            UpdateEditLight(lblEdit, OK(g_song.EditPos));
            UpdateHoldLight();

            UpdateChordLights();

            UpdateShuffleLight();
            UpdateOctaveLight();

            UpdatePlayStopLights();
            UpdateMemoryLights();
            UpdateEditLights();
            UpdateNewLights();
            UpdateAdjustLights(g_song);

            UpdateLockLights();
            UpdateGyroLight();
            UpdateTimerLight();
        }


        static void UpdateChordLights()
        {
            //if (TooComplex) return;

            if (    IsCurParam("Tune")
                && !(g_paramKeys || g_paramAuto))
            {
                var inst = SelectedInstrument;
                //var src  = g_song.CurSrc > -1 ? inst.Sources[g_song.CurSrc] : null;
                var tune = (Tune)GetCurrentParam(inst);

                UpdateLight(lblChord, tune.UseChord);

                UpdateLight(lblChordEdit, tune.UseChord ? "All" : " ", 10, 10);
                UpdateLight(lblChordEdit, tune.AllOctaves);
                // TODO same for source or anything else that needs Tune
            }
            else
            {
                UpdateLight(lblChord, g_chordEdit ? " " : "Chord", 9, 12);
                UpdateLight(lblChord, g_chordMode);

                if (g_chordMode)
                {
                    UpdateLight(lblChordEdit, "All", 10, 10);
                    UpdateLight(lblChordEdit, g_chordAll);
                }
                else
                {
                    UpdateLight(lblChordEdit, "Edit", 10, 10);
                    UpdateLight(lblChordEdit, g_chordEdit);
                }
            }

            UpdateChordLight(lblChord1, 1);
            UpdateChordLight(lblChord2, 2);
            UpdateChordLight(lblChord3, 3);
            UpdateChordLight(lblChord4, 4);
        }


        static void UpdateChordLight(IMyTextPanel lbl, int chord)
        {
            //if (TooComplex) return;

            var c = g_chords[chord-1];

            string chordName = GetChordName(c, S(chord));

            lbl.WriteText(chordName);

            UpdateLight(
                lbl,
                      g_chord == chord-1
                   && (   g_chordEdit
                       || g_chordMode)
                   && !IsCurParam("Tune")
                || lightsPressed.Contains(lbl),
                      g_chordMode
                   && g_chord == chord-1 
                || c.Count > 0);
        }


        void MarkChordLight(int chord)
        {
                 if (chord == 1 && g_chords[0].Count > 0) MarkLight(lblChord1);
            else if (chord == 2 && g_chords[1].Count > 0) MarkLight(lblChord2);
            else if (chord == 3 && g_chords[2].Count > 0) MarkLight(lblChord3);
            else if (chord == 4 && g_chords[3].Count > 0) MarkLight(lblChord4);
        }


        void UpdateShuffleLight()
        {
            if (g_spread)
            {
                UpdateLight(lblShuffle, "Sprd", 10, 10);
            }
            else if (ShowPiano)
            {
                UpdateLight(
                    lblShuffle,
                    " ▄█   █ █ ██ █ █ █   █▄ \n" +
                   " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +
                     " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ ",
                    2,
                    32);
            }
            else
            {
                UpdateLight(lblShuffle, "Shuf", 10, 10);
            }
        }


        void UpdateOctaveLight()
        {
            if (TooComplex) return;
            
            int val;

                //if (g_chordMode) 
                 if (g_spread)  val = g_chordSpread;
            else if (ShowPiano) val = CurrentChannel.Transpose;
            else                val = CurrentChannel.Shuffle;

            lblOctave.WriteText((val > 0 ? "+" : "") + S(val), false);
        }


        void UpdatePlayStopLights()
        {
            UpdateLight(lblPlay, g_song.PlayTime > -1);
            UpdateLight(lblStop, g_song.PlayTime > -1);
        }


        void UpdateMemoryLights()
        {
            UpdateLight(lblMemory, g_setMem);

            for (int m = 0; m < nMems; m++)
            {
                lMems[m].WriteText(
                      S((char)(65 + m)) + " "
                    + (g_mem[m] > -1 ? S(g_mem[m] + 1).PadLeft(3) : " "));
            }
        }


        void UpdateEditLights() 
        {
            var strStep = 
                EditStep == 0.5f
                ? "½"
                : S0(EditStep);

            string strLength;

                 if (EditStepLength == 0.25f ) strLength = "¼";
            else if (EditStepLength == 0.5f  ) strLength = "½";
            else if (EditStepLength == 65536f) strLength = "∞";
            else                               strLength = S0(EditStepLength);

            lblEditStep  .WriteText("·· " + strStep);
            lblEditLength.WriteText("─ "  + strLength);
        }


        void UpdateNewLights()
        {
            UpdateEnterLight();
            UpdateLabelColor(lblBack);
            UpdateLabelColor(lblBackOut);

            UpdateLabelColor(lblPrev);
            UpdateLabelColor(lblNext);
            UpdateLabelColor(lblNew);
            UpdateLabelColor(lblDuplicate);
            UpdateLabelColor(lblDelete);

            UpdateLight(lblMove, g_move ^ (CurSrc > -1), SelChan > -1 && !g_move);
        }


        void UpdateLabelColor(IMyTextPanel lbl) 
        {
            UpdateLight(lbl, CurSrc > -1, SelChan > -1); 
        }


        void UpdateEnterLight()
        {
            UpdateLight(lblEnter, CurSet < 0 && CurSrc < 0 ? "└►" : " ", 10, 10);
            UpdateLight(lblEnter, CurSet < 0 && CurSrc > -1, SelChan > -1 && CurSet < 0);
        }


        void UpdateAdjustLights(Song song)
        {
            if (CurSet > -1)
            {
                var path = g_settings.Last().GetPath(CurSrc);

                if (g_paramKeys)
                {
                    UpdateLight(lblCmd1, "Inter", 10, 10);

                    UpdateLight(
                        lblCmd3,
                        SelectedChannel.Notes.Find(n =>
                               n.SongStep >= song.EditPos
                            && n.SongStep <  song.EditPos+1
                            && n.Keys.Find(k => k.Path == path) != null) != null
                        ? "X"
                        : " ",
                        10, 
                        10);
                }
                else if (g_paramAuto)
                {
                    if (OK(song.EditPos))
                    { 
                        if (SelectedChannel.AutoKeys.Find(k =>
                                k.Path == path
                                && k.StepTime >= (song.EditPos % g_nSteps)
                                && k.StepTime <  (song.EditPos % g_nSteps) + 1) != null)
                        {
                            UpdateLight(lblCmd1, "Move", 10, 10);
                            UpdateLight(lblCmd3, "X",    10, 10);
                        }
                        else
                        {
                            UpdateLight(lblCmd1, " ", 10, 10);
                            UpdateLight(lblCmd3, "+", 10, 10);
                        }
                    }
                    else
                        UpdateLight(lblCmd3, " ", 10, 10);
                }
                else
                {
                    UpdateLight(lblCmd1, " ", 10, 10);
                    UpdateLight(lblCmd3, " ", 10, 10);
                }
            }
            else
            {
                if (CurSrc > -1)
                {
                    UpdateLight(lblCmd1, "On",    10, 10);
                    UpdateLight(lblCmd1, SelectedSource.On);
                    UpdateLight(lblCmd2, "Osc ↕", 10, 10);
                    UpdateLight(lblCmd3, " ",     10, 10);
                }
                else
                { 
                    UpdateLight(lblCmd1, SelChan < 0 ? "Copy" : " ", 10, 10);
                    UpdateLight(lblCmd1, false);

                    UpdateLight(lblCmd2, SelChan < 0 ? "Paste" : " ", 10, 10);
                    UpdatePasteLight();

                    UpdateLight(lblCmd3, g_transpose, g_song.EditNotes.Count > 0);

                    UpdateLight(
                        lblCmd3,     
                        SelChan < 0 
                        ? " ▄█   █ █ ██ █ █ █   █▄ \n" +
                         " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +  
                           " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ " 
                        : " ", 
                        2, 
                        32);
                }
            }



            bool canAdjust = 
                   IsCurParam()
                || IsCurSetting(typeof(Harmonics))
                ||    g_transpose 
                   && SelChan < 0;


            var strDown = "◄";
            var strUp   = "►";

            if (      canAdjust
                   && (   IsCurParam("Vol")
                       || IsCurParam("Tune")
                       || IsCurParam("Sus")
                       || IsCurParam("Amp")
                       || IsCurParam("Lvl")
                       || IsCurParam("Pow")
                       ||     IsCurParam("Cnt")
                          && (g_paramKeys || g_paramAuto)
                       || IsCurSetting(typeof(Harmonics)))
                || g_transpose)
            {
                strUp   = "▲";
                strDown = "▼";
            }

            UpdateLight(lblShift, canAdjust ? "Shft"  : " ", 10, 10);
            UpdateLight(lblDown,  canAdjust ? strDown : " ", 10, 10);
            UpdateLight(lblUp,    canAdjust ? strUp   : " ", 10, 10);
            
            UpdateLight(lblShift, canAdjust && g_shift);
            UpdateLight(lblDown,  canAdjust && g_shift);
            UpdateLight(lblUp,    canAdjust && g_shift);

            UpdateLight(lblLeft,  lightsPressed.Contains(lblLeft),  g_song.EditNotes.Count > 0);
            UpdateLight(lblRight, lightsPressed.Contains(lblRight), g_song.EditNotes.Count > 0);
        }


        void UpdatePasteLight()
        {
            UpdateLight(
                lblCmd2,
                lightsPressed_.Contains(lblCmd2),
                copyChan != null);//g_shift && bc);
        }


        void UpdateLockLights()
        {
            UpdateLight(lblLock,     g_locks.Find(l => l.IsLocked) != null);
            UpdateLight(lblAutoLock, g_locks.Find(l => l.AutoLock) != null);
        }


        void UpdateTimerLight()
        {
            UpdateLight(lblNoise, g_timers.Find(t => !t.Enabled) == null);
        }


        void UpdateGyroLight()
        {
            UpdateLight(lblGyro, g_gyros.Find(g => !g.Enabled) == null);
        }


        void UpdateKeyLights()
        {
            if (TooComplex) 
                return;
                
            if (ShowPiano)
            {
                UpdateLight(
                    lblHigh[10],
                      "     ║  ███  ║       ║  ███\n"
                    + "     ║       ║       ║     \n"
                    + "═════╬═══════╬═══════╬═════\n"
                    + "     ║       ║       ║     \n"
                    + " ███ ║  ███  ║  ███  ║  ███\n",
                    1.7f,
                    17);

                for (int h = 0; h < lblHigh.Count-1; h++)
                    UpdateLight(lblHigh[h], HighNoteName(h, g_halfSharp), 10, 10);

                for (int l = 0; l < lblLow.Count-1; l++)
                    UpdateLight(lblLow[l], LowNoteName(l, g_halfSharp), 10, 10);

                UpdatePianoLights();
            }
            else
            {
                UpdateLight(
                    lblHigh[10],
                      "█ █ ██ █ █ █\n"
                    + "█▄█▄██▄█▄█▄█\n"
                    + "▀▀▀▀▀▀▀▀▀▀▀▀\n",
                    3.7f,
                    10);

                for (int h = 0; h < lblHigh.Count-1; h++)
                    UpdateLight(lblHigh[h], " ", 10, 10);

                lblHigh[0].WriteText("◄∙∙");
                lblHigh[1].WriteText("∙∙►");

                lblHigh[2].WriteText("Pick");
                UpdateLight(lblHigh[2], g_pick);
                UpdateLight(lblHigh[3], "All Ch", 7.6f, 19.5f);
                UpdateLight(lblHigh[3], g_allChan);
                lblHigh[4].WriteText("Inst");
                UpdateLight(lblHigh[4], g_rndInst);

                lblHigh[5].WriteText("Rnd");
                lblHigh[6].WriteText("Clr");

                lblHigh[7].WriteText("1/4");
                lblHigh[8].WriteText("1/8");
                lblHigh[9].WriteText("Flip");

                for (int l = 0; l < lblLow.Count; l++)
                    UpdateLight(lblLow[l], " ", 10, 10);

                UpdateStepLights();
            }
        }


        void UpdatePianoLights()
        {
            UpdateHighLights(CurrentPattern, CurrentChannel);
            UpdateLowLights (CurrentPattern, CurrentChannel);
        }


        void UpdateHighLights(Pattern pat, Channel chan)
        {
            for (int h = 0; h < lblHigh.Count-1; h++)
                UpdateLight(pat, chan, lblHigh[h], HighToNote(h));//num);
        }


        void UpdateLowLights(Pattern pat, Channel chan)
        {
            UpdateLight(lblLow[15], ShowPiano ? "‡" : " ", 8, 17);
            
            if (ShowPiano)
                UpdateLight(lblLow[15], g_halfSharp);

            for (int l = 0; l < lblLow.Count-1; l++)
                UpdateLight(pat, chan, lblLow[l], LowToNote(l));//num);
        }


        void UpdateLight(Pattern pat, Channel chan, IMyTextPanel light, int num)
        {
            var step = g_song.PlayStep % g_nSteps;

            var p = g_song.Patterns.IndexOf(pat);


            if (IsCurParam("Tune"))
            {
                var tune =
                    CurSrc > -1
                    ? SelectedSource    .Tune
                    : SelectedInstrument.Tune;

                if (tune.UseChord)
                { 
                    UpdateLight(
                        light, 
                        tune.Chord     .Contains(num),
                        tune.FinalChord.Contains(num));
                }
            }
            else if (g_chord > -1
                  && g_chordEdit)
            {
                UpdateLight(light, g_chords[g_chord].Contains(num));
            }
            else
            {
                var thisChan =
                       chan.Notes.FindIndex(n =>
                              num == n.Number
                           && (      g_song.PlayStep >= p * g_nSteps + n.PatStep + n.ShOffset
                                  && g_song.PlayStep <  p * g_nSteps + n.PatStep + n.ShOffset + n.StepLength
                               ||    p * g_nSteps + n.PatStep >= g_song.EditPos 
                                  && p * g_nSteps + n.PatStep <  g_song.EditPos + EditStep)) > -1
                    ||    g_hold
                       && g_notes.FindIndex(n =>
                                 num == n.Number
                              && g_song.PlayStep >= n.PatStep
                              && g_song.PlayStep <  n.PatStep + n.StepLength) > -1;


                var otherChans = false;

                if (!thisChan)
                {
                    for (int ch = 0; ch < g_nChans; ch++)
                    {
                        var _chan = pat.Channels[ch];

                        otherChans |= _chan.Notes.FindIndex(n =>
                                  num == n.Number
                               && ch  == n.iChan
                               && (   g_song.PlayStep >= p * g_nSteps + n.PatStep + n.ShOffset
                                   && g_song.PlayStep <  p * g_nSteps + n.PatStep + n.ShOffset + n.StepLength
                            ||    p * g_nSteps + n.PatStep >= g_song.EditPos 
                               && p * g_nSteps + n.PatStep <  g_song.EditPos + EditStep)) > -1;
                    }
                }


                var down = false;

                if (lightsPressed.Contains(light))
                    down = true;

                UpdateLight(light, thisChan || down, otherChans);
            }
        }


        void UpdateStepLights()
        {
            for (int step = 0; step < g_nSteps; step++)
            {
                var light = lblLow[step];

                var _step = step + CurPat * g_nSteps;

                var on = CurrentChannel.Notes.Find(n => 
                       n.PatStep >= step
                    && n.PatStep <  step+1) != null;

                Color c;

                if (   OK(g_song.PlayStep)
                    && _step  == (int)g_song.PlayStep
                    && CurPat == g_song.PlayPat)  c = on ? color0 : color6;
                else if (on)                      c = color6;
                else if (g_song.EditPos == _step) c = color3;
                else                              c = step % 4 == 0 ? color2 : color0;

                light.BackgroundColor = c;
            }
        }


        static void UpdateLight(IMyTextPanel light, string text, float size, float pad)
        {
            if (light == null) return;

            light.WriteText(text);
            light.FontSize    = size;
            light.TextPadding = pad;
        }


        static void UpdateLight(IMyTextPanel light, bool b, bool b2 = false)
        {
            if (light == null) return;

            if (b)
            {
                light.FontColor       = color0;
                light.BackgroundColor = color6;
            }
            else
            {
                light.FontColor       = color6;
                light.BackgroundColor = b2 ? color3 : color0;
            }
        }


        static void UpdateEditLight(IMyTextPanel light, bool b)
        {
            if (light == null) return;

            if (b)
            {
                light.FontColor       = redColor0;
                light.BackgroundColor = redColor6;
            }
            else
            {
                light.FontColor       = redColor6;
                light.BackgroundColor = redColor0;
            }
        }


        void UpdateHoldLight() 
        { 
            UpdateLight(lblHold, g_hold && (!OK(g_song.EditPos) || g_song.EditNotes.Count > 0)); 
        }
    }
}
