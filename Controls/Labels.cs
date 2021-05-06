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
                                    lblEditStep, lblEditLength;
        //                          lblMixerVolumeUp, lblMixerVolumeDown, lblMixerAll, lblMixerMuteAll,
        //                          lblPlay, lblStop,
        //                          lblChord, lblChord1, lblChord2, lblChord3, lblChord4, lblChordEdit,
        //                          lblLoop, lblBlock, 
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
            InitTransportLabels();
            InitEditLabels();
            InitPianoLabels();

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
                lbl => OK(g_session.CurClip.EditPos), 
                null,
                (lbl, b) => 
                {
                    lbl.Panel.FontColor       = b ? editColor0 : editColor6;
                    lbl.Panel.BackgroundColor = b ? editColor6 : editColor0;
                });

            lblRec = new Label(false, Lbl("Rec"),
                lbl => g_session.CurClip.Recording, 
                null,
                (lbl, b) => 
                {
                    lbl.Panel.FontColor       = b ? recColor0 : recColor6;
                    lbl.Panel.BackgroundColor = b ? recColor6 : recColor0;
                });
        }


        void InitEditLabels()
        {
            lblLeft  = new Label(false, Lbl("Left"));
            lblRight = new Label(false, Lbl("Right"));

            lblStep  = new Label(false, Lbl("Step"));

            lblHold = new Label(false, Lbl("Hold"),
                lbl =>    
                       g_session.CurClip.Hold 
                    && (  !OK(g_session.CurClip.EditPos) 
                        || g_session.CurClip.EditNotes.Count > 0));

            lblEditStep   = new Label(false, Lbl("Edit Step"),   UpdateEditStepLabel);
            lblEditLength = new Label(false, Lbl("Edit Length"), UpdateEditLengthLabel);
        }


        void UpdateEditStepLabel(Label lbl, bool b) 
        {
            var clip = g_session.CurClip;

            var strStep = 
                clip.EditStep == 0.5f
                ? "½"
                : S0(clip.EditStep);

            lbl.Update("·· " + strStep);
        }


        void UpdateEditLengthLabel(Label lbl, bool b) 
        {
            var clip = g_session.CurClip;

            string strLength;

                 if (clip.EditLength == 0.25f )    strLength = "¼";
            else if (clip.EditLength == 0.5f  )    strLength = "½";
            else if (clip.EditLength == float_Inf) strLength = "∞";
            else                                   strLength = S0(clip.EditLength);

            lbl.Update("─ " + strLength);
        }


        void InitPianoLabels()
        {
            InitPianoLabelsHigh();
            InitPianoLabelsLow();

            lblOctave     = new Label(false, Lbl("Octave"),  UpdateOctaveLabel);
            lblShuffle    = new Label(false, Lbl("Shuffle"), UpdateShuffleLabel);
            lblOctaveUp   = new Label(false, Lbl("Octave Up"));
            lblOctaveDown = new Label(false, Lbl("Octave Down"));
        }


        void InitPianoLabelsHigh()
        {
            lblHigh = new List<Label>();

            var high = new List<IMyTextPanel>();
            Get(high, l => l.CustomName.Length >= 11 && l.CustomName.Substring(0, 11) == "Label High ");
            high = high.OrderBy(l => int.Parse(l.CustomName.Substring(11))).ToList();

            for (int h = 0; h < 10; h++)
                lblHigh.Add(new Label(true, high[h], IsPianoHighBright, IsPianoHighDim, UpdatePianoHigh, h));

            lblHigh.Add(new Label(false, high[10],
                lbl => IsPressed(lbl),
                null,
                UpdatePianoToggle));
        }


        bool IsPianoHighBright(Label lbl)
        {
            return
                ShowPiano
                ? NoteIsBright(HighToNote(lbl.Data), true)
                : ToggleIsBright(lbl);
        }


        bool IsPianoHighDim(Label lbl)
        {
            return 
                   ShowPiano 
                && NoteIsDim(HighToNote(lbl.Data), true);
        }


        void InitPianoLabelsLow()
        {
            lblLow = new List<Label>();

            var low = new List<IMyTextPanel>();
            Get(low, l => l.CustomName.Length >= 10 && l.CustomName.Substring(0, 10) == "Label Low ");
            low = low.OrderBy(l => int.Parse(l.CustomName.Substring(10))).ToList();

            for (int l = 0; l < low.Count; l++)
                lblLow.Add(new Label(true, low[l], IsPianoLowBright, IsPianoLowDim, UpdatePianoLow, -l));
        }


        bool IsPianoLowBright(Label lbl)
        {
            return
                ShowPiano
                ?    -lbl.Data == 15 && g_session.CurClip.HalfSharp
                  || NoteIsBright(LowToNote(-lbl.Data), false)
                : StepIsBright(lbl);
        }


        bool IsPianoLowDim(Label lbl)
        {
            return
                ShowPiano
                ? NoteIsDim(LowToNote(-lbl.Data), false)
                : false;
        }


        void UpdatePianoLow(Label lbl, bool b)
        {
            if (ShowPiano)
            {
                if (-lbl.Data < 15) lbl.Update(LowNoteName(-lbl.Data, g_session.CurClip.HalfSharp));
                else                lbl.Update("‡", 8, 17);
            }
            else                    lbl.Update(" ");
        }


        bool ToggleIsBright(Label lbl)
        {
            var clip = g_session.CurClip;

            return 
                   lbl.Data == 2 && clip.Pick
                || lbl.Data == 3 && clip.AllChan
                || lbl.Data == 4 && clip.RndInst;
        }


        bool StepIsBright(Label lbl)
        {
            var clip =  g_session.CurClip;
            var step = -lbl.Data;

            var _step = step + clip.CurPat * g_nSteps;

            var on = clip.CurrentChannel.Notes.Find(n => 
                   n.PatStep >= step
                && n.PatStep <  step+1) != null;

            if (   OK(clip.PlayStep)
                && _step == (int)clip.PlayStep
                && clip.CurPat == clip.PlayPat)
                // else c = step % 4 == 0 ? color2 : color0;
                return on;
            else if (on)
                return true;

            return false;
        }


        void UpdatePianoHigh(Label lbl, bool b)
        {
            if (ShowPiano)
                lbl.Update(HighNoteName(lbl.Data, g_session.CurClip.HalfSharp)); 

            else
            { 
                switch (lbl.Data)
                { 
                case 0: lbl.Update("◄∙∙");                 break;
                case 1: lbl.Update("∙∙►");                 break;
                                                                       
                case 2: lbl.Update("Pick");                break;
                case 3: lbl.Update("All Ch", 7.6f, 19.5f); break;
                case 4: lbl.Update("Inst");                break;
                                                                       
                case 5: lbl.Update("Rnd");                 break;
                case 6: lbl.Update("Clr");                 break;
                                                                       
                case 7: lbl.Update("1/4");                 break;
                case 8: lbl.Update("1/8");                 break;
                case 9: lbl.Update("Flip");                break;
                }
            }
        }


        void UpdatePianoToggle(Label lbl, bool b)
        {
            if (ShowPiano)
            {
                lbl.Update(
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
                lbl.Update(
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


                 if (g_session.CurClip.ColorIndex == 1) lightColor = new Color(0xFF, 0x50, 0);
            else if (g_session.CurClip.ColorIndex == 5) lightColor = new Color(0xAA, 0, 0xFF);


            var lights = new List<IMyInteriorLight>();

            var group = GridTerminalSystem.GetBlockGroupWithName("Rear Lights");
            if (group != null) group.GetBlocksOfType(lights);

            foreach (var l in lights)
                l.Color = lightColor;


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


        bool NoteIsBright(int note, bool high, int l = -1)
        {
            var clip = g_session.CurClip;

            var chan = clip.CurrentChannel;
            var pat  = clip.Patterns.IndexOf(clip.CurrentPattern);
            var step = pat * g_nSteps;
                        
            if (chan.Notes.FindIndex(n =>
                        note == n.Number
                    && (      clip.PlayStep >= step + n.PatStep + n.ShOffset
                            && clip.PlayStep <  step + n.PatStep + n.ShOffset + n.StepLength
                        ||    step + n.PatStep >= clip.EditPos 
                            && step + n.PatStep <  clip.EditPos + clip.EditStep)) > -1)
                return true;

            if (   clip.Hold
                && g_notes.FindIndex(n =>
                            note == n.Number
                        && clip.PlayStep >= n.PatStep
                        && clip.PlayStep <  n.PatStep + n.StepLength) > -1)
                return true;

            return false;
        }


        bool NoteIsDim(int note, bool high, int l = -1)
        {
            var clip = g_session.CurClip;
            
            var chan = clip.CurrentChannel;
            var pat  = clip.Patterns.IndexOf(clip.CurrentPattern);
            var step = pat * g_nSteps;

            for (int ch = 0; ch < g_nChans; ch++)
            {
                var _chan = clip.CurrentPattern.Channels[ch];

                if (_chan.Notes.FindIndex(n =>
                            note == n.Number
                        && ch   == n.iChan
                        && (   clip.PlayStep >= step + n.PatStep + n.ShOffset
                            && clip.PlayStep <  step + n.PatStep + n.ShOffset + n.StepLength
                    ||    step + n.PatStep >= clip.EditPos 
                        && step + n.PatStep <  clip.EditPos + clip.EditStep)) > -1)
                    return true;
            }

            return false;
        }


        void UpdateOctaveLabel(Label lbl, bool b)
        {
            if (TooComplex) return;

            var clip = g_session.CurClip;
            int val;

                 if (clip.Spread) val = clip.ChordSpread;
            else if (ShowPiano)   val = clip.CurrentChannel.Transpose;
            else                  val = clip.CurrentChannel.Shuffle;

            lbl.Update((val > 0 ? "+" : "") + S(val));
        }


        void UpdateShuffleLabel(Label lbl, bool b)
        {
            if (g_session.CurClip.Spread)
            {
                lbl.Update("Sprd");
            }
            else if (ShowPiano)
            {
                lbl.Update(
                    " ▄█   █ █ ██ █ █ █   █▄ \n" +
                   " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +
                     " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ ",
                    2,
                    32);
            }
            else
            {
                lbl.Update("Shuf");
            }
        }
        
        
        void UnmarkAllLabels()
        {
            //var be  = g_session.CurClip.EditNotes.Count > 0;
            //var cur = g_session.CurClip.CurSrc > -1;
            //var crd = g_session.CurClip.ChordEdit;
            //var ch  = g_session.CurClip.SelChan > -1;
            //var mov = g_session.CurClip.MovePat;
            //var sh  = g_session.CurClip.Shift;
            //var set = g_session.CurClip.CurSet < 0;


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

            //if (_lightsPressed.Contains(lblChord1))    UnmarkLabel(lblChord1, crd && g_session.CurClip.Chord == 0, g_session.CurClip.Chords[0].Count > 0);
            //if (_lightsPressed.Contains(lblChord2))    UnmarkLabel(lblChord2, crd && g_session.CurClip.Chord == 1, g_session.CurClip.Chords[1].Count > 0);
            //if (_lightsPressed.Contains(lblChord3))    UnmarkLabel(lblChord3, crd && g_session.CurClip.Chord == 2, g_session.CurClip.Chords[2].Count > 0);
            //if (_lightsPressed.Contains(lblChord4))    UnmarkLabel(lblChord4, crd && g_session.CurClip.Chord == 3, g_session.CurClip.Chords[3].Count > 0);

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

        //        UpdateLabel(lblChordEdit, tune.UseChord ? strAll : " ");
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
        //         if (chord == 1 && g_session.CurClip.Chords[0].Count > 0) //MarkLabel(lblChord1);
        //    else if (chord == 2 && g_session.CurClip.Chords[1].Count > 0) //MarkLabel(lblChord2);
        //    else if (chord == 3 && g_session.CurClip.Chords[2].Count > 0) //MarkLabel(lblChord3);
        //    else if (chord == 4 && g_session.CurClip.Chords[3].Count > 0) //MarkLabel(lblChord4);
        //}


        //void UpdatePlayStopLabels()
        //{
        //    UpdateLabel(lblPlay, g_playing);
        //    UpdateLabel(lblStop, g_playing);
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


        //void UpdateAdjustLabels(Clip clip)
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
        //                       n.SongStep >= clip.EditPos
        //                    && n.SongStep <  clip.EditPos+1
        //                    && n.Keys.Find(k => k.Path == path) != null) != null
        //                ? "X"
        //                : " ",
        //                10, 
        //                10);
        //        }
        //        else if (g_session.CurClip.ParamAuto)
        //        {
        //            if (OK(clip.EditPos))
        //            { 
        //                if (g_session.CurClip.SelectedChannel.AutoKeys.Find(k =>
        //                        k.Path == path
        //                        && k.StepTime >= (clip.EditPos % g_nSteps)
        //                        && k.StepTime <  (clip.EditPos % g_nSteps) + 1) != null)
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
        //        UpdateLabel(pat, chan, lblHigh[h], HighToNote(h));
        //}


        //void UpdateLowLabels(Pattern pat, Channel chan)
        //{
        //    UpdateLabel(lblLow[15], ShowPiano ? "‡" : " ", 8, 17);

        //    if (ShowPiano)
        //        UpdateLabel(lblLow[15], g_session.CurClip.HalfSharp);

        //    for (int l = 0; l < lblLow.Count-1; l++)
        //        UpdateLabel(pat, chan, lblLow[l], LowToNote(l));
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


        //void UpdateStepLabel(Label lbl, bool b)
        //{
        //    var clip = g_session.CurClip;
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
        //           g_session.CurClip.Hold 
        //        && (  !OK(g_session.CurClip.EditPos) 
        //            || g_session.CurClip.EditNotes.Count > 0)); 
        //}
    }
}
