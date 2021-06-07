namespace IngameScript
{
    partial class Program
    {
        void InitAdjustLabels()
        {
            lblCmd1  = new Label(1, GetLabel("Command 1"), Cmd1IsBright,   CF_null,   UpdateCmd1);
            lblCmd2  = new Label(1, GetLabel("Command 2"), CF_null,        CF_null,   UpdateCmd2);
                                                                                   
            lblUp    = new Label(2, GetLabel("Up"),        AdjustIsBright, CF_null,   UpdateAdjustUp);
            lblDown  = new Label(2, GetLabel("Down"),      AdjustIsBright, CF_null,   UpdateAdjustDown);
            lblShift = new Label(2, GetLabel("Shift"),     AdjustIsBright, CF_null,   UpdateAdjustShift);
            
            lblCmd3  = new Label(1, GetLabel("Command 3"), Cmd3IsBright,   Cmd3IsDim, UpdateCmd3);
        }


        bool Cmd1IsBright(Label lbl)
        {
            return
                   OK(ModDestConnecting)
                ||     OK(EditedClip.CurSrc)
                   && !OK(EditedClip.CurSet)
                   && EditedClip.SelSource.On
                ||    !OK(EditedClip.SelChan)
                   && LockView > 0
                || OK(g_editKey);
        }


        void UpdateCmd1(Label lbl)
        {
            if (OK(ModDestConnecting))
            {
                lbl.SetText("Conn");
                return;
            }
            else if (OK(EditedClip.CurSet))
            {
                var path = EditedClip.Settings.Last().GetPath(EditedClip.CurSrc);

                if (EditedClip.ParamKeys)
                    lbl.SetText("Inter");

                else if (EditedClip.ParamAuto)
                {
                    if (OK(EditedClip.EditPos))
                    {
                        lbl.SetText(
                            OK(EditedClip.SelChannel.AutoKeys.Find(k =>
                                   k.Path == path
                                && k.Step >= (EditedClip.EditPos % g_patSteps)
                                && k.Step <  (EditedClip.EditPos % g_patSteps) + 1))
                            ? "Move" 
                            : strEmpty);
                    }
                }
                else
                    lbl.SetText(HasTag(EditedClip.CurSetting, strMod) ? "Conn" : strEmpty);
            }
            else
            {
                if (OK(EditedClip.CurSrc)) lbl.SetText("On");
                else                       lbl.SetText(EditedClip.SelChan < 0 ? "Lock" : strEmpty);
            }
        }


        void UpdateCmd2(Label lbl)
        {
            lbl.SetText(OK(EditedClip.CurSrc) ? "Osc ↕" : strEmpty);
        }


        bool AdjustIsBright(Label lbl)
        {
            return
                   CanAdjust 
                && EditedClip.Shift;
        }


        void UpdateAdjustUp(Label lbl)
        {
            var str = AdjustArrowsAreVertical ? strUp : strRight;
            lbl.SetText(CanAdjust ? str : strEmpty);
        }


        void UpdateAdjustDown(Label lbl)
        {
            var str = AdjustArrowsAreVertical ? strDown : strLeft;
            lbl.SetText(CanAdjust ? str : strEmpty);
        }


        void UpdateAdjustShift(Label lbl)
        {
            lbl.SetText(CanAdjust ? strShift : strEmpty, 9, 14);
        }


        bool Cmd3IsBright(Label lbl)
        {
            return
                   EditedClip.CurSet  < 0
                && EditedClip.CurSrc  < 0
                && EditedClip.SelChan < 0
                && EditedClip.Transpose;
        }


        bool Cmd3IsDim(Label lbl)
        {
            return
                   EditedClip.CurSet < 0
                && EditedClip.CurSrc < 0
                && EditedClip.EditNotes.Count > 0;
        }


        void UpdateCmd3(Label lbl)
        {
            if (OK(EditedClip.CurSet))
            {
                var path = EditedClip.Settings.Last().GetPath(EditedClip.CurSrc);

                if (EditedClip.ParamKeys)
                {
                    lblCmd3.SetText(
                        OK(EditedClip.SelChannel.Notes.Find(n =>
                               n.ClipStep >= EditedClip.EditPos
                            && n.ClipStep <  EditedClip.EditPos+1
                            && OK(n.Keys.Find(k => k.Path == path))))
                        ? "X"
                        : strEmpty);
                }
                else if (EditedClip.ParamAuto)
                {
                    if (OK(EditedClip.EditPos))
                    { 
                        lblCmd3.SetText(
                            OK(EditedClip.SelChannel.AutoKeys.Find(k =>
                                k.Path == path
                                && k.Step >= (EditedClip.EditPos % g_patSteps)
                                && k.Step <  (EditedClip.EditPos % g_patSteps) + 1))
                            ? "X" : "+");
                    }
                    else
                        lblCmd3.SetText(strEmpty);
                }
                else
                    lblCmd3.SetText(EditedClip.CurSetting.CanDelete() ? "X" : strEmpty);
            }
            else
            {
                if (OK(EditedClip.CurSrc))
                    lblCmd3.SetText(strEmpty);

                else
                    lblCmd3.SetText(
                        EditedClip.SelChan < 0 
                        //? " ▄█   █ █ ██ █ █ █   █▄ \n" +
                        // " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +  
                        //   " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ " 
                        ? lblCmd3.Panel.CustomData
                        : strEmpty, 
                        2, 
                        32);
            }
        }
    }
}
