﻿namespace IngameScript
{
    partial class Program
    {
        void InitChordLabels()
        {
            lblChord     = new Label(1, GetLabel("Chord"),      ChordIsBright,           CF_null,       UpdateChord);
            lblChord1    = new Label(1, GetLabel("Chord 1"),    ChordNumIsBright,        ChordNumIsDim, UpdateChordNum, AL_null, 1);
            lblChord2    = new Label(1, GetLabel("Chord 2"),    ChordNumIsBright,        ChordNumIsDim, UpdateChordNum, AL_null, 2);
            lblChord3    = new Label(1, GetLabel("Chord 3"),    ChordNumIsBright,        ChordNumIsDim, UpdateChordNum, AL_null, 3);
            lblChord4    = new Label(1, GetLabel("Chord 4"),    ChordNumIsBright,        ChordNumIsDim, UpdateChordNum, AL_null, 4);
            lblChordEdit = new Label(1, GetLabel("Chord Edit"), ChordEditIsBright,       CF_null,       UpdateChordEdit);
            lblStrum     = new Label(1, GetLabel("Strum"),      lbl => EditedClip.Strum, CF_null,       lbl => lbl.SetText("Strum", 9, 14));
        }



        bool ChordIsBright(Label lbl)
        {
            //var tune =
            //    IsCurParam(strTune)
            //    ? (Tune)CurrentParam
            //    : Tune_null;

            //if (   OK(tune)
            //    && !(   EditedClip.ParamKeys 
            //         || EditedClip.ParamAuto))
            //    return tune.UseChord;
            //else
                return EditedClip.ChordMode;
        }



        void UpdateChord(Label lbl)
        {
            //if (   /*!IsCurParam(strTune) || */
            //       EditedClip.ParamKeys 
            //    || EditedClip.ParamAuto)
                lbl.SetText(EditedClip.ChordEdit ? strEmpty : "Chord", 9, 12);
        }



        bool ChordNumIsBright(Label lbl)
        {
            var chord = lbl.Data;

            return
                   EditedClip.Chord == chord-1
                && (   EditedClip.ChordEdit
                    || EditedClip.ChordMode)
                && !IsCurParam(strTune);
        }



        bool ChordNumIsDim(Label lbl)
        {
            var chord = lbl.Data;

            return
                      EditedClip.ChordMode
                   && EditedClip.Chord == chord-1 
                || EditedClip.Chords[chord-1].Count > 0;
        }



        void UpdateChordNum(Label lbl)
        {
            var chord = lbl.Data;
            lbl.SetText(S(chord));//GetChordName(EditedClip.Chords[chord-1], S(chord)));
        }



        void UpdateChordEdit(Label lbl)
        {
            //if (    IsCurParam(strTune)
            //    && !(EditedClip.ParamKeys || EditedClip.ParamAuto))
            //{
            //    var tune =
            //        IsCurParam(strTune)
            //        ? (Tune)CurrentParam
            //        : Tune_null;

            //    lbl.SetText(
            //           OK(tune)
            //        && tune.UseChord 
            //        ? strAll : strEmpty);
            //}
            //else
                lbl.SetText(
                    EditedClip.ChordMode 
                    ? (OK(EditedClip.EditPos) ? strPick : strEmpty) 
                    : strEdit);
        }



        bool ChordEditIsBright(Label lbl)
        {
            //var tune =
            //    IsCurParam(strTune)
            //    ? (Tune)CurrentParam
            //    : Tune_null;

            //if (   IsCurParam(strTune)
            //    && !(EditedClip.ParamKeys || EditedClip.ParamAuto))
            //    return
            //           OK(tune) 
            //        && tune.UseChord
            //        && tune.AllOctaves;

            //else return
            //          EditedClip.ChordMode
            //       && EditedClip.ChordAll
            //    || EditedClip.ChordEdit;

            return EditedClip.ChordEdit;
        }
    }
}
