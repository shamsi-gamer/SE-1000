using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;


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

        public static T Last<T>(this T[] array)
        {
            return array[array.Length-1];
        }

        public static T Last<T>(this List<T> list)
        {
            return list[list.Count-1];
        }

        public static void RemoveLast<T>(this List<T> list)
        {
            list.RemoveAt(list.Count-1);
        }
    }


    partial class Program
    {                                                                              
        List<Setting> g_settings = new List<Setting>();

                      
        Setting   LastSetting  { get { return g_settings.Count > 0 ? g_settings.Last() : null; } }


        Setting   CurSetting   { get { return CurSet > -1 ? g_settings[CurSet] : null; } }
        Parameter CurParam     { get { return (Parameter)CurSetting; } }


        Harmonics CurHarmonics { get { return (Harmonics)CurSetting; } }
        Harmonics CurOrParentHarmonics
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
        
        Arpeggio CurOrAnyParentArpeggio
        {
            get
            {
                var setting = CurSetting;
                while (setting != null)
                {
                    if (setting.GetType() == typeof(Arpeggio))
                        return (Arpeggio)setting;

                    setting = setting.Parent;
                }

                return null;
            }
        }


        Song CurSong
        {
            get
            {
                return
                    IsCurOrAnyParent(typeof(Arpeggio))
                    ? CurOrAnyParentArpeggio?.Song
                    : g_song;
            }
        }

        static Pattern    CurrentPattern     { get { return g_song.Patterns[CurPat]; } }
        static Channel    CurrentChannel     { get { return CurrentPattern.Channels[CurChan]; } }
        static Instrument CurrentInstrument  { get { return CurrentChannel.Instrument; } }
        static Channel    SelectedChannel    { get { return SelChan > -1 ? CurrentPattern.Channels[SelChan] : null; } }
        static Instrument SelectedInstrument { get { return SelectedChannel?.Instrument ?? null; } }
        static Source     SelectedSource     { get { return CurSrc > -1 ? SelectedInstrument.Sources[CurSrc] : null; } }


        static bool IsParam(Setting setting) 
        {
            if (setting == null) return false;

            return setting.GetType() == typeof(Tune)
                || setting.GetType() == typeof(Parameter); 
        }


        bool IsSettingType(Setting setting, Type type)
        {
            return
                   setting != null
                && setting.GetType() == type;
        }


        static bool HasTag(Setting setting, string tag)
        {
            return 
                   setting != null
                && setting.Tag == tag;
        }


        static bool SettingOrParentHasTag(Setting setting, string tag)
        {
            return HasTag(setting, tag)
                ||    setting.Parent != null
                   && HasTag(setting.Parent, tag);
        }


        static bool SettingOrAnyParentHasTag(Setting setting, string tag)
        {
            while (setting != null)
            {
                if (setting.Tag == tag)
                    return true;

                setting = setting.Parent;
            }

            return false;
        }


        bool IsCurParam()
        {
            return
                   CurSet > -1
                && IsParam(g_settings[CurSet]);
        }


        bool IsCurParam(string tag)
        {
            return 
                   CurSet > -1
                && HasTag(g_settings[CurSet], tag);
        }


        bool IsCurSetting(Type type)
        {
            return
                   CurSet > -1
                && g_settings[CurSet].GetType() == type;
        }


        bool IsCurOrParent(Type type)
        {
            return
                   CurSet > -1
                && (   IsCurSetting(type)
                    ||    CurSetting.Parent != null
                       && CurSetting.Parent.GetType() == type);
        }


        bool IsCurOrAnyParent(Type type)
        {
            if (CurSet < 0) return false;

            var setting = CurSetting;

            while (setting != null)
            {
                if (setting.GetType() == typeof(Arpeggio))
                    return true;

                setting = setting.Parent;
            }

            return false;
        }


        void UpdateDspOffset(ref int off, int pos, int count, int max, int dOff1, int dOff2)
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


        void UpdateSongOff()
        {
            UpdateDspOffset(ref g_songOff, CurPat, g_song.Patterns.Count, maxDspPats, 1, 1);
        }


        void UpdateInstOff(int ch)
        {
            var curInst = g_inst.IndexOf(CurrentPattern.Channels[ch].Instrument);
            UpdateDspOffset(ref g_instOff, curInst, g_inst.Count, maxDspInst, 0, 1);
        }


        void UpdateSrcOff()
        {
            UpdateDspOffset(ref g_srcOff, CurSrc, CurrentInstrument.Sources.Count, maxDspSrc, 0, 0);
        }


        //void UpdateInstOff(int ch)
        //{
        //    var inst = g_inst.IndexOf(CurrentPattern(g_song).Channels[ch].Instrument);

        //    if (   inst >= maxDspInst/2 + instOff
        //        || inst <  maxDspInst/2 + instOff)
        //        instOff = inst - maxDspInst/2 + 1;

        //         if (maxDspInst >= g_inst.Count)           instOff = 0;
        //    else if (instOff >  g_inst.Count - maxDspInst) instOff = g_inst.Count - maxDspInst;
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
        //    else if (g_song.CurSrc >= maxDspSrc + srcOff) srcOff = Math.Max(0, g_song.CurSrc - maxDspSrc + 1);
        //    else if (g_song.CurSrc <  srcOff            ) srcOff = g_song.CurSrc;
        //    else if (srcOff        <  0                 ) srcOff = 0;
        //}


        void SetCurInst(Instrument inst)
        {
            int first, last;
            GetPatterns(g_song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
                g_song.Patterns[p].Channels[CurChan].Instrument = inst;
        }


        bool IsModPresent()
        {
            return _nextToLoad < 10
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
            var tune = SelectedSource    ?.Tune
                    ?? SelectedInstrument?.Tune;

            return
                   g_piano
                ||    g_chordEdit 
                   && g_chord > -1
                ||    IsCurParam("Tune")
                   && (tune?.UseChord ?? false)
                   && !(g_paramKeys || g_paramAuto)
                || IsCurOrParent(typeof(Arpeggio));
        }}


        IMyTextPanel GetLightFromNote(int num)
        {
            num /= NoteScale;
            num -= 60;
            num -= CurrentChannel.Transpose * 12;

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
            var mod = (g_mixerShift ? 10 : 1) * dv;
            g_volume = MinMax(0, g_volume + dVol * mod, 2);

            MarkLight(
                dv > 0 
                ? lblMixerVolumeUp 
                : lblMixerVolumeDown);
        }


        void SetVolume(Song song, int ch, float dv)
        {
            var vol = CurrentPattern.Channels[ch].Volume;
            var mod = (g_mixerShift ? 10 : 1) * dv;

            int first, last;
            GetPatterns(song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
            {
                var chan = song.Patterns[p].Channels[ch];
                chan.Volume = MinMax(0, vol + dVol * mod, 2);
            }

            mixerPressed.Add(ch);
        }


        void UpdateInstName(bool add = true)
        {
            if (   CurPat  > -1
                && SelChan > -1)
                dspMain.Surface.WriteText(add ? SelectedChannel.Instrument.Name : "", false);
        }


        float GetBPM()
        {
            return 120f / (g_ticksPerStep * nSteps) * 120f;
        }


        void StopEdit(Song song)
        {
            if (song.EditNotes.Count > 0)
                g_hold = false;

            song.EditNotes.Clear();

            UpdateHoldLight();
        }


        void LimitRecPosition(Song song)
        {
            int st, nx;
            GetPosLimits(song, CurPat, out st, out nx);

                 if (song.EditPos >= nx) song.EditPos -= nx - st;
            else if (song.EditPos <  st) song.EditPos += nx - st;

            var cp = (int)(song.EditPos / nSteps);
            if (cp != CurPat) SetCurrentPattern(cp);
        }


        void GetPatterns(Song song, int p, out int f, out int l)
        {
            f = p;
            l = p;

            if (g_allPats)
            {
                var b = song.GetBlock(p);

                if (   g_block
                    && b != null)
                {
                    f = b.First;
                    l = b.Last;
                }
                else
                {
                    f = 0;
                    l = song.Patterns.Count-1;
                }
            }
        }


        void Lock()
        {
            foreach (var l in g_locks)
                l.ToggleLock();

            UpdateLockLights();
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

            UpdateTimerLight();
        }


        void Gyro()
        {
            var on = g_gyros[0].Enabled;

            foreach (var gyro in g_gyros)
                gyro.Enabled = !on;

            UpdateGyroLight();
        }


        void AutoLock()
        {
            var auto = false;

            foreach (var l in g_locks) auto |= l.AutoLock;
            foreach (var l in g_locks) l.AutoLock = !auto;

            UpdateLight(lblAutoLock, g_locks.Find(l => l.AutoLock) != null);
        }


        void ToggleLight()
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

            MarkLight(lblFold);
        }


        static long GetPatTime(int pat) 
        {
            return pat * nSteps * g_ticksPerStep; 
        } 


        static float note2freq(int note)
        {
            return 440 * (float)Math.Pow(2, (note/(float)NoteScale - 69) / 12f);
        }


        static int freq2note(double freq)
        {
            return (int)Math.Round((12 * Math.Log(freq / 440, 2) + 69) * NoteScale);
        }
    }
}
