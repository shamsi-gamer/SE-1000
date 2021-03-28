using System;
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
            Saw, 
            Square, 
            LowNoise, 
            HighNoise, 
            BandNoise, 
            Click,
            Crunch,
            Sample
        };


        public class Oscillator
        { 
            public OscType   Type;
            public string    ShortName;
            public float     Length;
            public List<int> Samples;

            public Oscillator(OscType type, string shortName, float length, string sampleName)
            {
                Type      = type;
                ShortName = shortName;
                Length    = length;
                Samples   = MakeOscillators(sampleName);
            }

            public Oscillator(OscType type, string shortName)
            {
                Type      = type;
                ShortName = shortName;
                Length    = 10;
                Samples   = MakeSamples();
            }
        }


        static Oscillator OscSine      = null,
                          OscTriangle  = null,  
                          OscSaw       = null,
                          OscSquare    = null,
                          OscLowNoise  = null,
                          OscHighNoise = null,
                          OscBandNoise = null,
                          OscClick     = null,
                          OscCrunch    = null,
                          OscSample    = null; 

        static int _loadStep = 0;


        void LoadOscillatorSamples(int i)
        {
            switch (i)
            { 
            case 0: OscSine      = new Oscillator(OscType.Sine,      "Sine",  10, "Sine");      break;
            case 1: OscTriangle  = new Oscillator(OscType.Triangle,  "Tri",   10, "Triangle");  break;
            case 2: OscSaw       = new Oscillator(OscType.Saw,       "Saw",   10, "Saw");       break;
            case 3: OscSquare    = new Oscillator(OscType.Square,    "Sqr",   10, "Square");    break;
            case 4: OscLowNoise  = new Oscillator(OscType.LowNoise,  "Lo #",   5, "LowNoise");  break;
            case 5: OscHighNoise = new Oscillator(OscType.HighNoise, "Hi #",   5, "HighNoise"); break;
            case 6: OscBandNoise = new Oscillator(OscType.BandNoise, "Bnd #",  5, "BandNoise"); break;
            case 7: OscClick     = new Oscillator(OscType.Click,     "Clk",    1, "Click");     break;
            case 8: OscCrunch    = new Oscillator(OscType.Crunch,    "Crch",   1, "Crunch");    break;
            case 9: OscSample    = new Oscillator(OscType.Sample,    "Smpl");                   break;
            }
        }


        static List<int> MakeOscillators(string name)
        {
            var samples = new List<int>();

            for (int i = 0; i < g_samples.Count; i++)
            {
                if (   g_samples[i].Length >= 7 + name.Length
                    && g_samples[i].Substring(0, 7 + name.Length) == "SE-909_" + name)
                    samples.Add(i);
            }

            samples = samples.OrderBy(i =>
            {
                var parts = g_samples[i].Split('_');

                var suffix = parts.Last();

                var len = 0;

                while (len < suffix.Length
                    && IsDigit(suffix[len]))
                    len++;

                var num =
                    len > 0
                    ? int.Parse(suffix.Substring(0, len))
                    : 0;

                return num;
            }).ToList();

            return samples;
        }


        static Oscillator OscillatorFromType(OscType type)
        {
            switch (type)
            { 
                case OscType.Sine:      return OscSine;
                case OscType.Triangle:  return OscTriangle;
                case OscType.Saw:       return OscSaw;
                case OscType.Square:    return OscSquare;
                case OscType.LowNoise:  return OscLowNoise;
                case OscType.HighNoise: return OscHighNoise;
                case OscType.BandNoise: return OscBandNoise;
                case OscType.Click:     return OscClick;
                case OscType.Crunch:    return OscCrunch;
                case OscType.Sample:    return OscSample;
            }

            return null;
        }


        static List<int> MakeSamples()
        {
            var samples = new List<int>();

            for (int i = 0; i < g_samples.Count; i++)
            {
                if (   g_samples[i].Length < 7
                    || g_samples[i].Substring(0, 7) != "SE-909_")
                    samples.Add(i);
            }

            return samples;
        }
    }
}
