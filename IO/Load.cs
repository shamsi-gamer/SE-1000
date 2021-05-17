﻿using System.Text;


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

            int curClipTrack, curClipIndex;
            g_session = Session.Load(out curClipTrack, out curClipIndex);

            if (OK(g_session))
            {
                g_setClip = T;
                g_session.Tracks[curClipTrack].SetClip(curClipIndex);
                InitPlaybackAfterLoad(CurClip.Track.PlayTime);
            }
            else
                g_session = new Session();


            //if (curPath != "")
            //    SwitchToSetting(CurClip.CurrentInstrument, CurSrc, curPath);

            //if (modConnPath != "")
            //{
            //    ModDestChannel    = CurClip.Patterns[modPat].Channels[modChan];
            //    ModDestConnecting = (Modulate)GetSettingFromPath(ModDestChannel.Instrument, modConnPath);
            //}

            SetLabelColor(CurClip.ColorIndex);
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
            if (c < cfg.Length && !LoadToggles(cfg[c++]))                  goto NothingLoaded;

            return;

        NothingLoaded:
            SetDefaultMachineState();
        }


        bool LoadToggles(string toggles)
        {
            uint f;
            if (!uint.TryParse(toggles, out f)) return F;

            var i = 0;

            g_showSession = ReadBit(f, i++);
            g_move        = ReadBit(f, i++);

            return T;
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
