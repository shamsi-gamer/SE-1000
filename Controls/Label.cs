using System;
using Sandbox.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class Label
        {
            public delegate bool CondFunc(Label label);

            public IMyTextPanel  Panel;
                                        
            public Color         ForeColor,
                                 HalfColor,
                                 BackColor;

            public CondFunc      BrightCondition,
                                 DimCondition;

            public Action<Label> UpdateFunc;
            public Action<Label> ColorFunc;

            public int           Data;

          //public bool          NeedsUpdate;


            public Label(bool          fast, 
                         IMyTextPanel  panel, 
                         CondFunc      condBright = null, 
                         CondFunc      condDim    = null, 
                         Action<Label> updateFunc = null, 
                         Action<Label> colorFunc  = null, 
                         int           data = 0)
            {
                Panel           = panel;

                ForeColor       = color6;
                HalfColor       = color3;
                BackColor       = color0;

                BrightCondition = condBright;
                DimCondition    = condDim;

                UpdateFunc      = updateFunc;
                ColorFunc       = colorFunc;

                Data            = data;

              //NeedsUpdate     = true;

                if (fast) g_fastLabels.Add(this);
                else      g_slowLabels.Add(this);
            }


            public void Update()
            {
                ForeColor = color6;
                HalfColor = color3;
                BackColor = color0;

                if (ColorFunc  != null) ColorFunc(this);
                if (UpdateFunc != null) UpdateFunc(this);

                var bCond = BrightCondition != null ? BrightCondition(this) : false;
                var dCond = DimCondition    != null ? DimCondition   (this) : false;

                Update(IsPressed(this) || bCond, dCond);
            }


            public void SetText(string text, float size = 10, float pad = 10)
            {
                Panel.WriteText(" ");

                Panel.FontSize    = size;
                Panel.TextPadding = pad;

                Panel.WriteText(text);
            }


            public void Update(bool full, bool half = false)
            {
                Panel.FontColor       = full ? BackColor : ForeColor;
                Panel.BackgroundColor = full ? ForeColor : (half ? HalfColor : BackColor);
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
