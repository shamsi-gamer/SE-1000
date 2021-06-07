using System;
using System.Text;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void SaveSongExt()
        {
            Save();

            g_ioAction = 1; // save
            g_ioState  = 0; // instruments
            g_ioPos    = 0;

            g_ioString = "";

            g_lcdPressed.Add(lcdInfo+1);
        }


        void UpdateSave()
        {
            if (g_ioState == 0) // instruments
            {
                if (g_ioPos < Instruments.Count) // save next instrument
                {
                    if (g_ioPos > 0) g_ioString += "\n";
                    g_ioString += Instruments[g_ioPos++].Save();
                }
                else // end of instruments
                {
                    pnlStorageInstruments.WriteText(g_ioString);

                    g_ioState  = 1; // tracks
                    g_ioPos    = 0; // clip

                    g_ioString = "";
                }
            }
            else if (g_ioState == 1) // tracks
            {
                if (g_ioPos < Tracks.Count)
                { 
                    if (g_ioPos > 0) g_ioString += "\n";
                    g_ioString += Tracks[g_ioPos++].Save();
                }
                else // end of tracks
                {
                    pnlStorageTracks.WriteText(g_ioString);
                    g_ioState = 2;
                }
            }
            else if (g_ioState == 2) // save external
            { 
                dspIO.Panel.WriteText(
                         pnlStorageState.GetText()
                    + PN("%%%")
                    + PN(pnlStorageInstruments.GetText())
                    + PN("%%%")
                    + PN(pnlStorageTracks.GetText()));
                
                ResetIO();
            }
        }


        void Save()
        {
            SaveMachineState();
        }


        void SaveMachineState()
        {
            pnlStorageState.WriteText(
                  SessionName
                + PS(SaveStateToggles())
                + PS(TicksPerStep)
                + PS(LockView)
                + PS(EditClip)
                + PS(OK(EditedClip) ? Tracks.IndexOf(EditedClip.Track)           : -1)
                + PS(OK(EditedClip) ? EditedClip.Track.Clips.IndexOf(EditedClip) : -1)
                + PS(OK(ClipCopy)   ? Tracks.IndexOf(ClipCopy.Track)             : -1)
                + PS(OK(ClipCopy)   ? ClipCopy.Track.Clips.IndexOf(ClipCopy)     : -1));

            //+   (OK(ModDestConnecting) ? ModDestConnecting.GetPath(ModDestSrcIndex) : "")
            //+ PS(ModDestSrcIndex)
            //+ PS(OK(ModDestChannel) ? Patterns.IndexOf(ModDestChannel.Pattern) : -1)
            //+ PS(OK(ModDestChannel) ? ModDestChannel.Pattern.Channels.IndexOf(ModDestChannel) : -1)
            //+ PS(ModCurPat)
            //+ PS(ModDestClip)
        }


        uint SaveStateToggles()
        {
            uint f = 0;
            var  i = 0;

            WriteBit(ref f, ShowClip,  i++);
            WriteBit(ref f, ShowMixer, i++);
            WriteBit(ref f, CueClip,   i++);

            return f;
        }


        //void SaveInstruments()
        //{
        //    var inst = "";

        //    for (int i = 0; i < Instruments.Count; i++)
        //    { 
        //        if (i > 0) inst += "\n";
        //        inst += Instruments[i].Save();
        //    }

        //    pnlStorageInstruments.WriteText(inst);
        //}


        //void SaveTracks()
        //{
        //    var tracks = "";

        //    for (int t = 0; t < Tracks.Count; t++)
        //    {
        //        if (t > 0) tracks += "\n";
        //        tracks += Tracks[t].Save();
        //    }

        //    pnlStorageTracks.WriteText(tracks);
        //}


        static string SaveSetting(Setting setting)
        {
            return
                OK(setting)
                ? P(setting.Save())
                : "";
        }
    }
}
