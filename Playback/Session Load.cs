using System.Text;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public partial class Session
        {
            public static Session Load(out int curClipTrack, out int curClipIndex)
            {
                var session = new Session();

                if (!session.LoadSession    (out curClipTrack, out curClipIndex)) return null;
                if (!session.LoadInstruments()) session.CreateDefaultInstruments();
                if (!session.LoadTracks     ()) session.CreateDefaultTracks();

                return session;
            }


            bool LoadSession(out int curClipTrack, out int curClipIndex)
            {
                curClipTrack = -1;
                curClipIndex = -1;

                var sb = new StringBuilder();
                pnlStorageSession.ReadText(sb);

                if (!sb.ToString().Contains(";"))
                    return false;

                var state = sb.ToString().Split(';');
                var s     = 0;

                Name = state[s++];

                if (!int.TryParse(state[s++], out TicksPerStep)) return false;

                if (!int.TryParse(state[s++], out curClipTrack)) return false;
                if (!int.TryParse(state[s++], out curClipIndex)) return false;

                return true;
            }


            bool LoadInstruments()
            {
                Instruments.Clear();


                var sb = new StringBuilder();
                pnlStorageInstruments.ReadText(sb);

                var lines = sb.ToString().Split('\n');
                var line  = 0;


                while (line < lines.Length)
                {
                    SkipWhiteSpace(lines, ref line);

                    if (line < lines.Length)
                        Instruments.Add(Instrument.Load(this, lines, ref line));
                }

            
                return Instruments.Count > 0;
            }


            public void ImportInstruments()
            {
                LoadInstruments();

                // set all instruments to first

                foreach (var track in g_session.Tracks)
                {
                    foreach (var clip in track.Clips)
                    { 
                        int first, last;
                        clip.GetCurPatterns(out first, out last);

                        for (int p = first; p <= last; p++)
                        {
                            for (int ch = 0; ch < g_nChans; ch++)
                                clip.Patterns[p].Channels[ch].Instrument = g_session.Instruments[0];
                        }
                    }
                }
            }


            bool LoadTracks()
            {
                Tracks.Clear();

                var sb = new StringBuilder();
                pnlStorageInstruments.ReadText(sb);

                var lines = sb.ToString().Split('\n');
                var line  = 0;

                int nTracks;
                if (!int.TryParse(lines[line++], out nTracks)) return false;

                for (int t = 0; t < nTracks; t++)
                {
                    SkipWhiteSpace(lines, ref line);
                    var track = Track.Load(this, lines, ref line);

                    if (track != null) Tracks.Add(track);
                    else               return false;
                }


                return Tracks.Count > 0;
            }
        }
    }
}
