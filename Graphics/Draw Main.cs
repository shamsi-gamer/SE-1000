using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawMain()
        {
            var dsp = dspMain;
            if (dsp == null) return;

            var Volume = dsp.Viewport;

            var x = Volume.X;
            var y = Volume.Y;
            var w = Volume.Width;
            var h = Volume.Height;

            var sprites = new List<MySprite>();

            if (   SelChan > -1
                && IsCurParam()
                && (   CurClip.ParamKeys
                    || CurClip.ParamAuto))
            {
                if (CurClip.Piano) DrawPianoDisplay  (sprites, x, y, w, h, CurClip, CurPat, true, null);
                else               DrawPatternDisplay(sprites, x, y, w, h, CurClip, CurPat, true);
            }
            else if (SelChan > -1)
            {
                if (IsCurSetting(typeof(Harmonics)))
                {
                     var hrm = CurOrParentHarmonics;
                     hrm.DrawSetting(sprites, x, y, w, h, CurClip, CurChannel, this);
                }
                else if (IsCurOrParentSetting(typeof(Arpeggio)))
                {
                     var arp = CurOrParentArpeggio;
                     DrawPianoDisplay(sprites, x, y, w, h, arp.Clip, CurPat, true, arp);
                }
                else DrawInstrument  (sprites, x, y, w, h);
            }
            else if (CurClip.Piano || g_lockView == 2) DrawPianoDisplay  (sprites, x, y, w, h, CurClip, CurPat, true, null);
            else                                       DrawPatternDisplay(sprites, x, y, w, h, CurClip, CurPat, true);

            dsp.Draw(sprites);
        }


        void DrawFuncButtons(List<MySprite> sprites, float w, float y, Clip clip)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (SelChan > -1)
            {
                if (CurSrc < 0) SelInstrument.DrawFuncButtons(sprites, w, y, SelChannel);
                else                   SelSource    .DrawFuncButtons(sprites, w, y, SelChannel);
            }
            else
            {
                if (GetEditNotes(clip, true).Count > 0)
                    DrawFuncButton(sprites, "Note", 2, w, y, false, false, clip.EditNotes.Count > 0);

                if (GetLongNotes(clip).Count > 0)
                    DrawFuncButton(sprites, strCut,  3, w, y, false, false);
            }
        }


        static void DrawFuncButton(List<MySprite> sprites, string str, int i, float w, float h, bool isSetting, bool hasSetting, bool down = false)
        {
            var bw =  w/6;
            var x0 = bw/2;

            var y  = h - 74;

            if (down)
                FillRect(sprites, i * bw, y + 24, bw, 50, color6);

            DrawString(
                sprites,
                str, 
                x0 + i * bw,
                y + 30, 
                1.2f, 
                down ? color0 : color6,
                TaC);

            if (isSetting)
            {
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
