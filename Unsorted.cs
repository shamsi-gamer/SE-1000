﻿using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        static bool IsCurParam()
        {
            return IsParam(EditedClip.CurSetting);
        }


        static bool IsCurParam(string tag)
        {
            return HasTag(EditedClip.CurSetting, tag);
        }


        static bool IsCurSetting(Type type)
        {
            return EditedClip.CurSetting?.GetType() == type;
        }


        static void UpdateClipOff()
        {
            UpdateDspOffset(
                ref EditedClip.SongOff,
                EditedClip.CurPat,
                EditedClip.Patterns.Count, 
                maxDspPats, 
                1,
                1);
        }


        static void UpdateInstOff(int ch)
        {
            var curInst = Instruments.IndexOf(EditedClip.CurPattern.Channels[ch].Instrument);
            UpdateDspOffset(ref EditedClip.InstOff, curInst, Instruments.Count, maxDspInst, 0, 1);
        }


        static void UpdateSrcOff()
        {
            UpdateDspOffset(
                ref EditedClip.SrcOff,
                EditedClip.CurSrc, 
                EditedClip.CurInstrument.Sources.Count, 
                maxDspSrc, 
                0,
                0);
        }


        static void SetCurInst(Instrument inst)
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                EditedClip.Patterns[p].Channels[EditedClip.CurChan].Instrument = inst;
        }


        static bool CanAdjust =>
                   IsCurParam()
                || IsCurSetting(typeof(Harmonics))
                ||    EditedClip.Transpose
                   && EditedClip.SelChan < 0;


        static bool AdjustArrowsAreVertical =>
                        CanAdjust
                    && (   IsCurParam(strVol)
                        || IsCurParam(strTune)
                        || IsCurParam(strSus)
                        || IsCurParam(strAmp)
                        || IsCurParam(strLvl)
                        || IsCurParam(strPow)
                        ||    IsCurParam(strCnt)
                            && (EditedClip.ParamKeys || EditedClip.ParamAuto)
                        || IsCurSetting(typeof(Harmonics)))
                || EditedClip.Transpose;
    }
}
