namespace IngameScript
{
    partial class Program
    {
        void InitEditLabels()
        {
            lblLeft  = new Label(false, Lbl("Left"),  null, lbl => CurClip.EditNotes.Count > 0);
            lblRight = new Label(false, Lbl("Right"), null, lbl => CurClip.EditNotes.Count > 0);

            lblStep  = new Label(false, Lbl("Step"));

            lblHold  = new Label(false, Lbl("Hold"),
                lbl =>    
                       CurClip.Hold 
                    && (  !OK(CurClip.EditPos) 
                        || CurClip.EditNotes.Count > 0));

            lblEditStep   = new Label(false, Lbl("Edit Step"),   null, null, UpdateEditStepLabel);
            lblEditLength = new Label(false, Lbl("Edit Length"), null, null, UpdateEditLengthLabel);
        }


        void UpdateEditStepLabel(Label lbl) 
        {
            var strStep = 
                CurClip.EditStep == 0.5f
                ? "½"
                : S0(CurClip.EditStep);

            lbl.SetText("·· " + strStep);
        }


        void UpdateEditLengthLabel(Label lbl) 
        {
            string strLength;

                 if (CurClip.EditStepLength == 0.25f )    strLength = "¼";
            else if (CurClip.EditStepLength == 0.5f  )    strLength = "½";
            else if (CurClip.EditStepLength == float_Inf) strLength = "∞";
            else                                          strLength = S0(CurClip.EditStepLength);

            lbl.SetText("─ " + strLength);
        }
    }
}
