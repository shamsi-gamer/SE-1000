using System;
using System.Text;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void SaveSongExt()
        {
            SaveClips();

            dspIO.Panel.WriteText(lblNext.CustomData);

            g_infoPressed.Add(1);
        }


        public void Save()
        {
            SaveMachineState();
            SaveInstruments();
            SaveClips();
        }


        void SaveMachineState()
        {
            lblMove.CustomData = N(SaveConfig(SaveToggles()));
        }


        uint SaveToggles()
        {
            uint f = 0;
            var  i = 0;

            WriteBit(ref f, g_session, i++);
            WriteBit(ref f, g_move,    i++);

            return f;
        }


        string SaveConfig(uint f)
        {
            return
                   S(f)              
                + PS(g_ticksPerStep);
                 
                //+   (ModDestConnecting != null ? ModDestConnecting.GetPath(ModDestSrcIndex) : "")
                //+ PS(ModDestSrcIndex)
                //+ PS(ModDestChannel != null ? Patterns.IndexOf(ModDestChannel.Pattern) : -1)
                //+ PS(ModDestChannel != null ? ModDestChannel.Pattern.Channels.IndexOf(ModDestChannel) : -1)
                //+ PS(ModCurPat)
                //+ PS(ModDestClip)
        }


        static string Save(Setting setting)
        {
            return 
                setting != null 
                ? P(setting.Save())
                : "";
        }


        void SaveInstruments()
        {
            var sbInst = new StringBuilder();

            for (int i = 0; i < g_inst.Count; i++)
            { 
                sbInst.Append(g_inst[i].Save());

                if (i < g_inst.Count-1)
                    sbInst.Append("\n");
            }

            lblPrev.CustomData = sbInst.ToString();
        }


        void SaveClips()
        {
            var clips = "";

            for (int t = 0; t < g_tracks.Length; t++)
            {
                var track = g_tracks[t];

                var strTrack = S(track.Clips.Count);

                foreach (var i in track.Indices)
                    strTrack += PS(i);

                clips += N(strTrack);

                foreach (var clip in track.Clips)
                    clips += PN(clip.Save());
            }

            lblNext.CustomData = clips;
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


        static void add(List<byte> b, short s) { b.AddArray(BitConverter.GetBytes(s)); }
        static void add(List<byte> b, int   i) { b.AddArray(BitConverter.GetBytes(i)); }
        static void add(List<byte> b, float f) { b.AddArray(BitConverter.GetBytes(f)); }
    }
}
