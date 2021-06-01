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

            dspIO.Panel.WriteText(
                  pnlStorageState      .GetText()
                + pnlStorageInstruments.GetText()
                + pnlStorageTracks     .GetText());

            g_lcdPressed.Add(lcdInfo+1);
        }


        void Save()
        {
            SaveMachineState();
            SaveInstruments();
            SaveTracks();
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
            WriteBit(ref f, Move,      i++);

            return f;
        }


        void SaveInstruments()
        {
            var inst = "";

            for (int i = 0; i < Instruments.Count; i++)
            { 
                if (i > 0) inst += "\n";
                inst += Instruments[i].Save();
            }

            pnlStorageInstruments.WriteText(inst);
        }


        void SaveTracks()
        {
            var tracks = "";

            for (int t = 0; t < Tracks.Count; t++)
            {
                if (t > 0) tracks += "\n";
                tracks += Tracks[t].Save();
            }

            pnlStorageTracks.WriteText(tracks);
        }


        static string SaveSetting(Setting setting)
        {
            return
                OK(setting)
                ? P(setting.Save())
                : "";
        }


        //String SaveEdit()
        //{
        //    var str = "";
        //    var s   = ";";

        //    str += S(g_song.EditNotes.Count);

        //    foreach (var n in g_song.EditNotes)
        //    {
        //        str +=
        //          s + S(g_song.Patterns.FindIndex(p => p.Channels.Contains(n.Channel)))
        //        + s + S(n.iChan)
        //        + s + S(n.Channel.Notes.IndexOf(n));
        //    }

        //    str += "\n";

        //    return str;
        //}


        //static void Add(List<byte> b, short s) { b.AddArray(BitConverter.GetBytes(s)); }
        //static void Add(List<byte> b, int   i) { b.AddArray(BitConverter.GetBytes(i)); }
        //static void Add(List<byte> b, float f) { b.AddArray(BitConverter.GetBytes(f)); }
    }
}
