﻿using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        string SaveMachineState()
        {
            var state = 
                  N("SE-909 mk2")
                + N(SaveSettings(SaveToggles()))
                + N(SaveMems())
                + N(SaveChords())
                + "\n";

            foreach (var inst in g_inst)
                state += N(N(inst.Save()));

            return state;
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
                
                + WS(PlayTime)       

                + WS(g_editStep)
                + WS(g_editLength)     

                + WS(g_curNote)      

                + WS(g_chord)        
                + WS(g_chordSpread)
                
                + WS(g_songOff)        
                + WS(g_instOff)        
                + WS(g_srcOff)

                + WS(g_cue)
                + WS(g_solo)

                + WS(g_volume)

                + WS(g_iCol, F);
        }


        static string SaveSetting(Setting setting)
        {
            return 
                setting != null 
                ? setting.Save() 
                : "_";
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


        //string SaveSource(Source src)
        //{
        //    var str = "";
        //    var s   = "$";

        //    uint f = 0;
        //    var  i = 0;

        //    WriteByte(ref f, src.On,       i++);

        //    str +=
        //          S(f) + s
        //        + S((int)src.Oscillator.Type) + s
        //        //+ S(src.Transpose) + s
        //        + SaveParam(src.Volume ) + s
        //        //+ SaveParam(src.Attack ) + s
        //        //+ SaveParam(src.Decay  ) + s
        //        //+ SaveParam(src.Sustain) + s
        //        //+ SaveParam(src.Release) + s
        //        //+ SaveParam(src.Offset ) 
        //        + "\n";

        //    return str;
        //}


        //string SaveParam(Parameter param)
        //{
        //    var s = "&";

        //    return
        //          S(param.Value) + s
        //        + SaveLfo(param.Lfo);
        //}


        //string SaveLfo(LFO lfo)
        //{
        //    var s = "&";

        //    return
        //          S((int)lfo.Type) + s
        //        + S(lfo.Amplitude) + s
        //        + S(lfo.Frequency) + s
        //        + S(lfo.Offset   );
        //}


        void SaveSongExt()
        {
            dspIO.Surface.WriteText(SaveSong());
            infoPressed.Add(1);
        }


        string SaveSong()
        {
            var song = "";

            //song +=
            //     "SE-909" + "\n"
            //    + g_song.Name.Replace("\n", "\u0085") + "\n"
            //    + cfg + "\n"
            //    + SaveInstruments()
            //    + SavePats()
            //    + SaveBlocks()
            //    + SaveEdit();

            return song;
        }


        string SavePats()
        {
            var str = S(g_song.Patterns.Count) + "\n";

            foreach (var pat in g_song.Patterns)
                str += Convert.ToBase64String(SavePat(pat).ToArray()) + "\n";

            return str;
        }


        List<byte> SavePat(Pattern pat)
        {
            var b = new List<byte>();

            for (int ch = 0; ch < nChans; ch++)
                b.AddRange(SaveChan(pat.Channels[ch]));

            return b;
        }


        List<byte> SaveChan(Channel chan)
        {
            var b = new List<byte>();

            add(b, (short)g_inst.IndexOf(chan.Instrument));

            byte flags = 0;
            if (chan.On) flags |= 1 << 0;
            b.Add(flags);

            add(b, chan.Volume);
            add(b, chan.Shuffle);
            add(b, chan.Transpose);

            b.AddRange(SaveNotes(chan.Notes));

            return b;
        }


        List<byte> SaveNotes(List<Note> notes)
        {
            var b = new List<byte>();

            b.Add((byte)notes.Count);

            foreach (var n in notes)
            {
                b.Add((byte)n.iChan);
                b.Add((byte)n.Number);
                add(b, n.PatStep);
                add(b, n.StepLength);
                add(b, n.Volume);
            }

            return b;
        }


        String SaveBlocks()
        {
            var str = "";
            var s   = ";";

            str += S(g_song.Blocks.Count);

            foreach (var b in g_song.Blocks)
            {
                str +=
                  s + S(b.First)
                + s + S(b.Last );
            }

            str += "\n";

            return str;
        }


        String SaveEdit()
        {
            var str = "";
            var s   = ";";

            str += S(g_song.EditNotes.Count);

            foreach (var n in g_song.EditNotes)
            {
                str +=
                  s + S(g_song.Patterns.FindIndex(p => p.Channels.Contains(n.Channel)))
                + s + S(n.iChan)
                + s + S(n.Channel.Notes.IndexOf(n));
            }

            str += "\n";

            return str;
        }


        void add(List<byte> b, short s) { b.AddArray(BitConverter.GetBytes(s)); }
        void add(List<byte> b, float f) { b.AddArray(BitConverter.GetBytes(f)); }
        void add(List<byte> b, int   i) { b.AddArray(BitConverter.GetBytes(i)); }
    }
}
