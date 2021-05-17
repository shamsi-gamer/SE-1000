using System;
using System.Text;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void SaveSongExt()
        {
            g_session.Save();

            //dspIO.Panel.WriteText(
            //         lblPrev.CustomData 
            //    + PN(lblNext.CustomData));


            g_lcdPressed.Add(lcdInfo+1);
        }


        public void Save()
        {
            SaveMachineState();
            g_session.Save();
        }


        void SaveMachineState()
        {
            //+   (OK(ModDestConnecting) ? ModDestConnecting.GetPath(ModDestSrcIndex) : "")
            //+ PS(ModDestSrcIndex)
            //+ PS(OK(ModDestChannel) ? Patterns.IndexOf(ModDestChannel.Pattern) : -1)
            //+ PS(OK(ModDestChannel) ? ModDestChannel.Pattern.Channels.IndexOf(ModDestChannel) : -1)
            //+ PS(ModCurPat)
            //+ PS(ModDestClip)

            var state = 
                   S(g_lockView)
                + PS(SaveToggles());

            pnlStorageState.WriteText(state);
        }


        uint SaveToggles()
        {
            uint f = 0;
            var  i = 0;

            WriteBit(ref f, g_showSession, i++);
            WriteBit(ref f, g_move,        i++);

            return f;
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
