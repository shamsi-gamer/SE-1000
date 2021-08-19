using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        static bool g_inputValid = True;



        static void UpdateInstName()
        {
            if (   !g_inputValid
                || !OK(SelChan)
                ||  OK(CurSrc))
            { 
                g_inputValid = True;
                return;
            }

            var inst      = Instruments[SelChan];
            var inputName = dspMain.Panel.GetText().Trim();

            var name = GetNewName(
                inputName, 
                n => Instruments.Count(i => i.Name == n) > 1);

            if (inputName != name)
                dspMain.Panel.WriteText(name);

            EditedClip.CurInstrument.Name = name;

            g_inputValid = True;
        }



        static void SetInstName(bool add = True)
        {
            if (    OK(SelChan)
                &&  OK(EditPat)
                &&  OK(SelChan)
                && !OK(CurSrc)
                && !OK(CurSet))
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
