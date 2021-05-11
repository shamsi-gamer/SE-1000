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
        static Label                lblPlay, lblStop,
                                    lblEdit, lblRec,
                                    lblOctave, lblShuffle,
                                    lblOctaveUp, lblOctaveDown,
                                    lblLeft, lblRight,
                                    lblStep, lblHold, 
                                    lblEditStep, lblEditLength,
                                    lblLoop, lblBlock, lblAllPatterns, lblFollow, lblAutoCue,
                                    lblNew, lblDup, lblDelete,
                                    lblMove, lblPrev, lblNext, 
                                    lblEnter, lblBack, lblOut,
                                    lblLock, lblAutoLock,
                                    lblFold, lblGyro, lblNoise,
                                    lblCmd1, lblCmd2, lblCmd3,
                                    lblUp, lblDown, lblShift,
                                    lblChord, lblChord1, lblChord2, lblChord3, lblChord4, lblChordEdit, lblSpread;

        //                          lblMixerVolumeUp, lblMixerVolumeDown, lblMixerAll, lblMixerMuteAll,
        //                          lblMixerShift, lblClips, lblMemSet, lblMemory,


        List<Label>                 lblHigh,
                                    lblLow;

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

        static List<Label>        g_labelsPressed = new List<Label>();
        static List<Label>         _labelsPressed = new List<Label>();

        static List<Label>        g_fastLabels    = new List<Label>();
        static List<Label>        g_slowLabels    = new List<Label>();


        void SetLabelColor(int iCol)
        {
            CurClip.ColorIndex = MinMax(0, iCol, 6);

            switch (CurClip.ColorIndex)
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
            InitTransportLabels();
            InitEditLabels();
            InitPianoLabels();
            InitToggleLabels();
            InitChordLabels();
            InitNavigationLabels();
            InitAdjustLabels();
            InitSideLabels();


            //lblMixerVolumeUp   = Lbl("M Up R");
            //lblMixerVolumeDown = Lbl("M Down R");
            //lblMixerAll        = Lbl("M Solo R");
            //lblMixerMuteAll    = Lbl("M Mute R");

            //lblMixerShift      = Lbl("M Shift");

            //lblClips           = Lbl("Clips");
            //lblMemSet          = Lbl("MemSet");
            //lblMemory          = Lbl("Mem");

            //for (int m = 0; m < nMems; m++)
            //    lblMems[m] = Lbl("Mem " + S(m));

            //lblMem = new List<IMyTextPanel>();
            //Get(lblMem, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Mem ");
            //lblMem = lblMem.OrderBy(l => int.Parse(l.CustomName.Substring(10))).ToList();


            frontLight   = Get("Front Light")              as IMyReflectorLight;
            warningLight = Get("Saturation Warning Light") as IMyInteriorLight;
        }


        void InitTransportLabels()
        {
            lblPlay = new Label(false, Lbl("Play"), lbl => g_playing, lbl => g_playing);
            lblStop = new Label(false, Lbl("Stop"), lbl => g_playing, lbl => g_playing);

            lblEdit = new Label(false, Lbl("Edit"),
                lbl => OK(CurClip.EditPos), 
                null,
                null,
                (lbl) => 
                {
                    lbl.ForeColor = editColor6;
                    lbl.HalfColor = 
                    lbl.BackColor = editColor0;
                });

            lblRec = new Label(false, Lbl("Rec"),
                lbl => CurClip.Recording, 
                null,
                null,
                (lbl) => 
                {
                    lbl.ForeColor = recColor6;
                    lbl.HalfColor = 
                    lbl.BackColor = recColor0;
                });
        }


        void InitToggleLabels()
        {
            lblLoop        = new Label(false, Lbl("Loop"),         lbl => CurClip.Loop);
            lblBlock       = new Label(false, Lbl("Block"),        lbl => CurClip.Block);
            lblAllPatterns = new Label(false, Lbl("All Patterns"), lbl => CurClip.AllPats);
            lblFollow      = new Label(false, Lbl("Follow"),       lbl => CurClip.Follow);
            lblAutoCue     = new Label(false, Lbl("Auto Cue"),     lbl => CurClip.AutoCue);
        }


        void InitNavigationLabels()
        {
            lblNew    = new Label(false, Lbl("New"),   NavIsBright,  NavIsDim);
            lblDup    = new Label(false, Lbl("Dup"),   NavIsBright,  NavIsDim);
            lblDelete = new Label(false, Lbl(strDel),  NavIsBright,  NavIsDim);

            lblMove   = new Label(false, Lbl("Move"),
                lbl => g_move ^ (CurSrc > -1), 
                lbl => SelChan > -1 && !g_move);

            lblPrev   = new Label(false, Lbl("Prev"),  MoveIsBright, NavIsDim);
            lblNext   = new Label(false, Lbl("Next"),  MoveIsBright, NavIsDim);

            lblOut    = new Label(false, Lbl("Out"),   NavIsBright,  NavIsDim);
            lblBack   = new Label(false, Lbl("Back"),  NavIsBright,  NavIsDim);
            lblEnter  = new Label(false, Lbl("Enter"), NavIsBright,  NavIsDim);
        }


        void InitSideLabels()
        {
            lblLock     = new Label(false, Lbl("Lock"),      lbl => g_locks.Find(l => l.IsLocked) != null);
            lblAutoLock = new Label(false, Lbl("Auto Lock"), lbl => g_locks.Find(l => l.AutoLock) != null);

            lblFold     = new Label(false, Lbl("Fold"));

            lblGyro     = new Label(false, Lbl("Gyro"),  lbl => g_gyros .Find(g => !g.Enabled) == null);
            lblNoise    = new Label(false, Lbl("Noise"), lbl => g_timers.Find(t => !t.Enabled) == null);
        }


        bool NavIsBright (Label lbl) { return CurSrc  > -1 && !g_labelsPressed.Contains(lbl); }
        bool NavIsDim    (Label lbl) { return SelChan > -1; }

        bool MoveIsBright(Label lbl) { return g_move ^ (CurSrc > -1); }
        bool MoveIsDim   (Label lbl) { return SelChan > -1 && !g_move; }


        void UpdatePianoToggle(Label lbl)
        {
            if (ShowPiano)
            {
                lbl.SetText(
                      "     ║  ███  ║       ║  ███\n"
                    + "     ║       ║       ║     \n"
                    + "═════╬═══════╬═══════╬═════\n"
                    + "     ║       ║       ║     \n"
                    + " ███ ║  ███  ║  ███  ║  ███\n",
                    1.7f,
                    17);
            }
            else
            {
                lbl.SetText(
                      "█ █ ██ █ █ █\n"
                    + "█▄█▄██▄█▄█▄█\n"
                    + "▀▀▀▀▀▀▀▀▀▀▀▀\n",
                    3.7f,
                    10);
            }
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
            Get(labels, l => l.CustomName != "Label Edit"
                          && l.CustomName != "Label Rec");

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


                 if (CurClip.ColorIndex == 1) lightColor = new Color(0xFF, 0x50, 0);
            else if (CurClip.ColorIndex == 5) lightColor = new Color(0xAA, 0, 0xFF);


            var lights = new List<IMyInteriorLight>();

            var group = GridTerminalSystem.GetBlockGroupWithName("Rear Lights");
            if (group != null) group.GetBlocksOfType(lights);

            foreach (var l in lights)
                l.Color = lightColor;


            frontLight.Color = new Color(
                lightColor.R + (int)((0xFF - lightColor.R) * 0.23f),
                lightColor.G + (int)((0xFF - lightColor.G) * 0.23f),
                lightColor.B + (int)((0xFF - lightColor.B) * 0.23f));


            switch (CurClip.ColorIndex)
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


        void UnmarkAllLabels()
        {
            //var be  = CurClip.EditNotes.Count > 0;
            //var crd = CurClip.ChordEdit;
            //var cur = CurSrc > -1;
            //var ch  = SelChan > -1;
            //var mov = CurClip.MovePat;
            //var sh  = CurClip.Shift;
            //var set = CurSet < 0;


            //if (_lightsPressed.Contains(lblLeft))      UnmarkLabel(lblLeft,  false, be);
            //if (_lightsPressed.Contains(lblRight))     UnmarkLabel(lblRight, false, be);

            //if (_lightsPressed.Contains(lblUp))        UnmarkLabel(lblUp,   sh);
            //if (_lightsPressed.Contains(lblDown))      UnmarkLabel(lblDown, sh);

            //if (_lightsPressed.Contains(lblNextPat))   UnmarkLabel(lblNextPat, mov);
            //if (_lightsPressed.Contains(lblPrevPat))   UnmarkLabel(lblPrevPat, mov);

            //if (_lightsPressed.Contains(lblNext))      UnmarkLabel(lblNext, g_move || cur, ch);
            //if (_lightsPressed.Contains(lblPrev))      UnmarkLabel(lblPrev, g_move || cur, ch);

            //if (_lightsPressed.Contains(lblBackOut))   UnmarkLabel(lblBack,  cur, ch);
            //if (_lightsPressed.Contains(lblBack))      UnmarkLabel(lblBack,  cur, ch);
            //if (_lightsPressed.Contains(lblEnter))     UnmarkLabel(lblEnter, cur && set, ch && set);

            //if (_lightsPressed.Contains(lblNew))       UnmarkLabel(lblNew,       cur, ch);
            //if (_lightsPressed.Contains(lblDuplicate)) UnmarkLabel(lblDuplicate, cur, ch);
            //if (_lightsPressed.Contains(lblDelete))    UnmarkLabel(lblDelete,    cur, ch);

            //if (_lightsPressed.Contains(lblChord1))    UnmarkLabel(lblChord1, crd && CurClip.Chord == 0, CurClip.Chords[0].Count > 0);
            //if (_lightsPressed.Contains(lblChord2))    UnmarkLabel(lblChord2, crd && CurClip.Chord == 1, CurClip.Chords[1].Count > 0);
            //if (_lightsPressed.Contains(lblChord3))    UnmarkLabel(lblChord3, crd && CurClip.Chord == 2, CurClip.Chords[2].Count > 0);
            //if (_lightsPressed.Contains(lblChord4))    UnmarkLabel(lblChord4, crd && CurClip.Chord == 3, CurClip.Chords[3].Count > 0);

            //if (_lightsPressed.Contains(lblCmd2))      UnmarkLabel(lblCmd2, false, copyChan != null);


            foreach (var lbl in _labelsPressed)
                lbl.Update(false);


            _mixerPressed .Clear();
            _infoPressed  .Clear();
            _labelsPressed.Clear();


            // mark for next cycle and clear pressed list

            _mixerPressed.AddRange(g_mixerPressed);
            g_mixerPressed.Clear();

            _infoPressed.AddRange(g_infoPressed);
            g_infoPressed.Clear();

            g_clipPressed.Clear();
            g_mainPressed.Clear();

            _labelsPressed.AddRange(g_labelsPressed);
            g_labelsPressed.Clear();
        }


        //void UpdateLabels()
        //{
        //    if (TooComplex) return;

        //    UpdateLabel(lblFollow,      CurClip?.Follow  ?? false);
        //    UpdateLabel(lblLoop,        CurClip?.Loop    ?? false);
        //    UpdateLabel(lblBlock,       CurClip?.Block   ?? false);
        //    UpdateLabel(lblAllPatterns, CurClip?.AllPats ?? false);
        //    UpdateLabel(lblMovePat,     CurClip?.MovePat ?? false);
        //    UpdateLabel(lblAutoCue,     CurClip?.AutoCue ?? false);

        //    UpdateEditLabel(lblEdit, OK(CurClip.EditPos));
        //    UpdateHoldLabel();

        //    UpdateChordLabels();

        //    UpdateShuffleLabel();
        //    UpdateOctaveLabel();

        //    UpdatePlayStopLabels();
        //    UpdateMemoryLabels();
        //    UpdateEditLabels();
        //    UpdateNewLabels();

        //    UpdateAdjustLabels(CurClip);

        //    UpdateLockLabels();
        //    UpdateGyroLabel();
        //    UpdateTimerLabel();

        //    UpdateClipsLabel();
        //}


        //void UpdatePlayStopLabels()
        //{
        //    UpdateLabel(lblPlay, g_playing);
        //    UpdateLabel(lblStop, g_playing);
        //}


        //static void UpdateMemoryLabels()
        //{
        //    UpdateLabel(lblMemory, CurClip.MemSet);

        //    for (int m = 0; m < nMems; m++)
        //    {
        //        lblMems[m].WriteText(
        //              S((char)(65 + m)) + " "
        //            + (CurClip.Mems[m] > -1 ? S(CurClip.Mems[m] + 1).PadLeft(3) : " "));
        //    }
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

        //    UpdateLabel(lblMove, g_move ^ (CurSrc > -1), SelChan > -1 && !g_move);
        //}


        //void UpdateLabelColor(IMyTextPanel lbl) 
        //{
        //    UpdateLabel(lbl, CurSrc > -1, SelChan > -1); 
        //}


        //void UpdateEnterLabel()
        //{
        //    UpdateLabel(lblEnter, CurSet < 0 && CurSrc < 0 ? "└►" : " ", 10, 10);
        //    UpdateLabel(lblEnter, CurSet < 0 && CurSrc > -1, SelChan > -1 && CurSet < 0);
        //}


        //void UpdatePasteLabel()
        //{
        //    UpdateLabel(
        //        lblCmd2,
        //        _lightsPressed.Contains(lblCmd2),
        //        copyChan != null);//CurClip.Shift && bc);
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
        //            UpdateLabel(lblHigh[h], HighNoteName(h, CurClip.HalfSharp), 10, 10);

        //        for (int l = 0; l < lblLow.Count-1; l++)
        //            UpdateLabel(lblLow[l], LowNoteName(l, CurClip.HalfSharp), 10, 10);

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
        //        UpdateLabel(lblHigh[2], CurClip.Pick);
        //        UpdateLabel(lblHigh[3], "All Ch", 7.6f, 19.5f);
        //        UpdateLabel(lblHigh[3], CurClip.AllChan);
        //        lblHigh[4].WriteText("Inst");
        //        UpdateLabel(lblHigh[4], CurClip.RndInst);

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
        //    UpdateHighLabels(CurPattern, CurChannel);
        //    UpdateLowLabels (CurPattern, CurChannel);
        //}


        //void UpdateHighLabels(Pattern pat, Channel chan)
        //{
        //    for (int h = 0; h < lblHigh.Count-1; h++)
        //        UpdateLabel(pat, chan, lblHigh[h], HighToNote(h));
        //}


        //void UpdateLowLabels(Pattern pat, Channel chan)
        //{
        //    UpdateLabel(lblLow[15], ShowPiano ? "‡" : " ", 8, 17);

        //    if (ShowPiano)
        //        UpdateLabel(lblLow[15], CurClip.HalfSharp);

        //    for (int l = 0; l < lblLow.Count-1; l++)
        //        UpdateLabel(pat, chan, lblLow[l], LowToNote(l));
        //}


        //void UpdateLabel(Pattern pat, Channel chan, IMyTextPanel light, int num)
        //{
        //    var step = PlayStep % g_nSteps;

        //    var p = CurClip.Patterns.IndexOf(pat);


        //    if (IsCurParam(strTune))
        //    {
        //        var tune =
        //            CurSrc > -1
        //            ? CurClip.SelectedSource    .Tune
        //            : CurClip.SelectedInstrument.Tune;

        //        if (tune.UseChord)
        //        { 
        //            UpdateLabel(
        //                light, 
        //                tune.Chord     .Contains(num),
        //                tune.FinalChord.Contains(num));
        //        }
        //    }
        //    else if (CurClip.Chord > -1
        //          && CurClip.ChordEdit)
        //    {
        //        UpdateLabel(light, CurClip.Chords[CurClip.Chord].Contains(num));
        //    }
        //    else
        //    {
        //        var thisChan =
        //               chan.Notes.FindIndex(n =>
        //                      num == n.Number
        //                   && (      PlayStep >= p * g_nSteps + n.PatStep + n.ShOffset
        //                          && PlayStep <  p * g_nSteps + n.PatStep + n.ShOffset + n.StepLength
        //                       ||    p * g_nSteps + n.PatStep >= CurClip.EditPos 
        //                          && p * g_nSteps + n.PatStep <  CurClip.EditPos + EditStep)) > -1
        //            ||    CurClip.Hold
        //               && g_notes.FindIndex(n =>
        //                         num == n.Number
        //                      && PlayStep >= n.PatStep
        //                      && PlayStep <  n.PatStep + n.StepLength) > -1;


        //        var otherChans = false;

        //        if (!thisChan)
        //        {
        //            for (int ch = 0; ch < g_nChans; ch++)
        //            {
        //                var _chan = pat.Channels[ch];

        //                otherChans |= _chan.Notes.FindIndex(n =>
        //                          num == n.Number
        //                       && ch  == n.iChan
        //                       && (   PlayStep >= p * g_nSteps + n.PatStep + n.ShOffset
        //                           && PlayStep <  p * g_nSteps + n.PatStep + n.ShOffset + n.StepLength
        //                    ||    p * g_nSteps + n.PatStep >= CurClip.EditPos 
        //                       && p * g_nSteps + n.PatStep <  CurClip.EditPos + EditStep)) > -1;
        //            }
        //        }


        //        var down = false;

        //        if (g_lightsPressed.Contains(light))
        //            down = true;

        //        UpdateLabel(light, thisChan || down, otherChans);
        //    }
        //}


        //void UpdateStepLabel(Label lbl)
        //{
        //    var clip = CurClip;
        //    var step = lbl.Data;

        //    var _step = step + clip.CurPat * g_nSteps;

        //    var on = clip.CurrentChannel.Notes.Find(n => 
        //           n.PatStep >= step
        //        && n.PatStep <  step+1) != null;

        //    Color c;

        //    if (   OK(clip.PlayStep)
        //        && _step == (int)clip.PlayStep
        //        && clip.CurPat == clip.PlayPat) c = on ? color0 : color6;
        //    else if (on)                        c = color6;
        //    else if (clip.EditPos == _step)     c = color3;
        //    else                                c = step % 4 == 0 ? color2 : color0;

        //    light.BackgroundColor = c;
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
        //           CurClip.Hold 
        //        && (  !OK(CurClip.EditPos) 
        //            || CurClip.EditNotes.Count > 0)); 
        //}
    }
}
