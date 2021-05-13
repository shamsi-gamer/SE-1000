namespace IngameScript
{
    partial class Program
    {
        void InitAdjustLabels()
        {
            lblCmd1  = new Label(Lbl("Command 1"), Cmd1IsBright, null, UpdateCmd1);
            lblCmd2  = new Label(Lbl("Command 2"), null,         null, UpdateCmd2);
            
            lblUp    = new Label(Lbl("Up"),    AdjustIsBright, null, UpdateAdjustUp);
            lblDown  = new Label(Lbl("Down"),  AdjustIsBright, null, UpdateAdjustDown);
            lblShift = new Label(Lbl("Shift"), AdjustIsBright, null, UpdateAdjustShift);
            
            lblCmd3  = new Label(Lbl("Command 3"), Cmd3IsBright, Cmd3IsDim, UpdateCmd3);
        }


        bool Cmd1IsBright(Label lbl)
        {
            return
                   ModDestConnecting != null
                ||    CurSrc > -1
                   && CurSet <  0
                   && SelSource.On;
        }


        void UpdateCmd1(Label lbl)
        {
            if (ModDestConnecting != null)
            {
                lbl.SetText("Conn");
                return;
            }
            else if (CurSet > -1)
            {
                var path = g_settings.Last().GetPath(CurSrc);

                if (CurClip.ParamKeys)
                    lbl.SetText("Inter");

                else if (CurClip.ParamAuto)
                {
                    if (OK(CurClip.EditPos))
                    {
                        lbl.SetText(
                            SelChannel.AutoKeys.Find(k =>
                                   k.Path == path
                                && k.StepTime >= (CurClip.EditPos % g_patSteps)
                                && k.StepTime <  (CurClip.EditPos % g_patSteps) + 1) != null
                            ? "Move" : " ");
                    }
                }
                else
                    lbl.SetText(HasTag(CurSetting, strMod) ? "Conn" : " ");
            }
            else
            {
                if (CurSrc > -1) lbl.SetText("On");
                else             lbl.SetText(SelChan < 0 ? "Copy" : " ");
            }
        }


        void UpdateCmd2(Label lbl)
        {
                 if (CurSrc > -1) lbl.SetText("Osc ↕");
            else if (CurSet > -1) lbl.SetText(" ");
            else                  lbl.SetText(SelChan < 0 ? "Paste" : " ");
        }


        bool AdjustIsBright(Label lbl)
        {
            return CanAdjust && CurClip.Shift;
        }


        void UpdateAdjustUp(Label lbl)
        {
            var str = AdjustArrowsAreVertical ? strUp : strRight;
            lbl.SetText(CanAdjust ? str : " ");
        }


        void UpdateAdjustDown(Label lbl)
        {
            var str = AdjustArrowsAreVertical ? strDown : strLeft;
            lbl.SetText(CanAdjust ? str : " ");
        }


        void UpdateAdjustShift(Label lbl)
        {
            lbl.SetText(CanAdjust ? strShft : " ");
        }


        bool CanAdjust { get
        {
            return
                   IsCurParam()
                || IsCurSetting(typeof(Harmonics))
                ||    CurClip.Transpose
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
                            && (CurClip.ParamKeys || CurClip.ParamAuto)
                        || IsCurSetting(typeof(Harmonics)))
                || CurClip.Transpose;
        } }


        bool Cmd3IsBright(Label lbl)
        {
            return
                   CurSet  < 0
                && CurSrc  < 0
                && SelChan < 0
                && CurClip.Transpose;
        }


        bool Cmd3IsDim(Label lbl)
        {
            return
                   CurSet < 0
                && CurSrc < 0
                && CurClip.EditNotes.Count > 0;
        }


        void UpdateCmd3(Label lbl)
        {
            if (CurSet > -1)
            {
                var path = g_settings.Last().GetPath(CurSrc);

                if (CurClip.ParamKeys)
                {
                    lblCmd3.SetText(
                        SelChannel.Notes.Find(n =>
                               n.SongStep >= CurClip.EditPos
                            && n.SongStep <  CurClip.EditPos+1
                            && n.Keys.Find(k => k.Path == path) != null) != null
                        ? "X"
                        : " ");
                }
                else if (CurClip.ParamAuto)
                {
                    if (OK(CurClip.EditPos))
                    { 
                        lblCmd3.SetText(
                            SelChannel.AutoKeys.Find(k =>
                                k.Path == path
                                && k.StepTime >= (CurClip.EditPos % g_patSteps)
                                && k.StepTime <  (CurClip.EditPos % g_patSteps) + 1) != null
                            ? "X" : "+");
                    }
                    else
                        lblCmd3.SetText(" ");
                }
                else
                    lblCmd3.SetText(CurSetting.CanDelete() ? "X" : " ");
            }
            else
            {
                if (CurSrc > -1)
                    lblCmd3.SetText(" ");

                else
                    lblCmd3.SetText(     
                        SelChan < 0 
                        ? " ▄█   █ █ ██ █ █ █   █▄ \n" +
                         " ▀██   █▄█▄██▄█▄█▄█   ██▀ \n" +  
                           " ▀   ▀▀▀▀▀▀▀▀▀▀▀▀   ▀ " 
                        : " ", 
                        2, 
                        32);
            }
        }
    }
}
