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
        public class Arpeggio : Setting
        {
            public Song      Song;

            public Parameter Length,
                             Scale;


            public Arpeggio(Instrument inst) : base("Arpeggio", "Arp")
            {
                Song = new Song("");
                Song.Arpeggio = this;
                Song.Patterns.Add(new Pattern(Song));
                
                SetInstrument(inst);

                Length = new Parameter("Length", "Len", 1, 256, 2, 6, 0.01f, 0.1f, 8);
                Length.Parent = this;

                Scale = new Parameter("Scale", "Scl", 0.01f, 16, 0.25f, 4, 0.01f, 0.1f, 1);
                Scale.Parent = this;
            }


            public Arpeggio(Arpeggio arp) : base(arp.Name, arp.Tag, arp.Prototype)
            {
                Song = new Song(arp.Song);

                Length = new Parameter(arp.Length);
                Length.Parent = this;

                Scale = new Parameter(arp.Scale);
                Scale.Parent = this;

                //if (arp.Song != null)
                //{ 
                //    Notes = new List<Note>();

                //    foreach (var note in arp.Notes)
                //        Notes.Add(note);

                //    if (arp.Length > 0)
                //        Length = arp.Length;

                //    if (arp.Scale != null)
                //    {
                //        Scale = new Parameter(arp.Scale);
                //        Scale.Parent = this;
                //    }
                //    else
                //        Scale = null;
                //}
                //else
                //{
                //    Notes  =  null;
                //    Length = -Math.Abs(Length); // turn off but keep current value
                //    Scale  =  null;
                //}
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return Scale != null;
            }


            public override void Remove(Setting setting)
            {
                if (setting == Scale) Scale  = null;
            }


            public void SetInstrument(Instrument inst)
            {
                foreach (var pat in Song.Patterns)
                    pat.Channels[0].Instrument = inst;
            }


            //public List<Note> GetNotes(long gTime, long sTime)
            //{

            //}


            public override void Randomize()
            {
                Length.Randomize();
                Scale .Randomize();
            }
        }
    }
}
