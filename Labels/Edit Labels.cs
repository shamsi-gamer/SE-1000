namespace IngameScript
{
    partial class Program
    {
        void InitEditLabels()
        {
            lblLeft  = new Label(Lbl("Left"),  null, lbl => EditedClip.EditNotes.Count > 0);
            lblRight = new Label(Lbl("Right"), null, lbl => EditedClip.EditNotes.Count > 0);

            lblStep  = new Label(Lbl("Step"));

            lblHold  = new Label(Lbl("Hold"),
                lbl => 
                       EditedClip.Hold 
                    && (  !OK(EditedClip.EditPos) 
                        || EditedClip.EditNotes.Count > 0));

            lblEditStep   = new Label(Lbl("Edit Step"),   null, null, UpdateEditStepLabel);
            lblEditLength = new Label(Lbl("Edit Length"), null, null, UpdateEditLengthLabel);
        }


        void UpdateEditStepLabel(Label lbl) 
        {
            var strStep = 
                EditedClip.EditStep == 0.5f
                ? "½"
                : S0(EditedClip.EditStep);

            lbl.SetText("·· " + strStep);
        }


        void UpdateEditLengthLabel(Label lbl) 
        {
            string strLength;

                 if (EditedClip.EditStepLength == 0.25f )    strLength = "¼";
            else if (EditedClip.EditStepLength == 0.5f  )    strLength = "½";
            else if (EditedClip.EditStepLength == float_Inf) strLength = "∞";
            else                                          strLength = S0(EditedClip.EditStepLength);

            lbl.SetText("─ " + strLength);
        }
    }
}
