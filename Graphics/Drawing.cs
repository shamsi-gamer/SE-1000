using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void DrawDisplays()
        {
            if (!TooComplex) DrawMain();
            if (!TooComplex) DrawInfo();
            if (!TooComplex) DrawSong();
            if (!TooComplex) DrawIO();

            if (!TooComplex)
            { 
                if (false) DrawClips();
                else       DrawMixer();
            }

            DrawVolume();
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
                Alignment       = TaC,
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
            DrawLine(sprites, x,   y,   x+w, y,   c, wd);
            DrawLine(sprites, x,   y+h, x+w, y+h, c, wd);
            DrawLine(sprites, x,   y,   x,   y+h, c, wd);
            DrawLine(sprites, x+w, y,   x+w, y+h, c, wd);
        }


        void FillCircle(List<MySprite> sprites, Vector2 p, float r, Color color)
        {
            DrawTexture(sprites, "Circle", p.X - r, p.Y - r, r*2, r*2, color);
        }


        void FillCircle(List<MySprite> sprites, float x, float y, float r, Color color)
        {
            DrawTexture(sprites, "Circle", x-r, y-r, r*2, r*2, color);
        }


        void DrawCircle(List<MySprite> sprites, Vector2 p, float r, Color color)
        {
            DrawTexture(sprites, "CircleHollow", p.X-r, p.Y-r, r*2, r*2, color);
        }


        void DrawCircle(List<MySprite> sprites, float x, float y, float r, Color color)
        {
            DrawTexture(sprites, "CircleHollow", x-r, y-r, r*2, r*2, color);
        }


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
    }
}
