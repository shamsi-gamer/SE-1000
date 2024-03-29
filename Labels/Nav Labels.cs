﻿namespace IngameScript
{
    partial class Program
    {
        void InitNavLabels()
        {
            lblNew   = new Label(1, GetLabel("New"),   NavIsBright,   NavIsDim,   UpdateNew);
            lblDup   = new Label(1, GetLabel("Dup"),   NavIsBright,   NavIsDim,   UpdateDup);
            lblDel   = new Label(1, GetLabel(strDel),  NavIsBright,   NavIsDim,   UpdateDel);
                                                                                  
            lblMove  = new Label(1, GetLabel("Move"),  MoveIsBright,  MoveIsDim,  UpdateMove);
                                                                                  
            lblPrev  = new Label(1, GetLabel("Prev"),  MoveIsBright,  NavIsDim,   UpdatePrev);
            lblNext  = new Label(1, GetLabel("Next"),  MoveIsBright,  NavIsDim,   UpdateNext);
                                                                                  
            lblOut   = new Label(1, GetLabel("Out"),   BackIsBright,  BackIsDim,  UpdateOut);
            lblBack  = new Label(1, GetLabel("Back"),  BackIsBright,  BackIsDim,  UpdateBack);

            lblEnter = new Label(1, GetLabel("Enter"), EnterIsBright, EnterIsDim, UpdateEnter);
        }



        bool NavIsBright(Label lbl) 
        { 
            return
                    OK(CurSrc)
                && !OK(CurSet)
                && !g_labelsPressed.Contains(lbl); 
        }
        


        bool NavIsDim(Label lbl) 
        { 
            return
                    OK(SelChan)
                && !OK(CurSet);
        }



        bool BackIsBright(Label lbl) 
        { 
            return
                    OK(CurSrc)
                && !g_labelsPressed.Contains(lbl); 
        }
        


        bool BackIsDim(Label lbl) 
        { 
            return OK(SelChan); 
        }



        bool EnterIsBright(Label lbl) 
        { 
            return
                    OK(CurSrc)
                && !OK(CurSet)
                && !g_labelsPressed.Contains(lbl); 
        }
        


        bool EnterIsDim(Label lbl) 
        { 
            return 
                    OK(SelChan) 
                && !OK(CurSet); 
        }



        void UpdateOut  (Label lbl) { lbl.SetText("◄◄"); }
        void UpdateBack (Label lbl) { lbl.SetText("◄┐"); }
        void UpdateEnter(Label lbl) { lbl.SetText(!OK(CurSet) ? "└►" : strEmpty); }



        void UpdateNew (Label lbl) { lbl.SetText(CurSet < 0 ? "New"  : strEmpty); }
        void UpdateDup (Label lbl) { lbl.SetText(CurSet < 0 ? "Dup"  : strEmpty); }
        void UpdateDel (Label lbl) { lbl.SetText(CurSet < 0 ? "Del"  : strEmpty); }
        void UpdateMove(Label lbl) { lbl.SetText(CurSet < 0 ? "▲\n▼" : strEmpty, 10, 20); }
        void UpdatePrev(Label lbl) { lbl.SetText(CurSet < 0 ? "►"    : strEmpty); }
        void UpdateNext(Label lbl) { lbl.SetText(CurSet < 0 ? "◄"    : strEmpty); }



        bool MoveIsBright(Label lbl) 
        { 
            return 
                   !OK(CurSet) 
                && (EditedClip.Move ^ OK(CurSrc)); 
        }



        bool MoveIsDim(Label lbl) 
        { 
            return
                    OK(SelChan)
                && !OK(CurSet)
                && !EditedClip.Move;
        }
    }
}
