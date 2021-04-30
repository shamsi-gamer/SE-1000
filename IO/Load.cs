using System.Text;


namespace IngameScript
{
    partial class Program
    {
        void Load()
        {
            Stop();


            //string curPath;
            //       modConnPath;
            //int    modPat,
            //       modChan;

            LoadMachineState();
            //LoadInstruments();
            //LoadClips(out curPath);

            g_session = Session.Load();

            //if (curPath != "")
            //    SwitchToSetting(g_session.CurClip.CurrentInstrument, g_session.CurClip.CurSrc, curPath);

            //if (modConnPath != "")
            //{
            //    ModDestChannel    = g_session.CurClip.Patterns[modPat].Channels[modChan];
            //    ModDestConnecting = (Modulate)GetSettingFromPath(ModDestChannel.Instrument, modConnPath);
            //}

            InitPlaybackAfterLoad(g_session.CurClip.PlayTime);

            SetLabelColor(g_session.CurClip.ColorIndex);
            //UpdateLabels();
        }


        void LoadMachineState()
        {
            var sb = new StringBuilder();
            pnlStorageState.ReadText(sb);
            var state = sb.ToString();

            var lines = state.Split('\n');
            var line  = 0;

            var cfg = lines[line++].Split(';');
            if (!LoadToggles(cfg[0])) goto NothingLoaded;
            //if (!LoadConfig(cfg))     goto NothingLoaded;

            return;


        NothingLoaded:
            SetDefaultMachineState();
        }


        bool LoadToggles(string toggles)
        {
            uint f;
            if (!uint.TryParse(toggles, out f)) return false;

            var i = 0;

            g_showSession = ReadBit(f, i++);
            g_move        = ReadBit(f, i++);

            return true;
        }


        void LoadSongExt()
        {
            //var song = new StringBuilder();
            //dspIO.Surface.ReadText(song, F);

            //if (!LoadSong(S(song)))
            //    NewSong();

            g_infoPressed.Add(0);
        }


        //bool Finalize(List<Instrument> loadedInst)
        //{
        //    foreach (var l in loadedInst)
        //    {
        //        var f = g_session.Instruments.FindIndex(i => i.Name == l.Name);

        //        if (f > -1) g_session.Instruments[f] = l;
        //        else g_session.Instruments.Add(l);
        //    }

        //    return true;
        //}


        //bool LoadEdit(string[] lines, ref int line)
        //{
        //    g_song.EditNotes.Clear();

        //    var cfg = lines[line++].Split(';');
        //    if (cfg.Length == 0) return false;

        //    var c = 0;

        //    int nNotes;
        //    if (!int.TryParse(cfg[c++], out nNotes)) return false;

        //    for (int i = 0; i < nNotes; i++)
        //    {
        //        int p, ch, n;
        //        if (!int.TryParse(cfg[c++], out p )) return false;
        //        if (!int.TryParse(cfg[c++], out ch)) return false;
        //        if (!int.TryParse(cfg[c++], out n )) return false;

        //        g_song.EditNotes.Add(g_song.Patterns[p].Channels[ch].Notes[n]);
        //    }

        //    return true;
        //}
    }
}
