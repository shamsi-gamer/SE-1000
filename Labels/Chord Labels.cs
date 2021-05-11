using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        void InitChordLabels()
        {
            lblChord     = new Label(false, Lbl("Chord"),      ChordIsBright,     null,          UpdateChord);
            lblChord1    = new Label(false, Lbl("Chord 1"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 1);
            lblChord2    = new Label(false, Lbl("Chord 2"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 2);
            lblChord3    = new Label(false, Lbl("Chord 3"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 3);
            lblChord4    = new Label(false, Lbl("Chord 4"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 4);
            lblChordEdit = new Label(false, Lbl("Chord Edit"), ChordEditIsBright, null,          UpdateChordEdit);
            lblSpread    = new Label(false, Lbl("Spread"),     lbl => CurClip.Spread);
        }


        bool ChordIsBright(Label lbl)
        {
            var tune =
                IsCurParam(strTune)
                ? (Tune)GetCurrentParam(SelInstrument) 
                : null;

            if (   !(CurClip.ParamKeys || CurClip.ParamAuto)
                && tune != null)
                return tune.UseChord;
            else
                return CurClip.ChordMode;
        }


        void UpdateChord(Label lbl)
        {
            if (   !IsCurParam(strTune)
                || CurClip.ParamKeys 
                || CurClip.ParamAuto)
                lbl.SetText(CurClip.ChordEdit ? " " : "Chord", 9, 12);
        }


        bool ChordNumIsBright(Label lbl)
        {
            var chord = lbl.Data;

            return
                    CurClip.Chord == chord-1
                && (   CurClip.ChordEdit
                    || CurClip.ChordMode)
                && !IsCurParam(strTune);
        }


        bool ChordNumIsDim(Label lbl)
        {
            var chord = lbl.Data;

            return
                      CurClip.ChordMode
                   && CurClip.Chord == chord-1 
                || CurClip.Chords[chord-1].Count > 0;
        }


        void UpdateChordNum(Label lbl)
        {
            var chord = lbl.Data;
            lbl.SetText(GetChordName(CurClip.Chords[chord-1], S(chord)));
        }


        void UpdateChordEdit(Label lbl)
        {
            if (    IsCurParam(strTune)
                && !(CurClip.ParamKeys || CurClip.ParamAuto))
            {
                var tune =
                    IsCurParam(strTune)
                    ? (Tune)GetCurrentParam(SelInstrument) 
                    : null;

                lbl.SetText(
                       tune != null 
                    && tune.UseChord 
                    ? strAll : " ");
            }
            else
                lbl.SetText(CurClip.ChordMode ? strAll : "Edit");
        }


        bool ChordEditIsBright(Label lbl)
        {
            var tune =
                IsCurParam(strTune)
                ? (Tune)GetCurrentParam(SelInstrument)
                : null;

            if (   IsCurParam(strTune)
                && !(CurClip.ParamKeys || CurClip.ParamAuto))
                return
                       tune != null 
                    && tune.AllOctaves;

            else return
                      CurClip.ChordMode
                   && CurClip.ChordAll
                || CurClip.ChordEdit;
        }
    }
}
