using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        const  float   float_Inf = 65536f;
        static float[] g_steps   = { 0.25f, 0.5f, 1, 2, 4, 8, 16, float_Inf };


        static long    g_time           = -1; // in ticks
        static float   TimeStep         => (float)g_time / TicksPerStep;

        static bool    g_started        = F,
                       g_init           = F;

        static int     g_curRuntimeTick = 0;
        static float[] g_runtimeMs      = new float[6];
        static float   g_maxRuntimeMs   = 0;

        float          g_instCount      = 0,
                       g_dspCount       = 0;

                        
        static int     LockView       = 0; // 1 = pattern, 2 = piano

        Key            g_editKey        = null;


        static string           SessionName;

        static List<Instrument> Instruments;
        static List<Track>      Tracks;

        static int              TicksPerStep;
                                
        static Clip             EditedClip,
                                ClipCopy;
                                
        static bool             ShowSession,
                                MixerShift,
                                Move;
                                
        static int              EditClip; // 0 = edit, 1 = dup, 2 = del

                 

        static bool IsPlaying => OK(Tracks.Find(track => track?.IsPlaying ?? F));


        static void InitMachineState()
        {
            SessionName = "New Session";

            Instruments = new List<Instrument>();

            CreateMachineState();
            CreateDefaultInstruments();
            CreateDefaultTracks();
        }


        static void CreateDefaultMachineState()
        {
            ClearMachineState();
            ClearTracks();

            InitMachineState();
        }


        static void ClearMachineState()
        {
            g_sm    .StopAll();

            g_notes .Clear();
            g_sounds.Clear();
        }


        static void ClearTracks()
        {
            if (!OK(Tracks))
                return;

            foreach (var track in Tracks)
            {
                for (int i = 0; i < g_nChans; i++)
                    track.Clips[i] = null;

                track.PlayClip = -1;
                track.NextClip = -1;
            }
        }


        static void CreateMachineState()
        {
            TicksPerStep = 8; // 113 bpm

            EditedClip   = null;
            ClipCopy     = null;
                         
            ShowSession  = T;
            MixerShift   = F;
            Move         = F;
                         
            EditClip     = -1;
        }


        static void CreateDefaultInstruments()
        {
            Instruments.Clear();

            Instruments.Add(new Instrument());
            Instruments[0].Sources.Add(new Source(Instruments[0]));
        }


        static void CreateDefaultTracks()
        {
            Tracks = new List<Track>(new Track[4]);

            for (int i = 0; i < Tracks.Count; i++)
                Tracks[i] = new Track();

            var track = Tracks[0];
            var clip  = new Clip(track);

            clip.Patterns.Add(new Pattern(Instruments[0], clip));

            EditedClip     = clip;
            track.Clips[0] = clip;

            track.PlayClip = -1;
            track.NextClip = -1;
        }


        static Clip GetClipAfterDelete(Clip clip)
        {
            var iTrack = Tracks.IndexOf(clip.Track);

            while (iTrack >= 0)
            { 
                var track = Tracks[iTrack--];

                var clips = track.Clips;
                var iClip = clips.IndexOf(clip);

                for (int i = iClip-1; i >= 0; i--)
                {
                    if (OK(clips[i]))
                        return clips[i];
                }

                for (int i = iClip+1; i < track.Clips.Length; i++)
                {
                    if (OK(clips[i]))
                        return clips[i];
                }
            }

            return null;
        }


        void ToggleSession()
        {
            if (ShowSession) 
            {
                EditClip = 
                    EditClip != 0
                    ?  0
                    : -1;
            }
            else
            { 
                ShowSession =  T;
                EditClip    = -1;
            }
        }
    }
}

