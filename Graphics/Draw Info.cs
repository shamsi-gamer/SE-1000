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

            if (PlayTime > -1)
            {
                var sec = (int)(PlayStep * g_ticksPerStep / FPS);
                var min = sec / 60;
                sec %= 60;

                FillRect(sprites, x + 220, y + h - 192, 250, 64, color6);
                DrawString(sprites, S(min) + ":" + sec.ToString("00"), x + 345, y + h - 191, 2f, color0, TextAlignment.CENTER);
            }


            DrawString(sprites, GetBPM().ToString("0"), x + 343, y + h - 122, 2.5f, color6, TextAlignment.CENTER);
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
            DrawString(sprites, "▼",    x + 240, y + h - 51, 1.5f, prBpmDown ? color0 : color6);
            DrawString(sprites, "▲",    x + 415, y + h - 51, 1.5f, prBpmUp   ? color0 : color6);


            var nameLines = g_song.Name.Split('\n');

            if (nameLines.Length > 0) 
                DrawString(sprites, nameLines[0], x + w/2, y + 185, 1.6f, color6, TextAlignment.CENTER);

            if (nameLines.Length > 1) 
            {
                for (var i = 1; i < Math.Min(nameLines.Length, 4); i++)
                    DrawString(sprites, nameLines[i], x + w/2, y + 211 + i * 30, 1, color6, TextAlignment.CENTER);
            }


            var cx = x + 137;


            DrawString(sprites, "CMP", x + 20,  y + 56, 1.2f, color6);
                                                
            FillRect  (sprites,        cx - 2,  y + 60, 357, 30, color6);
            FillRect  (sprites,        cx,      y + 62, 353, 26, color0);
                                                
            FillRect  (sprites,        cx,      y + 62, 353 * dspCount / Runtime.MaxInstructionCount, 26, color6);


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

            DrawString(sprites, "med " + strMed + ", max " + strMax, cx + 55, y + 107, 0.5f, color6);


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
                //var dance = new string[]
                //{
                //     " \\☻/ \n"
                //    + "  █ \n"
                //    + "  ║ \n",

                //     " \\☻/ \n"
                //    + "  █ \n"
                //    + " / \\ \n",

                //      "  ☻ \n"
                //    + " /█\\ \n"
                //    + " / \\ \n",

                //      "  ☻ \n"
                //    + " /█\\\n"
                //    + "  ║ \n"
                //};

                //var oy = new int[] { 10, 30, 20, 0 };


                //var dance = new string[]
                //{
                //      "°◦○∙\n"
                //    + "∙○◦°\n"
                //    + "○°∙◦\n",

                //      "∙°◦○\n"
                //    + "°∙○◦\n"
                //    + "◦○°∙\n",

                //      "○∙°◦\n"
                //    + "◦°∙○\n"
                //    + "∙◦○°\n",

                //      "◦○∙°\n"
                //    + "○◦°∙\n"
                //    + "°∙◦○\n"
                //};

                //var oy = new int[] { 0, 10, 0, -10 };


                //var dance = new string[]
                //{
                //      "──► \n"
                //    + "► ──\n"
                //    + " ──►\n",

                //      " ──►\n"
                //    + "──► \n"
                //    + "► ──\n",

                //      "► ──\n"
                //    + "► ──\n"
                //    + "─► ─\n",

                //      "─► ─\n"
                //    + "──► \n"
                //    + "──► \n"
                //};

                //var oy = new int[] { 0, 0, 0, 0 };


                //var dance = new string[]
                //{
                //      "┌   \n"
                //    + "   ■\n"
                //    + " ■  \n",

                //      " ┐  \n"
                //    + "   ■\n"
                //    + " ■  \n",

                //      "    \n"
                //    + " └ ■\n"
                //    + " ■  \n",

                //      "    \n"
                //    + "  ─■\n"
                //    + " ■  \n",

                //      "    \n"
                //    + "  ─┐\n"
                //    + " ■  \n",

                //      "    \n"
                //    + "   ┐\n"
                //    + " ■ ┘\n",

                //      "    \n"
                //    + "    \n"
                //    + " ■─┘\n",

                //      "∙   \n"
                //    + "    \n"
                //    + " ──┘\n",

                //      "■   \n"
                //    + "    \n"
                //    + "└── \n",

                //      "■   \n"
                //    + "│   \n"
                //    + "└─  \n",

                //      "■   \n"
                //    + "│  ∙\n"
                //    + "└   \n",

                //      "■   \n"
                //    + "│  ■\n"
                //    + "    \n",

                //      "■   \n"
                //    + "   ■\n"
                //    + " ∙  \n"
                //};

                //var oy = new int[dance.Length];
                //for (int i = 0; i < oy.Length; i++) oy[0] = 0;


                //var dance = new string[]
                //{
                //      "    \n"
                //    + "    \n"
                //    + "    \n",

                //      "■■■ \n"
                //    + "    \n"
                //    + "    \n",

                //      "    \n"
                //    + "■■■ \n"
                //    + "    \n",

                //      "    \n"
                //    + "    \n"
                //    + "■■■ \n",

                //      "  ■■\n"
                //    + "    \n"
                //    + "■■■ \n",

                //      "  ■ \n"
                //    + "  ■■\n"
                //    + "■■■ \n",

                //      "   ■\n"
                //    + "  ■■\n"
                //    + "■■■ \n",

                //      "  ■■\n"
                //    + "   ■\n"
                //    + "■■■ \n",

                //      "    \n"
                //    + "  ■■\n"
                //    + "■■■■\n",

                //      "    \n"
                //    + "    \n"
                //    + "  ■■\n",

                //      "  ■ \n"
                //    + "    \n"
                //    + "  ■■\n",

                //      " ■■ \n"
                //    + "  ■ \n"
                //    + "  ■■\n",

                //      " ■  \n"
                //    + " ■■ \n"
                //    + "  ■■\n",

                //      "■   \n"
                //    + "■■  \n"
                //    + "  ■■\n",

                //      "    \n"
                //    + "■   \n"
                //    + "■■■■\n",

                //      "    \n"
                //    + "    \n"
                //    + "■   \n",

                //      " ■  \n"
                //    + "    \n"
                //    + "■   \n",

                //      " ■  \n"
                //    + " ■  \n"
                //    + "■   \n",

                //      " ■  \n"
                //    + " ■  \n"
                //    + "■■  \n",

                //      "    \n"
                //    + "■■■ \n"
                //    + "■   \n",

                //      "    \n"
                //    + " ■■■\n"
                //    + "■   \n",

                //      "    \n"
                //    + "    \n"
                //    + "■■■■\n"
                //};

                //var oy = new int[dance.Length];
                //for (int i = 0; i < oy.Length; i++) oy[0] = 0;


                //var iMan =
                //    g_song.PlayTime > -1
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
            DrawString(sprites, "Copy\nPaste\nSong\nHere", cx, cy - 220, 3.5f, color6, TextAlignment.CENTER);

            dsp.Draw(sprites);
        }


        void DrawMissingMod()
        {
            var dsp = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(dsp);

            foreach (var d in dsp)
            {
                d.ContentType     = ContentType.TEXT_AND_IMAGE;
                d.BackgroundColor = color0;
                d.FontColor       = color0;
            }

            var s = dspMain.Surface;

            s.Alignment   = TextAlignment.CENTER;
            s.Font        = "Monospace";
            s.FontSize    = 1.7f;
            s.TextPadding = 20;
            s.FontColor   = new Color(0, 41, 224);

            s.WriteText("Install SE-909 mk2 Sounds mod\n\n\nsteamcommunity.com/\nsharedfiles/filedetails/\n?id=2430813153");
        }
    }
}
