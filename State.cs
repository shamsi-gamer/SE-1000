using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        bool[] g_on = new bool[g_nChans];


        static long        g_time         = -1; // in ticks
        static int         g_ticksPerStep = 7;

        static bool        g_move = false;

 
        static float       TimeStep { get { return (float)g_time / g_ticksPerStep; } }


        const  float       float_Inf = 65536f;
        static float[]     g_steps = { 0.25f, 0.5f, 1, 2, 4, 8, 16, float_Inf };

        static float       EditStep       { get { return g_steps[g_clip.EditStep  ]; } }
        static float       EditStepLength { get { return g_steps[g_clip.EditLength]; } }
        static int         EditLength     { get { return (int)(EditStepLength * g_ticksPerStep); } }






        void SetDefaultMachineState()
        {
            ClearSong();

            g_move         = false;


            g_ticksPerStep = 7;



            g_inst.Clear();
            g_inst.Add(new Instrument());
            g_inst[0].Sources.Add(new Source(g_inst[0]));
            g_clip.Patterns.Add(new Pattern(g_clip, g_inst[0]));
            g_clip.Name = "New Clip";


            UpdateSongDsp();

            SetLightColor(4);
            UpdateLights();
        }
    }
}

