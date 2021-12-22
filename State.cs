using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        const  float            float_Inf = 65536f;
        static float[]          g_steps   = { 0.25f, 0.5f, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, float_Inf };
                                
                                
        static long             g_time           = -1; // in ticks
        static float            TimeStep         => (float)g_time / TicksPerStep;
                                
        static bool             g_started        = False,
                                g_init           = False;
                                
        static int              g_curRuntimeTick = 0;
        static float[]          g_runtimeMs      = new float[10];
        static float            g_maxRuntimeMs   = 0;
                                
        float                   g_instCount      = 0,
                                g_dspCount       = 0;
                                
        static float            g_accComplexity  = 0,
                                g_accPolyphony   = 0;
                                          
                                 
        static int              LockView         = 0; // 1 = pattern, 2 = piano
                                                
        static Key              g_editKey        = Key_null;

        static string           SessionName;

        static List<Instrument> Instruments;
        static List<Track>      Tracks;

        static int              TicksPerStep;
                                
        static Clip             EditedClip,
                                ClipCopy;
                                
        static bool             ShowClip,
                                HideClip,
                                MixerShift,
                                SetOrPat, // true = Set, false = Pat
                                SetMemSet,
                                Recording;

        static Setting[]        Sets     = new Setting[nMems];
        static Clip   []        SetClips = new Clip   [nMems];

        static int              ShowMixer, // 0 = session view, 1 = clip volume, 2 = full mixer
                                CueClip,   // 0 = no cue, 1 = pattern cue, 2 = clip cue
                                EditClip;  // 0 = set, 1 = move, 2 = dup, 3 = del


        static bool Playing => OK(Tracks.Find(track => track?.Playing ?? False));



        static void CreateDefaultState(Program prog)
        {
            ClearState();

            SessionName = "New Session";

            CreateMachineState();
            CreateDefaultInstruments(prog);
            CreateDefaultTracks(prog);

            EditedClip = Tracks[0].Clips[0];
            UpdateClipDisplay(EditedClip);
        }



        static void ClearState()
        {
            for (int i = 0; i < nMems; i++)
            { 
                Sets[i]     = Setting_null;
                SetClips[i] = Clip_null;
            }

            g_sm    .StopAll();

            g_notes .Clear();
            g_sounds.Clear();
        }



        //static void ClearTracks()
        //{
        //    foreach (var track in Tracks)
        //    {
        //        track.Stop();

        //        for (int i = 0; i < g_nChans; i++)
        //            track.Clips[i] = Clip_null;
        //    }
        //}



        static void CreateMachineState()
        {
            TicksPerStep = 8; // 113 bpm

            EditedClip   = 
            ClipCopy     = Clip_null;
                         
            ShowClip     = True;

            HideClip     =                         
            MixerShift   =
            SetOrPat     = 
            SetMemSet    =
            Recording    = False;
                         
            ShowMixer    =  0; 
            CueClip      =  2;
            EditClip     = -1;
        }



        static void CreateDefaultInstruments(Program prog)
        {
            Instruments = new List<Instrument>() { new Instrument(prog) };
            Instruments[0].Sources.Add(new Source(Instruments[0]));
        }



        static void CreateTracks(Program prog)
        {
            Tracks = new List<Track>(new Track[4]);
            for (int t = 0; t < Tracks.Count; t++)
                Tracks[t] = new Track(prog);
        }



        static void CreateDefaultTracks(Program prog)
        {
            CreateTracks(prog);

            var track = Tracks[0];
            track.Stop();

            var clip = Clip.Create(track, prog);

            EditedClip     = clip;
            track.Clips[0] = clip;
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

            return Clip_null;
        }



        public void MemSet()
        {
            if (!SetOrPat) SetOrPat  = True;
            else           SetMemSet = !SetMemSet;

            EditedClip.SetMemPat = False;
        }



        void GetSetOrPat(int i)
        {
            if (SetOrPat)
            { 
                if (   SetMemSet
                    && OK(EditedClip.CurSetting))
                {
                    Sets[i]     = EditedClip.CurSetting;
                    SetClips[i] = EditedClip;

                    SetMemSet = False;
                }
                else if (OK(Sets[i]))
                    SwitchToSetting(SetClips[i], Sets[i]);
            }
            else 
                EditedClip.SetMem(i);
        }
    }
}

