using Sandbox.ModAPI.Ingame;
using System;


namespace IngameScript
{
    partial class Program
    {
        public void Main(string arg, UpdateType update)
        {
            pnlInfoLog.CustomData = "";


            if (!g_init) return;
            FinishStartup();


            if (ProcessArg(arg))
                return;


            _triggerDummy.Clear();


            if (_loadStep > OscCount)
            {
                if (OK(g_ioAction)) UpdateIO();
                else                Update1();
                
                if ((update & UpdateType.Update10) != 0) 
                    Update10();

                //if (!OK(g_ioAction))
                    UpdateTime();

                UpdateRuntimeInfo();
            }


            //pnlInfoLog.CustomData = "";
        }


        void Update1()
        {
            CurSetting?.AdjustFromController(EditedClip, this);

            if (!TooComplex)
                UpdatePlayback();

            if (    ShowClip
                && !TooComplex)
            { 
                foreach (var lbl in g_fastLabels)
                    lbl.Update();
            }
        }


        void Update10()
        {
            if (g_started)
            {
                UpdateInst();
                UpdateClipName();
            }
            else
                g_started = True;


            if (_loadStep > 10)
            {
                DrawDisplays();
                

                if (!TooComplex)
                    foreach (var lbl in g_slowLabels) lbl.Update();

                if (!TooComplex)
                { 
                    if (ShowClip)
                    { 
                        foreach (var lbl in g_clipLabels)   lbl.Update();
                        foreach (var lbl in g_adjustLabels) lbl.Update();
                    }
                    
                    if (HideClip)
                    {
                        ClearLabels(g_fastLabels);
                        ClearLabels(g_clipLabels);
                        ClearLabels(g_adjustLabels);

                        HideClip = False;
                    }
                }


                if (!TooComplex)
                { 
                    foreach (var track in Tracks)
                        track.DampenDisplayVolumes();

                    UnmarkAllLabels();
                }
            }


            ResetRuntimeInfo();


            g_dspCount  = g_instCount;
            g_instCount = 0;


            warningLight.Enabled = 
                   TooComplex 
                || g_sm.UsedRatio > 0.9f;
        }


        void UpdateTime()
        {
            g_time++;

            foreach (var lfo in g_lfo) if (!TooComplex) lfo.AdvanceTime();
            foreach (var mod in g_mod) if (!TooComplex) mod.AdvanceTime();

            foreach (var track in Tracks)
            { 
                if (   Playing
                    && OK(track.PlayTime))
                    track.PlayTime++;
            }
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
