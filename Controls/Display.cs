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
            public IMyTextPanel   Panel;
            public RectangleF     Viewport;

            public float          Scale,

                                  ContentWidth,
                                  ContentHeight;



            public Display(IMyTextPanel panel)
            {
                Panel = panel;
                Init();
            }



            void Init()
            {
                Panel.ContentType = ContentType.SCRIPT;

                Scale             = 1;
                                  
                Panel.Script      = "";

                Viewport          = new RectangleF((Panel.TextureSize - Panel.SurfaceSize) / 2, Panel.SurfaceSize);
                                  
                ContentWidth      = Viewport.Width;
                ContentHeight     = Viewport.Height;
            }



            public float ContentScale
            {
                get
                {
                    return
                          Math.Min(Panel.TextureSize.X, Panel.TextureSize.Y) / 512
                        * Math.Min(Panel.SurfaceSize.X, Panel.SurfaceSize.Y)
                        / Math.Min(Panel.TextureSize.Y, Panel.TextureSize.Y);
                }
            }



            public float UserScale
            {
                get
                {
                    if (Scale == 0)
                    {
                        return
                            Panel.SurfaceSize.X / ContentWidth < Panel.SurfaceSize.Y / ContentHeight
                            ? (Panel.SurfaceSize.X - 10) / ContentWidth
                            : (Panel.SurfaceSize.Y - 10) / ContentHeight;
                    }
                    else return Scale;
                }
            }



            public void Draw(List<MySprite> sprites)
            {
                var frame = Panel.DrawFrame();
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
