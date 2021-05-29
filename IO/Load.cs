using System.Text;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void LoadSongExt()
        {
            //var clip = new StringBuilder();
            //dspIO.Surface.ReadText(clip, F);

            //if (!LoadSong(S(clip)))
            //    NewSong();

            g_lcdPressed.Add(lcdInfo + 0);
        }


        void Load()
        {
            Stop();


            //string curPath;
            //       modConnPath;
            //int    modPat,
            //       modChan;


            int editTrack,
                editIndex,
                copyTrack,
                copyIndex;

            if (!LoadMachineState(out editTrack, out editIndex, out copyTrack, out copyIndex)) 
                                    CreateDefaultMachineState();
            if (!LoadInstruments()) CreateDefaultInstruments();
            if (!LoadTracks     ()) CreateDefaultTracks();


            if (!OK(EditedClip))
            { 
                EditedClip = Tracks[editTrack].Clips[editIndex];
                UpdateClipDisplay();
            }

            
            if (   OK(copyTrack) 
                && OK(copyIndex)) 
                ClipCopy = Tracks[copyTrack].Clips[copyIndex];


            SetLabelColor(EditedClip.ColorIndex);


            //if (curPath != "")
            //    SwitchToSetting(EditClip.CurrentInstrument, CurSrc, curPath);

            //if (modConnPath != "")
            //{
            //    ModDestChannel    = EditClip.Patterns[modPat].Channels[modChan];
            //    ModDestConnecting = (Modulate)GetSettingFromPath(ModDestChannel.Instrument, modConnPath);
            //}


            return;
        }


        bool LoadMachineState(out int editTrack, out int editIndex, 
                              out int copyTrack, out int copyIndex)
        {
            ClearMachineState();


            editTrack = editIndex =
            copyTrack = copyIndex = -1;

            if (!pnlStorageState.GetText().Contains(";"))
                return False;

            var state = pnlStorageState.GetText().Split(';');
            var s = 0;

            SessionName = state[s++];

            LoadToggles(state[s++]);

            return 
                   int_TryParse(state[s++], out TicksPerStep)
                && int_TryParse(state[s++], out LockView    )
                && int_TryParse(state[s++], out EditClip    )
                && int_TryParse(state[s++], out editTrack   )
                && int_TryParse(state[s++], out editIndex   )
                && int_TryParse(state[s++], out copyTrack   )
                && int_TryParse(state[s++], out copyIndex   );
        }


        bool LoadToggles(string toggles)
        {
            uint f;
            if (!uint.TryParse(toggles, out f)) return False;

            var i = 0;

            CueClip     = ReadBit(f, i++);
            ShowSession = ReadBit(f, i++);
            Move        = ReadBit(f, i++);

            return True;
        }


        bool LoadInstruments()
        {
            Instruments = new List<Instrument>();


            var sb = new StringBuilder();
            pnlStorageInstruments.ReadText(sb);

            var lines = sb.ToString().Split('\n');
            var line  = 0;


            while (line < lines.Length)
            {
                SkipWhiteSpace(lines, ref line);

                if (line < lines.Length)
                    Instruments.Add(Instrument.Load(lines, ref line));
            }

            
            return Instruments.Count > 0;
        }


        public void ImportInstruments()
        {
            LoadInstruments();

            // set all instruments to first

            foreach (var track in Tracks)
            {
                foreach (var clip in track.Clips)
                { 
                    if (!OK(clip))
                        continue;

                    int first, last;
                    clip.GetCurPatterns(out first, out last);

                    for (int p = first; p <= last; p++)
                    {
                        for (int ch = 0; ch < g_nChans; ch++)
                            clip.Patterns[p].Channels[ch].Instrument = Instruments[0];
                    }
                }
            }
        }


        bool LoadTracks()
        {
            CreateTracks();

            var lines = pnlStorageTracks.GetText().Split('\n');
            var line  = 0;

            for (int t = 0; t < Tracks.Count; t++)
            {
                SkipWhiteSpace(lines, ref line);

                var track = Track.Load(lines, ref line);
                if (!OK(track)) return False;

                Tracks[t] = track;
            }

            if (!SessionHasClips)
            {
                Tracks[0].Clips[0] = Clip.Create(Tracks[0]);
                EditedClip = Tracks[0].Clips[0];
                UpdateClipDisplay();
            }

            return True;
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
        //    if (!int_TryParse(cfg[c++], out nNotes)) return F;

        //    for (int i = 0; i < nNotes; i++)
        //    {
        //        int p, ch, n;
        //        if (!int_TryParse(cfg[c++], out p )) return F;
        //        if (!int_TryParse(cfg[c++], out ch)) return F;
        //        if (!int_TryParse(cfg[c++], out n )) return F;

        //        g_song.EditNotes.Add(g_song.Patterns[p].Channels[ch].Notes[n]);
        //    }

        //    return T;
        //}
    }
}
