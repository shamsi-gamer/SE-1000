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


            var clip = g_session.CurClip;

            if (clip.CueNext > -1)
            {
                clip.PlayTime = GetPatTime(clip.CueNext);
                clip.CueNext  = -1;
            }
            else
                clip.PlayTime = GetPatTime(clip.CurPat);

            clip.StartTime = g_time - clip.PlayTime;

            //UpdatePlayStopLabels();
        }


        static void Stop()
        {
            if (g_session == null)
                return;

            g_playing = false;


            var clip = g_session.CurClip;


            if (clip.PlayTime < 0)
            {
                var b = clip.GetBlock(clip.CurPat);

                var _block =
                       clip.Block
                    && b != null
                    && clip.CurPat > b.First;

                clip.SetCurrentPattern(_block ? b.First : 0);
                clip.CueNext = -1;
            }


            clip.TrimCurrentNotes();


            clip.PlayTime  = long_NaN;
            clip.StartTime = long_NaN;


            lastNotes.Clear();

            //UpdatePlayStopLabels();
        }
    }
}
