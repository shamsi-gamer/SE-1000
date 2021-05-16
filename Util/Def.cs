using System;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        const float Tau = (float)Math.PI * 2;

        const long  long_NaN = long.MinValue;

        const int   FPS = 60;

        const int   g_nChans = 12;
        const int   g_patSteps = 16;


        const float dVol = 0.01f;


        static  Random  g_rnd    = new Random(DateTime.Now.Millisecond);
        static  float[] g_random = new float[1000];


        static Color   color6,
                       color5,
                       color4,
                       color3,
                       color2,
                       color1,
                       color0,

                       recColor6  = new Color(63,  0, 0),
                       recColor0  = new Color( 1,  0, 0),

                       editColor6 = new Color(63, 25, 0),
                       editColor0 = new Color( 1,  0, 0);


        const int MaxSources = 12;

        // display limits
        const int maxDspInst = 15;
        const int maxDspSrc  = 6;
        const int maxDspPats = 8;

        const int nMems = 8;
    }
}
