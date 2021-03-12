using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class Display
        {
            bool g_useSurfaceSize;

            public IMyTextSurface Surface;
            public RectangleF     Viewport;

            public float          Scale,

                                  ContentWidth,
                                  ContentHeight;


            public Display(IMyTextSurface surface, bool useSurfaceSize = true)
            {
                Surface          = surface;
                g_useSurfaceSize = useSurfaceSize;

                Init();
            }


            void Init()
            {
                Surface.ContentType = ContentType.SCRIPT;

                Scale = 1;

                Surface.Script = "";

                Viewport = new RectangleF((Surface.TextureSize - Surface.SurfaceSize) / 2, Surface.SurfaceSize);

                ContentWidth  = Viewport.Width;
                ContentHeight = Viewport.Height;
            }


            public float ContentScale
            {
                get
                {
                    return
                          Math.Min(Surface.TextureSize.X, Surface.TextureSize.Y) / 512
                        * Math.Min(Surface.SurfaceSize.X, Surface.SurfaceSize.Y)
                        / Math.Min(Surface.TextureSize.Y, Surface.TextureSize.Y);
                }
            }


            public float UserScale
            {
                get
                {
                    if (Scale == 0)
                    {
                        return
                            Surface.SurfaceSize.X / ContentWidth < Surface.SurfaceSize.Y / ContentHeight
                            ? (Surface.SurfaceSize.X - 10) / ContentWidth
                            : (Surface.SurfaceSize.Y - 10) / ContentHeight;
                    }
                    else return Scale;
                }
            }


            public void Draw(List<MySprite> sprites)
            {
                var frame = Surface.DrawFrame();
                Draw(ref frame, sprites);
                frame.Dispose();
            }


            public void Draw(ref MySpriteDrawFrame frame, List<MySprite> sprites = null)
            {
                foreach (var sprite in sprites)
                    Draw(ref frame, sprite);
            }


            public void Draw(ref MySpriteDrawFrame frame, MySprite sprite)
            {
                     if (sprite.Type == SpriteType.TEXT   ) sprite.RotationOrScale *= UserScale;
                else if (sprite.Type == SpriteType.TEXTURE) sprite.Size            *= UserScale;

                sprite.Position *= UserScale;

                sprite.Position +=
                      Viewport.Position
                    + Viewport.Size / 2
                    - new Vector2(ContentWidth, ContentHeight) / 2 * UserScale;
                
                frame.Add(sprite);
            }
        }
    }
}
