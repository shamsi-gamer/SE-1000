using System.Collections.Generic;
using System.Linq;


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

            public Clip             EditedClip,
                                    ClipCopy;

            public bool             ShowSession,
                                    Move;

            public int              EditClip; // 0 = edit, 1 = dup, 2 = del

                        

            public bool IsPlaying { get { return OK(Tracks.Find(track => track.IsPlaying)); } }


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
                    track.NextClip = -1;
                }
            }


            public void CreateDefaultSession()
            {
                TicksPerStep = 7;

                EditedClip = null;
                ClipCopy   = null;

                ShowSession = T;
                Move        = F;

                EditClip    = -1;
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

                EditedClip       = clip;
                track.Clips[0] = clip;

                track.PlayClip = -1;
                track.NextClip = -1;
            }


            public Clip GetClipAfterDelete(Clip clip)
            {
                var iTrack = Tracks.IndexOf(clip.Track);

                while (iTrack >= 0)
                { 
                    var track = Tracks[iTrack--];

                    var clips = track.Clips;
                    var iClip = clips.IndexOf(clip);

                    for (int i = iClip; i >= 0; i--)
                    {
                        if (OK(clips[i]))
                            return clips[i];
                    }

                    for (int i = iClip+1; i < track.Clips.Length; i++)
                    {
                        if (OK(clips[i]))
                            return clips[i];
                    }
                }

                return null;
            }
        }


        void ToggleSession()
        {
            if (g_session.ShowSession) 
            {
                g_session.EditClip = 
                    g_session.EditClip != 0
                    ?  0
                    : -1;
            }
            else
            { 
                g_session.ShowSession =  T;
                g_session.EditClip    = -1;
            }
        }
    }
}
