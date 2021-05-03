using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        void Play()
        {
            if (OK(g_session.CurClip.PlayTime))
                return;

            if (g_session.CurClip.CueNext > -1)
            {
                g_session.CurClip.PlayTime = GetPatTime(g_session.CurClip.CueNext);
                g_session.CurClip.CueNext = -1;
            }
            else
                g_session.CurClip.PlayTime = GetPatTime(g_session.CurClip.CurPat);

            g_session.CurClip.StartTime = g_time - g_session.CurClip.PlayTime;

            //UpdatePlayStopLabels();
        }


        static void Stop()
        {
            if (g_session != null)
            { 
                if (g_session.CurClip.PlayTime < 0)
                {
                    var b = g_session.CurClip.GetBlock(g_session.CurClip.CurPat);

                    var _block =
                           g_session.CurClip.Block
                        && b != null
                        && g_session.CurClip.CurPat > b.First;

                    g_session.CurClip.SetCurrentPattern(_block ? b.First : 0);

                    g_session.CurClip.CueNext = -1;
                }


                g_session.CurClip.TrimCurrentNotes();


                g_session.CurClip.PlayTime  = long_NaN;
                g_session.CurClip.StartTime = long_NaN;
            }


            lastNotes.Clear();

            //UpdatePlayStopLabels();
        }
    }
}
