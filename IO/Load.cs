using System;
using System.Collections.Generic;
using System.Text;


namespace IngameScript
{
    partial class Program
    {
        bool LoadMachineState(string state)
        {
            Stop();
            ClearSong();

            var lines = state.Split('\n');
            var line = 0;

            if (   lines.Length < 1
                || lines[line++] != "SE-909 mk2")
                return F;

            var cfg = lines[line++].Split(';');


            if (!LoadToggles (cfg[0]))      return F;
            if (!LoadSettings(cfg))         return F;

            if (!LoadMems(lines[line++]))   return F;
            if (!LoadChords(lines[line++])) return F;



            g_inst.Clear();
            g_inst.Add(new Instrument());
            g_inst[0].Sources.Add(new Source(g_inst[0]));
            g_song.Patterns.Add(new Pattern(g_song, g_inst[0]));
            g_song.Name = "New Song";


            SetCurrentPattern(CurPat);

            PlayPat =
                PlayTime > -1
                ? (int)(PlayStep / nSteps)
                : 0;

            if (g_autoCue)
                Cue();

            CueNextPattern();


            //var insts = LoadInstruments(lines, ref line);
            //if (insts == null) return F;


            UpdateLights();
            SetLightColor(g_iCol);


            return T;
        }


        bool LoadToggles(string toggles)
        {
            uint f;
            if (!uint.TryParse(toggles, out f)) return F;

            var i = 0;

            g_movePat    = ReadBytes(f, i++);

            g_in         = ReadBytes(f, i++);
            g_out        = ReadBytes(f, i++);
            
            g_loop       = ReadBytes(f, i++);
            g_block      = ReadBytes(f, i++);
            g_allPats    = ReadBytes(f, i++);
            g_follow     = ReadBytes(f, i++);
            g_autoCue    = ReadBytes(f, i++);
            
            g_allChan    = ReadBytes(f, i++);
            g_rndInst    = ReadBytes(f, i++);
            
            g_piano      = ReadBytes(f, i++);
            g_move       = ReadBytes(f, i++);
            
            g_transpose  = ReadBytes(f, i++);
            g_spread     = ReadBytes(f, i++);

            g_shift      = ReadBytes(f, i++);
            g_mixerShift = ReadBytes(f, i++);
            
            g_hold       = ReadBytes(f, i++);
            g_pick       = ReadBytes(f, i++);

            g_chordMode  = ReadBytes(f, i++);
            g_chordEdit  = ReadBytes(f, i++);
            g_chordAll   = ReadBytes(f, i++);

            g_halfSharp  = ReadBytes(f, i++);

            g_paramKeys  = ReadBytes(f, i++);
            g_paramAuto  = ReadBytes(f, i++);
                         
            g_setMem     = ReadBytes(f, i++);

            return T;
        }


        bool LoadSettings(string[] cfg)
        {
            int c = 1; // 0 holds the toggles, loaded in LoadToggles()

            if (!int  .TryParse(cfg[c++], out g_ticksPerStep)) return F;

            if (!int  .TryParse(cfg[c++], out CurPat        )) return F;
            if (!int  .TryParse(cfg[c++], out CurChan       )) return F;
            if (!int  .TryParse(cfg[c++], out SelChan       )) return F;
            if (!int  .TryParse(cfg[c++], out CurSrc        )) return F;

            if (!long .TryParse(cfg[c++], out PlayTime      )) return F;
            StartTime = -1;
            PlayPat   = -1;

            if (!int  .TryParse(cfg[c++], out g_editStep    )) return F;
            if (!int  .TryParse(cfg[c++], out g_editLength  )) return F;

            if (!int  .TryParse(cfg[c++], out g_curNote     )) return F;
                                                            
            if (!int  .TryParse(cfg[c++], out g_chord       )) return F;
            if (!int  .TryParse(cfg[c++], out g_chordSpread )) return F;

            if (!int  .TryParse(cfg[c++], out g_songOff     )) return F;
            if (!int  .TryParse(cfg[c++], out g_instOff     )) return F;
            if (!int  .TryParse(cfg[c++], out g_srcOff      )) return F;
                                                            
            if (!int  .TryParse(cfg[c++], out g_cue         )) return F;
            if (!int  .TryParse(cfg[c++], out g_solo        )) return F;

            if (!float.TryParse(cfg[c++], out g_volume      )) return F;

            if (!int  .TryParse(cfg[c++], out g_iCol        )) return F;

            return T;
        }


