using System;


namespace IngameScript
{
    partial class Program
    {
        void InitAdjustLabels()
        {
            lblCmd1  = new Label(1, GetLabel("Command 1"), Cmd1IsBright,        CF_null,   UpdateCmd1);
            lblCmd2  = new Label(1, GetLabel("Command 2"), Cmd2IsBright,        CF_null,   UpdateCmd2);
                                                                                        
            lblUp    = new Label(2, GetLabel("Up"),        AdjustIsBright,      CF_null,   UpdateAdjustUp);
            lblDown  = new Label(2, GetLabel("Down"),      AdjustIsBright,      CF_null,   UpdateAdjustDown);
            lblShift = new Label(2, GetLabel("Shift"),     AdjustShiftIsBright, CF_null,   UpdateAdjustShift);
            
            lblCmd3  = new Label(1, GetLabel("Command 3"), Cmd3IsBright,        Cmd3IsDim, UpdateCmd3);
        }



        bool Cmd1IsBright(Label lbl)
        {
            return
                      OK(CurSrc)
                   && !OK(CurSet)
                   && SelSource.On
                ||    OK(CurSet)
                   && CurSetting.On
                ||    !OK(SelChan)
                   && LockView > 0
                || OK(g_editKey);
        }



        void UpdateCmd1(Label lbl)
        {
            if (OK(CurSet))
            {
                var path = CurSetting.Path;

                if (EditedClip.ParamKeys)
                    lbl.SetText("Inter");

                else if (EditedClip.ParamAuto)
                {
                    if (OK(EditedClip.EditPos))
                    {
                        lbl.SetText(
                            OK(SelChannel.AutoKeys.Find(k =>
                                   k.Path == path
                                && k.Step >= (EditedClip.EditPos % g_patSteps)
                                && k.Step <  (EditedClip.EditPos % g_patSteps) + 1))
                            ? "Move" 
                            : strEmpty);
                    }
                }
                else
                    lbl.SetText("On");
            }
            else
            {
                if (OK(CurSrc)) lbl.SetText("On");
                else            lbl.SetText(SelChan < 0 ? "Lock" : strEmpty);
            }
        }



        bool Cmd2IsBright(Label lbl)
        {
            return 
                   OK(ModDestConnecting)
                ||    IsCurSetting(strChord)
                   && ((TuneChord)CurSetting).Moving;
        }



        void UpdateCmd2(Label lbl)
        {
            if (OK(ModDestConnecting))
                lbl.SetText("Conn");
            else if (IsCurSetting(strChord))
                lbl.SetText("▲\n▼", 7, 0);
            else if (OK(CurSrc))
                lbl.SetText("Osc ↕");
            else if (!OK(SelChan)
                   && EditedClip.UseInst
                   && OK(Array.Find(EditPattern.Channels, c => c.Instrument == CurChannel.Instrument)))
                lbl.SetText("Clps");
            else
                lbl.SetText(HasTag(EditedClip.CurSetting, strMod) ? "Conn" : strEmpty);
        }



        bool AdjustIsBright(Label lbl)
        {
            return
                      IsCurSetting(strChord)
                   && ((TuneChord)CurSetting).Moving
                ||    CanAdjust 
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



        bool AdjustShiftIsBright(Label lbl)
        {
            return
                   CanAdjust 
                && EditedClip.Shift;
        }



        void UpdateAdjustShift(Label lbl)
        {
            lbl.SetText(CanAdjust ? strShift : strEmpty, 9, 14);
        }



        bool Cmd3IsBright(Label lbl)
        {
            return
                   CurSet  < 0
                && CurSrc  < 0
                && SelChan < 0
                && EditedClip.Transpose;
        }



        bool Cmd3IsDim(Label lbl)
        {
            return
                   CurSet < 0
                && CurSrc < 0
                && EditedClip.EditNotes.Count > 0;
        }



        void UpdateCmd3(Label lbl)
        {
            if (OK(CurSet))
            {
                var path = CurSetting.Path;//EditedClip.Settings.Last().Path(CurSrc);

                if (EditedClip.ParamKeys)
                {
                    lblCmd3.SetText(
                        OK(SelChannel.Notes.Find(n =>
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
                            OK(SelChannel.AutoKeys.Find(k =>
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
                if (OK(CurSrc))
                    lblCmd3.SetText(strEmpty);

                else
                    lblCmd3.SetText(
                        SelChan < 0 
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
