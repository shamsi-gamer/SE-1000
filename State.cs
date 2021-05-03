using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        const  float    float_Inf = 65536f;
        static float[]  g_steps = { 0.25f, 0.5f, 1, 2, 4, 8, 16, float_Inf };


        static long     g_time = -1; // in ticks
        static float    TimeStep { get { return (float)g_time / g_session.TicksPerStep; } }


        static bool     g_started         = false,
                        g_init            = false;
                                          
        static int      g_curRuntimeTick  = 0;
        static float[]  g_runtimeMs       = new float[6];
        static float    g_maxRuntimeMs    = 0;
                                          
        float           g_instCount       = 0,
                        g_dspCount        = 0;


        static bool     g_showSession     = true;
        static bool     g_setClip         = false;
                                          
        static bool     g_move            = false;

                                          
        static Session  g_session         = null;
        //static Clip   g_session.CurClip = null;

                                          
        Key             g_editKey         = null;


        static void SetDefaultMachineState()
        {
            ClearMachineState();

            g_move = false;
        }


        static void ClearMachineState()
        {
            g_sm    .StopAll();

            g_notes .Clear();
            g_sounds.Clear();
        }
    }
}

