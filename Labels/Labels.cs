using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static Label        lblCue, lblSession,
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
                            lblMixerShift,
                            lblMemSet, lblMemory;


        List<Label>         lblHigh,
                            lblLow;

        static  Label[]     lblMem = new Label[nMems];


        IMyInteriorLight    warningLight;
        IMyReflectorLight   frontLight;


        static  List<Label> g_fastLabels    = new List<Label>(),
                            g_slowLabels    = new List<Label>(),

                            g_labelsPressed = new List<Label>(),
                             _labelsPressed = new List<Label>();

        static  List<int>   g_lcdPressed    = new List<int>(),
                             _lcdPressed    = new List<int>();


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
            lblEdit = new Label(GetLabel("Edit"),
                lbl => OK(EditedClip.EditPos),
                CF_null,
                AL_null,
                lbl => 
                {
                    lbl.ForeColor = editColor6;
                    lbl.HalfColor = 
                    lbl.BackColor = editColor0;
                });

            lblRec = new Label(GetLabel("Rec"),
                lbl => EditedClip.Recording,
                CF_null,
                AL_null,
                lbl => 
                {
                    lbl.ForeColor = recColor6;
                    lbl.HalfColor = 
                    lbl.BackColor = recColor0;
                });
        }


        void InitToggleLabels()
        {
            lblLoop        = new Label(GetLabel("Loop"),         lbl => EditedClip.Loop);
            lblBlock       = new Label(GetLabel("Block"),        lbl => EditedClip.Block);
            lblAllPatterns = new Label(GetLabel("All Patterns"), lbl => EditedClip.AllPats);
            lblFollow      = new Label(GetLabel("Follow"),       lbl => EditedClip.Follow);
            lblAutoCue     = new Label(GetLabel("Auto Cue"),     lbl => EditedClip.AutoCue);
        }


        void InitNavLabels()
        {
            lblNew   = new Label(GetLabel("New"),   NavIsBright,  NavIsDim, UpdateNew);
            lblDup   = new Label(GetLabel("Dup"),   NavIsBright,  NavIsDim, UpdateDup);
            lblDel   = new Label(GetLabel(strDel),  NavIsBright,  NavIsDim, UpdateDel);

            lblMove  = new Label(GetLabel("Move"),
                lbl => (Move ^ (CurSrc > -1)) && CurSet < 0, 
                lbl => SelChan > -1 && CurSet < 0 && !Move,
                UpdateMove);

            lblPrev  = new Label(GetLabel("Prev"),  MoveIsBright, NavIsDim, UpdatePrev);
            lblNext  = new Label(GetLabel("Next"),  MoveIsBright, NavIsDim, UpdateNext);

            lblOut   = new Label(GetLabel("Out"),   NavIsBright,  NavIsDim);
            lblBack  = new Label(GetLabel("Back"),  NavIsBright,  NavIsDim);
            lblEnter = new Label(GetLabel("Enter"), NavIsBright,  NavIsDim);
        }


        void UpdateSessionLabel(Label lbl)
        {
            if (ShowSession) lbl.SetText("Clip",    8, 18);
            else             lbl.SetText("Session", 7, 21);
        }


        void InitSideLabels()
        {
            lblLock     = new Label(GetLabel("Lock"),      lbl => OK(g_locks.Find(l => l.IsLocked)),  CF_null, AL_null, AL_null, 0, False, True);
            lblAutoLock = new Label(GetLabel("Auto Lock"), lbl => OK(g_locks.Find(l => l.AutoLock)),  CF_null, AL_null, AL_null, 0, False, True);

            lblFold     = new Label(GetLabel("Fold"),      CF_null, CF_null, AL_null, AL_null, 0, False, True);
            lblTimers   = new Label(GetLabel("Timers"),    lbl => !OK(g_timers.Find(t => !t.Enabled)), CF_null, AL_null, AL_null, 0, False, True);

            lblGyro     = new Label(GetLabel("Gyro"),      lbl => !OK(g_gyros .Find(g => !g.Enabled)), CF_null, AL_null, AL_null, 0, False, True);
            lblMass     = new Label(GetLabel("Mass"),      lbl => !OK(g_mass  .Find(g => !g.Enabled)), CF_null, AL_null, AL_null, 0, False, True);
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


        bool MoveIsBright(Label lbl) { return CurSet < 0 && Move ^ (CurSrc > -1); }
        //bool MoveIsDim   (Label lbl) { return SelChan > -1 && !g_move; }


        void SetLabelColor(int iCol)
        {
            EditedClip.ColorIndex = MinMax(0, iCol, 6);

            var ci = EditedClip.ColorIndex;

            if (ci == 0) SetLabelColor(new Color(255,   0,   0), 0.35f);
            if (ci == 1) SetLabelColor(new Color(255,  92,   0), 0.35f);
            if (ci == 2) SetLabelColor(new Color(255, 255,   0), 0.4f); 
            if (ci == 3) SetLabelColor(new Color(0,   255,   0), 0.35f);
            if (ci == 4) SetLabelColor(new Color(0,    40, 255));       
            if (ci == 5) SetLabelColor(new Color(128,   0, 255), 0.4f); 
            if (ci == 6) SetLabelColor(new Color(255, 255, 255), 0.35f);
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


            var ci = EditedClip.ColorIndex;

            if (ci == 0) warningLight.Color = new Color(0,    0,    0xFF);
            if (ci == 1) warningLight.Color = new Color(0,    0,    0xFF);
            if (ci == 2) warningLight.Color = new Color(0xFF, 0,    0x80);
            if (ci == 3) warningLight.Color = new Color(0xFF, 0,    0xFF);
            if (ci == 4) warningLight.Color = new Color(0xFF, 0x40, 0   );
            if (ci == 5) warningLight.Color = new Color(0xFF, 0x30, 0   );
            if (ci == 6) warningLight.Color = new Color(0xFF, 0,    0   );
        }


        void UnmarkAllLabels()
        {
            foreach (var lbl in _labelsPressed)
                lbl.Update(False);

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
