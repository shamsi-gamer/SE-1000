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
                      Name
                    + PS(TicksPerStep)
                    + PS(Tracks.IndexOf(EditClip.Track))
                    + PS(EditClip.Track.Clips.IndexOf(EditClip));

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
                    if (t > 0) tracks += "\n";
                    tracks += Tracks[t].Save();
                }

                pnlStorageTracks.WriteText(tracks);
            }


            //string SaveClips(Track track)
            //{
            //    var clips = "";

            //    foreach (var clip in track.Clips)
            //        clips += PN(clip.Save());

            //    return clips;
            //}
        }
    }
}
