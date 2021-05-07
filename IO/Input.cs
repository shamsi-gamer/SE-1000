using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public bool g_inputValid = true;


        void UpdateInst()
        {
            if (   g_inputValid
                && CurClip.SelChan > -1
                && CurClip.CurSrc < 0)
            {
                var sb = new StringBuilder();
                dspMain.Panel.ReadText(sb, false);

                CurClip.CurrentInstrument.Name = S(sb).Trim().Trim(new char[] {';'});
            }

            g_inputValid = true;
        }


        void UpdateSongName()
        {
            var sb = new StringBuilder();
            dspInfo.Panel.ReadText(sb, false);

            CurClip.Name = S(sb).Trim();
        }


        void UpdateClipDsp()
        {
            dspInfo.Panel.WriteText(CurClip.Name.Replace("\u0085", "\n"));
        }
    }
}
