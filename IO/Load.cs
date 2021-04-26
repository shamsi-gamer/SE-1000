using System;
using System.Collections.Generic;
using System.Text;


namespace IngameScript
{
    partial class Program
    {
        void Load()
        {
            string curPath, 
                   modConnPath;
            int    modPat,
                   modChan;

            LoadMachineState();
            LoadInstruments();
            LoadSong(out curPath, out modConnPath, out modPat, out modChan);

            if (curPath != "")
                SwitchToSetting(g_clip.CurrentInstrument, g_clip.CurSrc, curPath);

            if (modConnPath != "")
            {
                ModDestChannel    = g_clip.Patterns[modPat].Channels[modChan];
                ModDestConnecting = (Modulate)GetSettingFromPath(ModDestChannel.Instrument, modConnPath);
            }
        }


        void LoadMachineState()
        {
            var state = lblMove.CustomData;

            var lines = state.Split('\n');
            var line  = 0;

            var cfg = lines[line++].Split(';');
            if (!LoadToggles(cfg[0])) goto NothingLoaded;
            if (!LoadConfig(cfg))     goto NothingLoaded;

            return;


        NothingLoaded:
            SetDefaultMachineState();
        }


        bool LoadToggles(string toggles)
        {
            uint f;
            if (!uint.TryParse(toggles, out f)) return false;

            var i = 0;

            g_session = ReadBit(f, i++);
            g_move    = ReadBit(f, i++);

            return true;
        }


        bool LoadConfig(string[] cfg)
        {
            int c = 1; // 0 holds the toggles, loaded in LoadToggles()

            if (!int.TryParse(cfg[c++], out g_ticksPerStep)) return false;
                                                             
            return true;
        }


        void LoadInstruments()
        {
            var lines = lblPrev.CustomData.Split('\n');
            var line = 0;

            g_inst.Clear();

            while (line < lines.Length)
            {
                while (line < lines.Length
                    && lines[line].Trim() == "") line++; // white space

                if (line < lines.Length)
                    g_inst.Add(Instrument.Load(lines, ref line));
            }

            
            if (g_inst.Count == 0) // nothing was loaded
                CreateDefaultInstruments();
        }


        void ImportInstruments()
        {
            LoadInstruments();

            // set all instruments to first
            
            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                for (int ch = 0; ch < g_nChans; ch++)
                    g_clip.Patterns[p].Channels[ch].Instrument = g_inst[0]; 
            }
        }


        void CreateDefaultInstruments()
        {
            if (g_inst.Count == 0)
            {
                g_inst.Add(new Instrument());
                g_inst[0].Sources.Add(new Source(g_inst[0]));
            }
        }


        void LoadSongExt()
        {
            //var song = new StringBuilder();
            //dspIO.Surface.ReadText(song, F);

            //if (!LoadSong(S(song)))
            //    NewSong();

            g_infoPressed.Add(0);
        }


        void LoadSong(out string curPath, out string modConnPath, out int modPat, out int modChan)
        {
            Stop();
            ClearSong();

            var lines = lblNext.CustomData.Split('\n');
            var line  = 0;

            g_clip = Clip.Load(
                lines, 
                ref line, 
                out curPath, 
                out modConnPath, 
                out modPat, 
                out modChan);

            if (g_clip == null)
                CreateDefaultClip();

            InitPlaybackAfterLoad(g_clip.PlayTime);
        }


        //bool Finalize(List<Instrument> loadedInst)
        //{
        //    foreach (var l in loadedInst)
        //    {
        //        var f = g_inst.FindIndex(i => i.Name == l.Name);

        //        if (f > -1) g_inst[f] = l;
        //        else g_inst.Add(l);
        //    }

        //    return true;
        //}


        void CreateDefaultClip()
        {
            g_clip = new Clip();
            g_clip.Patterns.Add(new Pattern(g_clip, g_inst[0]));
            g_clip.Name = "New Clip";
        }


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
