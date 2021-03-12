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


            if (   g_song.SelChan > -1
                && IsCurParam()
                && (   g_paramKeys
                    || g_paramAuto))
            {
                if (g_piano)  DrawPianoDisplay  (sprites, x, y, w, h, g_song, g_song.CurPat, true, null);
                else          DrawPatternDisplay(sprites, x, y, w, h, g_song, g_song.CurPat, true);
            }
            else if (g_song.SelChan > -1)
            {
                if (IsCurSetting(typeof(Harmonics)))
                {
                              var hrm = CurOrParentHarmonics;
                              DrawHarmonicsDisplay(sprites, x, y, w, h, g_song, CurrentChannel(g_song), hrm);
                }
                else if (IsCurOrParent(typeof(Arpeggio)))
                {
                              var arp = CurOrParentArpeggio;
                              DrawPianoDisplay  (sprites, x, y, w, h, arp.Song, arp.Song.CurPat, true, arp);
                }
                else          DrawInstrument    (sprites, x, y, w, h);
            }
            else if (g_piano) DrawPianoDisplay  (sprites, x, y, w, h, g_song, g_song.CurPat, true, null);
            else              DrawPatternDisplay(sprites, x, y, w, h, g_song, g_song.CurPat, true);


            dsp.Draw(sprites);
        }


        void DrawFuncButtons(List<MySprite> sprites, float w, float h, Song song)
        {
            var bw = w/6;
            var x0 = bw/2;

            if (song.SelChan > -1)
            {
                if (g_song.CurSrc < 0) DrawInstFuncLabels(sprites, w, h, song);
                else                   DrawSrcFuncLabels (sprites, w, h, song);
            }
            else
            {
                if (GetEditNotes(song, true).Count > 0)
                    DrawFuncButton(sprites, "Note", 2, w, h, false, false, song.EditNotes.Count > 0);
                if (GetLongNotes(song).Count > 0)
                    DrawFuncButton(sprites, "Cut",  3, w, h, false, false);
            }
        }


        void DrawInstFuncLabels(List<MySprite> sprites, float w, float h, Song song)
        {
            var chan = SelectedChannel(g_song);

            if (curSet > -1)
            { 
                var setting = g_settings[curSet];

                     if (setting.GetType() == typeof(Envelope)) DrawEnvelopeFuncButtons(sprites, (Envelope )setting, w, h, chan);
                else if (setting.GetType() == typeof(LFO     )) DrawLfoFuncButtons     (sprites, (LFO      )setting, w, h, chan);
                else if (setting.GetType() == typeof(Filter  )) DrawFilterFuncButtons  (sprites, (Filter   )setting, w, h, chan);
                else if (setting.GetType() == typeof(Delay   )) DrawDelayFuncButtons   (sprites, (Delay    )setting, w, h, chan);
                else if (setting.GetType() == typeof(Arpeggio)) DrawArpeggioFuncButtons(sprites, (Arpeggio )setting, w, h, chan);
                else if (IsParam(setting))                      DrawParamFuncButtons   (sprites, (Parameter)setting, w, h, song, chan);
            }
            else
            {
                var inst = CurrentInstrument(song);

                DrawFuncButton(sprites, "Vol",  1, w, h, true, inst.Volume.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Tune", 2, w, h, true, inst.Tune     != null);
                DrawFuncButton(sprites, "Flt",  3, w, h, true, inst.Filter   != null);
                DrawFuncButton(sprites, "Del",  4, w, h, true, inst.Delay    != null);
                DrawFuncButton(sprites, "Arp",  5, w, h, true, inst.Arpeggio != null);
            }
        }


        void DrawSrcFuncLabels(List<MySprite> sprites, float w, float h, Song song)
        {
            var chan = SelectedChannel(g_song);

            if (curSet > -1)
            { 
                var setting = g_settings[curSet];

                     if (setting.GetType() == typeof(Envelope )) DrawEnvelopeFuncButtons (sprites, (Envelope )setting, w, h, chan);
                else if (setting.GetType() == typeof(LFO      )) DrawLfoFuncButtons      (sprites, (LFO      )setting, w, h, chan);
                else if (setting.GetType() == typeof(Harmonics)) DrawHarmonicsFuncButtons(sprites, (Harmonics)setting, w, h, chan);
                else if (setting.GetType() == typeof(Filter   )) DrawFilterFuncButtons   (sprites, (Filter   )setting, w, h, chan);
                else if (setting.GetType() == typeof(Delay    )) DrawDelayFuncButtons    (sprites, (Delay    )setting, w, h, chan);
                else if (IsParam(setting))                       DrawParamFuncButtons    (sprites, (Parameter)setting, w, h, song, chan);
            }
            else
            {
                var src = SelectedSource(song);

                DrawFuncButton(sprites, "Off",  0, w, h, true, src.Offset    != null);
                DrawFuncButton(sprites, "Vol",  1, w, h, true, src.Volume.HasDeepParams(chan, g_song.CurSrc));
                DrawFuncButton(sprites, "Tune", 2, w, h, true, src.Tune      != null);
                DrawFuncButton(sprites, "Hrm",  3, w, h, true, src.Harmonics != null);
                DrawFuncButton(sprites, "Flt",  4, w, h, true, src.Filter    != null);
                DrawFuncButton(sprites, "Del",  5, w, h, true, src.Delay     != null);
            }
        }


        void DrawEnvelopeFuncButtons(List<MySprite> sprites, Envelope env, float w, float h, Channel chan)
        {
            DrawFuncButton(sprites, "A", 1, w, h, true, env.Attack .HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "D", 2, w, h, true, env.Decay  .HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "S", 3, w, h, true, env.Sustain.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "R", 4, w, h, true, env.Release.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "X", 5, w, h, false, false, mainPressed.Contains(5));
        }


        void DrawLfoFuncButtons(List<MySprite> sprites, LFO lfo, float w, float h, Channel chan)
        {
            DrawFuncButton(sprites, "Amp",   1, w, h, true, lfo.Amplitude.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Freq",  2, w, h, true, lfo.Frequency.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Off",   3, w, h, true, lfo.Offset   .HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Osc ↕", 4, w, h, false, false);
            DrawFuncButton(sprites, "X",     5, w, h, false, false, mainPressed.Contains(5));
        }


        void DrawHarmonicsFuncButtons(List<MySprite> sprites, Harmonics hrm, float w, float h, Channel chan)
        {
            DrawFuncButton(sprites, "Smth",  1, w, h, false, false, mainPressed.Contains(1));
            DrawFuncButton(sprites, "Pre ↕", 2, w, h, false, false, mainPressed.Contains(2));
            DrawFuncButton(sprites, "Set",   3, w, h, false, false, mainPressed.Contains(3));
            DrawFuncButton(sprites, "Tone",  4, w, h, true,  hrm.Tones[hrm.CurTone].HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "X",     5, w, h, false, false, mainPressed.Contains(5));
        }


        void DrawFilterFuncButtons(List<MySprite> sprites, Filter flt, float w, float h, Channel chan)
        {
            DrawFuncButton(sprites, "Cut", 1, w, h, true, flt.Cutoff   .HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Res", 2, w, h, true, flt.Resonance.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "X",   5, w, h, false, false, mainPressed.Contains(5));
        }


        void DrawDelayFuncButtons(List<MySprite> sprites, Delay del, float w, float h, Channel chan)
        {
            DrawFuncButton(sprites, "Cnt",  1, w, h, true, del.Count.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Time", 2, w, h, true, del.Time .HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Lvl",  3, w, h, true, del.Level.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Pow",  4, w, h, true, del.Power.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "X",    5, w, h, false, false, mainPressed.Contains(5));
        }


        void DrawArpeggioFuncButtons(List<MySprite> sprites, Arpeggio arp, float w, float h, Channel chan)
        {
            DrawFuncButton(sprites, "Len", 1, w, h, true, arp.Length.HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "Scl", 2, w, h, true, arp.Scale .HasDeepParams(chan, -1));
            DrawFuncButton(sprites, "X",   5, w, h, false, false, mainPressed.Contains(5));                
        }


        void DrawParamFuncButtons(List<MySprite> sprites, Parameter param, float w, float h, Song song, Channel chan)
        {
            if (   !SettingOrAnyParentHasTag(param, "Att")
                && !SettingOrAnyParentHasTag(param, "Dec")
                && !SettingOrAnyParentHasTag(param, "Sus")
                && !SettingOrAnyParentHasTag(param, "Rel"))
            {
                DrawFuncButton(sprites, "Env", 1, w, h, true, param.Envelope != null);

                if (   param.Tag != "Vol"
                    && (   param.Parent == null
                        || param.Parent.GetType() != typeof(Harmonics)))
                    DrawFuncButton(sprites, "X", 5, w, h, false, false, mainPressed.Contains(5));
            }

            DrawFuncButton(sprites, "LFO",  2, w, h, true, param.Lfo != null);

            DrawFuncButton(sprites, "Key",  3, w, h, true, chan.HasNoteKeys(param.GetPath(g_song.CurSrc)));
            DrawFuncButton(sprites, "Auto", 4, w, h, true, chan.HasAutoKeys(param.GetPath(g_song.CurSrc)));
        }


        void DrawFuncButton(List<MySprite> sprites, string str, int i, float w, float h, bool isSetting, bool hasSetting, bool down = false)
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
