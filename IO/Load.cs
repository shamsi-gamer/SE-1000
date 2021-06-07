using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void LoadSongExt()
        {
            var data = dspIO.Panel.GetText();

            if (data.Trim() == "")
            { 
                Load("", pnlStorageInstruments.GetText(), "");
                return;
            }


            var ext = data.Split(new string[] {"%%%"}, StringSplitOptions.None);
            if (ext.Length != 3) return;

            Load(ext[0], ext[1], ext[2]);
            SetLabelColor(EditedClip.ColorIndex);

            g_lcdPressed.Add(lcdInfo + 0);
        }


        void Load(string stateData, string instData, string trackData)
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

            if (!LoadMachineState(stateData, out editTrack, out editIndex, out copyTrack, out copyIndex))
                CreateDefaultMachineState();

            if (!LoadInstruments(instData))
                CreateDefaultInstruments();

            if (!LoadTracks(trackData))
                CreateDefaultTracks();


            if (!OK(EditedClip))
                EditedClip = Tracks[editTrack].Clips[editIndex];

            if (!OK(EditedClip))
                SetAnyEditedClip();

            UpdateClipDisplay();

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
        }


        void SetAnyEditedClip()
        {
            foreach (var track in Tracks)
                foreach (var clip in track.Clips)
                    if (OK(clip))
                    {
                        EditedClip = clip;
                        return;
                    }
        }


        bool LoadMachineState(string data,
                              out int editTrack, out int editIndex, 
                              out int copyTrack, out int copyIndex)
        {
            ClearMachineState();


            editTrack = editIndex =
            copyTrack = copyIndex = -1;

            if (!data.Contains(";"))
                return False;

            var state = data.Split(';');
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

            ShowClip  = ReadBit(f, i++);
            ShowMixer = ReadBit(f, i++);
            CueClip   = ReadBit(f, i++);

            return True;
        }


        bool LoadInstruments(string data)
        {
            Instruments = new List<Instrument>();


            var lines = data.Split('\n');
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
            LoadInstruments(pnlStorageInstruments.GetText());

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


        bool LoadTracks(string data)
        {
            CreateTracks();

            var lines = data.Split('\n');
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
    }
}
