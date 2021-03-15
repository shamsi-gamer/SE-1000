﻿using System;
using System.Collections.Generic;


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
                On         = T;
                           
                Oscillator = OscSine;

                Offset     = null;

                Volume     = NewParamFromTag("Vol", null);
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
                
                Offset     = src.Offset   ?.Copy(null);
                                                                                  
                Volume     = new Parameter(src.Volume, null);                               
                Tune       = src.Tune     ?.Copy();
                           
                Harmonics  = src.Harmonics?.Copy();
                Filter     = src.Filter   ?.Copy();
                           
                Delay      = src.Delay    ?.Copy();
            }


            public string GetSample(int note)
            {
                return g_samples[Oscillator.Samples[note - 12*NoteScale]];
            }


            public float OscMult { get  
            {
                     if (Oscillator == OscSine     ) return 1;
                else if (Oscillator == OscTriangle ) return 0.8f;
                else if (Oscillator == OscSaw      ) return 0.4f;
                else if (Oscillator == OscSquare   ) return 0.35f;
                else if (Oscillator == OscLowNoise ) return 0.5f;
                else if (Oscillator == OscHighNoise) return 0.5f;
                else if (Oscillator == OscBandNoise) return 0.5f;
                else if (Oscillator == OscClick    ) return 1;
                else if (Oscillator == OscCrunch   ) return 1;
                else if (Oscillator == OscSample   ) return 1;

                return fN;
            } }


            public float GetWaveform(float f)
            {
                     if (Oscillator == OscLowNoise ) f /= 24;
                else if (Oscillator == OscBandNoise) f /= 12;

                f *= 6;
                f -= (float)Math.Floor(f);

                while (f < 0) f += 1;

                float w = 0;

                     if (Oscillator == OscSine)      w = (float)Math.Sin(f * Tau);
                else if (Oscillator == OscTriangle)
                {
                         if (f <  0.25f)              w = f / 0.25f;
                    else if (f >= 0.25f && f < 0.75f) w = 1 - 4 * (f - 0.25f);
                    else                              w = (f - 0.75f) / 0.25f - 1;
                }                                                 
                else if (Oscillator == OscSaw      ) w =  2*f - 1;
                else if (Oscillator == OscSquare   ) w =  f < 0.5f ? 1 : -1;
                else if (Oscillator == OscLowNoise ) w = -1 + g_random[(int)(f * 100)] * 2;
                else if (Oscillator == OscHighNoise) w = -1 + g_random[(int)(f * 100)] * 2;
                else if (Oscillator == OscBandNoise) w = -1 + g_random[(int)(f * 100)] * 2;

                w *= Volume.Value;

                return w;
            }


            public void CreateSounds(List<Sound> sounds, Source src, Note note, Program prog)
            {
                var inst = src.Instrument;
                var _sounds = new List<Sound>();

                var triggerValues = new List<TriggerValue>();

                var sndTime = StartTime + note.SongTime + 1;

                var lTime = g_time - StartTime - note.SongTime;
                var sTime = StartTime > -1 ? g_time - StartTime : lTime;

                if (src.Offset != null)
                { 
                    sndTime += (int)Math.Round(
                          src.Offset.GetValue(g_time, lTime, sTime, note.FrameLength, note, src.Index, triggerValues)
                        * FPS);
                }


                var noteNum = AdjustNoteNumber(note, src, note.FrameLength);

                if (   src.Oscillator == OscSample
                    && (   noteNum % NoteScale > 0
                        || noteNum >= (12 + OscSample.Samples.Count) * NoteScale))
                    return;

                var vol = note.Volume;
                
                // calling GetValue() populates triggerValues, the return values are ignored
                inst.Volume.GetValue(g_time, lTime, sTime, note.FrameLength, note, -1,        triggerValues);
                src .Volume.GetValue(g_time, lTime, sTime, note.FrameLength, note, src.Index, triggerValues);


                var iSrc = Instrument.Sources.IndexOf(this);

                string relPath = "";
                
                     if (src .Volume.Envelope != null) relPath = src .Volume.Envelope.Release.GetPath(iSrc);
                else if (inst.Volume.Envelope != null) relPath = inst.Volume.Envelope.Release.GetPath(-1);

                var _relLen = triggerValues.Find(v => v.Path == relPath);
                var relLen = (int)((_relLen?.Value ?? 0) * FPS);


                var sample = GetSample(noteNum);


                if (Harmonics != null)
                {
                    Harmonics.CreateSounds(_sounds, this, note, noteNum, sndTime, note.FrameLength, relLen, vol, triggerValues);
                }
                else
                {
                    if (   noteNum <  12 * NoteScale
                        || noteNum > 150 * NoteScale)
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
                        F,
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
                Oscillator = OscillatorFromType((OscType)(int)(Math.Pow(g_rnd.NextDouble(), 1.5) * 7)); //9));


                if (   g_rnd.NextDouble() > 0.7f
                    && !used.Contains(Oscillator))
                {
                    Offset = NewParamFromTag("Off", null);
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


                if (   (   Oscillator == OscSine
                        || Oscillator == OscBandNoise)
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


            string Save(Setting setting) { return Program.SaveSetting(setting); }


            public string Save()
            {
                return
                      WS((int)Oscillator.Type)
                    + W (B(On))

                    + W (Volume.Save())
                    
                    + W (Save(Offset))
                    + W (Save(Tune))

                    + W (Save(Harmonics))
                    + W (Save(Filter))

                    +    Save(Delay);
            }


            public static Source Load(string[] lines, ref int line, Instrument inst)
            {
                var data = lines[line++].Split(';');
                var i    = 0;

                var src = new Source(inst);

                src.Oscillator = OscillatorFromType((OscType)int.Parse(data[i++]));
                src.On         = data[i++] == "1";

                src.Volume = Parameter.Load(data, ref i, null);

                while (i < data.Length
                    && data[i] != "_")
                { 
                    switch (data[i])
                    { 
                        case "Off":  src.Offset    = Parameter.Load(data, ref i, null); break;
                        case "Tune": src.Tune      = Tune     .Load(data, ref i);       break;
                        case "Hrm":  src.Harmonics = Harmonics.Load(data, ref i);       break;
                        case "Flt":  src.Filter    = Filter   .Load(data, ref i);       break;
                        case "Del":  src.Delay     = Delay    .Load(data, ref i);       break;
                    }
                }

                return src;
            }
        }
    }
}
