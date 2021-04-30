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
        //static Label              lblOctave, lblShuffle,
        //                          lblMixerVolumeUp, lblMixerVolumeDown, lblMixerAll, lblMixerMuteAll,
        //                          lblPlay, lblStop,
        //                          lblStep, lblHold, 
        //                          lblEdit, 
        //                          lblChord, lblChord1, lblChord2, lblChord3, lblChord4, lblChordEdit,
        //                          lblEditStep, lblEditLength,
        //                          lblLeft, lblRight,
        //                          lblLoop, lblBlock, 
        //                          lblTransposeUp, lblTransposeDown,
        //                          lblAllPatterns, lblMovePat, lblFollow,
        //                          lblMixerShift, lblClips, lblMemSet, lblAutoCue, lblMemory,
        //                          lblPrev, lblNext, lblMove, lblEnter, lblBack, lblBackOut,
        //                          lblNew, lblDuplicate, lblDelete,
        //                          lblCmd1, lblCmd2, lblCmd3,
        //                          lblSpread, lblRandom,
        //                          lblUp, lblDown, lblShift, 
        //                          lblPrevPat, lblNextPat,
        //                          lblLock, lblAutoLock,
        //                          lblFold, lblGyro, lblNoise;


        //List<Label>               lblHigh,
        //                          lblLow;

        //static List<IMyTextPanel> lblMem;
        //static IMyTextPanel[]     lblMems = new IMyTextPanel[nMems];


        IMyInteriorLight          warningLight;
        IMyReflectorLight         frontLight;


        static List<int>          g_mixerPressed  = new List<int>();
        static List<int>           _mixerPressed  = new List<int>();
                                                  
        static List<int>          g_infoPressed   = new List<int>();
        static List<int>           _infoPressed   = new List<int>();
                                                  
        static List<int>          g_clipPressed   = new List<int>();
        static List<int>          g_mainPressed   = new List<int>();

        static List<IMyTextPanel> g_lightsPressed = new List<IMyTextPanel>();
        static List<IMyTextPanel>  _lightsPressed = new List<IMyTextPanel>();



        void SetLabelColor(int iCol)
        {
            g_session.CurClip.ColorIndex = MinMax(0, iCol, 6);

            switch (g_session.CurClip.ColorIndex)
            {
                case 0: SetLabelColor(new Color(255,   0,   0), 0.35f); break;
                case 1: SetLabelColor(new Color(255,  92,   0), 0.35f); break;
                case 2: SetLabelColor(new Color(255, 255,   0), 0.4f);  break;
                case 3: SetLabelColor(new Color(0,   255,   0), 0.35f); break;
                case 4: SetLabelColor(new Color(0,    40, 255));        break;
                case 5: SetLabelColor(new Color(128,   0, 255), 0.4f);  break;
                case 6: SetLabelColor(new Color(255, 255, 255), 0.35f); break;
            }
        }


        Color MakeColor(Color c, float f)
        {
            return new Color(Color.Multiply(c, f), 1);
        }


        void InitLabels()
        {
            //lblOctave          = Lbl("Octave");
            //lblShuffle         = Lbl("Shuffle");
            //lblMixerVolumeUp   = Lbl("M Up R");
            //lblMixerVolumeDown = Lbl("M Down R");
            //lblMixerAll        = Lbl("M Solo R");
            //lblMixerMuteAll    = Lbl("M Mute R");

            //lblEdit            = Lbl("Edit");

            //lblPlay            = Lbl("Play");
            //lblStop            = Lbl("Stop");

            //lblPrevPat         = Lbl("Prev Pattern");
            //lblNextPat         = Lbl("Next Pattern");

            //lblLeft            = Lbl("Left");
            //lblRight           = Lbl("Right");
            //lblEditStep        = Lbl("Edit Step");
            //lblEditLength      = Lbl("Edit Length");

            //lblLoop            = Lbl("Loop");
            //lblBlock           = Lbl("Block");
            //lblAllPatterns     = Lbl("All Patterns");
            //lblMovePat         = Lbl("Move Pattern");
            //lblAutoCue         = Lbl("Auto Cue");
            //lblFollow          = Lbl("Follow");

            //lblMixerShift      = Lbl("M Shift");
            //lblClips           = Lbl("Clips");

            //lblMemSet          = Lbl("MemSet");
            //lblMemory          = Lbl("Mem");
            //lblStep            = Lbl("Step");
            //lblHold            = Lbl("Hold");
            //lblTransposeUp     = Lbl("Transpose Up");
            //lblTransposeDown   = Lbl("Transpose Down");

            //lblChord           = Lbl("Chord");
            //lblChord1          = Lbl("Chord 1");
            //lblChord2          = Lbl("Chord 2");
            //lblChord3          = Lbl("Chord 3");
            //lblChord4          = Lbl("Chord 4");
            //lblChordEdit       = Lbl("Chord Edit");

            //lblPrev            = Lbl("Prev");
            //lblNext            = Lbl("Next");
            //lblEnter           = Lbl("Enter");
            //lblBack            = Lbl("Back");
            //lblBackOut         = Lbl("Back Out");
            //lblMove            = Lbl("Move");
            //lblNew             = Lbl("New");
            //lblDuplicate       = Lbl("Dup");
            //lblDelete          = Lbl(strDel);

            //lblCmd1            = Lbl("Command 1");
            //lblCmd2            = Lbl("Command 2");
            //lblUp              = Lbl("Up");
            //lblDown            = Lbl("Down");
            //lblShift           = Lbl("Shift");
            //lblCmd3            = Lbl("Command 3");

            //lblSpread          = Lbl("Spread");
            //lblRandom          = Lbl("Random");

            //lblLock            = Lbl("Lock");
            //lblAutoLock        = Lbl("Auto Lock");
            //lblFold            = Lbl("Fold");
            //lblGyro            = Lbl("Gyro");
            //lblNoise           = Lbl("Noise");


            //for (int m = 0; m < nMems; m++)
            //    lblMems[m] = Lbl("Mem " + S(m));


            //lblHigh = new List<IMyTextPanel>();
            //lblLow  = new List<IMyTextPanel>();

            //Get(lblHigh, l => l.CustomName.Length >= 11 && l.CustomName.Substring(0, 11) == "Label High ");
            //lblHigh = lblHigh.OrderBy(l => int.Parse(l.CustomName.Substring(11))).ToList();

            //Get(lblLow, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Low ");
            //lblLow = lblLow.OrderBy(l => int.Parse(l.CustomName.Substring(10))).ToList();


            //lblMem = new List<IMyTextPanel>();
            //Get(lblMem, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Mem ");
            //lblMem = lblMem.OrderBy(l => int.Parse(l.CustomName.Substring(10))).ToList();


            frontLight   = Get("Front Light") as IMyReflectorLight;
            warningLight = Get("Saturation Warning Light") as IMyInteriorLight;
        }


        void SetLabelColor(Color c, float f = 1)
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
                l.FontColor       = color6;
                l.BackgroundColor = color0;
            }


            var max = Math.Max(Math.Max(color6.R, color6.G), color6.B);

            var lightColor = new Color(
                color6.R / max * 0xFF,
                color6.G / max * 0xFF,
                color6.B / max * 0xFF);


                 if (g_session.CurClip.ColorIndex == 1) lightColor = new Color(0xFF, 0x50, 0);
            else if (g_session.CurClip.ColorIndex == 5) lightColor = new Color(0xAA, 0, 0xFF);


            //var lights = new List<IMyInteriorLabel>();

            //var group = GridTerminalSystem.GetBlockGroupWithName("Rear Labels");
            //if (group != null) group.GetBlocksOfType(lights);

            //foreach (var l in lights)
            //    l.Color = lightColor;


            frontLight.Color = new Color(
                lightColor.R + (int)((0xFF - lightColor.R) * 0.23f),
                lightColor.G + (int)((0xFF - lightColor.G) * 0.23f),
                lightColor.B + (int)((0xFF - lightColor.B) * 0.23f));


            switch (g_session.CurClip.ColorIndex)
            {
            case 0: warningLight.Color = new Color(0,    0,    0xFF); break;
            case 1: warningLight.Color = new Color(0,    0,    0xFF); break;
            case 2: warningLight.Color = new Color(0xFF, 0,    0x80); break;
            case 3: warningLight.Color = new Color(0xFF, 0,    0xFF); break;
            case 4: warningLight.Color = new Color(0xFF, 0x40, 0   ); break;
            case 5: warningLight.Color = new Color(0xFF, 0x30, 0   ); break;
            case 6: warningLight.Color = new Color(0xFF, 0,    0   ); break;
            }


            //UpdateLabels();
        }



        //void UnmarkAllLabels()
        //{
        //    var be  = g_session.CurClip.EditNotes.Count > 0;
        //    var cur = g_session.CurClip.CurSrc > -1;
        //    var crd = g_session.CurClip.ChordEdit;
        //    var ch  = g_session.CurClip.SelChan > -1;
        //    var mov = g_session.CurClip.MovePat;
        //    var sh  = g_session.CurClip.Shift;
        //    var set = g_session.CurClip.CurSet < 0;


        //    if (_lightsPressed.Contains(lblLeft))      UnmarkLabel(lblLeft,  false, be);
        //    if (_lightsPressed.Contains(lblRight))     UnmarkLabel(lblRight, false, be);

        //    if (_lightsPressed.Contains(lblUp))        UnmarkLabel(lblUp,   sh);
        //    if (_lightsPressed.Contains(lblDown))      UnmarkLabel(lblDown, sh);

        //    if (_lightsPressed.Contains(lblNextPat))   UnmarkLabel(lblNextPat, mov);
        //    if (_lightsPressed.Contains(lblPrevPat))   UnmarkLabel(lblPrevPat, mov);

        //    if (_lightsPressed.Contains(lblNext))      UnmarkLabel(lblNext, g_move || cur, ch);
        //    if (_lightsPressed.Contains(lblPrev))      UnmarkLabel(lblPrev, g_move || cur, ch);

        //    if (_lightsPressed.Contains(lblBackOut))   UnmarkLabel(lblBack,  cur, ch);
        //    if (_lightsPressed.Contains(lblBack))      UnmarkLabel(lblBack,  cur, ch);
        //    if (_lightsPressed.Contains(lblEnter))     UnmarkLabel(lblEnter, cur && set, ch && set);

        //    if (_lightsPressed.Contains(lblNew))       UnmarkLabel(lblNew,       cur, ch);
        //    if (_lightsPressed.Contains(lblDuplicate)) UnmarkLabel(lblDuplicate, cur, ch);
        //    if (_lightsPressed.Contains(lblDelete))    UnmarkLabel(lblDelete,    cur, ch);

        //    if (_lightsPressed.Contains(lblChord1))    UnmarkLabel(lblChord1, crd && g_session.CurClip.Chord == 0, g_session.CurClip.Chords[0].Count > 0);
        //    if (_lightsPressed.Contains(lblChord2))    UnmarkLabel(lblChord2, crd && g_session.CurClip.Chord == 1, g_session.CurClip.Chords[1].Count > 0);
        //    if (_lightsPressed.Contains(lblChord3))    UnmarkLabel(lblChord3, crd && g_session.CurClip.Chord == 2, g_session.CurClip.Chords[2].Count > 0);
        //    if (_lightsPressed.Contains(lblChord4))    UnmarkLabel(lblChord4, crd && g_session.CurClip.Chord == 3, g_session.CurClip.Chords[3].Count > 0);

        //    if (_lightsPressed.Contains(lblCmd2))      UnmarkLabel(lblCmd2, false, copyChan != null);

        //    foreach (var lbl in _lightsPressed)
        //        UpdateLabel(lbl, false);


        //    _mixerPressed.Clear();
        //    _infoPressed  .Clear();
        //    _lightsPressed.Clear();


        //    // mark for next cycle and clear pressed list

        //    _mixerPressed.AddRange(g_mixerPressed);
        //    g_mixerPressed.Clear();

        //    _infoPressed.AddRange(g_infoPressed);
        //    g_infoPressed.Clear();

        //    g_clipPressed.Clear();
        //    g_mainPressed.Clear();

        //    _lightsPressed.AddRange(g_lightsPressed);
        //    g_lightsPressed.Clear();
        //}


        //void UpdateLabels()
        //{
        //    if (TooComplex) return;

        //    UpdateLabel(lblFollow,      g_session.CurClip?.Follow  ?? false);
        //    UpdateLabel(lblLoop,        g_session.CurClip?.Loop    ?? false);
        //    UpdateLabel(lblBlock,       g_session.CurClip?.Block   ?? false);
        //    UpdateLabel(lblAllPatterns, g_session.CurClip?.AllPats ?? false);
        //    UpdateLabel(lblMovePat,     g_session.CurClip?.MovePat ?? false);
        //    UpdateLabel(lblAutoCue,     g_session.CurClip?.AutoCue ?? false);

        //    UpdateEditLabel(lblEdit, OK(g_session.CurClip.EditPos));
        //    UpdateHoldLabel();

        //    UpdateChordLabels();

        //    UpdateShuffleLabel();
        //    UpdateOctaveLabel();

        //    UpdatePlayStopLabels();
        //    UpdateMemoryLabels();
        //    UpdateEditLabels();
        //    UpdateNewLabels();

        //    UpdateAdjustLabels(g_session.CurClip);

        //    UpdateLockLabels();
        //    UpdateGyroLabel();
        //    UpdateTimerLabel();

        //    UpdateClipsLabel();
        //}


        //static void UpdateChordLabels()
        //{
        //    //if (TooComplex) return;

        //    if (    IsCurParam(strTune)
        //        && !(g_session.CurClip.ParamKeys || g_session.CurClip.ParamAuto))
        //    {
        //        var inst = g_session.CurClip.SelectedInstrument;
        //        var tune = (Tune)GetCurrentParam(inst);

        //        UpdateLabel(lblChord, tune.UseChord);

        //        UpdateLabel(lblChordEdit, tune.UseChord ? strAll : " ", 10, 10);
        //        UpdateLabel(lblChordEdit, tune.AllOctaves);
        //        // TODO same for source or anything else that needs Tune
        //    }
        //    else
        //    {
        //        UpdateLabel(lblChord, g_session.CurClip.ChordEdit ? " " : "Chord", 9, 12);
        //        UpdateLabel(lblChord, g_session.CurClip.ChordMode);

        //        if (g_session.CurClip.ChordMode)
        //        {
        //            UpdateLabel(lblChordEdit, strAll, 10, 10);
        //            UpdateLabel(lblChordEdit, g_session.CurClip.ChordAll);
        //        }
        //        else
        //        {
        //            UpdateLabel(lblChordEdit, "Edit", 10, 10);
        //            UpdateLabel(lblChordEdit, g_session.CurClip.ChordEdit);
        //        }
        //    }

        //    UpdateChordLabel(lblChord1, 1);
        //    UpdateChordLabel(lblChord2, 2);
        //    UpdateChordLabel(lblChord3, 3);
        //    UpdateChordLabel(lblChord4, 4);
        //}


        //static void UpdateChordLabel(IMyTextPanel lbl, int chord)
        //{
        //    //if (TooComplex) return;

        //    var c = g_session.CurClip.Chords[chord-1];

        //    string chordName = GetChordName(c, S(chord));

        //    lbl.WriteText(chordName);

        //    UpdateLabel(
        //        lbl,
        //              g_session.CurClip.Chord == chord-1
        //           && (   g_session.CurClip.ChordEdit
        //               || g_session.CurClip.ChordMode)
        //           && !IsCurParam(strTune)
        //        || g_lightsPressed.Contains(lbl),
        //              g_session.CurClip.ChordMode
        //           && g_session.CurClip.Chord == chord-1 
        //        || c.Count > 0);
        //}


        //void MarkChordLabel(int chord)
        //{
        //         if (chord == 1 && g_session.CurClip.Chords[0].Count > 0) MarkLabel(lblChord1);
        //    else if (chord == 2 && g_session.CurClip.Chords[1].Count > 0) MarkLabel(lblChord2);
        //    else if (chord == 3 && g_session.CurClip.Chords[2].Count > 0) MarkLabel(lblChord3);
        //    else if (chord == 4 && g_session.CurClip.Chords[3].Count > 0) MarkLabel(lblChord4);
        //}


        //void UpdateShuffleLabel()
        //{
        //    if (g_session.CurClip.Spread)
        //    {
        //        UpdateLabel(lblShuffle, "Sprd", 10, 10);
        //    }
        //    else if (ShowPiano)
        //    {
        //        UpdateLabel(
        //            lblShuffle,
        //            " ▄█   █ █ ██ █ █ █   █▄ \n" +
        //           " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +
        //             " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ ",
        //            2,
        //            32);
        //    }
        //    else
        //    {
        //        UpdateLabel(lblShuffle, "Shuf", 10, 10);
        //    }
        //}


        //void UpdateOctaveLabel()
        //{
        //    if (TooComplex) return;
            
        //    int val = 0;

        //        //if (g_chordMode) 
        //            if (g_session.CurClip.Spread) val = g_session.CurClip.ChordSpread;
        //    else if (ShowPiano)     val = g_session.CurClip.CurrentChannel.Transpose;
        //    else                    val = g_session.CurClip.CurrentChannel.Shuffle;

        //    lblOctave.WriteText((val > 0 ? "+" : "") + S(val), false);
        //}


        //void UpdatePlayStopLabels()
        //{
        //    UpdateLabel(lblPlay, OK(g_session.CurClip.PlayTime));
        //    UpdateLabel(lblStop, OK(g_session.CurClip.PlayTime));
        //}


        //static void UpdateMemoryLabels()
        //{
        //    UpdateLabel(lblMemory, g_session.CurClip.MemSet);

        //    for (int m = 0; m < nMems; m++)
        //    {
        //        lblMems[m].WriteText(
        //              S((char)(65 + m)) + " "
        //            + (g_session.CurClip.Mems[m] > -1 ? S(g_session.CurClip.Mems[m] + 1).PadLeft(3) : " "));
        //    }
        //}


        //void UpdateEditLabels() 
        //{
        //    var strStep = 
        //        EditStep == 0.5f
        //        ? "½"
        //        : S0(EditStep);

        //    string strLength;

        //         if (g_session.CurClip.EditLength == 0.25f )    strLength = "¼";
        //    else if (g_session.CurClip.EditLength == 0.5f  )    strLength = "½";
        //    else if (g_session.CurClip.EditLength == float_Inf) strLength = "∞";
        //    else                                  strLength = S0(g_session.CurClip.EditLength);

        //    lblEditStep  .WriteText("·· " + strStep);
        //    lblEditLength.WriteText("─ "  + strLength);
        //}


        //void UpdateNewLabels()
        //{
        //    UpdateLabelColor(lblBackOut);
        //    UpdateLabelColor(lblBack);
        //    UpdateEnterLabel();

        //    UpdateLabelColor(lblPrev);
        //    UpdateLabelColor(lblNext);
        //    UpdateLabelColor(lblNew);
        //    UpdateLabelColor(lblDuplicate);
        //    UpdateLabelColor(lblDelete);

        //    UpdateLabel(lblMove, g_move ^ (g_session.CurClip.CurSrc > -1), g_session.CurClip.SelChan > -1 && !g_move);
        //}


        //void UpdateLabelColor(IMyTextPanel lbl) 
        //{
        //    UpdateLabel(lbl, g_session.CurClip.CurSrc > -1, g_session.CurClip.SelChan > -1); 
        //}


        //void UpdateEnterLabel()
        //{
        //    UpdateLabel(lblEnter, g_session.CurClip.CurSet < 0 && g_session.CurClip.CurSrc < 0 ? "└►" : " ", 10, 10);
        //    UpdateLabel(lblEnter, g_session.CurClip.CurSet < 0 && g_session.CurClip.CurSrc > -1, g_session.CurClip.SelChan > -1 && g_session.CurClip.CurSet < 0);
        //}


        //void UpdateAdjustLabels(Clip song)
        //{
        //    if (ModDestConnecting != null)
        //    {
        //        UpdateLabel(lblCmd1, "Conn", 10, 10);
        //        UpdateLabel(lblCmd1, true);
        //        return;
        //    }


        //    if (g_session.CurClip.CurSet > -1)
        //    {
        //        var path = g_settings.Last().GetPath(g_session.CurClip.CurSrc);

        //        if (g_session.CurClip.ParamKeys)
        //        {
        //            UpdateLabel(lblCmd1, "Inter", 10, 10);

        //            UpdateLabel(
        //                lblCmd3,
        //                g_session.CurClip.SelectedChannel.Notes.Find(n =>
        //                       n.SongStep >= song.EditPos
        //                    && n.SongStep <  song.EditPos+1
        //                    && n.Keys.Find(k => k.Path == path) != null) != null
        //                ? "X"
        //                : " ",
        //                10, 
        //                10);
        //        }
        //        else if (g_session.CurClip.ParamAuto)
        //        {
        //            if (OK(song.EditPos))
        //            { 
        //                if (g_session.CurClip.SelectedChannel.AutoKeys.Find(k =>
        //                        k.Path == path
        //                        && k.StepTime >= (song.EditPos % g_nSteps)
        //                        && k.StepTime <  (song.EditPos % g_nSteps) + 1) != null)
        //                {
        //                    UpdateLabel(lblCmd1, "Move", 10, 10);
        //                    UpdateLabel(lblCmd3, "X",    10, 10);
        //                }
        //                else
        //                {
        //                    UpdateLabel(lblCmd1, " ", 10, 10);
        //                    UpdateLabel(lblCmd3, "+", 10, 10);
        //                }
        //            }
        //            else
        //                UpdateLabel(lblCmd3, " ", 10, 10);
        //        }
        //        else
        //        {
        //            UpdateLabel(lblCmd1, HasTag(CurSetting, strMod) ? "Conn" : " ", 10, 10);
        //            UpdateLabel(lblCmd1, false);

        //            UpdateLabel(lblCmd3, CurSetting.CanDelete() ? "X" : " ", 10, 10);
        //            UpdateLabel(lblCmd3, false);
        //        }

        //        UpdateLabel(lblCmd2, " ", 10, 10);
        //    }
        //    else
        //    {
        //        if (g_session.CurClip.CurSrc > -1)
        //        {
        //            UpdateLabel(lblCmd1, "On",    10, 10);
        //            UpdateLabel(lblCmd1, g_session.CurClip.SelectedSource.On);
        //            UpdateLabel(lblCmd2, "Osc ↕", 10, 10);
        //            UpdateLabel(lblCmd3, " ",     10, 10);
        //            UpdateLabel(lblCmd3, false);
        //        }
        //        else
        //        { 
        //            UpdateLabel(lblCmd1, g_session.CurClip.SelChan < 0 ? "Copy" : " ", 10, 10);
        //            UpdateLabel(lblCmd1, false);

        //            UpdateLabel(lblCmd2, g_session.CurClip.SelChan < 0 ? "Paste" : " ", 10, 10);
        //            UpdatePasteLabel();

        //            UpdateLabel(
        //                lblCmd3,     
        //                g_session.CurClip.SelChan < 0 
        //                ? " ▄█   █ █ ██ █ █ █   █▄ \n" +
        //                 " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +  
        //                   " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ " 
        //                : " ", 
        //                2, 
        //                32);

        //            UpdateLabel(
        //                lblCmd3, 
        //                   g_session.CurClip.SelChan < 0 
        //                && g_session.CurClip.Transpose, 
        //                g_session.CurClip.EditNotes.Count > 0);
        //        }
        //    }


        //    bool canAdjust = 
        //           IsCurParam()
        //        || IsCurSetting(typeof(Harmonics))
        //        ||    g_session.CurClip.Transpose 
        //           && g_session.CurClip.SelChan < 0;


        //    var _strUp   = strRight;
        //    var _strDown = strLeft;

        //    if (      canAdjust
        //           && (   IsCurParam(strVol)
        //               || IsCurParam(strTune)
        //               || IsCurParam(strSus)
        //               || IsCurParam(strAmp)
        //               || IsCurParam(strLvl)
        //               || IsCurParam(strPow)
        //               ||     IsCurParam(strCnt)
        //                  && (g_session.CurClip.ParamKeys || g_session.CurClip.ParamAuto)
        //               || IsCurSetting(typeof(Harmonics)))
        //        || g_session.CurClip.Transpose)
        //    {
        //        _strUp   = strUp;
        //        _strDown = strDown;
        //    }

        //    UpdateLabel(lblShift, canAdjust ?  strShft : " ", 10, 10);
        //    UpdateLabel(lblDown,  canAdjust ? _strDown : " ", 10, 10);
        //    UpdateLabel(lblUp,    canAdjust ? _strUp   : " ", 10, 10);
            
        //    UpdateLabel(lblShift, canAdjust && g_session.CurClip.Shift);
        //    UpdateLabel(lblDown,  canAdjust && g_session.CurClip.Shift);
        //    UpdateLabel(lblUp,    canAdjust && g_session.CurClip.Shift);

        //    UpdateLabel(lblLeft,  g_lightsPressed.Contains(lblLeft),  g_session.CurClip.EditNotes.Count > 0);
        //    UpdateLabel(lblRight, g_lightsPressed.Contains(lblRight), g_session.CurClip.EditNotes.Count > 0);
        //}


        //void UpdatePasteLabel()
        //{
        //    UpdateLabel(
        //        lblCmd2,
        //        _lightsPressed.Contains(lblCmd2),
        //        copyChan != null);//g_session.CurClip.Shift && bc);
        //}


        //void UpdateLockLabels()
        //{
        //    UpdateLabel(lblLock,     g_locks.Find(l => l.IsLocked) != null);
        //    UpdateLabel(lblAutoLock, g_locks.Find(l => l.AutoLock) != null);
        //}


        //void UpdateTimerLabel()
        //{
        //    UpdateLabel(lblNoise, g_timers.Find(t => !t.Enabled) == null);
        //}


        //void UpdateClipsLabel()
        //{
        //    UpdateLabel(lblClips, g_showSession ? "Set" : "Clips", 8, 18);
        //    UpdateLabel(lblClips, g_setClip);
        //}


        //void UpdateGyroLabel()
        //{
        //    UpdateLabel(lblGyro, g_gyros.Find(g => !g.Enabled) == null);
        //}


        //void UpdateKeyLabels()
        //{
        //    if (TooComplex) 
        //        return;
                
        //    if (ShowPiano)
        //    {
        //        UpdateLabel(
        //            lblHigh[10],
        //              "     ║  ███  ║       ║  ███\n"
        //            + "     ║       ║       ║     \n"
        //            + "═════╬═══════╬═══════╬═════\n"
        //            + "     ║       ║       ║     \n"
        //            + " ███ ║  ███  ║  ███  ║  ███\n",
        //            1.7f,
        //            17);

        //        for (int h = 0; h < lblHigh.Count-1; h++)
        //            UpdateLabel(lblHigh[h], HighNoteName(h, g_session.CurClip.HalfSharp), 10, 10);

        //        for (int l = 0; l < lblLow.Count-1; l++)
        //            UpdateLabel(lblLow[l], LowNoteName(l, g_session.CurClip.HalfSharp), 10, 10);

        //        UpdatePianoLabels();
        //    }
        //    else
        //    {
        //        UpdateLabel(
        //            lblHigh[10],
        //              "█ █ ██ █ █ █\n"
        //            + "█▄█▄██▄█▄█▄█\n"
        //            + "▀▀▀▀▀▀▀▀▀▀▀▀\n",
        //            3.7f,
        //            10);

        //        for (int h = 0; h < lblHigh.Count-1; h++)
        //            UpdateLabel(lblHigh[h], " ", 10, 10);

        //        lblHigh[0].WriteText("◄∙∙");
        //        lblHigh[1].WriteText("∙∙►");

        //        lblHigh[2].WriteText("Pick");
        //        UpdateLabel(lblHigh[2], g_session.CurClip.Pick);
        //        UpdateLabel(lblHigh[3], "All Ch", 7.6f, 19.5f);
        //        UpdateLabel(lblHigh[3], g_session.CurClip.AllChan);
        //        lblHigh[4].WriteText("Inst");
        //        UpdateLabel(lblHigh[4], g_session.CurClip.RndInst);

        //        lblHigh[5].WriteText("Rnd");
        //        lblHigh[6].WriteText("Clr");

        //        lblHigh[7].WriteText("1/4");
        //        lblHigh[8].WriteText("1/8");
        //        lblHigh[9].WriteText("Flip");

        //        for (int l = 0; l < lblLow.Count; l++)
        //            UpdateLabel(lblLow[l], " ", 10, 10);

        //        UpdateStepLabels();
        //    }
        //}


        //void UpdatePianoLabels()
        //{
        //    UpdateHighLabels(g_session.CurClip.CurrentPattern, g_session.CurClip.CurrentChannel);
        //    UpdateLowLabels (g_session.CurClip.CurrentPattern, g_session.CurClip.CurrentChannel);
        //}


        //void UpdateHighLabels(Pattern pat, Channel chan)
        //{
        //    for (int h = 0; h < lblHigh.Count-1; h++)
        //        UpdateLabel(pat, chan, lblHigh[h], HighToNote(h));//num);
        //}


        //void UpdateLowLabels(Pattern pat, Channel chan)
        //{
        //    UpdateLabel(lblLow[15], ShowPiano ? "‡" : " ", 8, 17);
            
        //    if (ShowPiano)
        //        UpdateLabel(lblLow[15], g_session.CurClip.HalfSharp);

        //    for (int l = 0; l < lblLow.Count-1; l++)
        //        UpdateLabel(pat, chan, lblLow[l], LowToNote(l));//num);
        //}


        //void UpdateLabel(Pattern pat, Channel chan, IMyTextPanel light, int num)
        //{
        //    var step = g_session.CurClip.PlayStep % g_nSteps;

        //    var p = g_session.CurClip.Patterns.IndexOf(pat);


        //    if (IsCurParam(strTune))
        //    {
        //        var tune =
        //            g_session.CurClip.CurSrc > -1
        //            ? g_session.CurClip.SelectedSource    .Tune
        //            : g_session.CurClip.SelectedInstrument.Tune;

        //        if (tune.UseChord)
        //        { 
        //            UpdateLabel(
        //                light, 
        //                tune.Chord     .Contains(num),
        //                tune.FinalChord.Contains(num));
        //        }
        //    }
        //    else if (g_session.CurClip.Chord > -1
        //          && g_session.CurClip.ChordEdit)
        //    {
        //        UpdateLabel(light, g_session.CurClip.Chords[g_session.CurClip.Chord].Contains(num));
        //    }
        //    else
        //    {
        //        var thisChan =
        //               chan.Notes.FindIndex(n =>
        //                      num == n.Number
        //                   && (      g_session.CurClip.PlayStep >= p * g_nSteps + n.PatStep + n.ShOffset
        //                          && g_session.CurClip.PlayStep <  p * g_nSteps + n.PatStep + n.ShOffset + n.StepLength
        //                       ||    p * g_nSteps + n.PatStep >= g_session.CurClip.EditPos 
        //                          && p * g_nSteps + n.PatStep <  g_session.CurClip.EditPos + EditStep)) > -1
        //            ||    g_session.CurClip.Hold
        //               && g_notes.FindIndex(n =>
        //                         num == n.Number
        //                      && g_session.CurClip.PlayStep >= n.PatStep
        //                      && g_session.CurClip.PlayStep <  n.PatStep + n.StepLength) > -1;


        //        var otherChans = false;

        //        if (!thisChan)
        //        {
        //            for (int ch = 0; ch < g_nChans; ch++)
        //            {
        //                var _chan = pat.Channels[ch];

        //                otherChans |= _chan.Notes.FindIndex(n =>
        //                          num == n.Number
        //                       && ch  == n.iChan
        //                       && (   g_session.CurClip.PlayStep >= p * g_nSteps + n.PatStep + n.ShOffset
        //                           && g_session.CurClip.PlayStep <  p * g_nSteps + n.PatStep + n.ShOffset + n.StepLength
        //                    ||    p * g_nSteps + n.PatStep >= g_session.CurClip.EditPos 
        //                       && p * g_nSteps + n.PatStep <  g_session.CurClip.EditPos + EditStep)) > -1;
        //            }
        //        }


        //        var down = false;

        //        if (g_lightsPressed.Contains(light))
        //            down = true;

        //        UpdateLabel(light, thisChan || down, otherChans);
        //    }
        //}


        //void UpdateStepLabels()
        //{
        //    for (int step = 0; step < g_nSteps; step++)
        //    {
        //        var light = lblLow[step];

        //        var _step = step + g_session.CurClip.CurPat * g_nSteps;

        //        var on = g_session.CurClip.CurrentChannel.Notes.Find(n => 
        //               n.PatStep >= step
        //            && n.PatStep <  step+1) != null;

        //        Color c;

        //        if (   OK(g_session.CurClip.PlayStep)
        //            && _step == (int)g_session.CurClip.PlayStep
        //            && g_session.CurClip.CurPat == g_session.CurClip.PlayPat) c = on ? color0 : color6;
        //        else if (on)                            c = color6;
        //        else if (g_session.CurClip.EditPos == _step)       c = color3;
        //        else                                    c = step % 4 == 0 ? color2 : color0;

        //        light.BackgroundColor = c;
        //    }
        //}


        //static void UpdateEditLabel(IMyTextPanel light, bool b)
        //{
        //    if (light == null) return;

        //    if (b)
        //    {
        //        light.FontColor       = redColor0;
        //        light.BackgroundColor = redColor6;
        //    }
        //    else
        //    {
        //        light.FontColor       = redColor6;
        //        light.BackgroundColor = redColor0;
        //    }
        //}


        //static void UpdateHoldLabel() 
        //{ 
        //    UpdateLabel(
        //        lblHold, 
        //           g_session.CurClip.Hold 
        //        && (  !OK(g_session.CurClip.EditPos) 
        //            || g_session.CurClip.EditNotes.Count > 0)); 
        //}
    }
}
