using Sandbox.ModAPI.Ingame;
using System;


namespace IngameScript
{
    partial class Program
    {
        public void Main(string arg, UpdateType update)
        {
            if (arg.Length > 0)
            { 
                ProcessArg(arg);
                return;
            }


            if (!g_init)
                return;


            //pnlInfoLog.CustomData = "";

            FinishStartup();


            _triggerDummy.Clear();


            if ((update & UpdateType.Update1) != 0)
            {
                if (CurSet > -1)
                    g_settings[CurSet].AdjustFromController(g_song, this);

                if (!TooComplex) UpdatePlayback();

                if (    PlayTime > -1
                    && !TooComplex)
                    UpdateKeyLights();
            }


            if ((update & UpdateType.Update10) != 0)
            {
                if (    PlayTime < 0
                    && _nextToLoad > 10
                    && !TooComplex)
                    UpdateKeyLights();


                if (g_started)
                {
                    UpdateInst();
                    UpdateSongName();
                }
                else
                    g_started = true;


                if (_nextToLoad > 10)
                { 
                    if (!TooComplex) DrawMain();
                    if (!TooComplex) DrawInfo();
                    if (!TooComplex) DrawSongDsp();
                    if (!TooComplex) DrawMixer();
                    if (!TooComplex) DrawIO();

                    DampenDisplayVolumes();
                }


                ResetRuntimeInfo();


                dspCount = instCount;
                instCount = 0;


                UnmarkAllLights(); // by this point they have been visually marked on previous cycle


                warningLight.Enabled = g_sm.UsedRatio > 0.9f;
            }


            if (_nextToLoad > 10)
                FinalizePlayback(g_song);


            instCount = Math.Max(instCount, Runtime.CurrentInstructionCount);

            
            //pnlInfoLog.CustomData = "";


            if ((update & UpdateType.Update1) != 0)
                UpdateRuntimeInfo();
        }


        void UpdateRuntimeInfo()
        {
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
            g_maxRuntimeMs = 0;
        }
    }
}
