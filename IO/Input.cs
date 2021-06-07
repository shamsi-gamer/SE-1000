using System.Text;


namespace IngameScript
{
    partial class Program
    {
        static bool g_inputValid = True;


        static void UpdateInstName()
        {
            if (   g_inputValid
                &&  OK(EditedClip.SelChan)
                && !OK(EditedClip.CurSrc))
                EditedClip.CurInstrument.Name = dspMain.Panel.GetText().Trim().Trim(';');

            g_inputValid = True;
        }


        static void SetInstName(bool add = True)
        {
            if (    OK(EditedClip.SelChan)
                &&  OK(EditedClip.CurPat)
                && !OK(EditedClip.CurSrc)
                && !OK(EditedClip.CurSet)
                &&  OK(EditedClip.SelChan))
                dspMain.Panel.WriteText(add ? EditedClip.SelChannel.Instrument.Name : "", False);
        }


        static void UpdateClipName()
        {
            EditedClip.Name = dspInfo.Panel.GetText().Trim();
        }


        static void UpdateClipDisplay()
        {
            dspInfo.Panel.WriteText(EditedClip.Name.Replace("\u0085", "\n"));
        }
    }
}
