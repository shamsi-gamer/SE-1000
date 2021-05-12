﻿namespace IngameScript
{
    partial class Program
    {
        void InitMemLabels()
        {
            lblMemSet = new Label(Lbl("MemSet"));
            lblMemory = new Label(Lbl("Mem"), lbl => CurClip.MemSet);

            for (int m = 0; m < nMems; m++)
                lblMem[m] = new Label(Lbl("Mem " + S(m)), null, null, UpdateMem, null, m);
        }


        void UpdateMem(Label lbl)
        {
            var m = lbl.Data;

            lbl.SetText(
                  S((char)(65 + m)) + " "
                + (CurClip.Mems[m] > -1 ? S(CurClip.Mems[m] + 1).PadLeft(3) : " "));
        }
    }
}