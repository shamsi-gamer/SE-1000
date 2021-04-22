using Sandbox.ModAPI.Ingame;
using System;


namespace IngameScript
{
    partial class Program
    {
        public void Main(string arg, UpdateType update)
        {
            pnlInfoLog.CustomData = "";


            if (arg.Length > 0)
            { 
                ProcessArg(arg);
                return;
            }

            if (!g_init) return;

            FinishStartup();

            _triggerDummy.Clear();

            if ((update & UpdateType.Update1)  != 0) Update1();
            if ((update & UpdateType.Update10) != 0) Update10();

            if (_loadStep > 10)
            {
                g_time++;

                foreach (var lfo in g_lfo) lfo.AdvanceTime();
                foreach (var mod in g_mod) mod.AdvanceTime();

                g_song.FinalizePlayback();
            }

            UpdateRuntimeInfo();


            //pnlInfoLog.CustomData = "";
        }


        void Update1()
        {
            CurSetting?.AdjustFromController(g_song, this);

            if (!TooComplex) 
                UpdatePlayback();

            if (OK(g_song.PlayTime)) 
                UpdateKeyLights();
        }


        void Update10()
        { 
            if (   g_song.PlayTime < 0
                && _loadStep > 10
                && !TooComplex)
                UpdateKeyLights();


            if (g_started)
            {
                UpdateInst();
                UpdateSongName();
            }
            else
                g_started = true;


            if (_loadStep > 10)
            {
                DrawDisplays();
                DampenDisplayVolumes();
            }


            ResetRuntimeInfo();


            g_dspCount = g_instCount;
            g_instCount = 0;

            UpdateLockLights(); // catch the locks being set by autolock
            UnmarkAllLights();  // by this point they have been visually marked at a previous cycle


            warningLight.Enabled = g_sm.UsedRatio > 0.9f;
        }


        void UpdateRuntimeInfo()
        {
            g_instCount = Math.Max(g_instCount, Runtime.CurrentInstructionCount);

            if (g_curRuntimeTick >= g_runtimeMs.Length)
                return;

            var runMs = (float)Runtime.LastRunTimeMs;

            g_runtimeMs[g_curRuntimeTick++] = runMs;
            g_maxRuntimeMs = Math.Max(g_maxRuntimeMs, runMs);
        }


        void ResetRuntimeInfo()
        {
            for (int i = 0; i < g_runtimeMs.Length; i++)
                g_runtimeMs[i] = 0;

            g_curRuntimeTick = 0;
            g_maxRuntimeMs   = 0;
        }
    }
}
