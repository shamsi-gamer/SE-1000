using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        public enum OscType
        { 
            Sine, 
            Triangle, 
            Square, 
            Saw, 
            LowNoise, 
            HighNoise, 
            NarrowBandNoise, 
            WideBandNoise, 
            SlowSweepDown,
            FastSweepDown,
            SlowSweepUp,
            FastSweepUp,
            Pulse,
            Sample
        };



        public class Oscillator
        { 
            public OscType      Type;
            public string       ShortName;
            public List<Sample> Samples;


            public Oscillator(OscType type, string shortName, string sampleName)
            {
                Type      = type;
                ShortName = shortName;
                Samples   = MakeSamples(sampleName);
            }


            public Oscillator(OscType type, string shortName)
            {
                Type      = type;
                ShortName = shortName;
                Samples   = MakeSamples();
            }
        }



        static Oscillator OscSine            = Oscillator_null,
                          OscTriangle        = Oscillator_null,  
                          OscSquare          = Oscillator_null,
                          OscSaw             = Oscillator_null,
                          OscLowNoise        = Oscillator_null,
                          OscHighNoise       = Oscillator_null,
                          OscNarrowBandNoise = Oscillator_null,
                          OscWideBandNoise   = Oscillator_null,
                          OscSlowSweepDown   = Oscillator_null,
                          OscFastSweepDown   = Oscillator_null,
                          OscSlowSweepUp     = Oscillator_null,
                          OscFastSweepUp     = Oscillator_null,
                          OscPulse           = Oscillator_null,
                          OscSample          = Oscillator_null; 

        static int _loadStep = 0;



        void LoadOscillatorSamples(int i)
        {
            switch (i)
            { 
            case   0: OscSine            = new Oscillator(OscType.Sine,            "Sine",  strSine);            break;
            case   1: OscTriangle        = new Oscillator(OscType.Triangle,        "Tri",   strTri);             break;
            case   2: OscSquare          = new Oscillator(OscType.Square,          "Sqr",   strSqr);             break;
            case   3: OscSaw             = new Oscillator(OscType.Saw,             "Saw",   strSaw);             break;
            case   4: OscLowNoise        = new Oscillator(OscType.LowNoise,        "Lo X",  strLowNoise);        break;
            case   5: OscHighNoise       = new Oscillator(OscType.HighNoise,       "Hi X",  strHighNoise);       break;
            case   6: OscNarrowBandNoise = new Oscillator(OscType.NarrowBandNoise, "nBd X", strNarrowBandNoise); break;
            case   7: OscWideBandNoise   = new Oscillator(OscType.WideBandNoise,   "WBd X", strWideBandNoise);   break;
            case   8: OscSlowSweepDown   = new Oscillator(OscType.SlowSweepDown,   "Sw ▼",  strSlowDown);        break;
            case   9: OscFastSweepDown   = new Oscillator(OscType.FastSweepDown,   "Sw▼▼",  strFastDown);        break;
            case  10: OscSlowSweepUp     = new Oscillator(OscType.SlowSweepUp,     "Sw ▲",  strSlowUp);          break;
            case  11: OscFastSweepUp     = new Oscillator(OscType.FastSweepUp,     "Sw▲▲",  strFastUp);          break;
            case  12: OscPulse           = new Oscillator(OscType.Pulse,           "Pls",   strPulse);             break;
            case  13: OscSample          = new Oscillator(OscType.Sample,          "Smpl");                      break;
            }
        }



        static Oscillator OscillatorFromType(OscType type)
        {
            switch (type)
            { 
                case OscType.Sine:            return OscSine;
                case OscType.Triangle:        return OscTriangle;
                case OscType.Square:          return OscSquare;
                case OscType.Saw:             return OscSaw;
                case OscType.LowNoise:        return OscLowNoise;
                case OscType.HighNoise:       return OscHighNoise;
                case OscType.NarrowBandNoise: return OscNarrowBandNoise;
                case OscType.WideBandNoise:   return OscWideBandNoise;
                case OscType.SlowSweepDown:   return OscSlowSweepDown;
                case OscType.FastSweepDown:   return OscFastSweepDown;
                case OscType.SlowSweepUp:     return OscSlowSweepUp;
                case OscType.FastSweepUp:     return OscFastSweepUp;
                case OscType.Pulse:           return OscPulse;
                case OscType.Sample:          return OscSample;
            }

            return Oscillator_null;
        }



        static List<Sample> MakeSamples(string oscName)
        {
            var samples = new List<Sample>();

            for (int i = 0; i < g_samples.Count; i++)
            {
                if (   g_samples[i].Length >= strSE.Length + oscName.Length
                    && g_samples[i].Substring(0, strSE.Length + oscName.Length) == strSE + oscName)
                    samples.Add(new Sample(i, 0, 0));
            }

            samples = samples.OrderBy(smp =>
            {
                var parts  = g_samples[smp.Index].Split('_');
                
                var name   = parts[1];
                var suffix = parts[2];

                var len = 0;

                while (len < suffix.Length
                    && IsDigit(suffix[len]))
                    len++;

                var num =
                    len > 0
                    ? int_Parse(suffix.Substring(0, len))
                    : 0;

                smp.Note = num;
                
                  if (   oscName == strSine
                      || oscName == strTri
                      || oscName == strSaw
                      || oscName == strSqr
                      || oscName == strLowNoise
                      || oscName == strHighNoise
                      || oscName == strNarrowBandNoise
                      || oscName == strWideBandNoise) smp.Length = 5;
                else if (oscName == strSlowDown
                      || oscName == strSlowUp
                      || oscName == strFastDown
                      || oscName == strFastUp) smp.Length = 0.1f * 440/note2freq(smp.Note-12*NoteScale);
                else if (oscName == strPulse)  smp.Length = 1;
                
                return num;
            }).ToList();

            return samples;
        }



        static List<Sample> MakeSamples()
        {
            var samples = new List<Sample>();

            for (int i = 0; i < g_samples.Count; i++)
            {
                if (   g_samples[i].Length < strSE.Length
                    || g_samples[i].Substring(0, strSE.Length) != strSE)
                    samples.Add(new Sample(i, 0, 0));
            }

            return samples;
        }
    }
}
