using System;
using System.Collections.Generic;
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
            if (!OK(dsp)) return;


            var v = dsp.Viewport;

            var x = v.X;
            var y = v.Y;
            var w = v.Width;
            var h = v.Height;


            var sprites = new List<MySprite>();


            FillRect(sprites, x, y, w, h, color0);

            if (OK(g_ioAction)) // loading or saving
            { 
                DrawIoInfo(sprites, x, y + 130);

                if (g_ioAction == 0) DrawButton(sprites, "Load", 0, 3, w, 50, IsPressed(lcdInfo+0));
                else                 DrawButton(sprites, "Save", 1, 3, w, 50, IsPressed(lcdInfo+1));
            }
            else
            { 
                //if (g_session.IsPlaying)
                //{
                //    var sec = (int)(PlayStep * g_session.TicksPerStep / FPS);
                //    var min = sec / 60;
                //    sec %= 60;

                //    FillRect(sprites, x + 50, y + h - 192, 250, 64, color6);
                //    DrawString(sprites, S(min) + ":" + sec.ToString("00"), x + 175, y + h - 191, 2f, color0, TaC);
                //}


                DrawButton(sprites, "Load",  0, 3, w, 50, IsPressed(lcdInfo+0));
                DrawButton(sprites, "Save",  1, 3, w, 50, IsPressed(lcdInfo+1));

                DrawButton(sprites, strDown, 0, 3, w, h,  IsPressed(lcdInfo+2));
                DrawButton(sprites, strUp,   1, 3, w, h,  IsPressed(lcdInfo+3));

                var strPlay = Playing ? "Stop ■" : "Play ►";
                DrawButton(sprites, strPlay, 2, 3, w, h, Playing);


                DrawString(sprites, S0(GetBPM()), x + 173, y + h - 122, 2.5f, color6, TA_CENTER);
                DrawString(sprites, "BPM",        x + 142, y + h - 43, 1f, color6);


                var nameLines = EditedClip.Name.Split('\n');

                if (nameLines.Length > 0) 
                    DrawString(sprites, nameLines[0], x + w/2, y + 185, 1.6f, color6, TA_CENTER);

                if (nameLines.Length > 1) 
                {
                    for (var i = 1; i < Math.Min(nameLines.Length, 4); i++)
                        DrawString(sprites, nameLines[i], x + w/2, y + 211 + i * 30, 1, color6, TA_CENTER);
                }



                DrawComplexityInfo(sprites, x, y +  60);
                DrawRuntimeInfo   (sprites, x, y +  95);
                DrawPolyphonyInfo (sprites, x, y + 130);


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


        void DrawComplexityInfo(List<MySprite> sprites, float x, float y)
        {
            var cx = x + 136;

            DrawString(sprites, "CMP",  x + 20, y - 4, 1.2f,    color6);
            DrawProgressBar(sprites,    cx,     y, 355, 27, g_dspCount / Runtime.MaxInstructionCount);
        }


        void DrawRuntimeInfo(List<MySprite> sprites, float x, float y)
        {
            var cx = x + 136;

            DrawString(sprites, "RUN",  x + 20, y - 4, 1.2f,    color6);
            DrawRect  (sprites,        cx,      y,     355, 27, color6, 2);

            var avg = g_runtimeMs.Sum() / g_runtimeMs.Length;
            FillRect(sprites, cx + 1, y + 1, 353 * Math.Min(avg, 1), 25, color3);

            for (int i = 0; i < g_runtimeMs.Length; i++)
                FillRect(sprites, cx + 3, y + 4 + i*2, 40 * g_runtimeMs[i] / g_maxRuntimeMs, 1, color5);

            Array.Sort(g_runtimeMs);
            var med = (g_runtimeMs[2] + g_runtimeMs[3])/2;
            
            var strMed = PrintValue(med,            -3, True, 0);
            var strMax = PrintValue(g_maxRuntimeMs, -3, True, 0);

            DrawString(sprites, "med " + strMed + ", max " + strMax + " ms", cx + 55, y + 6, 0.5f, color6);
        }
        
        
        void DrawPolyphonyInfo(List<MySprite> sprites, float x, float y)
        {
            var cx = x + 136;

            DrawString(sprites, "POLY",  x + 20, y - 4, 1.2f,    color6);
            DrawProgressBar(sprites,     cx,     y, 355, 27, Math.Min(g_sm.UsedRatio, 1));
        }
        
        
        void DrawIoInfo(List<MySprite> sprites, float x, float y)
        {
            var cx = x + 20;

            var val = 1f;

            if (g_ioAction == 1) // save
            {
                var total = Instruments.Count + Tracks.Count;
                var done  = (g_ioState == 1 ? Instruments.Count : 0) + g_ioPos;

                if (total > 1)
                    val = (float)(done+1) / (total-1);
            }

            DrawProgressBar(sprites, cx, y, 471, 27, val);
        }


        void DrawProgressBar(List<MySprite> sprites, float x, float y, float w, float h, float val)
        {
            DrawRect(sprites, x,   y,    w,          h,   color6, 2);
            FillRect(sprites, x+1, y+1, (w-2) * val, h-2, color6);
        }


        void DrawIO()
        {
            var dsp = dspIO;
            if (!OK(dsp)) return;

            var v = dsp.Viewport;

            var x = v.X;
            var y = v.Y;
            var w = v.Width;
            var h = v.Height;

            var cx = x + w/2;
            var cy = y + h/2;

            var sprites = new List<MySprite>();

            FillRect(sprites, x, y, w, h, color0);
            DrawString(sprites, "Copy\nPaste\nSong\nHere", cx, cy - 220, 3.5f, color6, TA_CENTER);

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

            pnl.Alignment   = TA_CENTER;
            pnl.Font        = "Monospace";
            pnl.FontSize    = 1.7f;
            pnl.TextPadding = 20;
            pnl.FontColor   = new Color(0, 41, 224);

            pnl.WriteText(dspMain.Panel.CustomData);
        }
    }
}
