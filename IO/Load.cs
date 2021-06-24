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
            //SetLabelColor(EditedClip.ColorIndex);

            g_lcdPressed.Add(lcdInfo + 0);
        }



        void Load(string stateData, string instData, string trackData)
        {
            Stop();


            dspVol2.Panel.CustomData = instData;
            dspVol3.Panel.CustomData = trackData;

            if (!LoadMachineState(stateData, out g_editTrack, out g_editClip, out g_curPath, out g_copyTrack, out g_copyIndex))
                CreateDefaultMachineState(this);


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



        //void Load(string stateData, string instData, string trackData)
        //{
        //    Stop();


        //    //string modConnPath;
        //    //int    modPat,
        //    //       modChan;


        //    int editTrack,
        //        editClip,
        //        copyTrack,
        //        copyIndex;

        //    string curPath;


        //    if (!LoadMachineState(stateData, out editTrack, out editClip, out curPath, out copyTrack, out copyIndex))
        //        CreateDefaultMachineState();

        //    if (!LoadInstruments(instData))
        //        CreateDefaultInstruments();

        //    if (!LoadTracks(trackData))
        //        CreateDefaultTracks();


        //    if (OK(editTrack) && OK(editClip))
        //        EditedClip = Tracks[editTrack].Clips[editClip];

        //    if (!OK(EditedClip))
        //        SetAnyEditedClip();

            
        //    if (curPath != "")
        //        SwitchToSetting(EditedClip, EditedClip.SelChannel.Instrument, curPath);


        //    UpdateClipDisplay(EditedClip);

        //    SetLabelColor(EditedClip.ColorIndex);


        //    if (   OK(copyTrack) 
        //        && OK(copyIndex))
        //        ClipCopy = Tracks[copyTrack].Clips[copyIndex];

        //    //if (modConnPath != "")
        //    //{
        //    //    ModDestChannel    = EditClip.Patterns[modPat].Channels[modChan];
        //    //    ModDestConnecting = (Modulate)GetSettingFromPath(ModDestChannel.Instrument, modConnPath);
        //    //}
        //}



        void FinalizeLoad()
        {
            if (OK(g_editTrack) && OK(g_editClip))
                EditedClip = Tracks[g_editTrack].Clips[g_editClip];

            if (!OK(EditedClip))
                SetAnyEditedClip();

            
            if (g_curPath != "")
                SwitchToSetting(EditedClip, EditedClip.SelChannel.Instrument, g_curPath);


            UpdateClipDisplay(EditedClip);

            SetLabelColor(EditedClip.ColorIndex);


            if (   OK(g_copyTrack) 
                && OK(g_copyIndex))
                ClipCopy = Tracks[g_copyTrack].Clips[g_copyIndex];

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
                              out string curPath,
                              out int copyTrack, out int copyIndex)
        {
            ClearMachineState();

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



        //bool LoadInstruments(string data)
        //{
            //Instruments = new List<Instrument>();


            //var lines = data.Split('\n');
            //var line  = 0;


            //while (line < lines.Length)
            //{
            //    SkipWhiteSpace(lines, ref line);

            //    if (line < lines.Length)
            //        Instruments.Add(Instrument.Load(lines, ref line));
            //}

            
            //return Instruments.Count > 0;
        //}



         //public void ImportInstruments()
        //{
        //    LoadInstruments(pnlStorageInstruments.GetText());

        //    // set all instruments to first

        //    foreach (var track in Tracks)
        //    {
        //        foreach (var clip in track.Clips)
        //        { 
        //            if (!OK(clip))
        //                continue;

        //            int first, last;
        //            clip.GetCurPatterns(out first, out last);

        //            for (int p = first; p <= last; p++)
        //            {
        //                for (int ch = 0; ch < g_nChans; ch++)
        //                    clip.Patterns[p].Channels[ch].Instrument = Instruments[0];
        //            }
        //        }
        //    }
        //}



        //bool LoadTracks(string data)
        //{
        //    CreateTracks();

        //    var lines = data.Split('\n');
        //    var line  = 0;

        //    for (int t = 0; t < Tracks.Count; t++)
        //    {
        //        SkipWhiteSpace(lines, ref line);

        //        var track = Track.Load(lines, ref line);
        //        if (!OK(track)) return False;

        //        Tracks[t] = track;
        //    }

        //    if (!SessionHasClips)
        //    {
        //        Tracks[0].Clips[0] = Clip.Create(Tracks[0]);
        //        EditedClip = Tracks[0].Clips[0];
        //        UpdateClipDisplay(EditedClip);
        //    }

        //    return True;
        //}
    }
}
