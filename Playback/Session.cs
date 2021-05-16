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
                Name        = "New Session";

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

                    track.CurIndex = -1;
                }
            }


            public void SetClip(int tr, int index)
            { 
                Log("1");
                var track = Tracks[tr];
                Log("2");

                var found = track.Indices.FindIndex(i => i == index);
                Log("3");

                if (found < 0)
                {
                    Log("4");
                    CurClip = new Clip(track);
                    CurClip.Patterns.Add(new Pattern(Instruments[0], CurClip));
                    CurClip.Name = "New Clip";
                    Log("5");

                    track.Clips  .Add(CurClip);
                    track.Indices.Add(index);
                    track.CurIndex = index;
                    Log("6");
                }
                else
                {
                    Log("7");
                    track.NextIndex = 
                        track.CurIndex != index
                        ? index
                        : -1;
                }

                Log("8");
                if (g_setClip)
                {
                    Log("9");
                    g_showSession = F;
                    CurClip = track.Clips[track.CurIndex];
                    Log("10");
                }
                Log("11");

                g_setClip = F;
            }


            //void SetClip(Clip clip)
            //{ 
            //    clip.Track.CurIndex = clip.Track.Clips.IndexOf(clip);

            //    CurClip = clip;

            //    g_setClip = F;
            //    g_showSession = F;

            //    //UpdateLabels();
            //}


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
                Tracks[0].CurIndex = 0;

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
