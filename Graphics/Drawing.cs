using System;
using System.Collections.Generic;
using VRageMath;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        void DrawDisplays()
        {
            DrawVolume();

            if (!TooComplex) DrawMain();
            if (!TooComplex) DrawInfo();
            if (!TooComplex) DrawClip();
            if (!TooComplex) DrawIO();

            if (!TooComplex)
            { 
                if (ShowMixer) DrawMixer();
                else           DrawSessionClips();
            }
        }


        static void ClipDraw(List<MySprite> sprites, float x, float y, float w, float h)
        {
            sprites.Add(MySprite.CreateClipRect(new Rectangle((int)x, (int)y, (int)w, (int)h)));
        }


        static void ClearClip(List<MySprite> sprites)
        {
            sprites.Add(MySprite.CreateClearClipRect());
        }


        static void DrawString(List<MySprite> sprites, string str, float x, float y, float scale, Color c, TextAlignment align = TextAlignment.LEFT)
        {
            sprites.Add(new MySprite()
            {
                Type            = SpriteType.TEXT,
                Data            = str,
                Position        = new Vector2(x, y),
                RotationOrScale = scale,
                Color           = c,
                Alignment       = align,
                FontId          = "Monospace"
            });
        }
        
        
        static void DrawTexture(List<MySprite> sprites, string texture, Vector2 pos, Vector2 size, Color c, float rotation = 0)
        {
            sprites.Add(new MySprite()
            {
                Type            = SpriteType.TEXTURE,
                Data            = texture,
                Position        = pos + size/2,
                Size            = size,
                Color           = c,
                Alignment       = TA_CENTER,
                RotationOrScale = rotation
            });
        }


        static void DrawTexture(List<MySprite> sprites, string texture, float x, float y, float w, float h, Color c, float rotation = 0)
        {
            DrawTexture(sprites, texture, new Vector2(x, y), new Vector2(w, h), c, rotation);
        }
        
        
        static void FillRect(List<MySprite> sprites, float x, float y, float w, float h, Color c)
        {
            DrawTexture(sprites, "SquareSimple", x, y, w, h, c);
        }


        static void DrawRect(List<MySprite> sprites, float x, float y, float w, float h, Color c, float wd = 1)
        {
            var wd2 = wd/2;

            DrawLine(sprites, x-wd2, y,     x+w+wd2, y,   c, wd);
            DrawLine(sprites, x-wd2, y+h,   x+w+wd2, y+h, c, wd);
            DrawLine(sprites, x,     y-wd2, x,       y+h, c, wd);
            DrawLine(sprites, x+w,   y-wd2, x+w,     y+h, c, wd);
        }


        static void FillCircle(List<MySprite> sprites, Vector2 p, float r, Color color)
        {
            DrawTexture(sprites, "Circle", p.X - r, p.Y - r, r*2, r*2, color);
        }


        static void FillCircle(List<MySprite> sprites, float x, float y, float r, Color color)
        {
            DrawTexture(sprites, "Circle", x-r, y-r, r*2, r*2, color);
        }


        //void DrawCircle(List<MySprite> sprites, Vector2 p, float r, Color color)
        //{
        //    DrawTexture(sprites, "CircleHollow", p.X-r, p.Y-r, r*2, r*2, color);
        //}


        //void DrawCircle(List<MySprite> sprites, float x, float y, float r, Color color)
        //{
        //    DrawTexture(sprites, "CircleHollow", x-r, y-r, r*2, r*2, color);
        //}


        static void DrawLine(List<MySprite> sprites, Vector2 p1, Vector2 p2, Color col, float width = 1)
        {
            var dp    = p2 - p1;
            var len   = dp.Length();
            var angle = (float)Math.Atan2(p1.Y - p2.Y, p2.X - p1.X);

            DrawTexture(
                sprites,
                "SquareSimple",
                p1.X + dp.X/2 - len/2,
                p1.Y + dp.Y/2 - width/2,
                len,
                width,
                col,
                -angle);
        }
        
        
        static void DrawLine(List<MySprite> sprites, float x1, float y1, float x2, float y2, Color col, float width = 1)
        {
            DrawLine(
                sprites, 
                new Vector2(x1, y1), 
                new Vector2(x2, y2), 
                col, 
                width);
        }


        static void DrawButton(List<MySprite> sprites, string str, int i, int maxButtons, float w, float h, bool down = False)
        {
            var bw =  w/maxButtons;
            var x0 = bw/2;

            var y  = h - 50;

            if (down)
                FillRect(sprites, i * bw, y, bw, 50, color6);

            DrawString(
                sprites,
                str, 
                x0 + i * bw,
                y + 6, 
                1.2f, 
                down ? color0 : color6,
                TA_CENTER);
        }
    }
}
