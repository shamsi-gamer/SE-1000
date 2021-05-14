using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public bool g_inputValid = T;


        void UpdateInst()
        {
            if (   g_inputValid
                && SelChan > -1
                && CurSrc < 0)
            {
                var sb = new StringBuilder();
                dspMain.Panel.ReadText(sb, F);

                CurClip.CurInstrument.Name = S(sb).Trim().Trim(new char[] {';'});
            }

            g_inputValid = T;
        }


        void UpdateSongName()
        {
            var sb = new StringBuilder();
            dspInfo.Panel.ReadText(sb, F);

            CurClip.Name = S(sb).Trim();
        }


        void UpdateClipDsp()
        {
            dspInfo.Panel.WriteText(CurClip.Name.Replace("\u0085", "\n"));
        }
    }
}
