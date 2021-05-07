using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
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
        static List<Setting> g_settings = new List<Setting>();

                      
        static Setting       LastSetting  { get { return g_settings.Count > 0 ? g_settings.Last() : null; } }
                             
                             
        static Setting       CurSetting   { get { return CurSet > -1 ? g_settings[CurSet] : null; } }
        static Parameter     CurParam     { get { return (Parameter)CurSetting; } }
        static Modulate      CurModulate  { get { return (Modulate) CurSetting; } }


        static Harmonics     CurHarmonics { get { return (Harmonics)CurSetting; } }
        static Harmonics     CurOrParentHarmonics
        {
            get
            {
                return
                    IsCurSetting(typeof(Harmonics))
                    ? CurHarmonics
                    : (Harmonics)CurSetting.Parent;
            }
        }


        Arpeggio CurArpeggio { get { return (Arpeggio)CurSetting; } }
        Arpeggio CurOrParentArpeggio
        {
            get
            {
                return
                    IsCurSetting(typeof(Arpeggio))
                    ? CurArpeggio
                    : (Arpeggio)CurSetting.Parent;
            }
        }
        

        //Arpeggio CurOrAnyParentArpeggio
        //{
        //    get
        //    {
        //        var setting = CurSetting;
        //        while (setting != null)
        //        {
        //            if (setting.GetType() == typeof(Arpeggio))
        //                return (Arpeggio)setting;

        //            setting = setting.Parent;
        //        }

        //        return null;
        //    }
        //}


        static bool IsCurParam()
        {
            return IsParam(CurSetting);
        }


        static bool IsCurParam(string tag)
        {
            return HasTag(CurSetting, tag);
        }


        static bool IsCurSetting(Type type)
        {
            //return
            //       CurSet > -1
            //    && g_settings[CurSet].GetType() == type;

            return CurSetting?.GetType() == type;
        }


        bool IsCurOrParentSetting(Type type)
        {
            return
                   CurSet > -1
                && (   IsCurSetting(type)
                    ||    CurSetting.Parent != null
                       && CurSetting.Parent.GetType() == type);
        }


        //bool IsCurOrAnyParent(Type type)
        //{
        //    if (CurSet < 0) return false;

        //    var setting = CurSetting;

        //    while (setting != null)
        //    {
        //        if (setting.GetType() == typeof(Arpeggio))
        //            return true;

        //        setting = setting.Parent;
        //    }

        //    return false;
        //}


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


        //void UpdateSongOff(int pat)
        //{
        //    if (   pat >= maxDspPats/2     + songOff
        //        || pat <  maxDspPats/2 - 1 + songOff)
        //        songOff = pat - maxDspPats / 2 + 1;

        //    var nPats = g_song.Patterns.Count;

        //         if (maxDspPats >= nPats)             songOff = 0;
        //    else if (songOff >  nPats - maxDspPats)   songOff = nPats - maxDspPats;
        //    else if (pat     >= maxDspPats + songOff) songOff = Math.Max(0, pat - maxDspPats + 1);
        //    else if (pat     <  songOff)              songOff = pat;
        //    else if (songOff <  0)                    songOff = 0;
        //}


        static void UpdateSongOff()
        {
            UpdateDspOffset(
                ref CurClip.SongOff, 
                CurPat, 
                CurClip.Patterns.Count, 
                maxDspPats, 
                1,
                1);
        }


        void UpdateInstOff(int ch)
        {
            var curInst = g_session.Instruments.IndexOf(CurClip.CurrentPattern.Channels[ch].Instrument);
            UpdateDspOffset(ref CurClip.InstOff, curInst, g_session.Instruments.Count, maxDspInst, 0, 1);
        }


        void UpdateSrcOff()
        {
            UpdateDspOffset(
                ref CurClip.SrcOff, 
                CurClip.CurSrc, 
                CurClip.CurrentInstrument.Sources.Count, 
                maxDspSrc, 
                0,
                0);
        }


        //void UpdateInstOff(int ch)
        //{
        //    var inst = g_session.Instruments.IndexOf(CurrentPattern(g_song).Channels[ch].Instrument);

        //    if (   inst >= maxDspInst/2 + instOff
        //        || inst <  maxDspInst/2 + instOff)
        //        instOff = inst - maxDspInst/2 + 1;

        //         if (maxDspInst >= g_session.Instruments.Count)           instOff = 0;
        //    else if (instOff >  g_session.Instruments.Count - maxDspInst) instOff = g_session.Instruments.Count - maxDspInst;
        //    else if (inst    >= maxDspInst + instOff)      instOff = Math.Max(0, inst - maxDspInst + 1);
        //    else if (inst    <  instOff)                   instOff = inst;
        //    else if (instOff <  0)                         instOff = 0;
        //}


        //void UpdateSrcOff(Instrument inst, int src)
        //{
        //    var nSrc = inst.Sources.Count;

        //    if (   src >= maxDspSrc/2 + srcOff
        //        || src <  maxDspSrc/2 + srcOff)
        //        srcOff = src - maxDspSrc/2;

        //         if (maxDspSrc        >= nSrc           ) srcOff = 0;
        //    else if (srcOff        >  nSrc - maxDspSrc  ) srcOff = nSrc - maxDspSrc;
        //    else if (CurClip.CurSrc >= maxDspSrc + srcOff) srcOff = Math.Max(0, g_song.CurClip.CurSrc - maxDspSrc + 1);
        //    else if (CurClip.CurSrc <  srcOff            ) srcOff = g_song.CurClip.CurSrc;
        //    else if (srcOff        <  0                 ) srcOff = 0;
        //}


        void SetCurInst(Instrument inst)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                CurClip.Patterns[p].Channels[CurClip.CurChan].Instrument = inst;
        }


        bool IsModPresent()
        {
            return _loadStep < 10
                ||    (OscSine     ?.Samples.Count ?? 0) > 0
                   && (OscTriangle ?.Samples.Count ?? 0) > 0
                   && (OscSaw      ?.Samples.Count ?? 0) > 0
                   && (OscSquare   ?.Samples.Count ?? 0) > 0
                   && (OscLowNoise ?.Samples.Count ?? 0) > 0
                   && (OscHighNoise?.Samples.Count ?? 0) > 0
                   && (OscBandNoise?.Samples.Count ?? 0) > 0
                   && (OscClick    ?.Samples.Count ?? 0) > 0
                   && (OscCrunch   ?.Samples.Count ?? 0) > 0;
        }


        string GetNewName(string name, Func<string, bool> exists)
        {
            var numLength = GetNumLength(name);
            //numLength = GetNumLength(name);

            if (numLength > 0)
            {
                var len = name.Length - numLength;
                var num = int.Parse(name.Substring(len));

                string newName = "";
                while (newName == "" || exists(newName))
                    newName = name.Substring(0, len) + S(++num);

                return newName;
            }
            else
            {
                if (exists(name)
                    && numLength == 0)
                    name += " 2";

                return name;
            }
        }


        bool ShowPiano { get 
        {
            if (g_session == null)
                return false;

            var tune = CurClip.SelectedSource    ?.Tune
                    ?? CurClip.SelectedInstrument?.Tune;

            return
                   CurClip.Piano
                ||    CurClip.ChordEdit 
                   && CurClip.Chord > -1
                ||    IsCurParam(strTune)
                   && (tune?.UseChord ?? false)
                   && !(   CurClip.ParamKeys 
                        || CurClip.ParamAuto)
                ||    IsCurOrParentSetting(typeof(Arpeggio));
        }}


        Label GetLabelFromNote(int num)
        {
            num /= NoteScale;
            num -= 60;
            num -= CurChannel.Transpose * 12;

            switch (num)
            {
                case  0: return lblLow[ 0];
                case  2: return lblLow[ 1];
                case  4: return lblLow[ 2];
                case  5: return lblLow[ 3];
                case  7: return lblLow[ 4];
                case  9: return lblLow[ 5];
                case 11: return lblLow[ 6];
                case 12: return lblLow[ 7];
                case 14: return lblLow[ 8];
                case 16: return lblLow[ 9];
                case 17: return lblLow[10];
                case 19: return lblLow[11];
                case 21: return lblLow[12];
                case 23: return lblLow[13];
                case 24: return lblLow[14];

                case  1: return lblHigh[0];
                case  3: return lblHigh[1];
                case  6: return lblHigh[2];
                case  8: return lblHigh[3];
                case 10: return lblHigh[4];
                case 13: return lblHigh[5];
                case 15: return lblHigh[6];
                case 18: return lblHigh[7];
                case 20: return lblHigh[8];
                case 22: return lblHigh[9];
            }

            return null;
        }


        void SetVolumeAll(float dv)
        {
            var mod = (CurClip.MixerShift ? 10 : 1) * dv;
            CurClip.Volume = MinMax(0, CurClip.Volume + dVol * mod, 2);

            //MarkLabel(
                //dv > 0
                //? lblMixerVolumeUp
                //: lblMixerVolumeDown);
        }


        static void UpdateInstName(bool add = true)
        {
            if (   CurClip.CurPat  > -1
                && CurClip.SelChan > -1)
                dspMain.Panel.WriteText(add ? CurClip.SelectedChannel.Instrument.Name : "", false);
        }


        float GetBPM()
        {
            return 120f / (g_session.TicksPerStep * g_nSteps) * 120f;
        }


        void Lock()
        {
            foreach (var l in g_locks)
                l.ToggleLock();

            //UpdateLockLabels();
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

            foreach (var timer in g_timers)
                timer.Enabled = on;

            //UpdateTimerLabel();
        }


        void Gyro()
        {
            var on = g_gyros[0].Enabled;

            foreach (var gyro in g_gyros)
                gyro.Enabled = !on;

            //UpdateGyroLabel();
        }


        void AutoLock()
        {
            var auto = false;

            foreach (var l in g_locks) auto |= l.AutoLock;
            foreach (var l in g_locks) l.AutoLock = !auto;

            //UpdateLabel(lblAutoLock, g_locks.Find(l => l.AutoLock) != null);
        }


        void ToggleLabel()
        {
            var open  = Get("Timer Open 1")  as IMyTimerBlock;
            var close = Get("Timer Close 1") as IMyTimerBlock;

            var p = g_lightPiston;

            if (   p     == null
                || open  == null
                || close == null)
                return;


            NoiseEmitters(true);


            if (p.CurrentPosition <= (p.MinLimit + p.MaxLimit) / 2) open .Trigger();
            else                                                    close.Trigger();
        }


        void ToggleFold()
        {
            var hinge = Get("Hinge R") as IMyMotorStator;

            var fold = Get("Timer Fold 1")    as IMyTimerBlock;
            var recl = Get("Timer Recline 1") as IMyTimerBlock;

            if (   fold == null
                || recl == null)
                return;

            if (hinge.Angle > (hinge.LowerLimitRad + hinge.UpperLimitRad) / 2) fold.Trigger();
            else                                                               recl.Trigger();

            lblFold.Mark();
        }


        static long GetPatTime(int pat) 
        {
            return pat * g_nSteps * g_session.TicksPerStep; 
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


        bool TooComplex { get { return 
               Runtime.CurrentCallChainDepth   / (float)Runtime.MaxCallChainDepth   > 0.8f
            || Runtime.CurrentInstructionCount / (float)Runtime.MaxInstructionCount > 0.8f; } }


        void             Get<T>(List<T> blocks)                          where T : class { GridTerminalSystem.GetBlocksOfType(blocks);            }
        void             Get<T>(List<T> blocks, Func<T, bool> condition) where T : class { GridTerminalSystem.GetBlocksOfType(blocks, condition); }

        IMyTerminalBlock Get   (string s)             { return GridTerminalSystem.GetBlockWithName(s); }
        IMyTextPanel     GetLcd(string s)             { return Get(s) as IMyTextPanel; }
        IMyTextPanel     Lbl   (string s)             { return GetLcd("Label " + s); }
        IMyTextPanel     Dsp   (string s, int i = -1) { return GetLcd(s + " Display" + (i > -1 ? " " + S(i) : "")); }


        static void SkipWhiteSpace(string[] lines, ref int line)
        {
            while (line < lines.Length
                && lines[line].Trim() == "") line++;
        }


        static bool long_TryParse(string str, out long val)
        {
            if (str == "?")
            { 
                val = long_NaN;
                return true;
            }
            else
                return long.TryParse(str, out val);
        }


        static bool IsPressed(Label lbl) { return g_labelsPressed.Contains(lbl); }


        static Clip    CurClip    { get { return g_session.CurClip;      } }
                                                                         
        static int     CurPat     { get { return CurClip.CurPat;         } }
        static int     CurChan    { get { return CurClip.CurChan;        } }
        static int     SelChan    { get { return CurClip.SelChan;        } }
        static int     CurSrc     { get { return CurClip.CurSrc;         } }
        static int     CurSet     { get { return CurClip.CurSet;         } }

        static float   PlayStep   { get { return CurClip.PlayStep;       } }
        static int     PlayPat    { get { return CurClip.PlayPat;        } }

        static Channel CurChannel { get { return CurClip.CurrentChannel; } }
    }
}
