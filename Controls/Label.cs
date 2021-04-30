using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        public class Label
        {
            public delegate bool CondFunc();

            public IMyTextPanel Panel;

            public CondFunc     BrightCondition,
                                DimCondition;

            public bool         NeedsUpdate;


            public Label(IMyTextPanel panel, CondFunc condBright, CondFunc condDim)
            {
                Panel           = panel;

                BrightCondition = condBright;
                DimCondition    = condDim;

                NeedsUpdate     = true;
            }


            public void Update()
            {
                Update(
                    BrightCondition(), 
                    DimCondition());
            }


            public void Update(string text, float size, float pad)
            {
                Panel.WriteText(" ");

                Panel.FontSize    = size;
                Panel.TextPadding = pad;

                Panel.WriteText(text);
            }


            public void Update(bool b, bool b2 = false)
            {
                if (b)
                {
                    Panel.FontColor       = color0;
                    Panel.BackgroundColor = color6;
                }
                else
                {
                    Panel.FontColor       = color6;
                    Panel.BackgroundColor = b2 ? color3 : color0;
                }
            }


            public void Mark(bool on = true)
            {
                g_lightsPressed.Add(Panel);
                Update(on);
            }


            public void Unmark(bool on = false, bool half = false)
            {
                Update(on, half);
                _lightsPressed.Remove(Panel);
            }
        }
    }
}
