using System;
using System.Text;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        const float g_labelHeight = 18;


        public class Setting
        {
            public string  Tag;

            public Setting Parent;
            public Setting Prototype;

            public bool   _IsCurrent;


            public Setting(string tag, Setting parent, Setting proto = null)
            {
                Parent    = parent;
                Tag       = tag;
                Prototype = proto;
               _IsCurrent = false;
            }


            public Setting(Setting setting, Setting parent) : this(setting.Tag, parent, setting) { }


            public string GetPath(int src = -1)
            {
                var path = new StringBuilder();

                Setting setting = this;

                while (setting != null)
                {
                    path.Insert(0, setting.Tag + (path.Length > 0 ? "/" : ""));
                    setting = setting.Parent;
                }

                if (src > -1)
                    path.Insert(0, src + "/");

                return S(path);
            }


            public virtual Setting GetOrAddSettingFromTag(string tag) => null;
            
            public Parameter GetOrAddParamFromTag(Parameter param, string tag)
            {
                return param ?? (Parameter)NewSettingFromTag(tag, this);
            }

            public virtual bool HasDeepParams(Channel chan, int src) { return false; }
            public virtual void Remove(Setting setting) {}
                                
            public virtual void Clear() {}


            public virtual void Randomize(Program prog) {}
            public virtual void AdjustFromController(Song song, Program prog) {}


            public virtual string GetLabel(out float width) 
            { 
                width = 30; 
                return ""; 
            }


            public virtual string GetUpLabel()   { return ""; }
            public virtual string GetDownLabel() { return ""; }


            public virtual void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams dp)
            {
                if (dp.Program.TooComplex) return;
                if (sprites == null) return;


                var textCol = this == CurSetting ? color0 : color6;
                var lineCol = this == CurSetting ? color6 : color4;
                var boxCol  = this == CurSetting ? color6 : color3;


                var dx = 8f;
                
                // draw connector lines
                if (   !HasTag(this, "Vol")
                    && !HasTag(this, "Off")
                    && GetType() != typeof(Tune)
                    && GetType() != typeof(Harmonics)
                    && GetType() != typeof(Filter)
                    && GetType() != typeof(Delay)
                    && GetType() != typeof(Arpeggio))
                { 
                    var ly = y + g_labelHeight/2;

                    // horizontal
                    DrawLine(sprites, x-dx, ly, x, ly, boxCol);

                    // vertical
                    if (dp.TopY > 0)
                        DrawLine(sprites, x-dx, ly-dp.TopY, x-dx, ly, boxCol);
                }

                dp.TopY = 0;


                float ew;
                var str = GetLabel(out ew);

                // label background
                FillRect(sprites, x, y, ew, 15, boxCol);

                // label name
                DrawString(sprites, Tag,            x +  5, y +  2, 0.36f, textCol);
                DrawString(sprites, str,            x + 36, y +  2, 0.36f, textCol);

                DrawString(sprites, GetUpLabel(),   x + 36, y -  6, 0.36f, color4);
                DrawString(sprites, GetDownLabel(), x + 36, y + 10, 0.36f, color4);

                if (IsParam(this))
                    ((Parameter)this).PrevValue = ((Parameter)this).CurValue;

                dp.OffX = ew + dx;
            }


            public virtual void   DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp) {}
                
            public virtual string Save() => "";

            public virtual void   DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan) {}
            public virtual void   Func(int func) {}
        }


        static Parameter GetCurrentParam(Instrument inst)
        {
            return (Parameter)GetSettingFromPath(inst, CurSetting.GetPath(CurSrc));
        }


        static Setting GetSettingFromPath(Instrument inst, string path)
        {
            var tags = path.Split('/');

            Setting setting = null;

            foreach (var tag in tags)
            {
                setting = 
                    setting == null
                    ? inst   .GetOrAddSettingFromTag(tag)
                    : setting.GetOrAddSettingFromTag(tag);
            }

            return setting;
        }


        static string FullNameFromTag(string tag)
        {
            if (IsDigit(tag[0])) return "Harmonic " + tag;

            switch (tag)
            { 
            case "Vol":  return "Volume";

            case "Tune": return "Tune";

            case "Trig": return "Trigger";
            case "Env":  return "Envelope";
            case "Att":  return "Attack";
            case "Dec":  return "Decay";
            case "Sus":  return "Sustain";
            case "Rel":  return "Release";

            case "LFO":  return "LFO";
            case "Amp":  return "Amplitude";
            case "Freq": return "Frequency";
            case "Off":  return "Offset";

            case "Flt":  return "Filter";
            case "Cut":  return "Cutoff";
            case "Res":  return "Resonance";

            case "Mod":  return "Modulate";
            case "Amt":  return "Amount";

            case "Del":  return "Delay";
            case "Cnt":  return "Count";
            case "Time": return "Time";
            case "Lvl":  return "Level";
            case "Pow":  return "Power";
                         
            case "Arp":  return "Arpeggio";
            case "Len":  return "Length";
            case "Scl":  return "Scale";
            }

            return "";
        }


        static Setting NewSettingFromTag(string tag, Setting parent)
        {
            switch (tag)
            { 
            case "Vol":  return new Parameter(tag,    0,           2,   0.5f,  1,    0.01f,  0.1f,  1,    parent);
                                                                                       
            case "Trig": return new Parameter((Parameter)parent, parent, tag, false);
            case "Env":  return new Envelope(parent);                                  
            case "Att":  return new Parameter(tag,    0,          10,   0,     1,    0.01f,  0.1f,  0,    parent);
            case "Dec":  return new Parameter(tag,    0,          10,   0,     1,    0.01f,  0.1f,  0.2f, parent);
            case "Sus":  return new Parameter(tag,    0,           1,   0.01f, 1,    0.01f,  0.1f,  0.1f, parent);
            case "Rel":  return new Parameter(tag,    0,          10,   0,     2,    0.01f,  0.1f,  0.2f, parent);
                                                                                                          
            case "Amp":  return new Parameter(tag,    0,           1,   0,     1,    0.001f, 0.05f, 1,    parent);
            case "Freq": return new Parameter(tag,    0.01f,      30,   0.01f, 4,    0.001f, 0.05f, 1,    parent);
            case "Off":  return new Parameter(tag, -100,         100, -10,    10,    0.001f, 0.05f, 0,    parent);
                                                                                                          
            case "Cut":  return new Parameter(tag,    0,           1,   0.1f,  1,    0.01f,  0.1f,  0.5f, parent);
            case "Res":  return new Parameter(tag,    0,           1,   0,     0.7f, 0.01f,  0.1f,  0,    parent);
            case "Shrp": return new Parameter(tag,    0,           1,   0,     0.9f, 0.01f,  0.1f,  0.9f, parent);
                                                                                                    
            case "Amt":  return new Parameter(tag,  -10,          10,  -1,     1,    0.01f,  0.1f,  0,    parent);
                                                                                                          
            case "Dry":  return new Parameter(tag,    0,           1,   0,     1,    0.01f, 0.1f,   1,    parent);
            case "Cnt":  return new Parameter(tag,    1,         100,   2,    16,    1,     10,     4,    parent);
            case "Time": return new Parameter(tag,    0.000001f,  10,   0.01f, 0.3f, 0.01f,  0.1f,  0.2f, parent);
            case "Lvl":  return new Parameter(tag,    0,           1,   0.3f,  1,    0.01f,  0.1f,  0.5f, parent);
            case "Pow":  return new Parameter(tag,    0.01f,      10,   0.2f,  1.2f, 0.01f,  0.1f,  1,    parent);
                                                                                                          
            case "Len":  return new Parameter(tag,    1,         256,   2,     6,    0.01f,  0.1f,  8,    parent);
            case "Scl":  return new Parameter(tag,    0.01f,      16,   0.25f, 4,    0.01f,  0.1f,  1,    parent);
            }

            return null;
        }


        // I use these instead of Setting methods because this way
        // all the extra null checks are avoided and it keeps the code
        // more compact, since that's an issue here

        static bool IsParam(Setting setting) 
        {
            if (setting == null) return false;

            return setting.GetType() == typeof(Tune)
                || setting.GetType() == typeof(Parameter); 
        }


        bool IsSettingType(Setting setting, Type type)
        {
            return
                   setting != null
                && setting.GetType() == type;
        }


        static bool HasTag(Setting setting, string tag)
        {
            return 
                   setting != null
                && setting.Tag == tag;
        }


        static bool HasTagOrParent(Setting setting, string tag)
        {
            return HasTag(setting, tag)
                ||    setting.Parent != null
                   && HasTag(setting.Parent, tag);
        }


        static bool HasTagOrAnyParent(Setting setting, string tag)
        {
            while (setting != null)
            {
                if (setting.Tag == tag)
                    return true;

                setting = setting.Parent;
            }

            return false;
        }
    }
}
