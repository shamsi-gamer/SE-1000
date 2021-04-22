using System;
using System.Text;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public void Save()
        {
            SaveMachineState();
            SaveInstruments();
            SaveSong();
        }


        void SaveMachineState()
        {
            var sbMachine = new StringBuilder();

            sbMachine.Append(N(SaveSettings(SaveToggles())));
            sbMachine.Append(N(SaveMems()));
            sbMachine.Append(  SaveChords());

            lblMove.CustomData = sbMachine.ToString();
        }


        uint SaveToggles()
        {
            uint f = 0;
            var  i = 0;

            WriteByte(ref f, g_movePat,    i++);

            WriteByte(ref f, g_in,         i++);
            WriteByte(ref f, g_out,        i++);

            WriteByte(ref f, g_loop,       i++);
            WriteByte(ref f, g_block,      i++);
            WriteByte(ref f, g_allPats,    i++);
            WriteByte(ref f, g_follow,     i++);
            WriteByte(ref f, g_autoCue,    i++);

            WriteByte(ref f, g_allChan,    i++);
            WriteByte(ref f, g_rndInst,    i++);

            WriteByte(ref f, g_piano,      i++);
            WriteByte(ref f, g_move,       i++);

            WriteByte(ref f, g_transpose,  i++);
            WriteByte(ref f, g_spread,     i++);

            WriteByte(ref f, g_shift,      i++);
            WriteByte(ref f, g_mixerShift, i++);

            WriteByte(ref f, g_hold,       i++);
            WriteByte(ref f, g_pick,       i++);

            WriteByte(ref f, g_chordMode,  i++);
            WriteByte(ref f, g_chordEdit,  i++);
            WriteByte(ref f, g_chordAll,   i++);

            WriteByte(ref f, g_halfSharp,  i++);

            WriteByte(ref f, g_paramKeys,  i++);
            WriteByte(ref f, g_paramAuto,  i++);

            WriteByte(ref f, g_setMem,     i++);

            return f;
        }


        string SaveSettings(uint f)
        {
            return
                  WS(f)              

                + WS(g_ticksPerStep) 
                
                + WS(CurPat)         
                + WS(CurChan)        

                + WS(SelChan)        
                + WS(CurSrc)

                + W(CurSet > -1 ? CurSetting.GetPath(CurSrc) : "")

                + WS(g_editStep)
                + WS(g_editLength)     

                + WS(g_curNote)      

                + WS(g_chord)        
                + WS(g_chordSpread)
                
                + WS(g_songOff)        
                + WS(g_instOff)        
                + WS(g_srcOff )

                + WS(g_solo)

                + WS(g_volume)

                + W (ModDestConnecting != null ? ModDestConnecting.GetPath(ModDestSrcIndex) : "")
                + WS(ModCurChan)
                + WS(ModDestSrcIndex)
                + WS(ModDestChannel != null ? g_song.Patterns.IndexOf(ModDestChannel.Pattern)         : -1)
                + WS(ModDestChannel != null ? ModDestChannel.Pattern.Channels.IndexOf(ModDestChannel) : -1)

                +  S(g_iCol);
        }


        static string Save(Setting setting)
        {
            return 
                setting != null 
                ? P(setting.Save())
                : "";
        }


        string SaveMems()
        {
            var mems = "";

            for (int m = 0; m < nMems; m++)
                mems += S(g_mem[m]) + (m < nMems-1 ? ";" : "");

            return mems;
        }


        string SaveChords()
        {
            var chords = "";

            for (int c = 0; c < g_chords.Length; c++)
            {
                var chord = g_chords[c];

                for (int k = 0; k < chord.Count; k++)
                    chords += chord[k] + (k < chord.Count - 1 ? "," : "");

                if (c < g_chords.Length - 1)
                    chords += ";";
            }

            return chords;
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


        void SaveSongExt()
        {
            SaveSong();

            dspIO.Panel.WriteText(lblNext.CustomData);

            infoPressed.Add(1);
        }


        void SaveSong()
        {
            lblNext.CustomData = g_song.Save();
            //sb.AppendLine(SaveEdit());
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
