using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public bool g_inputValid = true;


        void UpdateInst()
        {
            if (   g_inputValid
                && g_session.CurClip.SelChan > -1
                && g_session.CurClip.CurSrc < 0)
            {
                var sb = new StringBuilder();
                dspMain.Panel.ReadText(sb, false);

                g_session.CurClip.CurrentInstrument.Name = S(sb).Trim().Trim(new char[] {';'});
            }

            g_inputValid = true;
        }


        void UpdateSongName()
        {
            var sb = new StringBuilder();
            dspInfo.Panel.ReadText(sb, false);

            g_session.CurClip.Name = S(sb).Trim();
        }


        void UpdateClipDsp()
        {
            dspInfo.Panel.WriteText(g_session.CurClip.Name.Replace("\u0085", "\n"));
        }
    }
}
