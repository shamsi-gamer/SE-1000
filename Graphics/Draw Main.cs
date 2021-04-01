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
                && (   g_paramKeys
                    || g_paramAuto))
            {
                if (g_piano)  DrawPianoDisplay  (sprites, x, y, w, h, g_song, CurPat, true, null);
                else          DrawPatternDisplay(sprites, x, y, w, h, g_song, CurPat, true);
            }
            else if (SelChan > -1)
            {
                if (IsCurSetting(typeof(Harmonics)))
                {
                              var hrm = CurOrParentHarmonics;
                              DrawHarmonicsDisplay(sprites, x, y, w, h, g_song, CurrentChannel, hrm);
                }
                else if (IsCurOrParentSetting(typeof(Arpeggio)))
                {
                              var arp = CurOrParentArpeggio;
                              DrawPianoDisplay  (sprites, x, y, w, h, arp.Song, CurPat, true, arp);
                }
                else          DrawInstrument    (sprites, x, y, w, h);
            }
            else if (g_piano) DrawPianoDisplay  (sprites, x, y, w, h, g_song, CurPat, true, null);
            else              DrawPatternDisplay(sprites, x, y, w, h, g_song, CurPat, true);

            dsp.Draw(sprites);
        }


        void DrawFuncButtons(List<MySprite> sprites, float w, float y, Song song)
        {
            var bw = w/6;
            var x0 = bw/2;

            if (SelChan > -1)
            {
                if (CurSrc < 0) SelectedInstrument.DrawFuncButtons(sprites, w, y, SelectedChannel);
                else            SelectedSource    .DrawFuncButtons(sprites, w, y, SelectedChannel);
            }
            else
            {
                if (GetEditNotes(song, true).Count > 0)
                    DrawFuncButton(sprites, "Note", 2, w, y, false, false, song.EditNotes.Count > 0);

                if (GetLongNotes(song).Count > 0)
                    DrawFuncButton(sprites, "Cut",  3, w, y, false, false);
            }
        }


        static void DrawFuncButton(List<MySprite> sprites, string str, int i, float w, float y, bool isSetting, bool hasSetting, bool down = false)
        {
            var bw =  w/6;
            var x0 = bw/2;

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
                    DrawString(sprites, "▲", x0 + i * bw - 5, y, 1f, color6, TaC);
                    DrawString(sprites, "▲", x0 + i * bw + 5, y, 1f, color6, TaC);
                }
                else
                    DrawString(sprites, "▲", x0 + i * bw,     y, 1f, color3, TaC);
            }
        }
    }
}