        bool LoadMems(string line)
        {
            var mems = line.Split(';');

            for (int m = 0; m < nMems; m++)
                if (!int.TryParse(mems[m], out g_mem[m])) return F;

            return T;
        }


        bool LoadChords(string strChords)
        {
            g_chords = new List<int>[4];

            for (int _c = 0; _c < g_chords.Length; _c++)
                g_chords[_c] = new List<int>();


            var chords = strChords.Split(';');

            for (int _c = 0; _c < chords.Length; _c++)
            { 
                var _keys = chords[_c].Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

                g_chords[_c] = new List<int>();

                int key;
                foreach (var k in _keys)
                {
                    if (!int.TryParse(k, out key)) return F;
                    g_chords[_c].Add(key);
                }
            }


            return T;
        }


        void LoadSongExt()
        {
            //var song = new StringBuilder();
            //dspIO.Surface.ReadText(song, F);

            //if (!LoadSong(S(song)))
            //    NewSong();

            infoPressed.Add(0);
        }


        bool LoadSong(string song)
        {
            return F;
            //Stop();
            //ClearSong();

            //var lines = song.Split('\n');
            //var line = 0;

            //if (   lines.Length < 1
            //    || lines[line++] != "SE-909")
            //    return F;

            //g_song.Name = lines[line++];
            //UpdateSongDsp();

            //var cfg = lines[line++].Split(';');
            //if (cfg.Length != 26) return F;

            //var insts = LoadInstruments(lines, ref line);
            //if (insts == null) return F;

            //if (!LoadPats(lines, ref line, insts)) return F;

            //if (!Finalize(insts)) return F;

            //if (!LoadBlocks(lines, ref line)) return F;
            //if (!LoadEdit(lines, ref line)) return F;
        }


        List<Instrument> LoadInstruments(string[] lines, ref int line)
        {
            var insts = new List<Instrument>();

            int nInst = 0;
            if (!int.TryParse(lines[line++], out nInst)) return null;

            for (int i = 0; i < nInst; i++)
            {
                var inst = LoadInstrument(lines, ref line);
                if (inst == null) return null;
                insts.Add(inst);
            }

            return insts;
        }

        Instrument LoadInstrument(string[] lines, ref int line)
        {
            var inst = new Instrument();

            inst.Name = lines[line++];

            var parts = lines[line++].Split(';');

            //LoadParam(ref inst.DelayCount, parts[0]);
            //LoadParam(ref inst.DelayTime,  parts[1]);
            //LoadParam(ref inst.DelayLevel, parts[2]);
            //LoadParam(ref inst.DelayPower, parts[3]);

            if (!LoadSources(lines, ref line, inst)) return null;

            return inst;
        }

        bool LoadSources(string[] lines, ref int line, Instrument inst)
        {
            int nSrc = 0;
            if (!int.TryParse(lines[line++], out nSrc)) return F;

            for (int i = 0; i < nSrc; i++)
            {
                var src = LoadSource(lines, ref line);
                if (src == null) return F;

                inst.Sources.Add(src);
                src.Instrument = inst;
            }

            return T;
        }

        Source LoadSource(string[] lines, ref int line)
        {
            var _src = lines[line++].Split('$');
            if (_src.Length != 9) return null;

            var src = new Source(null); // TODO figure out index

            uint f;
            if (!uint.TryParse(_src[0], out f)) return null;

            var i = 0;

            src.On = ReadBytes(f, i++);

            uint osc;
            if (!uint.TryParse(_src[1], out osc)) return null;
            src.Oscillator = OscillatorFromType((OscType)osc);

            //if (!int.TryParse(_src[2], out src.Transpose)) return null;

            if (!LoadParam(ref src.Volume,  _src[3])) return null;
            //if (!LoadParam(ref src.Attack,  _src[4])) return null;
            //if (!LoadParam(ref src.Decay,   _src[5])) return null;
            //if (!LoadParam(ref src.Sustain, _src[6])) return null;
            //if (!LoadParam(ref src.Release, _src[7])) return null;
            //if (!LoadParam(ref src.Offset,  _src[8])) return null;

            return src;
        }


