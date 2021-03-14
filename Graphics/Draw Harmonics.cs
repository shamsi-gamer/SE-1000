using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawHarmonicsDisplay(List<MySprite> sprites, float x, float y, float w, float h, Song song, Channel chan, Harmonics hrm)
        {
            FillRect(sprites, x, y, w, h, color0);

            var  rh = h - 90;
            var irh = h - 50;

            var xt  = 300f;
            var wt  = 600f;
            var yt  = 50f;
            var ht  = 300f;

            var gap = 4f;

            var wc  = wt / hrm.Tones.Length;

            float yo = 0;

            DrawSourceSettings(sprites, x + 5, y + 10, SelectedSource, false, ref yo);

            DrawHarmonicsSample(sprites, x + 100, y + 150, 100, 60, song, hrm);


            var sndLen    = (int)(EditLength * g_ticksPerStep);

            for (int i = 0; i < hrm.Tones.Length; i++)
            {
                var val     = hrm.Tones[i].CurValue;
                var toneVal = hrm.Tones[i].Value;

                var xh = xt + i*wc;

                FillRect(sprites, xh + gap/2,                yt + ht, wc - gap,     -ht,           color2);
                FillRect(sprites, xh + gap/2,                yt + ht, wc - gap - 8, -ht * val,     color5);

                FillRect(sprites, xh + gap/2 + (wc-gap) - 7, yt + ht, 7,            -ht * toneVal, color6);
            }


            // current tone
            FillRect(sprites, xt + hrm.CurTone * wc, yt + ht + 10, wc, 20, color5);


            // has param marks
            for (int i = 0; i < hrm.Tones.Length; i++)
            {
                if (hrm.Tones[i].HasDeepParams(chan, -1))
                    DrawString(sprites, "▲", xt + i*wc + wc/2, yt + ht + 10, 0.6f, color3, TextAlignment.CENTER);
            }


            // draw sample curve

            var bw = w/6;
            var be = 0f;

            DrawHarmonicsPreset(sprites, hrm.CurPreset, 2*bw - be/2, h - 100, bw + be, 50);

            DrawFuncButtons(sprites, w, h, song);
        }


        void DrawHarmonicsPreset(List<MySprite> sprites, Harmonics.Preset preset, float x, float y, float w, float h)
        {
            var x0 = w/2;

            DrawRect(sprites, x, y, w, h, color4);

            var str = "";

            switch (preset)
            {
                case Harmonics.Preset.Sine:     str = "Sine";    break;

                case Harmonics.Preset.Saw4:     str = "Saw 4";   break;
                case Harmonics.Preset.Saw8:     str = "Saw 8";   break;
                case Harmonics.Preset.Saw16:    str = "Saw 16";  break;
                case Harmonics.Preset.Saw24:    str = "Saw 24";  break;

                case Harmonics.Preset.Square4:  str = "Sqr 4";   break;
                case Harmonics.Preset.Square8:  str = "Sqr 8";   break;
                case Harmonics.Preset.Square16: str = "Sqr 16";  break;
                case Harmonics.Preset.Square24: str = "Sqr 24";  break;

                case Harmonics.Preset.Flat4:    str = "Flat 4";  break;
                case Harmonics.Preset.Flat8:    str = "Flat 8";  break;
                case Harmonics.Preset.Flat16:   str = "Flat 16"; break;
                case Harmonics.Preset.Flat24:   str = "Flat 24"; break;

                case Harmonics.Preset.Random4:  str = "Rnd 4";  break;
                case Harmonics.Preset.Random8:  str = "Rnd 8";  break;
                case Harmonics.Preset.Random16: str = "Rnd 16"; break;
                case Harmonics.Preset.Random24: str = "Rnd 24"; break;
            }

            DrawString(
                sprites,
                str, 
                x + x0,
                y + 4, 
                1.2f, 
                color6,
                TextAlignment.CENTER);
        }
    }
}
