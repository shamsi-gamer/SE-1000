using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        void Play()
        {
            if (OK(g_clip.PlayTime))
                return;

            if (g_clip.CueNext > -1)
            {
                g_clip.PlayTime = GetPatTime(g_clip.CueNext);
                g_clip.CueNext = -1;
            }
            else
                g_clip.PlayTime = GetPatTime(g_clip.CurPat);

            g_clip.StartTime = g_time - g_clip.PlayTime;

            UpdatePlayStopLights();
        }


        void Stop()
        {
            if (g_clip.PlayTime < 0)
            {
                var b = g_clip.GetBlock(g_clip.CurPat);

                var _block =
                       g_clip.Block
                    && b != null
                    && g_clip.CurPat > b.First;

                g_clip.SetCurrentPattern(_block ? b.First : 0);

                g_clip.CueNext = -1;
            }


            g_clip.TrimCurrentNotes();


            g_clip.PlayTime  = long_NaN;
            g_clip.StartTime = long_NaN;


            lastNotes.Clear();

            UpdatePlayStopLights();
        }
    }
}
