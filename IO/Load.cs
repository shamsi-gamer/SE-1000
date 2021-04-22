using System;
using System.Collections.Generic;
using System.Text;


namespace IngameScript
{
    partial class Program
    {
        void Load()
        {
            string curPath, modConnPath;
            int    modPat, modChan;

            LoadMachineState(out curPath, out modConnPath, out modPat, out modChan);
            LoadInstruments();
            LoadSong();

            if (curPath != "")
                SwitchToSetting(curPath, CurrentInstrument);

            if (modConnPath != "")
            {
                ModDestChannel    = g_song.Patterns[modPat].Channels[modChan];
                ModDestConnecting = (Modulate)GetSettingFromPath(ModDestChannel.Instrument, modConnPath);
            }
        }


        void LoadMachineState(out string curPath, out string modConnPath, out int modPat, out int modChan)
        {
            var state = lblMove.CustomData;

            var lines = state.Split('\n');
            var line  = 0;

            curPath     = "";
            modConnPath = "";
            modPat      = -1;
            modChan     = -1;

            var cfg = lines[line++].Split(';');
            if (!LoadToggles(cfg[0]))                                                      goto NothingLoaded;
            
            if (!LoadSettings(cfg, out curPath, out modConnPath, out modPat, out modChan)) goto NothingLoaded;

            if (!LoadMems  (lines[line++]))                                                goto NothingLoaded;
            if (!LoadChords(lines[line++]))                                                goto NothingLoaded;

            return;


        NothingLoaded:
            SetDefaultMachineState();
        }


        bool LoadToggles(string toggles)
        {
            uint f;
            if (!uint.TryParse(toggles, out f)) return false;

            var i = 0;

            g_movePat    = ReadBytes(f, i++);

            g_in         = ReadBytes(f, i++);
            g_out        = ReadBytes(f, i++);
            
            g_loop       = ReadBytes(f, i++);
            g_block      = ReadBytes(f, i++);
            g_allPats    = ReadBytes(f, i++);
            g_follow     = ReadBytes(f, i++);
            g_autoCue    = ReadBytes(f, i++);
            
            g_allChan    = ReadBytes(f, i++);
            g_rndInst    = ReadBytes(f, i++);
            
            g_piano      = ReadBytes(f, i++);
            g_move       = ReadBytes(f, i++);
            
            g_transpose  = ReadBytes(f, i++);
            g_spread     = ReadBytes(f, i++);

            g_shift      = ReadBytes(f, i++);
            g_mixerShift = ReadBytes(f, i++);
            
            g_hold       = ReadBytes(f, i++);
            g_pick       = ReadBytes(f, i++);

            g_chordMode  = ReadBytes(f, i++);
            g_chordEdit  = ReadBytes(f, i++);
            g_chordAll   = ReadBytes(f, i++);

            g_halfSharp  = ReadBytes(f, i++);

            g_paramKeys  = ReadBytes(f, i++);
            g_paramAuto  = ReadBytes(f, i++);
                         
            g_setMem     = ReadBytes(f, i++);

            return true;
        }


        bool LoadSettings(string[] cfg, out string curPath, out string modConnPath, out int modPat, out int modChan)
        {
            curPath     = "";
            modConnPath = "";
            modPat      = -1;
            modChan     = -1;

            int c = 1; // 0 holds the toggles, loaded in LoadToggles()

            if (!int  .TryParse(cfg[c++], out g_ticksPerStep )) return false;
                                                             
            if (!int  .TryParse(cfg[c++], out CurPat         )) return false;
            if (!int  .TryParse(cfg[c++], out CurChan        )) return false;
                                                             
            if (!int  .TryParse(cfg[c++], out SelChan        )) return false;
            if (!int  .TryParse(cfg[c++], out CurSrc         )) return false;
                                                             
            curPath = cfg[c++];                              
                                                             
            if (!int  .TryParse(cfg[c++], out g_editStep     )) return false;
            if (!int  .TryParse(cfg[c++], out g_editLength   )) return false;
                                                             
            if (!int  .TryParse(cfg[c++], out g_curNote      )) return false;
                                                             
            if (!int  .TryParse(cfg[c++], out g_chord        )) return false;
            if (!int  .TryParse(cfg[c++], out g_chordSpread  )) return false;
                                                             
            if (!int  .TryParse(cfg[c++], out g_songOff      )) return false;
            if (!int  .TryParse(cfg[c++], out g_instOff      )) return false;
            if (!int  .TryParse(cfg[c++], out g_srcOff       )) return false;
                                                             
            if (!int  .TryParse(cfg[c++], out g_solo         )) return false;
                                                             
            if (!float.TryParse(cfg[c++], out g_volume       )) return false;

            modConnPath = cfg[c++];

            if (!int  .TryParse(cfg[c++], out ModCurChan     )) return false;
            if (!int  .TryParse(cfg[c++], out ModDestSrcIndex)) return false;
            if (!int  .TryParse(cfg[c++], out modPat         )) return false;
            if (!int  .TryParse(cfg[c++], out modChan        )) return false;
                                                             
            if (!int  .TryParse(cfg[c++], out g_iCol         )) return false;

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
            GetPatterns(g_song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
            { 
                for (int ch = 0; ch < g_nChans; ch++)
                    g_song.Patterns[p].Channels[ch].Instrument = g_inst[0]; 
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


        bool LoadMems(string line)
        {
            var mems = line.Split(';');

            for (int m = 0; m < nMems; m++)
                if (!int.TryParse(mems[m], out g_mem[m])) return false;

            return true;
        }


        bool LoadChords(string strChords)
        {
            g_chords = new List<int>[4];

            for (int _c = 0; _c < g_chords.Length; _c++)
                g_chords[_c] = new List<int>();


            var chords = strChords.Split(';');

            for (int _c = 0; _c < chords.Length; _c++)
            { 
                var _keys = chords[_c].Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

                g_chords[_c] = new List<int>();

                int key;
                foreach (var k in _keys)
                {
                    if (!int.TryParse(k, out key)) return false;
                    g_chords[_c].Add(key);
                }
            }


            return true;
        }


        void LoadSongExt()
        {
            //var song = new StringBuilder();
            //dspIO.Surface.ReadText(song, F);

            //if (!LoadSong(S(song)))
            //    NewSong();

            infoPressed.Add(0);
        }


        void LoadSong()
        {
            Stop();
            ClearSong();

            var lines = lblNext.CustomData.Split('\n');
            var line  = 0;

            g_song = Song.Load(lines, ref line);

            if (g_song == null)
                CreateDefaultSong();

            InitPlaybackAfterLoad(g_song.PlayTime);
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


        void CreateDefaultSong()
        {
            g_song = new Song();
            g_song.Patterns.Add(new Pattern(g_song, g_inst[0]));
            g_song.Name = "New Song";
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
