using System;
using VRageMath;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        public class Label
        {
            public delegate bool CondFunc(Label label);

            public IMyTextPanel        Panel;
                                        
            public CondFunc            BrightCondition,
                                       DimCondition;

            public Action<Label, bool> UpdateFunc;

            public int                 Data;

          //public bool                NeedsUpdate;


            public Label(bool fast, IMyTextPanel panel, CondFunc condBright, CondFunc condDim, Action<Label, bool> updateFunc = null, int data = 0)
            {
                Panel           = panel;

                BrightCondition = condBright;
                DimCondition    = condDim;

                UpdateFunc      = updateFunc;

                Data            = data;

              //NeedsUpdate     = true;

                if (fast) g_fastLabels.Add(this);
                else      g_slowLabels.Add(this);
            }


            public void Update()
            {
                var bCond = BrightCondition(this);
                var dCond = bCond ? false : DimCondition(this);

                if (UpdateFunc != null) UpdateFunc(this, bCond);
                else                    Update(bCond, dCond);
            }


            public void Update(string text, float size = 10, float pad = 10)
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
                g_labelsPressed.Add(this);
                Update(on);
            }


            public void Unmark(bool on = false, bool half = false)
            {
                Update(on, half);
                _labelsPressed.Remove(this);
            }
        }
    }
}
