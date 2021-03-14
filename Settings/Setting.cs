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
        public class Setting
        {
            public Setting Parent;

            public string  Name,
                           Tag;

            public Setting Prototype;

            public bool   _IsCurrent;


            public Setting(string name, string tag, Setting proto = null)
            {
                Parent    = null;
                Name      = name;
                Tag       = tag;
                Prototype = proto;
               _IsCurrent = false;
            }

            public Setting(Setting setting) : this(setting.Name, setting.Tag, setting) { }


            public virtual bool HasDeepParams(Channel chan, int src) { return false; }
            public virtual void Remove(Setting setting) { }

            public virtual void Clear() { }

            //public string Path { get { return GetPath(); } }

            public string GetPath(int src = -1)
            {
                var path = new StringBuilder();

                Setting setting = this;

                while (setting != null)
                {
                    path.Insert(0, setting.Tag + (path.Length > 0 ? "/" : ""));
                    setting = setting.Parent;
                }

                if (src > -1)
                    path.Insert(0, src + "/");

                return path.ToString();
            }


            public virtual void Randomize() { }

            public virtual void AdjustFromController(Song song, Program prog) { }
        }


        static string LastTag(string path)
        {
            return path.Split('/').Last();
        }


        Parameter GetCurrentParam(Instrument inst)
        {
            return (Parameter)GetSettingFromPath(
                inst, 
                g_settings.Last().GetPath(CurSrc));
        }


        Setting GetSettingFromPath(Instrument inst, string path)
        {
            return GetSettingFromInstrument(inst, path.Split('/'));
        }
        

        Setting GetSettingFromInstrument(Instrument inst, string[] path)
        {
            if (IsDigit(path[0][0])) // source
            {
                var rest = path.Subarray(2, path.Length - 2);
                var src  = inst.Sources[int.Parse(path[0])];

                switch (path[1])
                {
                    case "Off":  return src.Offset    != null ? GetSettingFromParam    (src.Offset,    rest) : null;
                    case "Vol":  return GetSettingFromParam(inst.Volume, rest);
                    case "Tune": return src.Tune      != null ? GetSettingFromParam    (src.Tune,      rest) : null;
                    case "Hrm":  return src.Harmonics != null ? GetSettingFromHarmonics(src.Harmonics, rest) : null;
                    case "Flt":  return src.Filter    != null ? GetSettingFromFilter   (src.Filter,    rest) : null;
                    case "Del":  return src.Delay     != null ? GetSettingFromDelay    (src.Delay,     rest) : null;
                }
            }
            else // instrument
            {
                var rest = path.Subarray(1, path.Length-1);

                switch (path[0])
                {
                    case "Vol":  return GetSettingFromParam(inst.Volume, rest);
                    case "Tune": return inst.Tune     != null ? GetSettingFromParam   (inst.Tune,     rest) : null;
                    case "Del":  return inst.Delay    != null ? GetSettingFromDelay   (inst.Delay,    rest) : null;
                    case "Arp":  return inst.Arpeggio != null ? GetSettingFromArpeggio(inst.Arpeggio, rest) : null;
                }
            }

            return null;
        }


        Setting GetSettingFromHarmonics(Harmonics hrm, string[] path)
        {
            if (path.Length == 0) return hrm;
            var rest = path.Subarray(1, path.Length-1);

            if (IsDigit(path[0][0]))
                return GetSettingFromParam(hrm.Tones[int.Parse(path[0])-1], rest);

            return null;
        }


        Setting GetSettingFromFilter(Filter flt, string[] path)
        {
            if (path.Length == 0) return flt;
            var rest = path.Subarray(1, path.Length-1);

            switch (path[0])
            {
                case "Cut": return GetSettingFromParam(flt.Cutoff,    rest);
                case "Res": return GetSettingFromParam(flt.Resonance, rest);
            }

            return null;
        }


        Setting GetSettingFromDelay(Delay del, string[] path)
        {
            if (path.Length == 0) return del;
            var rest = path.Subarray(1, path.Length-1);

            switch (path[0])
            {
                case "Cnt":  return GetSettingFromParam(del.Count, rest);
                case "Time": return GetSettingFromParam(del.Time,  rest);
                case "Lvl":  return GetSettingFromParam(del.Level, rest);
                case "Pow":  return GetSettingFromParam(del.Power, rest);
            }

            return null;
        }


        Setting GetSettingFromParam(Parameter param, string[] path)
        {
            if (path.Length == 0) return param;
            var rest = path.Subarray(1, path.Length-1);

            switch (path[0])
            {
                case "Env":  return GetSettingFromEnvelope(param.Envelope, rest);
                case "LFO":  return GetSettingFromLfo     (param.Lfo,      rest);
            }

            return null;
        }


        Setting GetSettingFromEnvelope(Envelope env, string[] path)
        {
            if (path.Length == 0) return env;
            var rest = path.Subarray(1, path.Length-1);

            switch (path[0])
            {
                case "Att": return GetSettingFromParam(env.Attack,  rest);
                case "Dec": return GetSettingFromParam(env.Decay,   rest);
                case "Sus": return GetSettingFromParam(env.Sustain, rest);
                case "Rel": return GetSettingFromParam(env.Release, rest);
            }

            return null;
        }


        Setting GetSettingFromLfo(LFO lfo, string[] path)
        {
            if (path.Length == 0) return lfo;
            var rest = path.Subarray(1, path.Length-1);

            switch (path[0])
            {
                case "Att": return GetSettingFromParam(lfo.Amplitude, rest);
                case "Dec": return GetSettingFromParam(lfo.Frequency, rest);
                case "Sus": return GetSettingFromParam(lfo.Offset,    rest);
            }

            return null;
        }


        Setting GetSettingFromArpeggio(Arpeggio arp, string[] path)
        {
            if (path.Length == 0) return arp;
            var rest = path.Subarray(1, path.Length-1);


            switch (path[0])
            {
                case "Len": return GetSettingFromParam(arp.Length, rest);
                case "Scl": return GetSettingFromParam(arp.Scale,  rest);
            }

            return null;
        }
    }
}
