namespace IngameScript
{
    partial class Program
    {
        void InitEditLabels()
        {
            lblLeft  = new Label(1, GetLabel("Left"),  CF_null, lbl => EditedClip.EditNotes.Count > 0, lbl => lbl.SetText("◄"));
            lblRight = new Label(1, GetLabel("Right"), CF_null, lbl => EditedClip.EditNotes.Count > 0, lbl => lbl.SetText("►"));
            lblStep  = new Label(1, GetLabel("Step"),  CF_null, CF_null,                               lbl => lbl.SetText("Step"));

            lblHold  = new Label(1, GetLabel("Hold"),
                lbl => 
                       EditedClip.Hold 
                    && (  !OK(EditedClip.EditPos) 
                        || EditedClip.EditNotes.Count > 0),
                CF_null,
                lbl => lbl.SetText("Hold"));

            lblEditStep   = new Label(1, GetLabel("Edit Step"),   CF_null, CF_null, UpdateEditStepLabel);
            lblEditLength = new Label(1, GetLabel("Edit Length"), CF_null, CF_null, UpdateEditLengthLabel);

            lblNote = new Label(1, GetLabel(strNote),
                lbl => EditedClip.Note, //EditedClip.EditNotes.Count > 0,
                CF_null,
                lbl => lbl.SetText("Note"));

            lblCut  = new Label(1, GetLabel(strCut), 
                CF_null, 
                CF_null, 
                lbl =>
                { 
                    if (GetLongNotes(EditedClip).Count > 0) lbl.SetText("Cut", 9, 14);
                    else                                    lbl.SetText(strEmpty);
                });

            lblEdit = new Label(1, GetLabel(strEdit),
                lbl => OK(EditedClip.EditPos),
                CF_null,
                lbl => lbl.SetText(strEdit),
                lbl => 
                {
                    lbl.ForeColor = ShowClip ? editColor6 : editColor0;
                    lbl.HalfColor = 
                    lbl.BackColor = editColor0;
                });

            lblRec = new Label(1, GetLabel(strRec),
                lbl => Recording,
                CF_null,
                lbl => lbl.SetText(strRec),
                lbl => 
                {
                    lbl.ForeColor = ShowClip ? recColor6 : recColor0;
                    lbl.HalfColor = 
                    lbl.BackColor = recColor0;
                });
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
            else                                             strLength = S0(EditedClip.EditStepLength);

            lbl.SetText("─ " + strLength);
        }
    }
}
