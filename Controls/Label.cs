using System;
using VRageMath;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        public class Label
        {
            public delegate bool CondFunc();

            public IMyTextPanel        Panel;
                                        
            public CondFunc            BrightCondition,
                                       DimCondition;

            public Action<Label, bool> UpdateFunc;

          //public bool                NeedsUpdate;


            public Label(bool fast, IMyTextPanel panel, CondFunc condBright, CondFunc condDim, Action<Label, bool> updateFunc = null)
            {
                Panel           = panel;

                BrightCondition = condBright;
                DimCondition    = condDim;

                UpdateFunc      = updateFunc;

              //NeedsUpdate     = true;

                if (fast) g_fastLabels.Add(this);
                else      g_slowLabels.Add(this);
            }


            public void Update()
            {
                var bCond = BrightCondition();
                var dCond = bCond ? false : DimCondition();

                if (UpdateFunc != null) UpdateFunc(this, bCond);
                else                    Update(bCond, dCond);
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
