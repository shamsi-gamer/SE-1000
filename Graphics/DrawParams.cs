namespace IngameScript
{
    partial class Program
    {
        public class DrawParams
        {
            public float   Volume;
            public bool    Active;
            public bool    On;
            public bool    Children;
            public Program Program;
            public float   OffX;
            public float   OffY;

            public DrawParams(Program prog)
            {
                Volume   = 1;
                Active   = true;
                On       = true;
                Children = false;
                Program  = prog;
                OffX     = 0;
                OffY     = 0;
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

            public DrawParams(float vol, bool active, bool on, bool children, Program prog)
                : this(vol, prog)
            {
                Active   = active;
                On       = on;
                Children = children;
            }
        }
    }
}
