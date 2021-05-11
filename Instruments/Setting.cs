using System;
using System.Text;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        const float g_labelHeight = 18;


        public class Setting
        {
            public string     Tag;

            public Setting    Parent;
            public Setting    Prototype;

            public Instrument Instrument;
            public Source     Source;

            public bool      _IsCurrent;

            protected bool   m_valid;


            public Setting(string tag, Setting parent, Setting proto, Instrument inst, Source src)
            {
                Tag         = tag;
                Parent      = parent;
                Prototype   = proto;
                Instrument  = inst;
                Source      = src;
               _IsCurrent   = false;
                m_valid     = false;
            }


            public Setting(Setting setting, Setting parent, Instrument inst, Source src) 
                : this(setting.Tag, parent, setting, inst, src) { }


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
                return param ?? (Parameter)NewSettingFromTag(tag, this, Instrument, Source);
            }

            public virtual bool HasDeepParams(Channel chan, int src) { return false; }
            public virtual void Remove(Setting setting) {}
                                

            public virtual void Clear() {}


            public virtual void Reset()
            { 
                m_valid = false;
            }


            public virtual void Randomize(Program prog) {}
            public virtual void AdjustFromController(Clip clip, Program prog) {}


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
                if (   !HasTag(this, strVol)
                    && !HasTag(this, strOff)
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


            public bool           ParentIsEnvelope    { get { return HasTag(Parent, strEnv); } }
            public bool           AnyParentIsEnvelope { get { return HasTagOrAnyParent(Parent, strEnv); } }


            public virtual void   DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp) {}
                
            public virtual string Save() => "";

            public virtual void   DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan) {}
            public virtual void   Func(int func) {}

            public virtual bool   CanDelete() { return false; }
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
            case strVol:  return "Volume";

            case strTune: return strTune;

            case strTrig: return "Trigger";
            case strEnv:  return "Envelope";
            case strAtt:  return "Attack";
            case strDec:  return "Decay";
            case strSus:  return "Sustain";
            case strRel:  return "Release";

            case strLfo:  return strLfo;
            case strAmp:  return "Amplitude";
            case strFreq: return "Frequency";
            case strOff:  return "Offset";

            case strFlt:  return "Filter";
            case strCut:  return "Cutoff";
            case strRes:  return "Resonance";

            case strMod:  return "Modulate";
            case strAmt:  return "Amount";

            case strDel:  return "Delay";
            case strCnt:  return "Count";
            case strTime: return strTime;
            case strLvl:  return "Level";
            case strPow:  return "Power";
                         
            case strArp:  return "Arpeggio";
            case strLen:  return "Length";
            case strScl:  return "Scale";
            }

            return "";
        }


        static Setting NewSettingFromTag(string tag, Setting parent, Instrument inst, Source src)
        {
            switch (tag)
            { 
            case strVol:  return new Parameter(tag,    0,           2,   0.5f,  1,    0.01f,  0.1f,  1,    parent, inst, src);
                                                                                       
            case strTrig: return new Parameter((Parameter)parent, parent, tag, false);
            case strEnv:  return new Envelope(parent, inst, src);                                  
            case strAtt:  return new Parameter(tag,    0,          10,   0,     1,    0.01f,  0.1f,  0,    parent, inst, src);
            case strDec:  return new Parameter(tag,    0,          10,   0,     1,    0.01f,  0.1f,  0.2f, parent, inst, src);
            case strSus:  return new Parameter(tag,    0,           1,   0.01f, 1,    0.01f,  0.1f,  0.1f, parent, inst, src);
            case strRel:  return new Parameter(tag,    0,          10,   0,     2,    0.01f,  0.1f,  parent.GetType() == typeof(Modulate) ? 0 : 0.2f, parent, inst, src);

            case strAmp:  return new Parameter(tag,    0,           1,   0,     1,    0.001f, 0.05f, 1,    parent, inst, src);
            case strFreq: return new Parameter(tag,    0.01f,      30,   0.01f, 4,    0.001f, 0.05f, 1,    parent, inst, src);
            case strOff:  return new Parameter(tag, -100,         100, -10,    10,    0.01f,  0.1f,  0,    parent, inst, src);

            case strCut:  return new Parameter(tag,    0,           1,   0.1f,  1,    0.01f,  0.1f,  0.5f, parent, inst, src);
            case strRes:  return new Parameter(tag,    0,           1,   0,     0.7f, 0.01f,  0.1f,  0,    parent, inst, src);
            case strShrp: return new Parameter(tag,    0,           1,   0,     0.9f, 0.01f,  0.1f,  0.9f, parent, inst, src);

            case strAmt:  return new Parameter(tag,   -1,           1,  -0.5f,  0.5f, 0.01f,  0.1f,  0,    parent, inst, src);

            case strDry:  return new Parameter(tag,    0,           1,   0,     1,    0.01f, 0.1f,   1,    parent, inst, src);
            case strCnt:  return new Parameter(tag,    1,         100,   2,    16,    1,     10,     4,    parent, inst, src);
            case strTime: return new Parameter(tag,    0.000001f,  10,   0.01f, 0.3f, 0.01f,  0.1f,  0.2f, parent, inst, src);
            case strLvl:  return new Parameter(tag,    0,           1,   0.3f,  1,    0.01f,  0.1f,  0.5f, parent, inst, src);
            case strPow:  return new Parameter(tag,    0.01f,      10,   0.2f,  1.2f, 0.01f,  0.1f,  1,    parent, inst, src);

            case strLen:  return new Parameter(tag,    1,         256,   2,     6,    0.01f,  0.1f,  8,    parent, inst, src);
            case strScl:  return new Parameter(tag,    0.01f,      16,   0.25f, 4,    0.01f,  0.1f,  1,    parent, inst, src);
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


        static bool IsSettingType(Setting setting, Type type)
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
