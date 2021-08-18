using System;
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



        static bool IsCurSetting(string tag)
        {
            return (EditedClip.CurSetting?.Tag ?? "") == tag;
        }



        static bool IsCurSetting(Type type)
        {
            return EditedClip.CurSetting?.GetType() == type;
        }



        static void UpdateClipOff()
        {
            UpdateDspOffset(
                ref EditedClip.SongOff,
                EditPat,
                EditedClip.Patterns.Count, 
                maxDspPats, 
                1,
                1);
        }



        static void UpdateInstOff(int ch)
        {
            var curInst = Instruments.IndexOf(EditPattern.Channels[ch].Instrument);
            UpdateDspOffset(ref EditedClip.InstOff, curInst, Instruments.Count, maxDspInst, 0, 1);
        }



        static void UpdateSrcOff()
        {
            UpdateDspOffset(
                ref EditedClip.SrcOff,
                CurSrc, 
                EditedClip.CurInstrument.Sources.Count, 
                maxDspSrc, 
                0,
                0);
        }



        static void SetCurInst(Instrument newInst)
        {
            var oldInst = EditedClip.CurInstrument;

            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                var chans = EditedClip.GetCurChannels(p, oldInst, False);

                for (int ch = 0; ch < chans.Count; ch++)
                    SetInstrument(chans[ch], oldInst, newInst);
            }
        }

        

        static bool CanAdjust =>
                   IsCurParam()
                || IsCurSetting(strHrm)
                || IsCurSetting(strChord)
                ||    EditedClip.Transpose
                   && SelChan < 0;



        static bool AdjustArrowsAreVertical =>
                        CanAdjust
                    && (   IsCurParam(strVol)
                        || IsCurParam(strTune)
                        || IsCurParam(strSus)
                        || IsCurParam(strAmp)
                        || IsCurParam(strLvl)
                        || IsCurParam(strPow)
                        || IsCurParam(strStep)
                        || IsCurParam(strChord)
                        ||     IsCurParam(strCnt)
                            && (EditedClip.ParamKeys || EditedClip.ParamAuto)
                        || IsCurSetting(typeof(Harmonics)))
                || EditedClip.Transpose;
    }
}
