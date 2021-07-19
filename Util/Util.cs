using System;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;


namespace IngameScript
{
    public static class Extensions
    {
        public static T[] Subarray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

        public static T    Last      <T>(this T[] array)         { return array[array.Length-1]; }
        public static T    Last      <T>(this List<T> list)      { return list[list.Count-1]; }

        public static void RemoveLast<T>(this List<T> list)      { list.RemoveAt(list.Count-1); }

        public static int  IndexOf   <T>(this T[] array, T item) { return Array.IndexOf(array, item); }
        public static bool Contains  <T>(this T[] array, T item) { return Array.IndexOf(array, item) > -1; }
    }


    partial class Program
    {                                                                              
                             

        static void UpdateDspOffset(ref int off, int pos, int count, int max, int dOff1, int dOff2)
        {
            if (   pos >= max/2         + off
                || pos <  max/2 - dOff1 + off)
                off = pos - max/2 + dOff2;

                 if (max >= count      ) off = 0;
            else if (off >  count - max) off = count - max;
            else if (pos >= max + off  ) off = Math.Max(0, pos - max + 1);
            else if (pos <  off        ) off = pos;
            else if (off <  0          ) off = 0;
        }


        bool IsModPresent()
        {
            return _loadStep < 10
                ||    (OscSine         ?.Samples.Count ?? 0) > 0
                   && (OscTriangle     ?.Samples.Count ?? 0) > 0
                   && (OscSaw          ?.Samples.Count ?? 0) > 0
                   && (OscSquare       ?.Samples.Count ?? 0) > 0
                   && (OscLowNoise     ?.Samples.Count ?? 0) > 0
                   && (OscHighNoise    ?.Samples.Count ?? 0) > 0
                   && (OscBandNoise    ?.Samples.Count ?? 0) > 0
                   && (OscSlowSweepDown?.Samples.Count ?? 0) > 0
                   && (OscFastSweepDown?.Samples.Count ?? 0) > 0
                   && (OscSlowSweepUp  ?.Samples.Count ?? 0) > 0
                   && (OscFastSweepUp  ?.Samples.Count ?? 0) > 0
                   && (OscCrunch       ?.Samples.Count ?? 0) > 0;
        }


        static string GetNewName(string name, Func<string, bool> exists)
        {
            if (!exists(name))
                return name;


            var numLength = GetNumLength(name);

            if (numLength > 0)
            {
                var len = name.Length - numLength;
                var num = int_Parse(name.Substring(len));

                string newName = "";
                while (newName == "" || exists(newName))
                    newName = name.Substring(0, len) + S(++num);

                return newName;
            }

            else if (numLength == 0)
                return name + " 2";

            else
                return name;
        }



        static void GetNewClipName(Clip clip, Clip[] clips)
        {
            clip.Name = GetNewName(clip.Name, newName => 
                Array.Exists(clips, c => 
                       OK(c) 
                    && c.Name == newName));
        }



        static Track GetAnyCurrentPlayTrack()
        {
            return Tracks.Find(t => OK(t.PlayTime));
        }

        

        static long GetAnyCurrentPlayTime()
        {
            var track = GetAnyCurrentPlayTrack();

            return 
                OK(track) 
                ? track.PlayTime
                : long_NaN;
        }

        

        static bool EditedClipIsPlaying => EditedClip.Index == EditedClip.Track.PlayClip;



        static bool ShowPiano { get 
        {
            var tune = SelSource    ?.Tune
                    ?? SelInstrument?.Tune;

            return
                   EditedClip.Piano
                ||    EditedClip.ChordEdit 
                   && OK(EditedClip.Chord)
                ||    IsCurParam(strTune)
                   && (tune?.UseChord ?? False)
                   && !(   EditedClip.ParamKeys 
                        || EditedClip.ParamAuto);
        }}



        float GetBPM()
        {
            return 120f / (TicksPerStep * g_patSteps) * 120f;
        }



        void Lock()
        {
            foreach (var l in g_locks)
                l.ToggleLock();
        }



        void NoiseEmitters()
        {
            NoiseEmitters(!g_timers[0].Enabled);
        }



        void NoiseEmitters(bool on)
        {
            g_lightPiston.Enabled = on;
            g_lightHinge1.Enabled = on;
            g_lightHinge2.Enabled = on;
            g_hingeL     .Enabled = on;
            g_hingeR     .Enabled = on;

            foreach (var timer in g_timers)
                timer.Enabled = on;
        }



