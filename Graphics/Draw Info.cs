using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        void DrawInfo()
        {
            var dsp = dspInfo;
            if (dsp == null) return;


            var Volume = dsp.Viewport;

            var x = Volume.X;
            var y = Volume.Y;
            var w = Volume.Width;
            var h = Volume.Height;


            var sprites = new List<MySprite>();


            FillRect(sprites, x, y, w, h, color0);

            if (OK(g_song.PlayTime))
            {
                var sec = (int)(g_song.PlayStep * g_ticksPerStep / FPS);
                var min = sec / 60;
                sec %= 60;

                FillRect(sprites, x + 220, y + h - 192, 250, 64, color6);
                DrawString(sprites, S(min) + ":" + sec.ToString("00"), x + 345, y + h - 191, 2f, color0, TaC);
            }


            DrawString(sprites, S0(GetBPM()), x + 343, y + h - 122, 2.5f, color6, TaC);
            DrawString(sprites, "BPM", x + 312, y + h - 43, 1f, color6);

            var prLoad    = infoPressed_.Contains(0);
            var prSave    = infoPressed_.Contains(1);
            var prBpmDown = infoPressed_.Contains(2);
            var prBpmUp   = infoPressed_.Contains(3);

            if (prLoad   ) FillRect(sprites, x + 202, y + 8,      104, 40, color6);
            if (prSave   ) FillRect(sprites, x + 374, y + 8,      104, 40, color6);
            if (prBpmDown) FillRect(sprites, x + 201, y + h - 48, 104, 40, color6);
            if (prBpmUp  ) FillRect(sprites, x + 374, y + h - 48, 104, 40, color6);

            DrawString(sprites, "Load", x + 212, y + 10,     1.1f, prLoad    ? color0 : color6);
            DrawString(sprites, "Save", x + 384, y + 10,     1.1f, prSave    ? color0 : color6);
            DrawString(sprites, strDown,    x + 240, y + h - 51, 1.5f, prBpmDown ? color0 : color6);
            DrawString(sprites, strUp,    x + 415, y + h - 51, 1.5f, prBpmUp   ? color0 : color6);


            var nameLines = g_song.Name.Split('\n');

            if (nameLines.Length > 0) 
                DrawString(sprites, nameLines[0], x + w/2, y + 185, 1.6f, color6, TaC);

            if (nameLines.Length > 1) 
            {
                for (var i = 1; i < Math.Min(nameLines.Length, 4); i++)
                    DrawString(sprites, nameLines[i], x + w/2, y + 211 + i * 30, 1, color6, TaC);
            }


            var cx = x + 137;


            DrawString(sprites, "CMP", x + 20,  y + 56, 1.2f, color6);
                                                
            FillRect  (sprites,        cx - 2,  y + 60, 357, 30, color6);
            FillRect  (sprites,        cx,      y + 62, 353, 26, color0);
                                                
            FillRect  (sprites,        cx,      y + 62, 353 * g_dspCount / Runtime.MaxInstructionCount, 26, color6);


            DrawString(sprites, "RUN", x + 20,  y + 96, 1.2f, color6);
                                                
            FillRect  (sprites,        cx - 2,  y + 100, 357, 30, color6);
            FillRect  (sprites,        cx,      y + 102, 353, 26, color0);


            var avg = g_runtimeMs.Sum() / g_runtimeMs.Length;


            FillRect  (sprites,        cx,      y + 102, 353 * Math.Min(avg, 1), 26, color3);


            for (int i = 0; i < g_runtimeMs.Length; i++)
                FillRect(sprites, cx + 2, y + 104 + i*4, 40 * g_runtimeMs[i] / g_maxRuntimeMs, 2, color5);

            Array.Sort(g_runtimeMs);
            var med = (g_runtimeMs[2] + g_runtimeMs[3])/2;
            
            var strMed = printValue(med,            -3, true, 0);
            var strMax = printValue(g_maxRuntimeMs, -3, true, 0);

            DrawString(sprites, "med " + strMed + ", max " + strMax + " ms", cx + 55, y + 107, 0.5f, color6);


            DrawString(sprites, "POLY", x + 20, y + 136, 1.2f, color6);

            FillRect  (sprites,        cx - 2,  y + 140, 357, 30, color6);
            FillRect  (sprites,        cx,      y + 142, 353, 26, color0);
                                                
            FillRect  (sprites,        cx,      y + 142, 353 * Math.Min(g_sm.UsedRatio, 1), 26, color6);


            if (true)
            {
                var min = Math.Max(0, g_log.Count - 15);
                var sb  = new StringBuilder();

                for (int i = min; i < g_log.Count; i++)
                { 
                    DrawString(
                        sprites,
                        S(g_logTime[i]) + ": " + g_log[i], 
                        x + 6, 
                        y + 306 + (i-min) * 25*0.5f, 
                        0.4f, 
                        color6);
                }
            }
            else
            {
                //var oy = new int[dance.Length];
                //for (int i = 0; i < oy.Length; i++) oy[0] = 0;


                //var iMan =
                //    OK(g_song.PlayTime)
                //    ? (int)(g_song.PlayStep / 2) % dance.Length
                //    : 3;

                //dsp.Add(ref frame, DrawString(dance[iMan], x + 30, y + 330 - oy[iMan], 1.8f, color6));
            }


            dsp.Draw(sprites);
        }


        void DrawIO()
        {
            var dsp = dspIO;
            if (dsp == null) return;

            var Volume = dsp.Viewport;

            var x = Volume.X;
            var y = Volume.Y;
            var w = Volume.Width;
            var h = Volume.Height;

            var cx = x + w/2;
            var cy = y + h/2;

            var sprites = new List<MySprite>();

            FillRect(sprites, x, y, w, h, color0);
            DrawString(sprites, "Copy\nPaste\nSong\nHere", cx, cy - 220, 3.5f, color6, TaC);

            dsp.Draw(sprites);
        }


        void DrawMissingMod()
        {
            var dsp = new List<IMyTextPanel>();
            Get(dsp);

            foreach (var d in dsp)
            {
                d.ContentType     = ContentType.TEXT_AND_IMAGE;
                d.BackgroundColor = color0;
                d.FontColor       = color0;
            }

            var pnl = dspMain.Panel;

            pnl.Alignment   = TaC;
            pnl.Font        = "Monospace";
            pnl.FontSize    = 1.7f;
            pnl.TextPadding = 20;
            pnl.FontColor   = new Color(0, 41, 224);

            pnl.WriteText(dspMain.Panel.CustomData);
        }
    }
}
