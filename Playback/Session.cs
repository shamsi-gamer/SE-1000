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
                var track = Tracks[tr];

                if (g_setClip)
                {
                    var found = track.Indices.FindIndex(i => i == index);

                    if (found < 0)
                    {
                        CurClip = new Clip(track);
                        CurClip.Patterns.Add(new Pattern(Instruments[0], CurClip));
                        CurClip.Name = "New Clip";

                        track.Clips  .Add(CurClip);
                        track.Indices.Add(index);
                        track.CurIndex = index;
                    }
                    else
                    {
                        CurClip = track.Clips[found];
                    }

                    g_setClip     = false;
                    g_showSession = false;

                    //UpdateLabels();
                }
                else
                {
                    track.CurIndex = 
                        track.CurIndex != index
                        ? index
                        : -1;
                }
            }


            //void SetClip(Clip clip)
            //{ 
            //    clip.Track.CurIndex = clip.Track.Clips.IndexOf(clip);

            //    CurClip = clip;

            //    g_setClip = false;
            //    g_showSession = false;

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
            if (g_showSession) g_setClip     = true;
            else               g_showSession = true;

            //UpdateClipsLabel();
        }
    }
}
