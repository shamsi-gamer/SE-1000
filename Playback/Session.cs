using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public partial class Session
        {
            public string           Name;
            public List<Instrument> Instruments;
            public List<Track>      Tracks;

            public int              TicksPerStep;

            public Clip             CurClip;


            public Session()
            {
                Name = "New Session";

                Instruments = new List<Instrument>();

                CreateDefaultSession();
                CreateDefaultInstruments();
                CreateDefaultTracks();
            }


            public void Clear()
            {
                foreach (var track in Tracks)
                {
                    track.Clips  .Clear();
                    track.Indices.Clear();

                    track.PlayClip = -1;
                }
            }


            public void CreateDefaultSession()
            {
                TicksPerStep = 7;
            }


            void CreateDefaultInstruments()
            {
                Instruments.Clear();

                Instruments.Add(new Instrument());
                Instruments[0].Sources.Add(new Source(Instruments[0]));
            }


            void CreateDefaultTracks()
            {
                Tracks = new List<Track>(new Track[4]);

                for (int i = 0; i < Tracks.Count; i++)
                    Tracks[i] = new Track(this);

                var clip = new Clip(Tracks[0]);
                clip.Patterns.Add(new Pattern(Instruments[0], clip));

                Tracks[0].Add(clip, 0);
                Tracks[0].PlayClip = 0;

                CurClip = Tracks[0].Clips[0];
            }
        }


        void Clips()
        {
            if (g_showSession) g_setClip     = T;
            else               g_showSession = T;

            //UpdateClipsLabel();
        }
    }
}
