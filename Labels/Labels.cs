using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static Label              lblPlay, lblStop,
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
                                  lblChord, lblChord1, lblChord2, lblChord3, lblChord4, lblChordEdit, lblSpread,
                                  lblMixerVolumeUp, lblMixerVolumeDown, lblMixerAll, lblMixerMuteAll,
                                  lblMixerShift, lblSession,
                                  lblMemSet, lblMemory;


        List<Label>               lblHigh,
                                  lblLow;

        static Label[]            lblMem = new Label[nMems];


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
            InitMemLabels();
            InitMixerLabels();
            InitSideLabels();

            frontLight   = Get("Front Light")      as IMyReflectorLight;
            warningLight = Get("Saturation Light") as IMyInteriorLight;
        }


        void InitTransportLabels()
        {
            lblPlay = new Label(Lbl("Play"), lbl => g_playing, lbl => g_playing);
            lblStop = new Label(Lbl("Stop"), lbl => g_playing, lbl => g_playing);

            lblEdit = new Label(Lbl("Edit"),
                lbl => OK(CurClip.EditPos), 
                null,
                null,
                (lbl) => 
                {
                    lbl.ForeColor = editColor6;
                    lbl.HalfColor = 
                    lbl.BackColor = editColor0;
                });

            lblRec = new Label(Lbl("Rec"),
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
            lblLoop        = new Label(Lbl("Loop"),         lbl => CurClip.Loop);
            lblBlock       = new Label(Lbl("Block"),        lbl => CurClip.Block);
            lblAllPatterns = new Label(Lbl("All Patterns"), lbl => CurClip.AllPats);
            lblFollow      = new Label(Lbl("Follow"),       lbl => CurClip.Follow);
            lblAutoCue     = new Label(Lbl("Auto Cue"),     lbl => CurClip.AutoCue);
        }


        void InitNavigationLabels()
        {
            lblNew    = new Label(Lbl("New"),   NavIsBright,  NavIsDim);
            lblDup    = new Label(Lbl("Dup"),   NavIsBright,  NavIsDim);
            lblDelete = new Label(Lbl(strDel),  NavIsBright,  NavIsDim);

            lblMove   = new Label(Lbl("Move"),
                lbl => g_move ^ (CurSrc > -1), 
                lbl => SelChan > -1 && !g_move);

            lblPrev   = new Label(Lbl("Prev"),  MoveIsBright, NavIsDim);
            lblNext   = new Label(Lbl("Next"),  MoveIsBright, NavIsDim);

            lblOut    = new Label(Lbl("Out"),   NavIsBright,  NavIsDim);
            lblBack   = new Label(Lbl("Back"),  NavIsBright,  NavIsDim);
            lblEnter  = new Label(Lbl("Enter"), NavIsBright,  NavIsDim);
        }


        void InitMixerLabels()
        { 
            lblMixerVolumeUp   = new Label(Lbl("M Up R"));
            lblMixerVolumeDown = new Label(Lbl("M Down R"));
            lblMixerAll        = new Label(Lbl("M Solo R"));
            lblMixerMuteAll    = new Label(Lbl("M Mute R"));

            lblMixerShift = new Label(
                Lbl("M Shift"), 
                lbl => CurClip.MixerShift);

            lblSession = new Label(
                Lbl("Session"), 
                lbl => g_showSession && g_setClip,
                null,
                lbl => lbl.SetText(g_showSession ? "Clip" : "Clips", 8, 18));
        }


        void InitSideLabels()
        {
            lblLock     = new Label(Lbl("Lock"),      lbl => g_locks.Find(l => l.IsLocked) != null);
            lblAutoLock = new Label(Lbl("Auto Lock"), lbl => g_locks.Find(l => l.AutoLock) != null);

            lblFold     = new Label(Lbl("Fold"));

            lblGyro     = new Label(Lbl("Gyro"),  lbl => g_gyros .Find(g => !g.Enabled) == null);
            lblNoise    = new Label(Lbl("Noise"), lbl => g_timers.Find(t => !t.Enabled) == null);
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
        }


        void UnmarkAllLabels()
        {
            foreach (var lbl in _labelsPressed)
                lbl.Update(F);

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
    }
}
