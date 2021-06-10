using System;
using System.Text;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void SaveSongExt()
        {
            SaveMachineState();

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

                    g_ioString = "";
                    g_ioState  = 1; // tracks
                    g_ioPos    = 0; // clip
                }
            }
            else if (g_ioState == 1) // tracks
            {
                if (g_ioPos < Tracks.Count)
                { 
                    if (g_ioPos > 0) g_ioString += "\n";
                    g_ioString += Tracks[g_ioPos].Save();
                    g_ioPos++;
                }
                else // end of tracks
                {
                    pnlStorageTracks.WriteText(g_ioString);

                    g_ioString = "";
                    g_ioState  = 2;
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
            var state =
                  SessionName.Replace("\n", "\u0085")

                + PS(SaveStateToggles())
                
                + PS(TicksPerStep)
                + PS(LockView)
                + PS(EditClip)

                + PS(Tracks.IndexOf(EditedClip.Track))
                + PS(EditedClip.Index)

                + P (OK(EditedClip.CurSetting) ? EditedClip.CurSetting.GetPath(EditedClip.CurSrc) : "")

                + PS(OK(ClipCopy) ? Tracks.IndexOf(ClipCopy.Track) : -1)
                + PS(OK(ClipCopy) ? ClipCopy.Index                 : -1);

            for (int i = 0; i < nMems; i++)
            {
                if (OK(Sets[i]))
                { 
                    var clip = SetClips[i];
                    var set  = Sets[i];
                    var src  = set.Source;
                    var inst = src.Instrument;

                    state += 
                          PS(Tracks.IndexOf(clip.Track))
                        + PS(clip.Index)
                        + P (OK(src) && OK(set) ? set .GetPath(src.Index) : "")
                        + P (OK(inst)           ? inst.Name               : "");
                }
                else
                {
                    state += ";-1;-1;;";
                }
            }

            //+   (OK(ModDestConnecting) ? ModDestConnecting.GetPath(ModDestSrcIndex) : "")
            //+ PS(ModDestSrcIndex)
            //+ PS(OK(ModDestChannel) ? Patterns.IndexOf(ModDestChannel.Pattern) : -1)
            //+ PS(OK(ModDestChannel) ? ModDestChannel.Pattern.Channels.IndexOf(ModDestChannel) : -1)
            //+ PS(ModCurPat)
            //+ PS(ModDestClip)

            pnlStorageState.WriteText(state);
        }



        uint SaveStateToggles()
        {
            uint f = 0;
            var  i = 0;

            WriteBit(ref f, ShowClip,  i++);
            WriteBit(ref f, ShowMixer, i++);
            WriteBit(ref f, CueClip,   i++);
            WriteBit(ref f, Recording, i++);

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
