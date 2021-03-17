using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public bool inputValid = true;


        void UpdateInst()
        {
            if (   inputValid
                && SelChan > -1
                && CurSrc < 0)
            {
                var sb = new StringBuilder();
                dspMain.Surface.ReadText(sb, false);

                CurrentInstrument.Name = S(sb).Trim().Trim(new char[] {';'});
            }

            inputValid = true;
        }

        void UpdateSongName()
        {
            var sb = new StringBuilder();
            dspInfo.Surface.ReadText(sb, false);

            g_song.Name = S(sb).Trim();
        }

        void UpdateSongDsp()
        {
            dspInfo.Surface.WriteText(g_song.Name.Replace("\u0085", "\n"));
        }
    }
}
