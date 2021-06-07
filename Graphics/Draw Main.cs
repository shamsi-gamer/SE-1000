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

            if (ShowClip)
            { 
                if (   OK(EditedClip.SelChan)
                    && IsCurParam()
                    && (   EditedClip.ParamKeys
                        || EditedClip.ParamAuto))
                {
                    if (EditedClip.Piano) DrawPianoDisplay  (sprites, x, y, w, h, EditedClip, EditedClip.CurPat, True);
                    else                  DrawPatternDisplay(sprites, x, y, w, h, EditedClip, EditedClip.CurPat, True);
                }
                else if (OK(EditedClip.SelChan))
                {
                    if (IsCurSetting(typeof(Harmonics)))
                    {
                        var hrm = EditedClip.CurOrParentHarmonics;
                        hrm.DrawSetting(sprites, x, y, w, h, EditedClip.CurChannel, this);
                    }
                    else 
                        DrawInstrument(sprites, x, y, w, h);
                }
                else if ( EditedClip.Piano 
                       && LockView != 1
                    || LockView == 2) DrawPianoDisplay  (sprites, x, y, w, h, EditedClip, EditedClip.CurPat, True);
                else                  DrawPatternDisplay(sprites, x, y, w, h, EditedClip, EditedClip.CurPat, True);
            }
            else
                FillRect(sprites, x, y, w, h, color0);


            if (!TooComplex)
                dsp.Draw(sprites);
        }


        void DrawFuncButtons(List<MySprite> sprites, float w, float y, Clip clip)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (OK(EditedClip.SelChan))
            {
                if (EditedClip.CurSrc < 0) EditedClip.SelInstrument.DrawFuncButtons(sprites, w, y, EditedClip.SelChannel);
                else                       EditedClip.SelSource    .DrawFuncButtons(sprites, w, y, EditedClip.SelChannel);
            }
        }


        static void DrawFuncButton(List<MySprite> sprites, string str, int i, float w, float h, bool isSetting, bool hasSetting, bool down = False)
        {
            DrawButton(sprites, str, i, 6, w, h, down);


            if (isSetting)
            {
                var bw =  w/6;
                var x0 = bw/2;

                var y  = h - 74;
 
                if (hasSetting)
                {
                    DrawString(sprites, strUp, x0 + i * bw - 5, y, 1f, color6, TA_CENTER);
                    DrawString(sprites, strUp, x0 + i * bw + 5, y, 1f, color6, TA_CENTER);
                }
                else
                    DrawString(sprites, strUp, x0 + i * bw,     y, 1f, color3, TA_CENTER);
            }
        }
    }
}