        bool LoadParam(ref Parameter param, string line)
        {
            var _param = line.Split('&');
            if (_param.Length != 6) return F;

            float val;
            if (!float.TryParse(_param[0], out val)) return F;
            param.SetValue(val, null, -1);

            return LoadLfo(ref param.Lfo, line.Substring(_param[0].Length + 1));
        }


        bool LoadLfo(ref LFO lfo, string line)
        {
            var _lfo = line.Split('&');
            if (_lfo.Length != 5) return F;

            uint type;
            if (!uint.TryParse(_lfo[0], out type)) return F;
            lfo.Type = (LFO.LfoType)type;

            //if (!float.TryParse(_lfo[2], out lfo.Offset   )) return F;
            //if (!float.TryParse(_lfo[3], out lfo.Amplitude)) return F;
            //if (!float.TryParse(_lfo[4], out lfo.Frequency)) return F;

            return T;
        }


        bool Finalize(List<Instrument> loaded)
        {
            foreach (var l in loaded)
            {
                var f = g_inst.FindIndex(i => i.Name == l.Name);

                if (f > -1) g_inst[f] = l;
                else g_inst.Add(l);
            }

            return T;
        }



        bool LoadPats(string[] lines, ref int line, List<Instrument> loaded)
        {
            int nPat = 0;
            if (!int.TryParse(lines[line++], out nPat)) return F;

            for (int i = 0; i < nPat; i++)
            {
                var pat = LoadPattern(lines[line++], loaded);
                if (pat == null) return F;
                g_song.Patterns.Add(pat);
            }

            return T;
        }

        Pattern LoadPattern(string str, List<Instrument> loaded)
        {
            var pat = new Pattern();

            var b = Convert.FromBase64String(str);

            var index = 0;
            for (int ch = 0; ch < nChans; ch++)
                LoadChannel(pat.Channels[ch], b, ref index, loaded);

            return pat;
        }

        bool LoadChannel(Channel chan, byte[] b, ref int index, List<Instrument> loaded)
        {
            var l = BitConverter.ToInt16(b, index);

            chan.Instrument = loaded[l]; index += 2;

            byte flags = b[index++];
            chan.On = (flags & 1) != 0;

            chan.Volume    = BitConverter.ToSingle(b, index); index += 4;
            chan.Shuffle   = BitConverter.ToInt32(b, index); index += 4;
            chan.Transpose = BitConverter.ToInt32(b, index); index += 4;

            if (!LoadNotes(chan, b, ref index)) return F;

            return T;
        }

        bool LoadNotes(Channel chan, byte[] b, ref int index)
        {
            var nNotes = b[index++];

            try
            {
                for (int i = 0; i < nNotes; i++)
                {
                    var note = new Note();

                    note.Channel    = chan;
                    note.iChan      = b[index++];
                    note.Number     = b[index++];
                    note.PatStep   = BitConverter.ToSingle(b, index); index += 4;
                    note.StepLength = BitConverter.ToSingle(b, index); index += 4;
                    note.Volume     = BitConverter.ToSingle(b, index); index += 4;

                    chan.Notes.Add(note);
                }
            }
            catch (Exception)
            {
                return F;
            }

            return T;
        }


        bool LoadBlocks(string[] lines, ref int line)
        {
            g_song.Blocks.Clear();

            var cfg = lines[line++].Split(';');
            if (cfg.Length == 0) return F;

            var c = 0;

            int nBlocks;
            if (!int.TryParse(cfg[c++], out nBlocks)) return F;

            for (int i = 0; i < nBlocks; i++)
            {
                int first, last;
                if (!int.TryParse(cfg[c++], out first)) return F;
                if (!int.TryParse(cfg[c++], out last)) return F;

                g_song.Blocks.Add(new Block(first, last));
            }

            return T;
        }

        bool LoadEdit(string[] lines, ref int line)
        {
            g_song.EditNotes.Clear();

            var cfg = lines[line++].Split(';');
            if (cfg.Length == 0) return F;

            var c = 0;

            int nNotes;
            if (!int.TryParse(cfg[c++], out nNotes)) return F;

            for (int i = 0; i < nNotes; i++)
            {
                int p, ch, n;
                if (!int.TryParse(cfg[c++], out p )) return F;
                if (!int.TryParse(cfg[c++], out ch)) return F;
                if (!int.TryParse(cfg[c++], out n )) return F;

                g_song.EditNotes.Add(g_song.Patterns[p].Channels[ch].Notes[n]);
            }

            return T;
        }
    }
}
