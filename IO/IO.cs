﻿namespace IngameScript
{
    partial class Program
    {
        static int      g_ioAction, // 0 = load, 1 = save
                        g_ioState,  // 0 = instruments, 1 = tracks, 2 = save external
                        g_ioPos;    // instrument or track

        static string[] g_instLines,
                        g_trackLines;

        static int      g_editTrack,
                        g_editClip,
                        g_copyTrack,
                        g_copyIndex,
                        g_trackIndex;
                        
        static string   g_ioString,
                        g_curPath;


        void ResetIO()
        { 
            g_ioAction   =
            g_ioState    =
            g_ioPos      = -1;
                         
            g_ioString   = "";
        }



        void UpdateIO()
        {
            if (g_ioAction == 0) UpdateLoad();
            if (g_ioAction == 1) UpdateSave();
        }
    }
}
