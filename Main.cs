using Sandbox.ModAPI.Ingame;
using System;


namespace IngameScript
{
    partial class Program
    {
        public void Main(string arg, UpdateType update)
        {
            //pnlInfoLog.CustomData = "";


            if (!g_init) return;
            FinishStartup();

            if (arg.Length > 0)
            {
                ProcessArg(arg);
                return;
            }

            _triggerDummy.Clear();

            if (_loadStep > OscCount)
            {
                if ((update & UpdateType.Update1)  != 0) Update1();
                if ((update & UpdateType.Update10) != 0) Update10();

                g_time++;

                foreach (var lfo in g_lfo) lfo.AdvanceTime();
                foreach (var mod in g_mod) mod.AdvanceTime();

                foreach (var track in g_session.Tracks)
                    track.FinalizePlayback();
            }

            UpdateRuntimeInfo();


            //pnlInfoLog.CustomData = "";
        }


        void Update1()
        {
            CurSetting?.AdjustFromController(EditedClip, this);

            if (!TooComplex)
                UpdatePlayback();

            //foreach (var track in g_session.Tracks)
            //    track.UpdateNotesArePlaying();

            if (!TooComplex)
                foreach (var lbl in g_fastLabels)
                    lbl.Update();
        }


        void Update10()
        {
            if (g_started)
            {
                UpdateInst();
                UpdateSongName();
            }
            else
                g_started = T;


            if (_loadStep > 10)
            {
                DrawDisplays();
                

                if (!TooComplex)
                    foreach (var lbl in g_slowLabels)
                        lbl.Update();


                foreach (var track in g_session.Tracks)
                {
                    track.DampenDisplayVolumes();
                    //track.NotesArePlaying = F;
                }


                if (!TooComplex)
                    UnmarkAllLabels();
            }


            ResetRuntimeInfo();


            g_dspCount = g_instCount;
            g_instCount = 0;


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
