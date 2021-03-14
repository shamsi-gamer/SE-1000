using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        void LoadMachine()
        {

        }


        void LoadSongExt()
        {
            var song = new StringBuilder();
            dspIO.Surface.ReadText(song, false);

            if (!LoadSong(song.ToString()))
                NewSong();

            infoPressed.Add(0);
        }


        bool LoadSong(string song)
        {
            Stop();
            ClearSong();

            var lines = song.Split('\n');
            var line = 0;

            if (   lines.Length < 1
                || lines[line++] != "SE-909")
                return false;

            g_song.Name = lines[line++];
            UpdateSongDsp();

            var cfg = lines[line++].Split(';');
            if (cfg.Length != 26) return false;

            var insts = LoadInstruments(lines, ref line);
            if (insts == null) return false;

            if (!LoadPats(lines, ref line, insts)) return false;

            if (!Finalize(insts)) return false;

            if (!LoadBlocks(lines, ref line)) return false;
            if (!LoadEdit(lines, ref line)) return false;


            var c = 0;

            if (!float.TryParse(cfg[c++], out g_volume       )) return false;
            if (!int  .TryParse(cfg[c++], out g_ticksPerStep )) return false;
            if (!int  .TryParse(cfg[c++], out CurPat  )) return false;

            if (!long .TryParse(cfg[c++], out PlayTime)) return false;


            if (!int.TryParse(cfg[c++], out CurChan)) return false;
            if (!int.TryParse(cfg[c++], out SelChan)) return false;
            if (!int.TryParse(cfg[c++], out CurSrc )) return false;

            //int _set;
            //if (!int.TryParse(cfg[c++], out _set)) return false; g_set = (Set)_set;

            if (!int.TryParse(cfg[c++], out g_chord)) return false;

            if (!int  .TryParse(cfg[c++], out g_editStep))   return false;
            if (!int  .TryParse(cfg[c++], out g_editLength)) return false;
            //if (!float.TryParse(cfg[c++], out editPos)) return false;
            if (!int  .TryParse(cfg[c++], out g_curNote))         return false;
            if (!int  .TryParse(cfg[c++], out songOff)) return false;
            if (!int  .TryParse(cfg[c++], out instOff)) return false;
            if (!int  .TryParse(cfg[c++], out srcOff))  return false;

            uint f;
            if (!uint.TryParse(cfg[c++], out f)) return false;

            var i = 0;

            loopPat      = ReadBytes(f, i++);
            g_block      = ReadBytes(f, i++);
            g_in         = ReadBytes(f, i++);
            g_out        = ReadBytes(f, i++);
            movePat      = ReadBytes(f, i++);
            allPats      = ReadBytes(f, i++);
            g_autoCue    = ReadBytes(f, i++);
            g_follow     = ReadBytes(f, i++);
            allChan      = ReadBytes(f, i++);
            g_piano      = ReadBytes(f, i++);
            g_move       = ReadBytes(f, i++);
            g_shift      = ReadBytes(f, i++);
            g_hold       = ReadBytes(f, i++);
            g_chordMode    = ReadBytes(f, i++);
            //g_seventh    = ReadBytes(f, i++);
            rndInst      = ReadBytes(f, i++);
            g_pick       = ReadBytes(f, i++);
            g_mixerShift = ReadBytes(f, i++);

            if (!int.TryParse(cfg[c++], out g_iCol)) return false;

            for (int m = 0; m < nMems; m++)
                if (!int.TryParse(cfg[c++], out g_mem[m])) return false;


            //g_inter = null;


            if (g_autoCue)
                Cue();

            //if (   g_set == Set.Volume
            //    && (   g_song.SelChan > -1
            //        || g_edit.Count == 0))
            //    g_set = Set.None;

            UpdateLights();
            SetLightColor(g_iCol);

            SetCurrentPattern(CurPat);

            PlayPat =
                PlayTime > -1
                ? (int)(PlayStep / nSteps)
                : 0;

            CueNextPattern();

            return true;
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
            if (!int.TryParse(lines[line++], out nSrc)) return false;

            for (int i = 0; i < nSrc; i++)
            {
                var src = LoadSource(lines, ref line);
                if (src == null) return false;

                inst.Sources.Add(src);
                src.Instrument = inst;
            }

            return true;
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
            if (_param.Length != 6) return false;

            float val;
            if (!float.TryParse(_param[0], out val)) return false;
            param.SetValue(val, null, -1);

            return LoadLfo(ref param.Lfo, line.Substring(_param[0].Length + 1));
        }


        bool LoadLfo(ref LFO lfo, string line)
        {
            var _lfo = line.Split('&');
            if (_lfo.Length != 5) return false;

            uint type;
            if (!uint.TryParse(_lfo[0], out type)) return false;
            lfo.Type = (LFO.LfoType)type;

            //if (!float.TryParse(_lfo[2], out lfo.Offset   )) return false;
            //if (!float.TryParse(_lfo[3], out lfo.Amplitude)) return false;
            //if (!float.TryParse(_lfo[4], out lfo.Frequency)) return false;

            return true;
        }


        bool Finalize(List<Instrument> loaded)
        {
            foreach (var l in loaded)
            {
                var f = g_inst.FindIndex(i => i.Name == l.Name);

                if (f > -1) g_inst[f] = l;
                else g_inst.Add(l);
            }

            return true;
        }



        bool LoadPats(string[] lines, ref int line, List<Instrument> loaded)
        {
            int nPat = 0;
            if (!int.TryParse(lines[line++], out nPat)) return false;

            for (int i = 0; i < nPat; i++)
            {
                var pat = LoadPattern(lines[line++], loaded);
                if (pat == null) return false;
                g_song.Patterns.Add(pat);
            }

            return true;
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

            if (!LoadNotes(chan, b, ref index)) return false;

            return true;
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
                return false;
            }

            return true;
        }


        bool LoadBlocks(string[] lines, ref int line)
        {
            g_song.Blocks.Clear();

            var cfg = lines[line++].Split(';');
            if (cfg.Length == 0) return false;

            var c = 0;

            int nBlocks;
            if (!int.TryParse(cfg[c++], out nBlocks)) return false;

            for (int i = 0; i < nBlocks; i++)
            {
                int first, last;
                if (!int.TryParse(cfg[c++], out first)) return false;
                if (!int.TryParse(cfg[c++], out last)) return false;

                g_song.Blocks.Add(new Block(first, last));
            }

            return true;
        }

        bool LoadEdit(string[] lines, ref int line)
        {
            g_song.EditNotes.Clear();

            var cfg = lines[line++].Split(';');
            if (cfg.Length == 0) return false;

            var c = 0;

            int nNotes;
            if (!int.TryParse(cfg[c++], out nNotes)) return false;

            for (int i = 0; i < nNotes; i++)
            {
                int p, ch, n;
                if (!int.TryParse(cfg[c++], out p )) return false;
                if (!int.TryParse(cfg[c++], out ch)) return false;
                if (!int.TryParse(cfg[c++], out n )) return false;

                g_song.EditNotes.Add(g_song.Patterns[p].Channels[ch].Notes[n]);
            }

            return true;
        }
    }
}
