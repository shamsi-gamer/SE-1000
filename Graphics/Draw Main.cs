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


            if (   g_session.CurClip.SelChan > -1
                && IsCurParam()
                && (   g_session.CurClip.ParamKeys
                    || g_session.CurClip.ParamAuto))
            {
                if (g_session.CurClip.Piano)  DrawPianoDisplay  (sprites, x, y, w, h, g_session.CurClip, g_session.CurClip.CurPat, true, null);
                else          DrawPatternDisplay(sprites, x, y, w, h, g_session.CurClip, g_session.CurClip.CurPat, true);
            }
            else if (g_session.CurClip.SelChan > -1)
            {
                if (IsCurSetting(typeof(Harmonics)))
                {
                              var hrm = CurOrParentHarmonics;
                              hrm.DrawSetting(sprites, x, y, w, h, g_session.CurClip, g_session.CurClip.CurrentChannel, this);
                }
                else if (IsCurOrParentSetting(typeof(Arpeggio)))
                {
                              var arp = CurOrParentArpeggio;
                              DrawPianoDisplay  (sprites, x, y, w, h, arp.Clip, g_session.CurClip.CurPat, true, arp);
                }
                else          DrawInstrument    (sprites, x, y, w, h);
            }
            else if (g_session.CurClip.Piano) DrawPianoDisplay  (sprites, x, y, w, h, g_session.CurClip, g_session.CurClip.CurPat, true, null);
            else              DrawPatternDisplay(sprites, x, y, w, h, g_session.CurClip, g_session.CurClip.CurPat, true);

            dsp.Draw(sprites);
        }


        void DrawFuncButtons(List<MySprite> sprites, float w, float y, Clip song)
        {
            var bw =  w/6;
            var x0 = bw/2;

            if (g_session.CurClip.SelChan > -1)
            {
                if (g_session.CurClip.CurSrc < 0) g_session.CurClip.SelectedInstrument.DrawFuncButtons(sprites, w, y, g_session.CurClip.SelectedChannel);
                else                   g_session.CurClip.SelectedSource    .DrawFuncButtons(sprites, w, y, g_session.CurClip.SelectedChannel);
            }
            else
            {
                if (GetEditNotes(song, true).Count > 0)
                    DrawFuncButton(sprites, "Note", 2, w, y, false, false, song.EditNotes.Count > 0);

                if (GetLongNotes(song).Count > 0)
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
