using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class Harmonics : Setting
        {
            public enum Preset { Sine, Square4, Square8, Square16, Square24, Saw4, Saw8, Saw16, Saw24, Pulse4, Pulse8, Pulse16, Pulse24, Random4, Random8, Random16, Random24 };

            public   Parameter[] Tones    = new Parameter[24];
             float[]     m_values = new float    [24];

            public Preset      CurPreset;
            public int         CurTone;


            public Harmonics(Instrument inst, Source src) 
                : base(strHrm, Setting_null, Setting_null, inst, src)
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i] = NewHarmonicParam(i, this, inst, src);

                CurPreset = Preset.Sine;
                CurTone   = 0;
            }


            public Harmonics(Harmonics hrm) 
                : base(hrm.Tag, Setting_null, hrm, hrm.Instrument, hrm.Source)
            {
                for (int i = 0; i < hrm.Tones.Length; i++)
                    Tones[i] = new Parameter(hrm.Tones[i], this);

                CurPreset = hrm.CurPreset;
                CurTone   = hrm.CurTone;
            }


            public Harmonics Copy()
            {
                return new Harmonics(this);
            }


            public void SetPreset(Preset preset)
            {
                switch (preset)
                {
                case Preset.Sine:     FillSine();              break;
                                                                 
                case Preset.Square4:  FillSquare  ( 4);        break;
                case Preset.Square8:  FillSquare  ( 8);        break;
                case Preset.Square16: FillSquare  (16);        break;
                case Preset.Square24: FillSquare  (24);        break;
                                                                
                case Preset.Saw4:     FillSaw     ( 4);        break;
                case Preset.Saw8:     FillSaw     ( 8);        break;
                case Preset.Saw16:    FillSaw     (16);        break;
                case Preset.Saw24:    FillSaw     (24);        break;
                                                                   
                case Preset.Pulse4:   FillPulse   ( 4);        break;
                case Preset.Pulse8:   FillPulse   ( 8);        break;
                case Preset.Pulse16:  FillPulse   (16);        break;
                case Preset.Pulse24:  FillPulse   (24);        break;

                case Preset.Random4:  FillRandom  ( 4, g_rnd); break;
                case Preset.Random8:  FillRandom  ( 8, g_rnd); break;
                case Preset.Random16: FillRandom  (16, g_rnd); break;
                case Preset.Random24: FillRandom  (24, g_rnd); break;
                }
            }


            public void FillSine()
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i == 0 ? 1 : 0, Note_null, -1);
            }


            public void FillSquare(int nTones)
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i >= nTones || i%2 != 0 ? 0 : 1f/(i+1), Note_null, -1);
            }


            public void FillSaw(int nTones)
            {
                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i >= nTones ? 0 : 1f/(i+1), Note_null, -1);
            }


            public void FillPulse(int nTones)
            {
                var div = (float)Math.Log(nTones, 1.75);

                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(i >= nTones ? 0 : 1f/div, Note_null, -1);
            }


            public void FillRandom(int nTones, Random rnd)
            {
                var total = 0f;

                for (int i = 0; i < Tones.Length; i++)
                { 
                    m_values[i] =
                           i >= nTones 
                        || rnd.NextDouble() < 0.5 
                        ? 0 
                        : (float)rnd.NextDouble();

                    total += m_values[i] / (float)Math.Pow(i+1, 0.5);
                }

                for (int i = 0; i < Tones.Length; i++)
                    Tones[i].SetValue(m_values[i]/total, Note_null, -1);
            }


            public void CreateSounds(List<Sound> sounds, Source src, Note note, int noteNum, long sndTime, int sndLen, int relLen, List<TriggerValue> triggerValues, Program prog)
            {
                var inst = src.Instrument;
                var iSrc = inst.Sources.IndexOf(src);

                Sound snd0 = Sound_null;

                for (int i = 0; i < Tones.Length; i++)
                {
                    if (prog.TooComplex) return;

                    if (    Tones[i].Value == 0
                        && !Tones[i].HasDeepParams(note.Channel, iSrc))
                        continue;

                    var _noteNum = freq2note(note2freq(noteNum) * (i+1));

                    if (   _noteNum <  12*NoteScale
                        || _noteNum > 150*NoteScale)
                        continue;

                    var sample = src.GetSample(_noteNum);
                    var hrmPos = i / (float)Tones.Length;

                    var snd = new Sound(
                        sample, 
                        note.Channel,
                        note.iChan,
                        sndTime,
                        sndLen,
                        relLen,
                        1, // is set below
                        inst,
                        iSrc,
                        note,
                        triggerValues,
                        False,
                        Sound_null,
                        0,
                        Tones[i],
                        snd0,
                        hrmPos);

                    if (i == 0) snd0 = snd;

                    var lTime = g_time - sndTime;

                    if (!prog.TooComplex)
                    { 
                        var tp = new TimeParams(g_time, lTime, note, sndLen, iSrc, snd.TriggerValues, prog);
                        Tones[i].UpdateValue(tp);
                        sounds.Add(snd);
                    }
                }
            }


            public override void Clear()
            {
                foreach (var tone in Tones)
                    tone.Clear();
            }


            public override void Reset()
            {
                base.Reset();

                foreach (var tone in Tones)
                    tone.Reset();
            }


            public override void Randomize(Program prog)
            {
                SetPreset((Preset)g_rnd.Next(0, 17));

                if (RND > 0.5)
                    Smooth();

                for (int i = 0; i < Tones.Length; i++)
                {
                    var tone = Tones[i];

                    if (RND > 0.9f) tone.Randomize(prog);
                    else            tone.Clear();
                }
            }


            public void Adjust(float delta)
            { 
                if (OK(CurTone))
                {
                    var tone = Tones[CurTone];
                    tone.SetValue(tone.AdjustValue(tone.Value, delta, EditedClip.Shift), Note_null, -1);
                }
                else
                {
                    foreach (var tone in Tones)
                        tone.SetValue(tone.AdjustValue(tone.Value, delta, EditedClip.Shift, True), Note_null, -1);
                }
            }


            public void Smooth()
            { 
                var values = new float[Tones.Length];

                for (var i = 0; i < values.Length; i++)
                { 
                         if (i == 0)               values[i] = (Tones[i  ].Value + Tones[i+1].Value)/2;
                    else if (i == values.Length-1) values[i] = (Tones[i-1].Value + Tones[i  ].Value)/2;
                    else                           values[i] = 0.25f*Tones[i-1].Value + 0.5f*Tones[i].Value + 0.25f*Tones[i+1].Value;          
                }

                for (var i = 0; i < values.Length; i++)
                    Tones[i].SetValue(values[i], Note_null, -1);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                return Tones[int_Parse(tag)];
            }


            public void Delete(Clip clip, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                foreach (var tone in Tones)
                    tone.Delete(clip, iSrc);
            }


            public override string Save()
            {
                var hrm = W(Tag);

                for (int i = 0; i < Tones.Length; i++)
                    hrm += W(Tones[i].Save());

                hrm += WS((int)CurPreset);
                hrm +=  S(CurTone);
                
                return hrm;
            }


            public static Harmonics Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tag = data[i++];

                var hrm = new Harmonics(
                    inst, 
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);

                for (int j = 0; j < hrm.Tones.Length; j++)
                    hrm.Tones[j] = Parameter.Load(data, ref i, inst, iSrc, hrm, hrm.Tones[j]);
                
                hrm.CurPreset = (Preset)int_Parse(data[i++]);
                hrm.CurTone   =         int_Parse(data[i++]);

                return hrm;
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                for (int i = 0; i < Tones.Length; i++)
                { 
                    if (Tones[i].HasDeepParams(CurChannel, CurSrc)) 
                        Tones[i].DrawLabels(sprites, x, y, dp); 
                }

                _dp.Next(dp);
            }


            public void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, Channel chan, Program prog)
            {
                FillRect(sprites, x, y, w, h, color0);

                var  rh = h - 90;
                var irh = h - 50;

                var xt  = 300f;
                var wt  = 600f;
                var yt  = 50f;
                var ht  = 300f;

                var gap = 4f;

                var wc  = wt / Tones.Length;

                var dp = new DrawParams(False, prog);
                SelSource.DrawLabels(sprites, x + 5, y + 10, dp);

                DrawSample(sprites, x + 100, y + 150, 100, 60);


                for (int i = 0; i < Tones.Length; i++)
                {
                    var curVal = Tones[i].CurValue;
                    var val    = Tones[i].Value;

                    var xh = xt + i*wc;

                    FillRect(sprites, xh + gap/2,                yt + ht, wc - gap,     -ht,          color2);
                    FillRect(sprites, xh + gap/2,                yt + ht, wc - gap - 8, -ht * curVal, color4);
                    FillRect(sprites, xh + gap/2 + (wc-gap) - 7, yt + ht, 7,            -ht * val,    color6);

                    DrawString(sprites, S(i+1), xt + i*wc + wc/2 - 3, yt + ht - 14, 0.4f, color3, TA_CENTER);
                }

                // current tone
                if (OK(CurTone)) FillRect(sprites, xt + CurTone * wc, yt + ht + 10, wc,    20, color5);
                else              FillRect(sprites, xt,                yt + ht + 10, wc*24, 20, color5);

                // has param marks
                for (int i = 0; i < Tones.Length; i++)
                {
                    if (Tones[i].HasDeepParams(chan, -1))
                        DrawString(sprites, strUp, xt + i*wc + wc/2, yt + ht + 10, 0.6f, color3, TA_CENTER);
                }


                var bw = w/6;
                var be = 0f;

                DrawPreset(sprites, CurPreset, 2*bw - be/2, h - 100, bw + be, 50);

                DrawFuncButtons(sprites, w, h, chan);
            }


            void DrawSample(List<MySprite> sprites, float x, float y, float w, float h)
            {
                var pPrev = new Vector2(float_NaN, float_NaN);


                var df = 1/48f;

                for (float f = 0; f < 1+df/2; f += df)
                {
                    var wf = 0f;
                
                    for (int i = 0; i < Tones.Length; i++)
                    {
                        var val = Tones[i].CurValue;
                        wf += (float)Math.Sin(f*(i+1) * Tau) * val;
                    }

                    var p = new Vector2(
                        x + w * f,
                        y + h/2 - wf * h/2);

                    if (   OK(pPrev.X)
                        && OK(pPrev.Y))
                        DrawLine(sprites, pPrev, p, color6, 2);

                    pPrev = p;
                }
            } 


            void DrawPreset(List<MySprite> sprites, Preset preset, float x, float y, float w, float h)
            {
                var str = "";

                switch (preset)
                {
                    case Preset.Sine:     str = "Sine";    break;

                    case Preset.Saw4:     str = "Saw 4";   break;
                    case Preset.Saw8:     str = "Saw 8";   break;
                    case Preset.Saw16:    str = "Saw 16";  break;
                    case Preset.Saw24:    str = "Saw 24";  break;

                    case Preset.Square4:  str = "Sqr 4";   break;
                    case Preset.Square8:  str = "Sqr 8";   break;
                    case Preset.Square16: str = "Sqr 16";  break;
                    case Preset.Square24: str = "Sqr 24";  break;

                    case Preset.Pulse4:   str = "Pls 4";   break;
                    case Preset.Pulse8:   str = "Pls 8";   break;
                    case Preset.Pulse16:  str = "Pls 16";  break;
                    case Preset.Pulse24:  str = "Pls 24";  break;
                                                       
                    case Preset.Random4:  str = "Rnd 4";   break;
                    case Preset.Random8:  str = "Rnd 8";   break;
                    case Preset.Random16: str = "Rnd 16";  break;
                    case Preset.Random24: str = "Rnd 24";  break;
                }


                var x0 = w/2;

                DrawRect  (sprites,      x,      y,     w, h, color4);
                DrawString(sprites, str, x + x0, y + 4,       1.2f, color6, TA_CENTER);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                DrawFuncButton(sprites, "Smth",  1, w, h, False, False, IsPressed(lcdMain+1));
                DrawFuncButton(sprites, "Pre ↕", 2, w, h, False, False, IsPressed(lcdMain+2));
                DrawFuncButton(sprites, "Set",   3, w, h, False, False, IsPressed(lcdMain+3));
                
                if (OK(CurTone))
                    DrawFuncButton(sprites, "Tone", 4, w, h, True, Tones[CurTone].HasDeepParams(chan, -1));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                    case 1: Smooth(); break;
                    case 2:
                    { 
                        var cp = (int)CurPreset + 1;

                        if (cp > (int)Preset.Random24)
                            cp = (int)Preset.Sine;

                        CurPreset = (Preset)cp;
                        g_lcdPressed.Add(lcdMain+func);

                        break;
                    }
                    case 3: SetPreset(CurPreset); break; 
                    case 4:
                        if (OK(CurTone)) 
                            AddNextSetting(S(CurTone));
                        break;
                }
            }


            public void MoveEdit(int move)
            {
                CurTone += move;

                if (CurTone >= Tones.Length) CurTone = -1;
                if (CurTone <  -1          ) CurTone = Tones.Length-1;
            }


            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}
