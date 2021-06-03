﻿namespace IngameScript
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
                ||     OK(CurSrc)
                   && !OK(CurSet)
                   && SelSource.On
                ||    !OK(SelChan)
                   && LockView > 0;
        }


        void UpdateCmd1(Label lbl)
        {
            if (OK(ModDestConnecting))
            {
                lbl.SetText("Conn");
                return;
            }
            else if (OK(CurSet))
            {
                var path = g_settings.Last().GetPath(CurSrc);

                if (EditedClip.ParamKeys)
                    lbl.SetText("Inter");

                else if (EditedClip.ParamAuto)
                {
                    if (OK(EditedClip.EditPos))
                    {
                        lbl.SetText(
                            OK(SelChannel.AutoKeys.Find(k =>
                                   k.Path == path
                                && k.StepTime >= (EditedClip.EditPos % g_patSteps)
                                && k.StepTime <  (EditedClip.EditPos % g_patSteps) + 1))
                            ? "Move" 
                            : strEmpty);
                    }
                }
                else
                    lbl.SetText(HasTag(CurSetting, strMod) ? "Conn" : strEmpty);
            }
            else
            {
                if (OK(CurSrc)) lbl.SetText("On");
                else            lbl.SetText(SelChan < 0 ? "Lock" : strEmpty);
            }
        }


        void UpdateCmd2(Label lbl)
        {
            lbl.SetText(OK(CurSrc) ? "Osc ↕" : strEmpty);
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


        bool CanAdjust { get
        {
            return
                   IsCurParam()
                || IsCurSetting(typeof(Harmonics))
                ||    EditedClip.Transpose
                   && SelChan < 0;
        } }


        bool AdjustArrowsAreVertical { get 
        {
            return
                       CanAdjust
                    && (   IsCurParam(strVol)
                        || IsCurParam(strTune)
                        || IsCurParam(strSus)
                        || IsCurParam(strAmp)
                        || IsCurParam(strLvl)
                        || IsCurParam(strPow)
                        ||     IsCurParam(strCnt)
                            && (EditedClip.ParamKeys || EditedClip.ParamAuto)
                        || IsCurSetting(typeof(Harmonics)))
                || EditedClip.Transpose;
        } }


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
                var path = g_settings.Last().GetPath(CurSrc);

                if (EditedClip.ParamKeys)
                {
                    lblCmd3.SetText(
                        OK(SelChannel.Notes.Find(n =>
                               n.SongStep >= EditedClip.EditPos
                            && n.SongStep <  EditedClip.EditPos+1
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
                                && k.StepTime >= (EditedClip.EditPos % g_patSteps)
                                && k.StepTime <  (EditedClip.EditPos % g_patSteps) + 1))
                            ? "X" : "+");
                    }
                    else
                        lblCmd3.SetText(strEmpty);
                }
                else
                    lblCmd3.SetText(CurSetting.CanDelete() ? "X" : strEmpty);
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
