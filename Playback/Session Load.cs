using System.Text;


namespace IngameScript
{
    partial class Program
    {
        public partial class Session
        {
            public static Session Load()
            {
                var session = new Session();

                int curClipTrack, curClipIndex;
                session.LoadSession    (out curClipTrack, out curClipIndex);
                session.LoadInstruments();
                session.LoadTracks     ();

                g_setClip = true;
                g_session.SetClip(curClipTrack, curClipIndex);

                return session;
            }


            void LoadSession(out int curClipTrack, out int curClipIndex)
            {
                var sb = new StringBuilder();
                pnlStorageSession.ReadText(sb);

                var state = sb.ToString().Split(';');
                var s = 0;

                TicksPerStep = int.Parse(state[s++]);

                curClipTrack = int.Parse(state[s++]);
                curClipIndex = int.Parse(state[s++]);
            }


            void LoadInstruments()
            {
                Instruments.Clear();


                var sb = new StringBuilder();
                pnlStorageInstruments.ReadText(sb);

                var lines = sb.ToString().Split('\n');
                var line  = 0;


                while (line < lines.Length)
                {
                    while (line < lines.Length
                        && lines[line].Trim() == "") line++; // white space

                    if (line < lines.Length)
                        Instruments.Add(Instrument.Load(lines, ref line));
                }

            
                if (Instruments.Count == 0) // nothing was loaded
                    CreateDefaultInstruments();
            }


            //void ImportInstruments()
            //{
            //    LoadInstruments();

            //    // set all instruments to first
            
            //    int first, last;
            //    g_session.CurClip.GetCurPatterns(out first, out last);

            //    for (int p = first; p <= last; p++)
            //    { 
            //        for (int ch = 0; ch < g_nChans; ch++)
            //            g_session.CurClip.Patterns[p].Channels[ch].Instrument = g_session.Instruments[0]; 
            //    }
            //}


            void CreateDefaultInstruments()
            {
                if (Instruments.Count == 0)
                {
                    Instruments.Add(new Instrument());
                    Instruments[0].Sources.Add(new Source(Instruments[0]));
                }
            }


            void LoadTracks()
            {
                Tracks.Clear();

                var sb = new StringBuilder();
                pnlStorageInstruments.ReadText(sb);

                var lines = sb.ToString().Split('\n');
                var line  = 0;


                var nTracks = int.Parse(lines[line++]);

                for (int t = 0; t < nTracks; t++)
                    Tracks.Add(Track.Load(this, lines, ref line));


                //curPath = "";

                //else
                //{
                //    g_session.Tracks[0].Clips.Add(g_session.CurClip);
                //    g_session.Tracks[0].Indices.Add(index);
                //    g_session.Tracks[0].CurIndex = index;
                //}
            }
        }
    }
}
