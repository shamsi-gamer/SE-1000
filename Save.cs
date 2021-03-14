using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void SaveMachine()
        {

        }


        void SaveSongExt()
        {
            dspIO.Surface.WriteText(SaveSong());
            infoPressed.Add(1);
        }


        string SaveSong()
        {
            const string s = ";";

            var song = "";

            uint f = 0;
            var  i = 0;

            WriteByte(ref f, loopPat,      i++);
            WriteByte(ref f, g_block,      i++);
            WriteByte(ref f, g_in,         i++);
            WriteByte(ref f, g_out,        i++);
            WriteByte(ref f, movePat,      i++);
            WriteByte(ref f, allPats,      i++);
            WriteByte(ref f, g_autoCue,    i++);
            WriteByte(ref f, g_follow,     i++);
            WriteByte(ref f, allChan,      i++);
            WriteByte(ref f, g_piano,      i++);
            WriteByte(ref f, g_move,       i++);
            WriteByte(ref f, g_shift,      i++);
            WriteByte(ref f, g_hold,       i++);
            WriteByte(ref f, g_chordMode,  i++);
            WriteByte(ref f, rndInst,      i++);
            WriteByte(ref f, g_pick,       i++);
            WriteByte(ref f, g_mixerShift, i++);

            var cfg = // 26 int total with mems
                      g_volume      .ToString()
                + s + g_ticksPerStep.ToString()
                + s + CurPat        .ToString()
                + s + PlayStep      .ToString()
                + s + CurChan       .ToString()
                + s + SelChan       .ToString()
                + s + CurSrc        .ToString()
                //+ s + ((int)g_set)  .ToString()
                + s + g_chord       .ToString()
                + s + EditStep      .ToString()
                + s + EditLength    .ToString()
                //+ s + editPos       .ToString()
                + s + g_curNote       .ToString()
                + s + songOff       .ToString()
                + s + instOff       .ToString()
                + s + srcOff        .ToString()
                + s + f             .ToString()
                + s + g_iCol        .ToString();

            for (int m = 0; m < nMems; m++)
                cfg += s + g_mem[m].ToString();

            song +=
                 "SE-909" + "\n"
                + g_song.Name.Replace("\n", "\u0085") + "\n"
                + cfg + "\n"
                + SaveInstruments()
                + SavePats()
                + SaveBlocks()
                + SaveEdit();

            return song;
        }



        string SaveInstruments()
        {
            var str = g_inst.Count.ToString() + "\n";

            foreach (var inst in g_inst)
                str += SaveInstrument(inst);

            return str;
        }

        string SaveInstrument(Instrument inst)
        {
            var str = "";
            //var s   = ";";

            str += inst.Name + "\n";

            //str +=
            //      SaveParam(inst.DelayCount) + s
            //    + SaveParam(inst.DelayTime)  + s
            //    + SaveParam(inst.DelayLevel) + s
            //    + SaveParam(inst.DelayPower) + "\n";

            str += inst.Sources.Count.ToString() + "\n";

            foreach (var src in inst.Sources)
                str += SaveSource(src);

            return str;
        }


        string SaveSource(Source src)
        {
            var str = "";
            var s   = "$";

            uint f = 0;
            var  i = 0;

            WriteByte(ref f, src.On,       i++);

            str +=
                  f                         .ToString() + s
                + ((int)src.Oscillator.Type).ToString() + s
                //+ src.Transpose        .ToString() + s
                + SaveParam(src.Volume ) + s
                //+ SaveParam(src.Attack ) + s
                //+ SaveParam(src.Decay  ) + s
                //+ SaveParam(src.Sustain) + s
                //+ SaveParam(src.Release) + s
                //+ SaveParam(src.Offset ) 
                + "\n";

            return str;
        }


        string SaveParam(Parameter param)
        {
            var s = "&";

            return
                  param.Value.ToString() + s
                + SaveLfo(param.Lfo);
        }


        string SaveLfo(LFO lfo)
        {
            var s = "&";

            return
                  ((int)lfo.Type).ToString() + s
                + lfo.Amplitude  .ToString() + s
                + lfo.Frequency  .ToString() + s
                + lfo.Offset     .ToString();
        }


        string SavePats()
        {
            var str = g_song.Patterns.Count.ToString() + "\n";

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

            str += g_song.Blocks.Count.ToString();

            foreach (var b in g_song.Blocks)
            {
                str +=
                  s + b.First.ToString()
                + s + b.Last .ToString();
            }

            str += "\n";

            return str;
        }


        String SaveEdit()
        {
            var str = "";
            var s   = ";";

            str += g_song.EditNotes.Count.ToString();

            foreach (var n in g_song.EditNotes)
            {
                str +=
                  s + g_song.Patterns.FindIndex(p => p.Channels.Contains(n.Channel)).ToString()
                + s + n.iChan.ToString()
                + s + n.Channel.Notes.IndexOf(n).ToString();
            }

            str += "\n";

            return str;
        }


        void add(List<byte> b, short s) { b.AddArray(BitConverter.GetBytes(s)); }
        void add(List<byte> b, float f) { b.AddArray(BitConverter.GetBytes(f)); }
        void add(List<byte> b, int   i) { b.AddArray(BitConverter.GetBytes(i)); }
    }
}
