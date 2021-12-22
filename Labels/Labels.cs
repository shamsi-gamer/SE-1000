using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static Label        lblShowClip, lblMix, lblCueClip, 
                            lblOctave, lblShuffle,
                            lblOctaveUp, lblOctaveDown,
                            lblLeft, lblRight,
                            lblStep, lblHold, lblScale,
                            lblEditStep, lblEditLength,
                            lblNote, lblCut, lblEdit, lblRec, 
                            lblLoop, lblBlock, lblAllPatterns, lblFollow, lblAutoCue,
                            lblNew, lblDup, lblDel,
                            lblMove, lblPrev, lblNext, 
                            lblEnter, lblBack, lblOut,
                            lblFold, lblTimers, lblMass,
                            lblCmd1, lblCmd2, lblCmd3,
                            lblUp, lblDown, lblShift,
                            lblChord, lblChord1, lblChord2, lblChord3, lblChord4, lblChordEdit, lblStrum,
                            lblMixerVolumeUp, lblMixerVolumeDown, lblMixerAll, lblMixerMuteAll,
                            lblMixerShift,
                            lblMemPat, lblMemSet;


        List<Label>         lblHigh,
                            lblLow;

        static  Label[]     lblMem = new Label[nMems];


        static IMyInteriorLight       g_warningLight;
        static IMyReflectorLight      g_frontLight;

        static List<IMyTextPanel>     g_allLabels  = new List<IMyTextPanel>();

        static List<IMyInteriorLight> g_rearLights = new List<IMyInteriorLight>();


        static List<Label>  g_fastLabels    = new List<Label>(),
                            g_slowLabels    = new List<Label>(),
                            g_clipLabels    = new List<Label>(),
                            g_adjustLabels  = new List<Label>(),

                            g_labelsPressed = new List<Label>(),
                             _labelsPressed = new List<Label>();

        static  List<int>   g_lcdPressed    = new List<int>(),
                             _lcdPressed    = new List<int>();


        const int lcdInfo  =    0,
                  lcdMain  = 1000,
                  lcdClip  = 2000,
                  lcdMixer = 3000;


        static Color MakeColor(Color c, float f)
        {
            return new Color(Color.Multiply(c, f), 1);
        }


        void InitLabels()
        {
            InitEditLabels();
            InitPianoLabels();
            InitToggleLabels();
            InitChordLabels();
            InitNavLabels();
            InitAdjustLabels();
            InitMemLabels();
            InitMixerLabels();
            InitSideLabels();

            Get(g_allLabels, l => l.CustomName != "Label Edit"
                               && l.CustomName != "Label Rec");

            var rearLights = GridTerminalSystem.GetBlockGroupWithName("Rear Lights");
            if (OK(rearLights)) rearLights.GetBlocksOfType(g_rearLights);

            g_frontLight   = Get("Front Light") as IMyReflectorLight;
            g_warningLight = Get("Sat Light")   as IMyInteriorLight;
        }



        void InitToggleLabels()
        {
            lblLoop        = new Label(1, GetLabel("Loop"),         lbl => EditedClip.Loop,    CF_null, lbl => lbl.SetText("Loop",     9,    14));
            lblBlock       = new Label(1, GetLabel("Block"),        lbl => EditedClip.Block,   CF_null, lbl => lbl.SetText("Block",    8,    18));
            lblAllPatterns = new Label(1, GetLabel("All Patterns"), lbl => EditedClip.AllPats, CF_null, lbl => lbl.SetText("All Pat",  6.5f, 26));
            lblFollow      = new Label(1, GetLabel("Follow"),       lbl => EditedClip.Follow,  CF_null, lbl => lbl.SetText("Follow",   8,    18));
            lblAutoCue     = new Label(1, GetLabel("Auto Cue"),     lbl => EditedClip.AutoCue, CF_null, lbl => lbl.SetText("Auto Cue", 6.3f, 26));
        }



        void InitSideLabels()
        {
            lblFold     = new Label(0, GetLabel("Fold"),      CF_null,                                  CF_null, AL_null, AL_null, 0, True);
            lblTimers   = new Label(0, GetLabel("Timers"),    lbl => OK(g_timers.Find(t => t.Enabled)), CF_null, AL_null, AL_null, 0, True);

            lblMass     = new Label(0, GetLabel("Mass"),      lbl => OK(g_mass  .Find(g => g.Enabled)), CF_null, AL_null, AL_null, 0, True);
        }



        void MarkChordLabel(int chord)
        {
            switch (chord)
            {
            case 0: lblChord1.Mark(); break;
            case 1: lblChord2.Mark(); break;
            case 2: lblChord3.Mark(); break;
            case 3: lblChord4.Mark(); break;
            }
        }



        static void SetLabelColor(int iCol)
        {
            EditedClip.ColorIndex = MinMax(0, iCol, 6);

            var ci = EditedClip.ColorIndex;

            if (ci == 0) SetLabelColor(new Color(255,   0,   0), 0.35f);
            if (ci == 1) SetLabelColor(new Color(255,  92,   0), 0.35f);
            if (ci == 2) SetLabelColor(new Color(255, 255,   0), 0.4f ); 
            if (ci == 3) SetLabelColor(new Color(0,   255,   0), 0.35f);
            if (ci == 4) SetLabelColor(new Color(0,    40, 255));       
            if (ci == 5) SetLabelColor(new Color(128,   0, 255), 0.4f ); 
            if (ci == 6) SetLabelColor(new Color(255, 255, 255), 0.35f);
        }



        static void SetLabelColor(Color c, float f = 1)
        {
            color6 = MakeColor(c, 0.878f * f);
            color5 = MakeColor(c, 0.622f * f);
            color4 = MakeColor(c, 0.353f * f);
            color3 = MakeColor(c, 0.170f * f);
            color2 = MakeColor(c, 0.051f * f);
            color1 = MakeColor(c, 0.020f * f);
            color0 = MakeColor(c, 0.004f * f);


            foreach (var l in g_allLabels)
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


            foreach (var l in g_rearLights)
                l.Color = lightColor;


            g_frontLight.Color = new Color(
                lightColor.R + (int)((0xFF - lightColor.R) * 0.23f),
                lightColor.G + (int)((0xFF - lightColor.G) * 0.23f),
                lightColor.B + (int)((0xFF - lightColor.B) * 0.23f));


            var ci = EditedClip.ColorIndex;

            if (ci == 0) g_warningLight.Color = new Color(0,    0,    0xFF);
            if (ci == 1) g_warningLight.Color = new Color(0,    0,    0xFF);
            if (ci == 2) g_warningLight.Color = new Color(0xFF, 0,    0x80);
            if (ci == 3) g_warningLight.Color = new Color(0xFF, 0,    0xFF);
            if (ci == 4) g_warningLight.Color = new Color(0xFF, 0x40, 0   );
            if (ci == 5) g_warningLight.Color = new Color(0xFF, 0x30, 0   );
            if (ci == 6) g_warningLight.Color = new Color(0xFF, 0,    0   );
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



        void ClearLabels(List<Label> labels)
        {
            foreach (var lbl in labels) 
            { 
                lbl.SetText(strEmpty); 
                lbl.BackColor = color0;
                lbl.Panel.BackgroundColor = color0;
            }
        }
    }
}
