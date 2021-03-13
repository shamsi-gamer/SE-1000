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
            //public Modulate Modulate;


            public Parameter(string name, string tag, float min, float max, float normalMin, float normalMax, float delta, float bigDelta, float defVal = 0) : base(name, tag)
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
            }


            public Parameter(Parameter param) : base(param.Name, param.Tag, param.Prototype)
            {
                Tag       = param.Tag;
                          
                m_value   = param.m_value;

                Min       = param.Min;
                Max       = param.Max;

                NormalMin = param.NormalMin;
                NormalMax = param.NormalMax;

                Delta     = param.Delta;
                BigDelta  = param.BigDelta;

                Envelope  = param.Envelope != null ? new Envelope(param.Envelope) : null;
                Lfo       = param.Lfo      != null ? new LFO     (param.Lfo     ) : null;
            }


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
                return GetAutoValue(note.PatStepTime, song.GetNotePat(note), note.iChan, path);
            }


            public static float GetAutoValue(float songStep, int pat, int ch, string path)
            {
                // the expected pos is in song time, NOT in pattern time

                var prevKey = PrevSongAutoKey(songStep, pat, ch, path);
                var nextKey = NextSongAutoKey(songStep, pat, ch, path);

                     if (prevKey == null && nextKey == null) return float.NaN;
                else if (prevKey != null && nextKey == null) return prevKey.Value;
                else if (prevKey == null && nextKey != null) return nextKey.Value;
                else
                    return prevKey.Value + (nextKey.Value - prevKey.Value) * (songStep - prevKey.StepTime) / (nextKey.StepTime - prevKey.StepTime);
            }


            public float AdjustValue(float value, float delta, bool shift)
            {
                     if (Tag == "Att") ((Envelope)Parent).TrigAttack  = float.NaN;
                else if (Tag == "Dec") ((Envelope)Parent).TrigDecay   = float.NaN;
                else if (Tag == "Rel") ((Envelope)Parent).TrigRelease = float.NaN;

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
                    Envelope = new Envelope();
                    Envelope.Parent = this;
                    Envelope.Randomize();
                }
                else 
                    Envelope = null;


                if (   !SettingOrParentHasTag(this, "Att")
                    && !SettingOrParentHasTag(this, "Dec")
                    && !SettingOrParentHasTag(this, "Rel")
                    && g_rnd.NextDouble() > 0.9f)
                {
                    Lfo = new LFO();
                    Lfo.Parent = this;
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
        }
    }
}
