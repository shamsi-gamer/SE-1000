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
                Name         = "Untitled";
                Instruments  = new List<Instrument>();

                Tracks = new List<Track>(new Track[4]);
                for (int i = 0; i < Tracks.Count; i++)
                    Tracks[i] = new Track(this);

                TicksPerStep = 7;

                CurClip         = null;
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
                        CurClip.Patterns.Add(new Pattern(CurClip, Instruments[0]));
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


            void SetClip(Clip clip)
            { 
                clip.Track.CurIndex = clip.Track.Clips.IndexOf(clip);

                CurClip = clip;

                g_setClip = false;
                g_showSession = false;

                //UpdateLabels();
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
