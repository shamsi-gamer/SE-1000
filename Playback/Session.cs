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

            public Clip             EditClip;


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
                    for (int i = 0; i < g_nChans; i++)
                        track.Clips[i] = null;

                    track.PlayClip = -1;
                }
            }


            public void CreateDefaultSession()
            {
                TicksPerStep = 8;
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

                var track = Tracks[0];
                var clip  = new Clip(track);

                clip.Patterns.Add(new Pattern(Instruments[0], clip));

                EditClip       = clip;
                track.Clips[0] = clip;

                track.PlayClip = 0;
                track.NextClip = 0;
            }
        }


        void ToggleSession()
        {
            if (g_showSession) 
            {
                if (++g_editClip > 2) 
                    g_editClip = 0; 
            }
            else
                g_showSession = T;
        }
    }
}
