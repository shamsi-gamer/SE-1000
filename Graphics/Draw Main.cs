using System;
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


        void DrawFuncButtons(List<MySprite> sprites, float w, float h, Song song)
        {
            var bw = w/6;
            var x0 = bw/2;

            if (SelChan > -1)
            {
                if (CurSrc < 0) DrawInstFuncButtons(sprites, w, h, song);
                else            DrawSrcFuncButtons (sprites, w, h, song);
            }
            else
            {
                if (GetEditNotes(song, true).Count > 0)
                    DrawFuncButton(sprites, "Note", 2, w, h, false, false, song.EditNotes.Count > 0);
                if (GetLongNotes(song).Count > 0)
                    DrawFuncButton(sprites, "Cut",  3, w, h, false, false);
            }
        }


        void DrawInstFuncButtons(List<MySprite> sprites, float w, float h, Song song)
        {
            var chan = SelectedChannel;

            if (CurSet > -1)
            { 
                var setting = g_settings[CurSet];
                setting.DrawFuncButtons(sprites, w, h, chan);
            }
            else
            {
                var inst = CurrentInstrument;

                DrawFuncButton(sprites, "Vol",  1, w, h, true, inst.Volume.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Tune", 2, w, h, true, inst.Tune     != null);
                DrawFuncButton(sprites, "Flt",  3, w, h, true, inst.Filter   != null);
                DrawFuncButton(sprites, "Del",  4, w, h, true, inst.Delay    != null);
                DrawFuncButton(sprites, "Arp",  5, w, h, true, inst.Arpeggio != null);
            }
        }


        void DrawSrcFuncButtons(List<MySprite> sprites, float w, float h, Song song)
        {
            var chan = SelectedChannel;

            if (CurSet > -1)
            { 
                CurSetting.DrawFuncButtons(sprites, w, h, chan);
            }
            else
            {
                var src = SelectedSource;

                DrawFuncButton(sprites, "Off",  0, w, h, true, src.Offset    != null);
                DrawFuncButton(sprites, "Vol",  1, w, h, true, src.Volume.HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, "Tune", 2, w, h, true, src.Tune      != null);
                DrawFuncButton(sprites, "Hrm",  3, w, h, true, src.Harmonics != null);
                DrawFuncButton(sprites, "Flt",  4, w, h, true, src.Filter    != null);
                DrawFuncButton(sprites, "Del",  5, w, h, true, src.Delay     != null);
            }
        }


        static void DrawFuncButton(List<MySprite> sprites, string str, int i, float w, float h, bool isSetting, bool hasSetting, bool down = false)
        {
            var bw = w/6;
            var x0 = bw/2;

            if (down)
                FillRect(sprites, i * bw, h - 50, bw, 50, color6);

            DrawString(
                sprites,
                str, 
                x0 + i * bw,
                h - 44, 
                1.2f, 
                down ? color0 : color6,
                TextAlignment.CENTER);

            if (isSetting)
            {
                if (hasSetting)
                {
                    DrawString(sprites, "▲", x0 + i * bw - 5, h - 74, 1f, color6, TextAlignment.CENTER);
                    DrawString(sprites, "▲", x0 + i * bw + 5, h - 74, 1f, color6, TextAlignment.CENTER);
                }
                else
                    DrawString(sprites, "▲", x0 + i * bw, h - 74, 1f, color3, TextAlignment.CENTER);
            }
        }
    }
}
