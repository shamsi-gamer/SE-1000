using System.Text;


namespace IngameScript
{
    partial class Program
    {
        void Load()
        {
            Play(F);


            //string curPath;
            //       modConnPath;
            //int    modPat,
            //       modChan;

            LoadMachineState();
            //LoadInstruments();
            //LoadClips(out curPath);


            int curClipTrack, 
                curClipIndex, 
                copyClipTrack, 
                copyClipIndex;

            g_session = Session.Load(
                out curClipTrack, 
                out curClipIndex, 
                out copyClipTrack, 
                out copyClipIndex);

            
            if (OK(g_session))
            {
                //g_session.Tracks[curClipTrack].SetClip(curClscipIndex, T);
                //InitPlaybackAfterLoad(EditClip.Track.PlayTime);
            }
            else
            { 
                g_session = new Session();
                //g_session.Tracks[0].SetClip(0, T);
            }


            //if (curPath != "")
            //    SwitchToSetting(EditClip.CurrentInstrument, CurSrc, curPath);

            //if (modConnPath != "")
            //{
            //    ModDestChannel    = EditClip.Patterns[modPat].Channels[modChan];
            //    ModDestConnecting = (Modulate)GetSettingFromPath(ModDestChannel.Instrument, modConnPath);
            //}

            SetLabelColor(EditedClip.ColorIndex);
        }


        void LoadMachineState()
        {
            var sb = new StringBuilder();
            pnlStorageState.ReadText(sb);
            var state = sb.ToString();

            var lines = state.Split('\n');
            var line  = 0;

            var cfg = lines[line++].Split(';');

            var c = 0;

            if (c < cfg.Length && !int.TryParse(cfg[c++], out g_lockView)) goto NothingLoaded;

            return;

        NothingLoaded:
            SetDefaultMachineState();
        }


        void LoadSongExt()
        {
            //var clip = new StringBuilder();
            //dspIO.Surface.ReadText(clip, F);

            //if (!LoadSong(S(clip)))
            //    NewSong();

            g_lcdPressed.Add(lcdInfo+0);
        }


        //bool Finalize(List<Instrument> loadedInst)
        //{
        //    foreach (var l in loadedInst)
        //    {
        //        var f = g_session.Instruments.FindIndex(i => i.Name == l.Name);

        //        if (f > -1) g_session.Instruments[f] = l;
        //        else g_session.Instruments.Add(l);
        //    }

        //    return T;
        //}


        //bool LoadEdit(string[] lines, ref int line)
        //{
        //    g_song.EditNotes.Clear();

        //    var cfg = lines[line++].Split(';');
        //    if (cfg.Length == 0) return F;

        //    var c = 0;

        //    int nNotes;
        //    if (!int.TryParse(cfg[c++], out nNotes)) return F;

        //    for (int i = 0; i < nNotes; i++)
        //    {
        //        int p, ch, n;
        //        if (!int.TryParse(cfg[c++], out p )) return F;
        //        if (!int.TryParse(cfg[c++], out ch)) return F;
        //        if (!int.TryParse(cfg[c++], out n )) return F;

        //        g_song.EditNotes.Add(g_song.Patterns[p].Channels[ch].Notes[n]);
        //    }

        //    return T;
        //}
    }
}
