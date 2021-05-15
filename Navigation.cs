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


            if (CurClip.CueNext > -1)
            {
                CurClip.Track.PlayTime = GetPatTime(CurClip.CueNext);
                CurClip.CueNext  = -1;
            }
            else
                CurClip.Track.PlayTime = GetPatTime(CurPat);

            CurClip.Track.StartTime = g_time - CurClip.Track.PlayTime;

            //UpdatePlayStopLabels();
        }


        static void Stop()
        {
            if (g_session == null)
                return;

            g_playing = F;


            if (!g_playing)//CurClip.Track.PlayTime < 0)
            {
                var b = CurClip.GetBlock(CurPat);

                var _block =
                       CurClip.Block
                    && b != null
                    && CurPat > b.First;

                CurClip.SetCurrentPattern(_block ? b.First : 0);
                CurClip.CueNext = -1;
            }


            CurClip.TrimCurrentNotes();


            CurClip.Track.PlayTime  = long_NaN;
            CurClip.Track.StartTime = long_NaN;


            lastNotes.Clear();

            //UpdatePlayStopLabels();
        }
    }
}
