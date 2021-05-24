namespace IngameScript
{
    partial class Program
    {
        void InitChordLabels()
        {
            lblChord     = new Label(Lbl("Chord"),      ChordIsBright,     null,          UpdateChord);
            lblChord1    = new Label(Lbl("Chord 1"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 1);
            lblChord2    = new Label(Lbl("Chord 2"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 2);
            lblChord3    = new Label(Lbl("Chord 3"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 3);
            lblChord4    = new Label(Lbl("Chord 4"),    ChordNumIsBright,  ChordNumIsDim, UpdateChordNum, null, 4);
            lblChordEdit = new Label(Lbl("Chord Edit"), ChordEditIsBright, null,          UpdateChordEdit);
            lblSpread    = new Label(Lbl("Spread"),     lbl => EditClip.Spread);
        }


        bool ChordIsBright(Label lbl)
        {
            var tune =
                IsCurParam(strTune)
                ? (Tune)GetCurrentParam(SelInstrument) 
                : null;

            if (   !(EditClip.ParamKeys || EditClip.ParamAuto)
                && OK(tune))
                return tune.UseChord;
            else
                return EditClip.ChordMode;
        }


        void UpdateChord(Label lbl)
        {
            if (   !IsCurParam(strTune)
                || EditClip.ParamKeys 
                || EditClip.ParamAuto)
                lbl.SetText(EditClip.ChordEdit ? " " : "Chord", 9, 12);
        }


        bool ChordNumIsBright(Label lbl)
        {
            var chord = lbl.Data;

            return
                   EditClip.Chord == chord-1
                && (   EditClip.ChordEdit
                    || EditClip.ChordMode)
                && !IsCurParam(strTune);
        }


        bool ChordNumIsDim(Label lbl)
        {
            var chord = lbl.Data;

            return
                      EditClip.ChordMode
                   && EditClip.Chord == chord-1 
                || EditClip.Chords[chord-1].Count > 0;
        }


        void UpdateChordNum(Label lbl)
        {
            var chord = lbl.Data;
            lbl.SetText(GetChordName(EditClip.Chords[chord-1], S(chord)));
        }


        void UpdateChordEdit(Label lbl)
        {
            if (    IsCurParam(strTune)
                && !(EditClip.ParamKeys || EditClip.ParamAuto))
            {
                var tune =
                    IsCurParam(strTune)
                    ? (Tune)GetCurrentParam(SelInstrument) 
                    : null;

                lbl.SetText(
                       OK(tune)
                    && tune.UseChord 
                    ? strAll : " ");
            }
            else
                lbl.SetText(EditClip.ChordMode ? strAll : "Edit");
        }


        bool ChordEditIsBright(Label lbl)
        {
            var tune =
                IsCurParam(strTune)
                ? (Tune)GetCurrentParam(SelInstrument)
                : null;

            if (   IsCurParam(strTune)
                && !(EditClip.ParamKeys || EditClip.ParamAuto))
                return
                       OK(tune) 
                    && tune.AllOctaves;

            else return
                      EditClip.ChordMode
                   && EditClip.ChordAll
                || EditClip.ChordEdit;
        }
    }
}