        void AutoLock()
        {
            var auto = False;

            foreach (var l in g_locks) auto |= l.AutoLock;
            foreach (var l in g_locks) l.AutoLock = !auto;
        }



        void ToggleLabel()
        {
            var open  = Get("Timer Open 1")  as IMyTimerBlock;
            var close = Get("Timer Close 1") as IMyTimerBlock;

            var p = g_lightPiston;

            if (   !OK(p)
                || !OK(open)
                || !OK(close))
                return;


            NoiseEmitters(True);


            if (p.CurrentPosition <= (p.MinLimit + p.MaxLimit) / 2) open .Trigger();
            else                                                    close.Trigger();
        }



        void ToggleFold()
        {
            var hinge = Get("Hinge R") as IMyMotorStator;

            var fold  = Get("Timer Fold 1")    as IMyTimerBlock;
            var recl  = Get("Timer Recline 1") as IMyTimerBlock;

            if (   !OK(fold)
                || !OK(recl))
                return;

            if (hinge.Angle > (hinge.LowerLimitRad + hinge.UpperLimitRad) / 2) fold.Trigger();
            else                                                               recl.Trigger();

            lblFold.Mark();
        }



        static long GetPatTime(int pat) 
        {
            return pat * g_patSteps * TicksPerStep; 
        } 



        static float note2freq(int note)
        {
            return 440 * (float)Math.Pow(2, (note/(float)NoteScale - 69) / 12f);
        }



        static int freq2note(double freq)
        {
            return (int)Math.Round((12 * Math.Log(freq / 440, 2) + 69) * NoteScale);
        }



        //static float dbAdd(float a, float b)
        //{
        //    return (float)(10 * Math.Log10(Math.Pow(10, a/10) + Math.Pow(10, b/10)));
        //}



        static float sndAdd(float a, float b)
        {
            return 1 + (float)Math.Log10(a + b);
        }



        static void Swap<T>(ref T clip1, ref T clip2) 
        {
            var swap  = clip2;
                clip2 = clip1;
                clip1 = swap;
        }



        bool TooComplex =>
               Runtime.CurrentCallChainDepth   / (float)Runtime.MaxCallChainDepth   > 0.8f
            || Runtime.CurrentInstructionCount / (float)Runtime.MaxInstructionCount > 0.8f;



        void             Get<T>(List<T> blocks)                          where T : class { GridTerminalSystem.GetBlocksOfType(blocks);            }
        void             Get<T>(List<T> blocks, Func<T, bool> condition) where T : class { GridTerminalSystem.GetBlocksOfType(blocks, condition); }


        IMyTerminalBlock Get       (string s)             { return GridTerminalSystem.GetBlockWithName(s); }
        IMyMotorBase     GetMotor  (string s)             { return Get(s) as IMyMotorBase; }
        IMyMotorBase     GetHinge  (string s)             { return GetMotor("Hinge " + s); }
        IMyTextPanel     GetLcd    (string s)             { return Get(s) as IMyTextPanel; }
        IMyTextPanel     GetLabel  (string s)             { return GetLcd("Label " + s); }
        IMyTextPanel     GetDisplay(string s, int i = -1) { return GetLcd(s + " Display" + (OK(i) ? strEmpty + S(i) : "")); }


        static void SkipWhiteSpace(string[] lines, ref int line)
        {
            while (line < lines.Length
                && lines[line].Trim() == "") line++;
        }


        static int   int_Parse(string str) => int.Parse(str);
        static bool  int_TryParse(string str, out int val) => int.TryParse(str, out val);

        static bool long_TryParse(string str, out long val)
        {
            if (str == "?")
            { 
                val = long_NaN;
                return True;
            }
            else
                return long.TryParse(str, out val);
        }



        static bool IsPressed(Label lbl) { return g_labelsPressed.Contains(lbl); }
        static bool IsPressed(int   lbl) { return    g_lcdPressed.Contains(lbl); }



        static bool SessionHasClips => Tracks.Exists(t => Array.Exists(t.Clips, c => OK(c)));



        void ResetLfos()
        {
            if (TooComplex) return;

            foreach (var lfo in g_lfo)
            {
                if (Math.Abs(lfo.Offset.Value) > 0.001)
                    lfo.Phase = 0;
            }
        }



        static bool ControllerIsActive()
        { 
            var mi = g_remote.MoveIndicator;
            var ri = g_remote.RotationIndicator;

            var min = 0.0001f;

            return 
                   Math.Abs(mi.X) > min
                || Math.Abs(mi.Z) > min

                || Math.Abs(ri.X) > min
                || Math.Abs(ri.Y) > min;
        }
    }
}