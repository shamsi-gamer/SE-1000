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
        public class SpeakerManager
        {
            public List<Speaker> Speakers;

            public List<Speaker> Available;
            public List<Speaker> Used;

            public float         UsedRatio => (float)Used.Count / Speakers.Count;


            public SpeakerManager()
            {
                Speakers = new List<Speaker>();

                Available = new List<Speaker>();
                Used      = new List<Speaker>();
            }


            public Speaker GetSpeaker()
            {
                if (Available.Count == 0)
                    return null;

                var spk = Available.Last();
                spk.SetUsed();

                Available.RemoveLast();
                Used.Add(spk);

                return spk;
            }


            public void FreeSpeaker(Speaker spk)
            {
                spk.Block.Stop();
                spk.Free();
                Used.Remove(spk);
                Available.Add(spk);
            }


            public void StopAll()
            {
                foreach (var spk in Speakers)
                    FreeSpeaker(spk);
            }
        }



        static  SpeakerManager g_sm = new SpeakerManager();


        void InitSpeakers()
        {
            var speakers = new List<IMySoundBlock>();
            Get(speakers);

            for (int i = 0; i < speakers.Count; i++)
            {
                var s = speakers[i];
                var spk = new Speaker(s);

                g_sm.Speakers .Add(spk);
                g_sm.Available.Add(spk);

                s.CustomName = "Speaker " + S(i + 1);
                s.Stop();
            }
        }
    }
}
