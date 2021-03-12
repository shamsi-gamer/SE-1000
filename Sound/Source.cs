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
        public class Source
        {
            public Instrument Instrument;
            public bool       On;
            
            public Oscillator Oscillator;

            public Parameter  Offset;

            public Parameter  Volume;
            public Tune       Tune;

            public Harmonics  Harmonics;
            public Filter     Filter;

            public Delay      Delay;


            public int Index { get { return Instrument.Sources.IndexOf(this); } }


            public Source(Instrument inst)
            {
                Instrument = inst;
                On         = true;
                           
                Oscillator = Oscillator.Sine;

                Offset     = null;

                Volume     = new Parameter("Volume", "Vol", 0, 2, 0.01f, 1, 0.01f, 0.1f, 1);
                Tune       = null;

                Harmonics  = null;
                Filter     = null;

                Delay      = null;
            }


            public Source(Source src, Instrument inst)
            {
                Instrument = inst;
                On         = src.On;

                Oscillator = src.Oscillator;
                
                Offset     = src.Offset    != null ? new Parameter(src.Offset)    : null;
                                                                                  
                Volume     = new Parameter(src.Volume);                               
                Tune       = src.Tune      != null ? new Tune     (src.Tune)      : null;
                           
                Harmonics  = src.Harmonics != null ? new Harmonics(src.Harmonics) : null;
                Filter     = src.Filter    != null ? new Filter   (src.Filter   ) : null;
                           
                Delay      = src.Delay     != null ? new Delay    (src.Delay)     : null;
            }


            public string GetSample(int note)
            {
                     if (Oscillator == Oscillator.Sine     ) return g_smp[g_sine     [note-12*NoteScale]];
                else if (Oscillator == Oscillator.Triangle ) return g_smp[g_triangle [note-12*NoteScale]];
                else if (Oscillator == Oscillator.Saw      ) return g_smp[g_saw      [note-12*NoteScale]];
                else if (Oscillator == Oscillator.Square   ) return g_smp[g_square   [note-12*NoteScale]];
                else if (Oscillator == Oscillator.LowNoise ) return g_smp[g_lowNoise [note-12*NoteScale]];
                else if (Oscillator == Oscillator.HighNoise) return g_smp[g_highNoise[note-12*NoteScale]];
                else if (Oscillator == Oscillator.BandNoise) return g_smp[g_bandNoise[note-12*NoteScale]];
                else if (Oscillator == Oscillator.Samples1 ) return g_smp[g_samples1 [note-12*NoteScale]];
                else if (Oscillator == Oscillator.Samples2 ) return g_smp[g_samples2 [note-12*NoteScale]];

                return "";
            }


            public float OscMult { get  
            {
                     if (Oscillator == Oscillator.Sine     ) return 1;
                else if (Oscillator == Oscillator.Triangle ) return 0.8f;
                else if (Oscillator == Oscillator.Saw      ) return 0.35f;
                else if (Oscillator == Oscillator.Square   ) return 0.35f;
                else if (Oscillator == Oscillator.LowNoise ) return 1;
                else if (Oscillator == Oscillator.HighNoise) return 1;
                else if (Oscillator == Oscillator.BandNoise) return 1;
                else if (Oscillator == Oscillator.Samples1 ) return 1;
                else if (Oscillator == Oscillator.Samples2 ) return 1;

                return float.NaN;
            } }


            public float GetWaveform(float f)
            {
                     if (Oscillator == Oscillator.LowNoise ) f /= 24;
                else if (Oscillator == Oscillator.BandNoise) f /= 12;

                f *= 6;
                f -= (float)Math.Floor(f);

                while (f < 0) f += 1;

                float w = 0;

                     if (Oscillator == Oscillator.Sine)      w = (float)Math.Sin(f * Tau);
                else if (Oscillator == Oscillator.Triangle)
                {
                         if (f <  0.25f)                     w = f / 0.25f;
                    else if (f >= 0.25f && f < 0.75f)        w = 1 - 4 * (f - 0.25f);
                    else                                     w = (f - 0.75f) / 0.25f - 1;
                }                                                 
                else if (Oscillator == Oscillator.Saw      ) w =  2*f - 1;
                else if (Oscillator == Oscillator.Square   ) w =  f < 0.5f ? 1 : -1;
                else if (Oscillator == Oscillator.LowNoise ) w = -1 + g_random[(int)(f * 100)] * 2;
                else if (Oscillator == Oscillator.HighNoise) w = -1 + g_random[(int)(f * 100)] * 2;
                else if (Oscillator == Oscillator.BandNoise) w = -1 + g_random[(int)(f * 100)] * 2;

                w *= Volume.Value;

                return w;
            }


            public void CreateSounds(List<Sound> sounds, Source src, Note note, Program prog)
            {
                var inst = src.Instrument;
                var _sounds = new List<Sound>();

                var triggerValues = new List<TriggerValue>();

                var startTime = OK(g_song.PlayStep) ? g_song.StartTime : 0;//g_time;
                var sndTime   = startTime + note.SongTime + 1;

                var lTime = g_time - g_song.StartTime - note.SongTime;
                var sTime = g_song.StartTime > -1 ? g_time - g_song.StartTime : lTime;

                if (src.Offset != null)
                { 
                    sndTime += (int)Math.Round(
                          src.Offset.GetValue(g_time, lTime, sTime, note.FrameLength, note, src.Index, triggerValues)
                        * FPS);
                }

                var noteNum = AdjustNoteNumber(note, src, note.FrameLength);

                if (   src.Oscillator == Oscillator.Samples2
                    && noteNum >= 24 + 103 - 84)
                    return;

                var vol = note.Volume;
                
                // calling GetValue() populates triggerValues, the return values are ignored
                inst.Volume.GetValue(g_time, lTime, sTime, note.FrameLength, note, -1,        triggerValues);
                src .Volume.GetValue(g_time, lTime, sTime, note.FrameLength, note, src.Index, triggerValues);


                var iSrc = Instrument.Sources.IndexOf(this);

                var relPath = 
                    src.Volume.Envelope != null 
                    ? src .Volume.Envelope.Release.GetPath(iSrc)
                    : inst.Volume.Envelope.Release.GetPath(-1);

                var _relLen = triggerValues.Find(v => v.Path == relPath);
                var relLen = (int)((_relLen?.Value ?? 0) * FPS);


                var sample = GetSample(noteNum);


                if (Harmonics != null)
                {
                    Harmonics.CreateSounds(_sounds, this, note, noteNum, sndTime, note.FrameLength, relLen, vol, triggerValues);
                }
                else
                {
                    if (   noteNum <  12*NoteScale
                        || noteNum > 150*NoteScale)
                        return;


                    vol *= OscMult;

                    vol = ApplyFilter(
                        vol, 
                        this, 
                        0, 
                        sndTime, 
                        0, 
                        sTime,
                        note.FrameLength, 
                        note, 
                        iSrc,
                        triggerValues);


                    _sounds.Add(new Sound(
                        sample,
                        note.Channel,
                        note.iChan,
                        sndTime,
                        note.FrameLength,
                        relLen,
                        vol * src.OscMult,
                        Instrument,
                        iSrc,
                        note,
                        triggerValues,
                        false,
                        null,
                        0));
                }


                // add sounds
                foreach (var snd in _sounds)
                    sounds.Add(snd);


                // add echos
                foreach (var snd in _sounds)
                { 
                    prog.AddSoundEchos(
                        sounds, 
                        snd, 
                        src.Delay ?? inst.Delay, 
                        src.Delay != null
                        ? snd.SourceIndex
                        : -1);
                }
            }


            public void Randomize(List<Oscillator> used)
            {
                Oscillator = (Oscillator)(int)(Math.Pow(g_rnd.NextDouble(), 1.5) * 7); //9);


                if (   g_rnd.NextDouble() > 0.7f
                    && !used.Contains(Oscillator))
                {
                    Offset = new Parameter("Offset", "Off", -1, 1, 0, 0.3f, 0.01f, 0.1f, 0);
                    Offset.Randomize();
                }
                else
                    Offset = null;


                if (Index == 0)
                    Volume.SetValue(1, null, Index);
                else
                    Volume.Randomize();


                if (   Index > 0
                    && (   g_rnd.NextDouble() > 0.7f
                        || used.Contains(Oscillator)))
                {
                    Tune = new Tune();
                    Tune.Randomize();
                }
                else
                    Tune = null;


                if (   (   Oscillator == Oscillator.Sine
                        || Oscillator == Oscillator.BandNoise)
                    && g_rnd.NextDouble() > 0.7f
                    && !used.Contains(Oscillator))
                {
                    Harmonics = new Harmonics();
                    Harmonics.Randomize();
                }
                else
                    Harmonics = null;


                if (   Harmonics != null
                    && g_rnd.NextDouble() > 0.7f)
                {
                    Filter = new Filter();
                    Filter.Randomize();
                }
                else
                    Filter = null;


                if (g_rnd.NextDouble() > 0.9f)
                {
                    Delay = new Delay();
                    Delay.Randomize();
                }
                else
                    Delay = null;


                used.Add(Oscillator);
            }
        }
    }
}
