using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        void Play()
        {
            if (g_playing)
                return;


            g_playing = T;


            foreach (var track in g_session.Tracks)
            { 
                if (track.NextPat > -1)
                {
                    track.PlayTime = GetPatTime(track.NextPat);
                    track.NextPat  = -1;
                }
                else
                    track.PlayTime = GetPatTime(CurPat);

                track.StartTime = g_time - track.PlayTime;
            }
        }


        static void Stop()
        {
            if (NO(g_session))
                return;

            g_playing = F;


            if (!g_playing)
            {
                var b = CurClip.GetBlock(CurPat);

                var _block =
                       CurClip.Block
                    && OK(b)
                    && CurPat > b.First;

                CurClip.SetCurrentPattern(_block ? b.First : 0);
                CurClip.Track.NextPat = -1;
            }


            CurClip.TrimCurrentNotes();


            CurClip.Track.PlayTime  = long_NaN;
            CurClip.Track.StartTime = long_NaN;


            lastNotes.Clear();

            //UpdatePlayStopLabels();
        }
    }
}
