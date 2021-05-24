namespace IngameScript
{
    partial class Program
    {
        void InitEditLabels()
        {
            lblLeft  = new Label(Lbl("Left"),  null, lbl => EditClip.EditNotes.Count > 0);
            lblRight = new Label(Lbl("Right"), null, lbl => EditClip.EditNotes.Count > 0);

            lblStep  = new Label(Lbl("Step"));

            lblHold  = new Label(Lbl("Hold"),
                lbl => 
                       EditClip.Hold 
                    && (  !OK(EditClip.EditPos) 
                        || EditClip.EditNotes.Count > 0));

            lblEditStep   = new Label(Lbl("Edit Step"),   null, null, UpdateEditStepLabel);
            lblEditLength = new Label(Lbl("Edit Length"), null, null, UpdateEditLengthLabel);
        }


        void UpdateEditStepLabel(Label lbl) 
        {
            var strStep = 
                EditClip.EditStep == 0.5f
                ? "½"
                : S0(EditClip.EditStep);

            lbl.SetText("·· " + strStep);
        }


        void UpdateEditLengthLabel(Label lbl) 
        {
            string strLength;

                 if (EditClip.EditStepLength == 0.25f )    strLength = "¼";
            else if (EditClip.EditStepLength == 0.5f  )    strLength = "½";
            else if (EditClip.EditStepLength == float_Inf) strLength = "∞";
            else                                          strLength = S0(EditClip.EditStepLength);

            lbl.SetText("─ " + strLength);
        }
    }
}
