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

            public Action<Label> UpdateFunc,
                                 ColorFunc;

            public int           Data;

            public bool          UsedForSession;

          //public bool          NeedsUpdate;


            public Label(int           category, 
                         IMyTextPanel  panel,
                         CondFunc      condBright     = CF_null, 
                         CondFunc      condDim        = CF_null, 
                         Action<Label> updateFunc     = AL_null, 
                         Action<Label> colorFunc      = AL_null, 
                         int           data           = 0,
                         bool          usedForSession = False)
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

                UsedForSession  = usedForSession;

              //NeedsUpdate     = T;

                     if (category == 2) g_fastLabels.Add(this);
                else if (category == 1) g_clipLabels.Add(this);
                else                    g_slowLabels.Add(this);
            }


            public void Update()
            {
                ForeColor = color6;
                HalfColor = color3;
                BackColor = color0;

                ColorFunc ?.Invoke(this);
                UpdateFunc?.Invoke(this);

                var bCond = OK(BrightCondition) && BrightCondition(this);
                var dCond = OK(DimCondition)    && DimCondition   (this);

                Update(IsPressed(this) || bCond, dCond);
            }


            public void SetText(string text, float size = 10, float pad = 10)
            {
                Panel.WriteText(strEmpty);

                Panel.FontSize    = size;
                Panel.TextPadding = pad;

                Panel.WriteText(text);
            }


            public void Update(bool full, bool half = False)
            {
                if (   UsedForSession
                    || OK(EditedClip))
                {
                    Panel.FontColor       = full ? BackColor : ForeColor;
                    Panel.BackgroundColor = full ? ForeColor : (half ? HalfColor : BackColor);
                }
                else
                {
                    Panel.FontColor       = color0;
                    Panel.BackgroundColor = color0;
                }
            }


            public void Mark(bool on = True)
            {
                g_labelsPressed.Add(this);
                Update(on);
            }


            public void Unmark(bool on = False, bool half = False)
            {
                Update(on, half);
                _labelsPressed.Remove(this);
            }
        }
    }
}
