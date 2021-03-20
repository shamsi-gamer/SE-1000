using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Parameter : Setting
        {
            float           m_value;

            public float    CurValue = 0,

                            Default,

                            Min,
                            Max,
                        
                            NormalMin,
                            NormalMax,
                        
                            Delta,
                            BigDelta;

            public Envelope Envelope;
            public LFO      Lfo;
            public Modulate Modulate;


            public Parameter(string tag, float min, float max, float normalMin, float normalMax, float delta, float bigDelta, float defVal, Setting parent) : base(tag, parent)
            {
                Tag       = tag;

                m_value   = defVal;
                Default   = defVal;

                Min       = min;
                Max       = max;

                NormalMin = normalMin;
                NormalMax = normalMax;
                          
                Delta     = delta;
                BigDelta  = bigDelta;

                Envelope  = null;
                Lfo       = null;
                Modulate  = null;
            }


            public Parameter(Parameter param, Setting parent) : base(param.Tag, parent, param.Prototype)
            {
                Tag       = param.Tag;
                          
                m_value   = param.m_value;

                Min       = param.Min;
                Max       = param.Max;

                NormalMin = param.NormalMin;
                NormalMax = param.NormalMax;

                Delta     = param.Delta;
                BigDelta  = param.BigDelta;

                Envelope  = param.Envelope?.Copy(this);
                Lfo       = param.Lfo     ?.Copy(this);
                Modulate  = param.Modulate?.Copy(this);
            }


            public Parameter Copy(Setting parent) => new Parameter(this, parent);


            public void MakeValid()
            {
                Prototype = this;
            }


            public float Value
            { 
                get { return m_value; }
            }


            public void SetValue(float val, Note note, int src)
            {
                if (note != null)
                {
                    var key = note.Keys.Find(k => k.Path == GetPath(src));

                    if (key != null) key.Value = MinMax(Min, val, Max);
                    else             m_value   = MinMax(Min, val, Max);
                }
                else m_value = MinMax(Min, val, Max);
            }


            public float GetValue(long gTime, long lTime, long sTime, int noteLen, Note note, int src, List<TriggerValue> triggerValues)
            {
                var value = GetKeyValue(note, src);
                var path  = GetPath(src);


                if (Envelope != null)
                    value *= Envelope.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues);

                if (   triggerValues.Find(v => v.Path == path) == null
                    && Lfo != null)
                { 
                    var dv = Lfo.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues)
                           * Math.Abs(Max - Min)/2;

                    value += dv;

                    if (   Tag == "Vol"
                        || Tag == "Att"
                        || Tag == "Dec"
                        || Tag == "Rel")
                        triggerValues.Add(new TriggerValue(path, value));
                }
                

                float auto;

                if (   Tag == "Tune"
                    || Tag == "Off") auto = 0;
                else                 auto = 1;

                if (note != null)
                { 
                    var val = GetAutoValue(note.Channel.Pattern.Song, note, path);
                    
                    if (OK(val))
                    {
                        if (   Tag == "Tune"
                            || Tag == "Off") auto += val;
                        else                 auto *= val;
                    }
                }

                if (   Tag == "Tune"
                    || Tag == "Off") value += auto;
                else                 value *= auto;


                return CurValue = MinMax(Min, value, Max);
            }


            public float GetKeyValue(Note note, int src)
            {
                if (note != null)
                {
                    var key = note.Keys.Find(k => k.Path == GetPath(src));
                    return key?.Value ?? m_value; 
                }

                else return m_value;
            }


            public float GetAutoValue(Song song, Note note, string path)
            {
                return GetAutoValue(note.PatStep, song.GetNotePat(note), note.iChan, path);
            }


            public static float GetAutoValue(float songStep, int pat, int ch, string path)
            {
                // the expected pos is in song time, NOT in pattern time

                var prevKey = PrevSongAutoKey(songStep, pat, ch, path);
                var nextKey = NextSongAutoKey(songStep, pat, ch, path);

                     if (prevKey == null && nextKey == null) return fN;
                else if (prevKey != null && nextKey == null) return prevKey.Value;
                else if (prevKey == null && nextKey != null) return nextKey.Value;
                else
                    return prevKey.Value + (nextKey.Value - prevKey.Value) * (songStep - prevKey.StepTime) / (nextKey.StepTime - prevKey.StepTime);
            }


            public float AdjustValue(float value, float delta, bool shift)
            {
                     if (Tag == "Att") ((Envelope)Parent).TrigAttack  = fN;
                else if (Tag == "Dec") ((Envelope)Parent).TrigDecay   = fN;
                else if (Tag == "Rel") ((Envelope)Parent).TrigRelease = fN;

                return value + delta * (shift ? BigDelta : Delta);
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Envelope != null
                    || Lfo      != null
                    || (chan?.HasKeys(GetPath(src)) ?? false)
                    || _IsCurrent;
            }


            public override void Remove(Setting setting)
            {
                     if (setting == Envelope) Envelope = null;
                else if (setting == Lfo)      Lfo      = null;
            }


            public override void Clear()
            {
                m_value = Default;

                Envelope?.Clear();
                Envelope = null;

                Lfo?.Clear();
                Lfo = null;
            }


            public override void Randomize()
            {
                m_value = NormalMin + (float)g_rnd.NextDouble() * (NormalMax - NormalMin);
                
                if (   Tag != "Att"
                    && Tag != "Dec"
                    && Tag != "Sus"
                    && Tag != "Rel"
                    && g_rnd.NextDouble() > 0.9f)
                {
                    Envelope = new Envelope(this);
                    Envelope.Randomize();
                }
                else 
                    Envelope = null;


                if (   !SettingOrParentHasTag(this, "Att")
                    && !SettingOrParentHasTag(this, "Dec")
                    && !SettingOrParentHasTag(this, "Rel")
                    && g_rnd.NextDouble() > 0.9f)
                {
                    Lfo = new LFO(this);
                    Lfo.Randomize();
                }
                else
                    Lfo = null;
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.RotationIndicator.X != 0) 
                    prog.AdjustFromController(song, this, -g_remote.RotationIndicator.X/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Env": return Envelope ?? (Envelope = new Envelope(this));
                    case "LFO": return Lfo      ?? (Lfo      = new LFO     (this));
                    case "Mod": return Modulate ?? (Modulate = new Modulate(this));
                }

                return null;
            }


            public override string Save()
            {
                return
                      W(Tag)

                    + S(m_value)

                    + Program.Save(Envelope)
                    + Program.Save(Lfo)
                    + Program.Save(Modulate);
            }


            public static Parameter Load(string[] data, ref int i, Instrument inst, int iSrc, Setting parent, Parameter proto = null)
            {
                var tag = data[i++];

                Parameter param;

                     if (proto  != null) param = proto;                                                    
                else if (parent != null) param = (Parameter)parent.GetOrAddSettingFromTag(tag);            
                else if (iSrc > -1)      param = (Parameter)inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
                else                     param = (Parameter)inst.GetOrAddSettingFromTag(tag);              

                param.m_value = float.Parse(data[i++]);

                while (i < data.Length
                    && (   data[i] == "Env" 
                        || data[i] == "LFO" 
                        || data[i] == "Mod"))
                {
                    switch (data[i])
                    { 
                        case "Env": param.Envelope = Envelope.Load(data, ref i, inst, iSrc, param); break;
                        case "LFO": param.Lfo      = LFO     .Load(data, ref i, inst, iSrc, param); break;
                        case "Mod": param.Modulate = Modulate.Load(data, ref i, inst, iSrc, param); break;
                    }
                }

                return param;
            }
        }


        static Parameter NewHarmonicParam(int i, Setting parent)
        {
            return new Parameter(S(i), 0, 1, 0.1f, 0.9f, 0.01f, 0.1f, i == 0 ? 1 : 0, parent);
        }
    }
}
