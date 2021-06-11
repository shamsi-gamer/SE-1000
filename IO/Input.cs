using System.Text;


namespace IngameScript
{
    partial class Program
    {
        static bool g_inputValid = True;


        static void UpdateInstName()
        {
            if (   !g_inputValid
                || !OK(SelChan)
                || OK(CurSrc))
                return;

            EditedClip.CurInstrument.Name = GetNewName(
                dspMain.Panel.GetText().Trim(), 
                str => Instruments.Exists(i => i.Name == str));

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
            var name = dspInfo.Panel.GetText().Trim();

            if (ShowClip) EditedClip.Name = name;
            else          SessionName     = name;
        }


        static void UpdateClipDisplay(Clip clip)
        {
            dspInfo.Panel.WriteText((ShowClip ? clip.Name : SessionName).Replace("\u0085", "\n"));
        }
    }
}
