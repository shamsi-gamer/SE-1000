using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public bool g_inputValid = True;


        void UpdateInst()
        {
            if (   g_inputValid
                && SelChan > -1
                && CurSrc < 0)
            {
                var sb = new StringBuilder();
                dspMain.Panel.ReadText(sb, False);

                EditedClip.CurInstrument.Name = S(sb).Trim().Trim(new char[] {';'});
            }

            g_inputValid = True;
        }


        void UpdateSongName()
        {
            var sb = new StringBuilder();
            dspInfo.Panel.ReadText(sb, False);

            EditedClip.Name = S(sb).Trim();
        }


        void UpdateClipDsp()
        {
            dspInfo.Panel.WriteText(EditedClip.Name.Replace("\u0085", "\n"));
        }
    }
}
