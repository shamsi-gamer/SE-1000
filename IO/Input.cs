using System.Text;


namespace IngameScript
{
    partial class Program
    {
        static bool g_inputValid = True;


        static void UpdateInstName()
        {
            if (   g_inputValid
                &&  OK(SelChan)
                && !OK(CurSrc))
                EditedClip.CurInstrument.Name = dspMain.Panel.GetText().Trim();

            g_inputValid = True;
        }


        static void SetInstName(bool add = True)
        {
            if (    OK(SelChan)
                &&  OK(CurPat)
                && !OK(CurSrc)
                && !OK(CurSet)
                &&  OK(SelChan))
                dspMain.Panel.WriteText(add ? SelChannel.Instrument.Name : "");
        }


        static void UpdateClipName()
        {
            EditedClip.Name = dspInfo.Panel.GetText().Trim();
        }


        static void UpdateClipDisplay(Clip clip)
        {
            dspInfo.Panel.WriteText((ShowClip ? clip.Name : SessionName).Replace("\u0085", "\n"));
        }
    }
}
