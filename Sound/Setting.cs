using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public class Setting
        {
            public Setting Parent;

            public string  Tag;

            public Setting Prototype;

            public bool   _IsCurrent;


            public Setting(string tag, Setting parent, Setting proto = null)
            {
                Parent    = parent;
                Tag       = tag;
                Prototype = proto;
               _IsCurrent = false;
            }


            public Setting(Setting setting, Setting parent) : this(setting.Tag, parent, setting) { }


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

                return S(path);
            }


            public virtual Setting GetOrAddSettingFromTag(string tag) => null;



            public virtual bool    HasDeepParams(Channel chan, int src) { return false; }
            public virtual void    Remove(Setting setting) { }
                                   
            public virtual void    Clear() { }


            public virtual void    Randomize() {}
            public virtual void    AdjustFromController(Song song, Program prog) {}

            public virtual string  Save() => "";
        }


        Parameter GetCurrentParam(Instrument inst)
        {
            return (Parameter)GetSettingFromPath(
                inst, 
                g_settings[CurSet].GetPath(CurSrc));
        }


        static Setting GetSettingFromPath(Instrument inst, string path)
        {
            var tags = path.Split('/');

            Setting setting = null;

            foreach (var tag in tags)
            {
                setting = 
                    setting == null
                    ? inst   .GetOrAddSettingFromTag(tag)
                    : setting.GetOrAddSettingFromTag(tag);
            }

            return setting;
//            return GetSettingFromInstrument(inst, path.Split('/'));
        }
        

        //static Setting GetSettingFromInstrument(Instrument inst, string[] path)
        //{
        //    if (IsDigit(path[0][0])) // source
        //    {
        //        var rest = path.Subarray(2, path.Length - 2);
        //        var src  = inst.Sources[int.Parse(path[0])];

        //        switch (path[1])
        //        {
        //            case "Off":  return src.Offset    != null ? GetSettingFromParam    (src.Offset,    rest) : null;
        //            case "Vol":  return GetSettingFromParam(inst.Volume, rest);
        //            case "Tune": return src.Tune      != null ? GetSettingFromParam    (src.Tune,      rest) : null;
        //            case "Hrm":  return src.Harmonics != null ? GetSettingFromHarmonics(src.Harmonics, rest) : null;
        //            case "Flt":  return src.Filter    != null ? GetSettingFromFilter   (src.Filter,    rest) : null;
        //            case "Del":  return src.Delay     != null ? GetSettingFromDelay    (src.Delay,     rest) : null;
        //        }
        //    }
        //    else // instrument
        //    {
        //        var rest = path.Subarray(1, path.Length-1);

        //        switch (path[0])
        //        {
        //            case "Vol":  return GetSettingFromParam(inst.Volume, rest);
        //            case "Tune": Log("inst.Tune = " + inst.Tune);return inst.Tune     != null ? GetSettingFromParam   (inst.Tune,     rest) : null;
        //            case "Del":  return inst.Delay    != null ? GetSettingFromDelay   (inst.Delay,    rest) : null;
        //            case "Arp":  return inst.Arpeggio != null ? GetSettingFromArpeggio(inst.Arpeggio, rest) : null;
        //        }
        //    }

        //    return null;
        //}


        //Setting GetSettingFromHarmonics(Harmonics hrm, string[] path)
        //{
        //    if (path.Length == 0) return hrm;
        //    var rest = path.Subarray(1, path.Length-1);

        //    if (IsDigit(path[0][0]))
        //        return GetSettingFromParam(hrm.Tones[int.Parse(path[0])-1], rest);

        //    return null;
        //}


        //Setting GetSettingFromFilter(Filter flt, string[] path)
        //{
        //    if (path.Length == 0) return flt;
        //    var rest = path.Subarray(1, path.Length-1);

        //    switch (path[0])
        //    {
        //        case "Cut": return GetSettingFromParam(flt.Cutoff,    rest);
        //        case "Res": return GetSettingFromParam(flt.Resonance, rest);
        //    }

        //    return null;
        //}


        //static Setting GetSettingFromDelay(Delay del, string[] path)
        //{
        //    if (path.Length == 0) return del;
        //    var rest = path.Subarray(1, path.Length-1);

        //    switch (path[0])
        //    {
        //        case "Cnt":  return GetSettingFromParam(del.Count, rest);
        //        case "Time": return GetSettingFromParam(del.Time,  rest);
        //        case "Lvl":  return GetSettingFromParam(del.Level, rest);
        //        case "Pow":  return GetSettingFromParam(del.Power, rest);
        //    }

        //    return null;
        //}


        //static Setting GetSettingFromParam(Parameter param, string[] path)
        //{
        //    if (path.Length == 0) return param;
        //    var rest = path.Subarray(1, path.Length-1);

        //    switch (path[0])
        //    {
        //        case "Env": return GetSettingFromEnvelope(param.Envelope, rest);
        //        case "LFO": return GetSettingFromLfo     (param.Lfo,      rest);
        //        case "Mod": return GetSettingFromModulate(param.Modulate, rest);
        //    }

        //    return null;
        //}


        //static Setting GetSettingFromEnvelope(Envelope env, string[] path)
        //{
        //    if (path.Length == 0) return env;
        //    var rest = path.Subarray(1, path.Length-1);

        //    switch (path[0])
        //    {
        //        case "Att": return GetSettingFromParam(env.Attack,  rest);
        //        case "Dec": return GetSettingFromParam(env.Decay,   rest);
        //        case "Sus": return GetSettingFromParam(env.Sustain, rest);
        //        case "Rel": return GetSettingFromParam(env.Release, rest);
        //    }

        //    return null;
        //}


        //static Setting GetSettingFromLfo(LFO lfo, string[] path)
        //{
        //    if (path.Length == 0) return lfo;
        //    var rest = path.Subarray(1, path.Length-1);

        //    switch (path[0])
        //    {
        //        case "Att": return GetSettingFromParam(lfo.Amplitude, rest);
        //        case "Dec": return GetSettingFromParam(lfo.Frequency, rest);
        //        case "Sus": return GetSettingFromParam(lfo.Offset,    rest);
        //    }

        //    return null;
        //}


        //static Setting GetSettingFromModulate(Modulate mod, string[] path)
        //{
        //    if (path.Length == 0) return mod;
        //    var rest = path.Subarray(1, path.Length-1);

        //    switch (path[0])
        //    {
        //        case "Amt": return GetSettingFromParam(mod.Amount,  rest);
        //        case "Att": return GetSettingFromParam(mod.Attack,  rest);
        //        case "Rel": return GetSettingFromParam(mod.Release, rest);
        //    }

        //    return null;
        //}


        //Setting GetSettingFromArpeggio(Arpeggio arp, string[] path)
        //{
        //    if (path.Length == 0) return arp;
        //    var rest = path.Subarray(1, path.Length-1);


        //    switch (path[0])
        //    {
        //        case "Len": return GetSettingFromParam(arp.Length, rest);
        //        case "Scl": return GetSettingFromParam(arp.Scale,  rest);
        //    }

        //    return null;
        //}


        static string FullNameFromTag(string tag)
        {
            if (IsDigit(tag[0])) return "Harmonic " + tag;

            switch (tag)
            { 
            case "Vol":  return "Volume";

            case "Tune": return "Tune";

            case "Env":  return "Envelope";
            case "Att":  return "Attack";
            case "Dec":  return "Decay";
            case "Sus":  return "Sustain";
            case "Rel":  return "Release";

            case "LFO":  return "LFO";
            case "Amp":  return "Amplitude";
            case "Freq": return "Frequency";
            case "Off":  return "Offset";

            case "Flt":  return "Filter";
            case "Cut":  return "Cutoff";
            case "Res":  return "Resonance";

            case "Mod":  return "Modulate";
            case "Amt":  return "Amount";

            case "Del":  return "Delay";
            case "Cnt":  return "Count";
            case "Time": return "Time";
            case "Lvl":  return "Level";
            case "Pow":  return "Power";
                         
            case "Arp":  return "Arpeggio";
            case "Len":  return "Length";
            case "Scl":  return "Scale";
            }

            return "";
        }


        static Setting NewSettingFromTag(string tag, Setting parent)
        {
            //var setting = GetSettingFromPath(SelectedInstrument, parent.GetPath(CurSrc));

            switch (tag)
            { 
            case "Vol":  return new Parameter("Vol",     0,          2,   0.5f,  1, 0.01f,  0.1f, 1,    parent);

            case "Tune": return new Tune();

            case "Env":  return new Envelope(parent);
            case "Att":  return new Parameter("Att",     0,         10,   0,     1, 0.01f,  0.1f, 0,    parent);
            case "Dec":  return new Parameter("Dec",     0,         10,   0,     1, 0.01f,  0.1f, 0.2f, parent);
            case "Sus":  return new Parameter("Sus",     0,          1,   0.01f, 1, 0.01f,  0.1f, 0.1f, parent);
            case "Rel":  return new Parameter("Rel",     0,         10,   0,     2, 0.01f,  0.1f, 0.2f, parent);

            case "LFO":  return new LFO(parent);
            case "Amp":  return new Parameter("Amp",     0,          1,   0,     1, 0.01f,  0.1f, 0,    parent);
            case "Freq": return new Parameter("Freq",    0.000001f, 30,   0.01f, 4, 0.01f,  0.1f, 0.5f, parent);
            case "Off":  return new Parameter("Off",  -100,        100, -10,    10, 0.01f,  0.1f, 0,    parent);

            case "Hrm":  return new Harmonics();

            case "Flt":  return new Filter();
            case "Cut":  return new Parameter("Cut",    -1,          1,  -1,     1, 0.01f,  0.1f, 0,    parent);
            case "Res":  return new Parameter("Res",     0.01f,      1,   0.01f, 1, 0.01f,  0.1f, 0,    parent);

            case "Mod":  return new Modulate(parent);
            case "Amt":  return new Parameter("Amt",   -10,         10,  -1,     1, 0.01f,  0.1f, 0,    parent);

            case "Del":  return new Delay();
            case "Cnt":  return new Parameter("Cnt",    0,         100,   1,    16, 1,     10,    4,    parent);
            case "Time": return new Parameter("Time",   0.000001f,  10,   0.01f, 1, 0.01f,  0.1f, 0.2f, parent);
            case "Lvl":  return new Parameter("Lvl",    0,           1,   0.01f, 1, 0.01f,  0.1f, 0.5f, parent);
            case "Pow":  return new Parameter("Pow",    0.000001f,   1,   0.01f, 1, 0.01f,  0.1f, 1,    parent);

            case "Arp":  return new Arpeggio((Instrument)null);
            case "Len":  return new Parameter("Len",    1,         256,   2,     6, 0.01f,  0.1f, 8,    parent);
            case "Scl":  return new Parameter("Scl",    0.01f,      16,   0.25f, 4, 0.01f,  0.1f, 1,    parent);
            }

            return null;
        }
    }
}
