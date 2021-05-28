namespace IngameScript
{
    partial class Program
    {
        public class DrawParams
        {
            public float   Volume;
            public bool    Active,
                           On,
                           Children;
            public Program Program;
            public float   OffX,
                           OffY,
                           TopY;


            public DrawParams(Program prog)
            {
                Volume   = 1;
                Active   = True;
                On       = True;
                Children = False;
                Program  = prog;
                OffX     = 0;
                OffY     = 0;
                TopY     = 0;
            }


            public DrawParams(float vol, Program prog) 
                : this(prog)
            {
                Volume = vol;
            }


            public DrawParams(bool active, Program prog)
                : this(prog)
            {
                Active = active;
            }


            public DrawParams(float vol, bool active, bool on, Program prog) 
                : this(vol, prog)
            {
                Active = active;
                On     = on;
            }

            
            public DrawParams(bool active, bool children, Program prog)
                : this(prog)
            {
                Active   = active;
                Children = children;
            }

            
            public DrawParams(DrawParams dp)
            {
                Volume   = dp.Volume;
                Active   = dp.Active;
                On       = dp.On;
                Children = False;
                Program  = dp.Program;
                OffX     = dp.OffX;
                OffY     = 0;
                TopY     = dp.TopY;
            }


            public DrawParams(float vol, bool active, bool on, bool children, Program prog)
                : this(vol, prog)
            {
                Active   = active;
                On       = on;
                Children = children;
            }

            
            public void Next(DrawParams dp)
            {
                OffY += dp.OffY;

                if (!dp.Children)
                    OffY += g_labelHeight;

                TopY = OffY;

                Children = True;
            }
        }
    }
}
