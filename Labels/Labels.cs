using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static Label        lblPlay, 
                            lblEdit, lblRec,
                            lblOctave, lblShuffle,
                            lblOctaveUp, lblOctaveDown,
                            lblLeft, lblRight,
                            lblStep, lblHold, 
                            lblEditStep, lblEditLength,
                            lblLoop, lblBlock, lblAllPatterns, lblFollow, lblAutoCue,
                            lblNew, lblDup, lblDel,
                            lblMove, lblPrev, lblNext, 
                            lblEnter, lblBack, lblOut,
                            lblLock, lblAutoLock,
                            lblFold, lblTimers,
                            lblGyro, lblMass,
                            lblCmd1, lblCmd2, lblCmd3,
                            lblUp, lblDown, lblShift,
                            lblChord, lblChord1, lblChord2, lblChord3, lblChord4, lblChordEdit, lblSpread,
                            lblMixerVolumeUp, lblMixerVolumeDown, lblMixerAll, lblMixerMuteAll,
                            lblMixerShift, lblSession,
                            lblMemSet, lblMemory;


        List<Label>         lblHigh,
                            lblLow;

        static  Label[]     lblMem = new Label[nMems];


        IMyInteriorLight    warningLight;
        IMyReflectorLight   frontLight;


        static  List<Label> g_fastLabels    = new List<Label>();
        static  List<Label> g_slowLabels    = new List<Label>();

        static  List<Label> g_labelsPressed = new List<Label>();
        static  List<Label>  _labelsPressed = new List<Label>();

        static  List<int>   g_lcdPressed   = new List<int>();
        static  List<int>    _lcdPressed   = new List<int>();


        const int lcdInfo  =    0,
                  lcdMain  = 1000,
                  lcdClip  = 2000,
                  lcdMixer = 3000;


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
            InitNavLabels();
            InitAdjustLabels();
            InitMemLabels();
            InitMixerLabels();
            InitSideLabels();

            frontLight   = Get("Front Light") as IMyReflectorLight;
            warningLight = Get("Sat Light")   as IMyInteriorLight;
        }


        void InitTransportLabels()
        {
            lblPlay = new Label(Lbl("Play"), 
                lbl => g_session.IsPlaying,
                null, 
                null,
                lbl =>
                { 
                    if (g_session.IsPlaying) lbl.SetText("Stop ▐█", 6.5f, 24);
                    else                     lbl.SetText("► Play",  7,    24);
                },
                0, 
                false, 
                true);

            lblEdit = new Label(Lbl("Edit"),
                lbl => OK(EditedClip.EditPos), 
                null,
                null,
                lbl => 
                {
                    lbl.ForeColor = editColor6;
                    lbl.HalfColor = 
                    lbl.BackColor = editColor0;
                });

            lblRec = new Label(Lbl("Rec"),
                lbl => EditedClip.Recording, 
                null,
                null,
                lbl => 
                {
                    lbl.ForeColor = recColor6;
                    lbl.HalfColor = 
                    lbl.BackColor = recColor0;
                });
        }


        void InitToggleLabels()
        {
            lblLoop        = new Label(Lbl("Loop"),         lbl => EditedClip.Loop);
            lblBlock       = new Label(Lbl("Block"),        lbl => EditedClip.Block);
            lblAllPatterns = new Label(Lbl("All Patterns"), lbl => EditedClip.AllPats);
            lblFollow      = new Label(Lbl("Follow"),       lbl => EditedClip.Follow);
            lblAutoCue     = new Label(Lbl("Auto Cue"),     lbl => EditedClip.AutoCue);
        }


        void InitNavLabels()
        {
            lblNew   = new Label(Lbl("New"),   NavIsBright,  NavIsDim, UpdateNew);
            lblDup   = new Label(Lbl("Dup"),   NavIsBright,  NavIsDim, UpdateDup);
            lblDel   = new Label(Lbl(strDel),  NavIsBright,  NavIsDim, UpdateDel);

            lblMove  = new Label(Lbl("Move"),
                lbl => (g_session.Move ^ (CurSrc > -1)) && CurSet < 0, 
                lbl => SelChan > -1 && CurSet < 0 && !g_session.Move,
                UpdateMove);

            lblPrev  = new Label(Lbl("Prev"),  MoveIsBright, NavIsDim, UpdatePrev);
            lblNext  = new Label(Lbl("Next"),  MoveIsBright, NavIsDim, UpdateNext);

            lblOut   = new Label(Lbl("Out"),   NavIsBright,  NavIsDim);
            lblBack  = new Label(Lbl("Back"),  NavIsBright,  NavIsDim);
            lblEnter = new Label(Lbl("Enter"), NavIsBright,  NavIsDim);
        }


        void UpdateSessionLabel(Label lbl)
        {
            if (g_session.ShowSession) lbl.SetText("Clip",    8, 18);
            else                       lbl.SetText("Session", 7, 21);
        }


        void InitSideLabels()
        {
            lblLock     = new Label(Lbl("Lock"),      lbl => OK(g_locks.Find(l => l.IsLocked)),  null, null, null, 0, F, T);
            lblAutoLock = new Label(Lbl("Auto Lock"), lbl => OK(g_locks.Find(l => l.AutoLock)),  null, null, null, 0, F, T);

            lblFold     = new Label(Lbl("Fold"),      null, null, null, null, 0, F, T);
            lblTimers   = new Label(Lbl("Timers"),    lbl => !OK(g_timers.Find(t => !t.Enabled)), null, null, null, 0, F, T);

            lblGyro     = new Label(Lbl("Gyro"),      lbl => !OK(g_gyros .Find(g => !g.Enabled)), null, null, null, 0, F, T);
            lblMass     = new Label(Lbl("Mass"),      lbl => !OK(g_mass  .Find(g => !g.Enabled)), null, null, null, 0, F, T);
        }


        bool NavIsBright(Label lbl) 
        { 
            return 
                   CurSrc > -1 
                && CurSet <  0 
                && !g_labelsPressed.Contains(lbl); 
        }
        

        bool NavIsDim(Label lbl) { return SelChan > -1 && CurSet < 0; }


        void UpdateNew (Label lbl) { lbl.SetText(CurSet < 0 ? "New"  : " "); }
        void UpdateDup (Label lbl) { lbl.SetText(CurSet < 0 ? "Dup"  : " "); }
        void UpdateDel (Label lbl) { lbl.SetText(CurSet < 0 ? "Del"  : " "); }
        void UpdateMove(Label lbl) { lbl.SetText(CurSet < 0 ? "▲\n▼" : " ", 10, 20); }
        void UpdatePrev(Label lbl) { lbl.SetText(CurSet < 0 ? "►"    : " "); }
        void UpdateNext(Label lbl) { lbl.SetText(CurSet < 0 ? "◄"    : " "); }


        bool MoveIsBright(Label lbl) { return CurSet < 0 && g_session.Move ^ (CurSrc > -1); }
        //bool MoveIsDim   (Label lbl) { return SelChan > -1 && !g_move; }


        void SetLabelColor(int iCol)
        {
            EditedClip.ColorIndex = MinMax(0, iCol, 6);

            switch (EditedClip.ColorIndex)
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


        void SetLabelColor(Color c, float f = 1)
        {
            color6 = MakeColor(c, 0.878f * f);
            color5 = MakeColor(c, 0.622f * f);
            color4 = MakeColor(c, 0.353f * f);
            color3 = MakeColor(c, 0.170f * f);
            color2 = MakeColor(c, 0.051f * f);
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


                 if (EditedClip.ColorIndex == 1) lightColor = new Color(0xFF, 0x50, 0);
            else if (EditedClip.ColorIndex == 5) lightColor = new Color(0xAA, 0, 0xFF);


            var lights = new List<IMyInteriorLight>();

            var group = GridTerminalSystem.GetBlockGroupWithName("Rear Lights");
            if (OK(group)) group.GetBlocksOfType(lights);

            foreach (var l in lights)
                l.Color = lightColor;


            frontLight.Color = new Color(
                lightColor.R + (int)((0xFF - lightColor.R) * 0.23f),
                lightColor.G + (int)((0xFF - lightColor.G) * 0.23f),
                lightColor.B + (int)((0xFF - lightColor.B) * 0.23f));


            switch (EditedClip.ColorIndex)
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

            // mark for next cycle and clear pressed list

            _labelsPressed.Clear();
               _lcdPressed.Clear();

            _labelsPressed.AddRange(g_labelsPressed);
               _lcdPressed.AddRange(g_lcdPressed);

            g_labelsPressed.Clear();
               g_lcdPressed.Clear();
        }
    }
}
