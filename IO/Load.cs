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
                SetLabelColor(EditedClip.ColorIndex);
                return;
            }

            var ext = data.Split(new string[] {"%%%"}, StringSplitOptions.None);
            if (ext.Length != 3) return;

            Load(ext[0], ext[1], ext[2]);
            //SetLabelColor(EditedClip.ColorIndex);

            g_lcdPressed.Add(lcdInfo + 0);
        }



        void Load(string stateData, string instData, string trackData)
        {
            Stop();


            dspVol2.Panel.CustomData = instData;
            dspVol3.Panel.CustomData = trackData;

            if (!LoadState(stateData, out g_editTrack, out g_editClip, out g_curPath, out g_copyTrack, out g_copyIndex))
                CreateDefaultState(this);


            Instruments = new List<Instrument>();
            CreateTracks(this);


            g_ioAction   = 0; // load
            g_ioState    = 0; // instruments
            g_ioPos      = 0;

            g_instLines  = instData .Split('\n');
            g_trackLines = trackData.Split('\n');
        }



        void UpdateLoad()
        {
            if (g_ioState == 0) // instruments
            {
                if (g_ioPos < g_instLines.Length) // load next instrument
                {
                    SkipWhiteSpace(g_instLines, ref g_ioPos);

                    if (g_ioPos < g_instLines.Length)
                        Instruments.Add(Instrument.Load(g_instLines, ref g_ioPos, this));

                    if (Instruments.Count == 0)
                    { 
                        CreateDefaultInstruments(this);
                        CreateDefaultTracks(this);
                        g_ioState = -1;
                        FinalizeLoad();
                        return;
                    }

                    //if (g_ioPos > 0) g_ioString += "\n";
                    //g_ioString += Instruments[g_ioPos++].Save();
                }
                else // end of instruments
                {
                    //pnlStorageInstruments.WriteText(g_ioString);

                    //g_ioString = "";
                    g_ioState    = 1; // tracks
                    g_ioPos      = 0; // clip
                    g_trackIndex = 0;
                }
            }
            else if (g_ioState == 1) // tracks
            {
                if (g_ioPos < g_trackLines.Length) // load next track
                {
                    SkipWhiteSpace(g_trackLines, ref g_ioPos);

                    var track = Track.Load(g_trackLines, ref g_ioPos, this);
                    if (!OK(track))
                    {
                        CreateDefaultTracks(this);
                        ResetIO();
                        return; 
                    }

                    Tracks[g_trackIndex++] = track;
                }
                else // end of track
                {
                    if (!SessionHasClips)
                    {
                        Tracks[0].Clips[0] = Clip.Create(Tracks[0], this);
                        EditedClip = Tracks[0].Clips[0];
                        UpdateClipDisplay(EditedClip);
                    }

                    FinalizeLoad();
                    ResetIO();
                }
            }
        }



        void FinalizeLoad()
        {
            if (OK(g_editTrack) && OK(g_editClip))
                EditedClip = Tracks[g_editTrack].Clips[g_editClip];

            if (!OK(EditedClip))
                SetAnyEditedClip();
            
            if (g_curPath != "")
                SwitchToSetting(EditedClip, g_curPath);

            if (   OK(g_copyTrack) 
                && OK(g_copyIndex))
                ClipCopy = Tracks[g_copyTrack].Clips[g_copyIndex];

            FinalizeLoadModulate();

            UpdateClipDisplay(EditedClip);
            SetLabelColor(EditedClip.ColorIndex);
        }



        void FinalizeLoadModulate()
        {
            foreach (var mod in g_mod)
            {
                var inst = Instruments.Find(i => i.Name == mod.LoadInstName);

                mod.ModSettings.Add(
                    mod.LoadSetPath != "" 
                    ? GetSettingFromPath(mod.LoadSetPath) 
                    : Setting_null);

                mod.ModSources.Add(
                    OK(mod.LoadSrcIndex) 
                    ? inst.Sources[mod.LoadSrcIndex] 
                    : Source_null);

                mod.ModInstruments.Add(inst);

                mod.LoadSetPath  = "";
                mod.LoadInstName = "";
                mod.LoadSrcIndex = -1;
            }
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



        bool LoadState(string data,
                       out int editTrack, out int editIndex, 
                       out string curPath,
                       out int copyTrack, out int copyIndex)
        {
            ClearState();

            editTrack = editIndex =
            copyTrack = copyIndex = -1;

            curPath = "";


            if (!data.Contains(";"))
                return False;


            var state = data.Split(';');
            var s = 0;

            SessionName = state[s++];

            LoadStateToggles(state[s++]);


            if (   !int_TryParse(state[s++], out TicksPerStep)
                || !int_TryParse(state[s++], out LockView)
                || !int_TryParse(state[s++], out ShowMixer)
                || !int_TryParse(state[s++], out CueClip)
                || !int_TryParse(state[s++], out EditClip)

                || !int_TryParse(state[s++], out editTrack)
                || !int_TryParse(state[s++], out editIndex))
                return False;

            curPath = state[s++];


            if (   !int_TryParse(state[s++], out copyTrack)
                || !int_TryParse(state[s++], out copyIndex))
                return False;


            for (int i = 0; i < nMems; i++)
            {
                int clipTrack, clipIndex;

                if (   !int_TryParse(state[s++], out clipTrack)
                    || !int_TryParse(state[s++], out clipIndex))
                    return False;

                var path = state[s++];
                var inst = state[s++];

                if (OK(clipTrack) && OK(clipIndex))
                {
                    var _inst = Instruments.Find(_i => _i.Name == inst);

                    SetClips[i] = Tracks[clipTrack].Clips[clipIndex];
                    Sets    [i] = GetSettingFromPath(path);
                }
            }


            return True;
        }



        bool LoadStateToggles(string toggles)
        {
            uint f;
            if (!uint.TryParse(toggles, out f)) return False;

            var i = 0;

            ShowClip  = ReadBit(f, i++);
            Recording = ReadBit(f, i++);

            return True;
        }
    }
}
