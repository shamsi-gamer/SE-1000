using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawMain()
        {
            var dsp = dspMain;
            if (!OK(dsp)) return;

            var v = dsp.Viewport;

            var x = v.X;
            var y = v.Y;
            var w = v.Width;
            var h = v.Height;

            var sprites = new List<MySprite>();

            if (!OK(EditedClip))
            {
                FillRect(sprites, x, y, w, h, color0);
            }
            else
            { 
                if (   SelChan > -1
                    && IsCurParam()
                    && (   EditedClip.ParamKeys
                        || EditedClip.ParamAuto))
                {
                    if (EditedClip.Piano) DrawPianoDisplay  (sprites, x, y, w, h, EditedClip, CurPat, T, null);
                    else               DrawPatternDisplay(sprites, x, y, w, h, EditedClip, CurPat, T);
                }
                else if (SelChan > -1)
                {
                    if (IsCurSetting(typeof(Harmonics)))
                    {
                         var hrm = CurOrParentHarmonics;
                         hrm.DrawSetting(sprites, x, y, w, h, CurChannel, this);
                    }
                    else if (IsCurOrParentSetting(typeof(Arpeggio)))
                    {
                         var arp = CurOrParentArpeggio;
                         DrawPianoDisplay(sprites, x, y, w, h, arp.Clip, CurPat, T, arp);
                    }
                    else DrawInstrument  (sprites, x, y, w, h);
                }
                else if (EditedClip.Piano 
                      && g_lockView != 1
                   || g_lockView == 2) DrawPianoDisplay  (sprites, x, y, w, h, EditedClip, CurPat, T, null);
                else                   DrawPatternDisplay(sprites, x, y, w, h, EditedClip, CurPat, T);
            }

            dsp.Draw(sprites);
        }


        void DrawFuncButtons(List<MySprite> sprites, float w, float y, Clip clip)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (SelChan > -1)
            {
                if (CurSrc < 0) SelInstrument.DrawFuncButtons(sprites, w, y, SelChannel);
                else            SelSource    .DrawFuncButtons(sprites, w, y, SelChannel);
            }
            else
            {
                if (GetEditNotes(clip, T).Count > 0)
                    DrawFuncButton(sprites, "Note", 2, w, y, F, F, clip.EditNotes.Count > 0);

                if (GetLongNotes(clip).Count > 0)
                    DrawFuncButton(sprites, strCut, 3, w, y, F, F);
            }
        }


        static void DrawFuncButton(List<MySprite> sprites, string str, int i, float w, float h, bool isSetting, bool hasSetting, bool down = F)
        {
            DrawButton(sprites, str, i, 6, w, h, down);


            if (isSetting)
            {
                var bw =  w/6;
                var x0 = bw/2;

                var y  = h - 74;
 
                if (hasSetting)
                {
                    DrawString(sprites, strUp, x0 + i * bw - 5, y, 1f, color6, TaC);
                    DrawString(sprites, strUp, x0 + i * bw + 5, y, 1f, color6, TaC);
                }
                else
                    DrawString(sprites, strUp, x0 + i * bw,     y, 1f, color3, TaC);
            }
        }
    }
}
