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
        public class Tune : Parameter
        {
            public bool      UseChord,
                             AllOctaves;

            public List<int> Chord,
                             FinalChord;


            public Tune() : base("Tune", "Tune", -240, 240, -12, 12, 0.5f, 24, 0, null)
            {
                UseChord   = F;
                AllOctaves = F;

                Chord      = new List<int>();
                FinalChord = new List<int>();
            }


            public Tune(Tune tune) : base(tune, null)
            {
                UseChord   = tune.UseChord;
                AllOctaves = tune.AllOctaves;

                Chord = new List<int>();
                foreach (var note in tune.Chord)
                    Chord.Add(note);

                FinalChord = new List<int>();
                foreach (var note in tune.FinalChord)
                    FinalChord.Add(note);
            }


            public Tune Copy()
            {
                return new Tune(this);
            }
        }
    }
}
