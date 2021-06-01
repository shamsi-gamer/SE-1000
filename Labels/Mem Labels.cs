namespace IngameScript
{
    partial class Program
    {
        void InitMemLabels()
        {
            lblMemSet = new Label(0, GetLabel("MemSet"));
            lblMemory = new Label(0, GetLabel("Mem"), lbl => EditedClip.MemSet);

            for (int m = 0; m < nMems; m++)
                lblMem[m] = new Label(0, GetLabel("Mem " + S(m)), CF_null, CF_null, UpdateMem, AL_null, m);
        }


        void UpdateMem(Label lbl)
        {
            var m = lbl.Data;

            lbl.SetText(
                  S((char)(65 + m)) + strEmpty
                + (OK(EditedClip.Mems[m]) ? S(EditedClip.Mems[m] + 1).PadLeft(3) : strEmpty));
        }
    }
}
