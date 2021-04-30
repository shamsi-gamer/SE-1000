namespace IngameScript
{
    partial class Program
    {
        public partial class Session
        {
            public void Save()
            {
                SaveSession();
                SaveInstruments();
                SaveTracks();
            }


            public void SaveSession()
            {
                var state = 
                       S(TicksPerStep)
                    + PS(Tracks.IndexOf(CurClip.Track))
                    + PS(CurClip.Track.Clips.IndexOf(CurClip));

                pnlStorageSession.WriteText(state);
            }


            public void SaveInstruments()
            {
                var inst = "";

                for (int i = 0; i < Instruments.Count; i++)
                { 
                    if (i > 0) inst += "\n";
                    inst += Instruments[i].Save();
                }

                pnlStorageInstruments.WriteText(inst);
            }


            public void SaveTracks()
            {
                var tracks = N(S(Tracks.Count));

                for (int t = 0; t < Tracks.Count; t++)
                {
                    var track = Tracks[t];

                    var strTrack = S(track.Clips.Count);

                    foreach (var i in track.Indices)
                        strTrack += PS(i);

                    tracks += PN(N(strTrack));
                    tracks += SaveClips(track);
                }

                pnlStorageTracks.WriteText(tracks);
            }


            string SaveClips(Track track)
            {
                var clips = "";

                foreach (var clip in track.Clips)
                    clips += PN(clip.Save());

                return clips;
            }
        }
    }
}
