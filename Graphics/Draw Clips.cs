using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawClips()
        {
            if (!TooComplex) DrawClips(dspMixer1, 0);
            if (!TooComplex) DrawClips(dspMixer2, 1);
        }


        void DrawClips(Display dsp, int nDsp)
        {
            if (dsp == null) return;


            var sprites = new List<MySprite>();

            var x = dsp.Viewport.X;
            var y = dsp.Viewport.Y;
            var w = dsp.Viewport.Width;
            var h = dsp.Viewport.Height;

            var gap = 8f;


            FillRect(sprites, x, y, w, h, color0);

            
            for (int iy = 0; iy < 4; iy++)
            {
                for (int ix = 0; ix < 6; ix++)
                {
                    FillRect(
                        sprites, 
                        x + w/6*ix + gap/2, 
                        y + h/4*iy + gap/2, 
                        w/6 - gap,
                        h/4 - gap, 
                        color3); 

                    var clipName = "Clip\nName";
                    var nNameLines = clipName.Split('\n').Length;

                    DrawString(
                        sprites,
                        clipName,
                        x + w/6*ix + w/12,
                        y + h/4*iy + h/8 - 15 - (nNameLines-1)*20,
                        1,
                        color0,
                        TextAlignment.CENTER);
                }
            }


            dsp.Draw(sprites);
        }
    }
}
