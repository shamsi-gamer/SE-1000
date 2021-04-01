using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


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
                           
                Oscillator = OscSine;

                Offset     = null;

                Volume     = (Parameter)NewFromTag("Vol", null);
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

                     if (Oscillator == OscSine     )  w = (float)Math.Sin(f * Tau);
                else if (Oscillator == OscTriangle )  
                {
                         if (f <  0.25f)              w = f / 0.25f;
                    else if (f >= 0.25f && f < 0.75f) w = 1 - 4 * (f - 0.25f);
                    else                              w = (f - 0.75f) / 0.25f - 1;
                }                                                 
                else if (Oscillator == OscSaw      )  w =  2*f - 1;
                else if (Oscillator == OscSquare   )  w =  f < 0.5f ? 1 : -1;
                else if (Oscillator == OscLowNoise )  w = -1 + g_random[(int)(f * 100)] * 2;
                else if (Oscillator == OscHighNoise)  w = -1 + g_random[(int)(f * 100)] * 2;
                else if (Oscillator == OscBandNoise)  w = -1 + g_random[(int)(f * 100)] * 2;

                w *= Volume.Value;

                return w;
            }


            public void CreateSounds(List<Sound> sounds, Note note, Program prog)
            {
                var  inst   = Instrument;
                var _sounds = new List<Sound>();

                var triggerValues = new List<TriggerValue>();


                var sndTime = 
                    g_song.StartTime > -1
                    ? g_song.StartTime + note.SongTime + 1
                    : g_time + 1;

                var lTime = g_time - g_song.StartTime - note.SongTime;
                var sTime = g_song.StartTime > -1 ? g_time - g_song.StartTime : lTime;

                var tp = new TimeParams(sndTime, lTime, sTime, note, note.FrameLength, Index, triggerValues, prog);


                if (Offset != null)
                    sndTime += (int)Math.Round(Offset.GetValue(tp) * FPS);


                var noteNum = AdjustNoteNumber(note, this, note.FrameLength, prog);

                if (   Oscillator == OscSample
                    && (   noteNum % NoteScale > 0
                        || noteNum >= (12 + OscSample.Samples.Count) * NoteScale))
                    return;

                var vol = note.Volume;
                

                // the next two GetValue() calls populate triggerValues, the return values are ignored

                tp.SourceIndex = -1;
                inst.Volume.Trigger?.GetValue(tp);

                tp.SourceIndex = Index;
                Volume.Trigger?.GetValue(tp);


                string relPath = "";
                
                     if (     Volume.Envelope != null) relPath =      Volume.Envelope.Release.GetPath(Index);
                else if (inst.Volume.Envelope != null) relPath = inst.Volume.Envelope.Release.GetPath(-1);

                var _relLen = triggerValues.Find(v => v.Path == relPath);
                var relLen = (int)((_relLen?.Value ?? 0) * FPS);


                var sample = GetSample(noteNum);

                if (Harmonics != null)
                {
                    Harmonics.CreateSounds(_sounds, this, note, noteNum, sndTime, note.FrameLength, relLen, vol, triggerValues, prog);
                }
                else
                {
                    if (   noteNum <  12 * NoteScale
                        || noteNum > 150 * NoteScale)
                        return;

                    vol = ApplyFilter(vol, this, 0, tp);

                    _sounds.Add(new Sound(
                        sample,
                        note.Channel,
                        note.iChan,
                        sndTime,
                        note.FrameLength,
                        relLen,
                        vol * OscMult,
                        Instrument,
                        Index,
                        note,
                        triggerValues,
                        false,
                        null,
                        0));
                }


                // add sound and echos
                foreach (var snd in _sounds)
                { 
                    var del = Delay ?? inst.Delay;

                    if (del != null)
                        prog.AddSoundAndEchos(
                            sounds, 
                            snd, 
                            del);
                    else
                        sounds.Add(snd);
                }
            }


            public void Randomize(List<Oscillator> used, Program prog)
            {
                if (prog.TooComplex) return;


                Oscillator = OscillatorFromType((OscType)(int)(Math.Pow(RND, 2) * 9));


                if (   RND > 0.7f
                    && !used.Contains(Oscillator))
                {
                    Offset = (Parameter)NewFromTag("Off", null);
                    Offset.Randomize(prog);
                }
                else
                    Offset = null;


                if (Index == 0)
                    Volume.SetValue(1, null, Index);
                else
                    Volume.Randomize(prog);


                if (   Index > 0
                    && (   RND > 0.7f
                        || used.Contains(Oscillator)))
                {
                    Tune = new Tune();
                    Tune.Randomize(prog);
                }
                else
                    Tune = null;


                if (   (   Oscillator == OscSine
                        || Oscillator == OscBandNoise)
                    && RND > 0.7f
                    && !used.Contains(Oscillator))
                {
                    Harmonics = new Harmonics();
                    Harmonics.Randomize(prog);
                }
                else
                    Harmonics = null;


                if (   Harmonics != null
                    && RND > 0.7f)
                {
                    Filter = new Filter();
                    Filter.Randomize(prog);
                }
                else
                    Filter = null;


                if (RND > 0.9f)
                {
                    Delay = new Delay();
                    Delay.Randomize(prog);
                }
                else
                    Delay = null;


                used.Add(Oscillator);
            }


            public Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Vol":  return Volume;
                    case "Off":  return Offset    ?? (Offset    = new Parameter("Off", -100, 100, -10, 10, 0.01f, 0.1f, 0, null));
                    case "Tune": return Tune      ?? (Tune      = new Tune());
                    case "Hrm":  return Harmonics ?? (Harmonics = new Harmonics());
                    case "Flt":  return Filter    ?? (Filter    = new Filter());
                    case "Del":  return Delay     ?? (Delay     = new Delay());
                }

                return null;
            }


            public string Save()
            {
                return
                      WS((int)Oscillator.Type)
                    + WB(On)

                    + Volume.Save()
                    
                    + Program.Save(Offset)
                    + Program.Save(Tune)

                    + Program.Save(Harmonics)
                    + Program.Save(Filter)

                    + Program.Save(Delay);
            }


            public static void Load(string[] lines, ref int line, Instrument inst, int iSrc)
            {
                var data = lines[line++].Split(';');
                var i    = 0;

                var src = new Source(inst);
                inst.Sources.Add(src);

                src.Oscillator = OscillatorFromType((OscType)int.Parse(data[i++]));
                src.On         = data[i++] == "1";

                src.Volume = Parameter.Load(data, ref i, inst, iSrc, null);

                while (i < data.Length
                    && (   data[i] == "Off"
                        || data[i] == "Tune"
                        || data[i] == "Hrm"
                        || data[i] == "Flt"
                        || data[i] == "Del"))
                { 
                    switch (data[i])
                    { 
                        case "Off":  src.Offset    = Parameter.Load(data, ref i, inst, iSrc, null); break;
                        case "Tune": src.Tune      = Tune     .Load(data, ref i, inst, iSrc);       break;
                        case "Hrm":  src.Harmonics = Harmonics.Load(data, ref i, inst, iSrc);       break;
                        case "Flt":  src.Filter    = Filter   .Load(data, ref i, inst, iSrc);       break;
                        case "Del":  src.Delay     = Delay    .Load(data, ref i, inst, iSrc);       break;
                    }
                }
            }


            public void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                if (CurSet > -1)
                { 
                    CurSetting.DrawFuncButtons(sprites, w, y, chan);
                }
                else
                {
                    DrawFuncButton(sprites, "Off",  0, w, y, true, Offset    != null);
                    DrawFuncButton(sprites, "Vol",  1, w, y, true, Volume.HasDeepParams(chan, Index));
                    DrawFuncButton(sprites, "Tune", 2, w, y, true, Tune      != null);
                    DrawFuncButton(sprites, "Hrm",  3, w, y, true, Harmonics != null);
                    DrawFuncButton(sprites, "Flt",  4, w, y, true, Filter    != null);
                    DrawFuncButton(sprites, "Del",  5, w, y, true, Delay     != null);
                }
            }
        }
    }
}
