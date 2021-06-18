namespace IngameScript
{
    partial class Program
    {
        void InitMemLabels()
        {
            lblMemSet = new Label(0, GetLabel("Mem Set"), lbl => SetMemSet,                      lbl =>  SetOrPat);
            lblMemPat = new Label(0, GetLabel("Mem Pat"), lbl => EditedClip?.SetMemPat ?? False, lbl => !SetOrPat);

            for (int m = 0; m < nMems; m++)
                lblMem[m] = new Label(0, GetLabel("Mem " + S(m)), CF_null, CF_null, UpdateMem, AL_null, m);
        }


        void UpdateMem(Label lbl)
        {
            var i = lbl.Data;

            if (SetOrPat)
            {
                lbl.SetText(OK(Sets[i]) ? Sets[i].Tag : strEmpty);
            }
            else
            { 
                lbl.SetText(
                      S((char)(65 + i)) + strEmpty
                    + (OK(EditedClip.Mems[i]) ? S(EditedClip.Mems[i] + 1).PadLeft(3) : strEmpty));
            }
        }
    }
}
