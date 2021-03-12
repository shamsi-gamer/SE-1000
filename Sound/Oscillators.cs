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
        public enum Oscillator 
        { 
            Sine, 
            Triangle, 
            Saw, 
            Square, 
            LowNoise, 
            HighNoise, 
            BandNoise, 
            Samples1, 
            Samples2 
        };


        static List<int>
            g_sine,
            g_triangle,
            g_saw,
            g_square,
            g_lowNoise,
            g_highNoise,
            g_bandNoise,
            g_samples1,
            g_samples2;


        public void MakeOscillators()
        { 
            g_sine      = MakeOscillators("Sine");
            g_triangle  = MakeOscillators("Triangle");
            g_saw       = MakeOscillators("Saw");
            g_square    = MakeOscillators("Square");
            g_lowNoise  = MakeOscillators("LowNoise");
            g_highNoise = MakeOscillators("HighNoise");
            g_bandNoise = MakeOscillators("BandNoise");
            g_samples1  = MakeSamples( 0,  84);
            g_samples2  = MakeSamples(84, 103);
        }


        List<int> MakeOscillators(string name)
        {
            var samples = new List<int>();

            for (int i = 0; i < g_smp.Count; i++)
            {
                if (   g_smp[i].Length >= 7 + name.Length
                    && g_smp[i].Substring(0, 7 + name.Length) == "SE-909_" + name)
                    samples.Add(i);
            }

            samples = samples.OrderBy(i =>
            {
                var parts = g_smp[i].Split('_');

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


        List<int> MakeSamples(int first, int last)
        {
            var allSamples = new List<int>();

            for (int i = 0; i < g_smp.Count; i++)
            {
                if (   g_smp[i].Length < 7
                    || g_smp[i].Substring(0, 7) != "SE-909_")
                    allSamples.Add(i);
            }

            var samples = new List<int>();

            for (int i = first; i < Math.Min(last, allSamples.Count); i++)
                samples.Add(allSamples[i]);

            return samples;
        }
    }
}
