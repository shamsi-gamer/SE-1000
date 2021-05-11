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


            g_playing = true;


            if (CurClip.CueNext > -1)
            {
                CurClip.PlayTime = GetPatTime(CurClip.CueNext);
                CurClip.CueNext  = -1;
            }
            else
                CurClip.PlayTime = GetPatTime(CurPat);

            CurClip.StartTime = g_time - CurClip.PlayTime;

            //UpdatePlayStopLabels();
        }


        static void Stop()
        {
            if (g_session == null)
                return;

            g_playing = false;


            if (!g_playing)//CurClip.PlayTime < 0)
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


            CurClip.PlayTime  = long_NaN;
            CurClip.StartTime = long_NaN;


            lastNotes.Clear();

            //UpdatePlayStopLabels();
        }
    }
}
