using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        public class Envelope : Setting
        {
            public Parameter Attack, // these params can't have envelopes
                             Decay,
                             Sustain,
                             Release;

            public float     TrigAttack,
                             TrigDecay,
                             TrigSustain,
                             TrigRelease;

            public float     CurValue;


            public Envelope(Setting parent, Instrument inst, Source src) 
                : base(strEnv, parent, Setting_null, inst, src)
            {
                Attack      = (Parameter)NewSettingFromTag(strAtt, this, inst, src);
                Decay       = (Parameter)NewSettingFromTag(strDec, this, inst, src);
                Sustain     = (Parameter)NewSettingFromTag(strSus, this, inst, src);
                Release     = (Parameter)NewSettingFromTag(strRel, this, inst, src);

                TrigAttack  = 
                TrigDecay   = 
                TrigSustain =
                TrigRelease = float_NaN;
            }


            public Envelope(Envelope env, Setting parent) 
                : base(env.Tag, parent, env, env.Instrument, env.Source)
            {
                Attack      = new Parameter(env.Attack,  this);
                Decay       = new Parameter(env.Decay,   this);
                Sustain     = new Parameter(env.Sustain, this);
                Release     = new Parameter(env.Release, this);

                TrigAttack  = env.TrigAttack;
                TrigDecay   = env.TrigDecay;
                TrigSustain = env.TrigSustain;
                TrigRelease = env.TrigRelease;
            }


            public Envelope Copy(Setting parent)
            {
                return new Envelope(this, parent);
            }


            public float UpdateValue(TimeParams tp)
            {
                if ( /*m_valid
                    ||*/tp.Program.TooComplex) 
                    return CurValue;


                if (OK(tp.Note))
                    tp.NoteLength = tp.Note.FrameLength;

                CurValue = UpdateValue(
                    tp.LocalTime, 
                    tp.NoteLength, 
                    tp.GetTriggerValue(Attack ),
                    tp.GetTriggerValue(Decay  ),
                    tp.GetTriggerValue(Sustain),
                    tp.GetTriggerValue(Release));

                m_valid = True;

                return CurValue;
            }


            public static float UpdateValue(long lTime, int noteLen, float a, float d, float s, float r)
            {
                var lt = lTime  /(float)FPS;
                var nl = noteLen/(float)FPS;

                if (nl < a + d)
                    nl = a + d;

                if (lt >= nl + r)
                    return 0;
                
                else if (lt >= nl) // release
                {
                         if (a   >= nl) s  = nl/a;
                    else if (a+d >= nl) s += (1 - (nl-a)/d) * (1-s);

                    return s * (1 - MinMax(0, (float)Math.Pow((lt-nl)/r, 2), 1));
                }
                else if (lt >= a + d) // sustain
                    return s;
                
                else if (lt >= a) // decay
                    return s + (1 - (float)Math.Pow((lt-a)/d, 2)) * (1-s);
                
                else if (lt >= 0) // attack 
                    return lt/a;

                return 0;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Attack .HasDeepParams(chan, src)
                    || Decay  .HasDeepParams(chan, src)
                    || Sustain.HasDeepParams(chan, src)
                    || Release.HasDeepParams(chan, src);
            }


            public override void Clear()
            {
                Attack .Clear();
                Decay  .Clear();
                Sustain.Clear();
                Release.Clear();
            }


            public override void Reset()
            {
                base.Reset();

                Attack .Reset();
                Decay  .Reset();
                Sustain.Reset();
                Release.Reset();
            }


            public override void Randomize(Program prog)
            {
                Attack .Randomize(prog);
                Decay  .Randomize(prog);
                Release.Randomize(prog);
                Sustain.Randomize(prog);
            }


            public override void AdjustFromController(Clip clip, Program prog)
            {
                var mi = g_remote.MoveIndicator;
                var ri = g_remote.RotationIndicator;

                if (mi.Z != 0) prog.AdjustFromController(clip, Attack,  -mi.Z/ControlSensitivity);
                if (mi.X != 0) prog.AdjustFromController(clip, Decay,    mi.X/ControlSensitivity);

                if (ri.X != 0) prog.AdjustFromController(clip, Sustain, -ri.X/ControlSensitivity);
                if (ri.Y != 0) prog.AdjustFromController(clip, Release,  ri.Y/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strAtt: return GetOrAddParamFromTag(Attack,  tag);
                    case strDec: return GetOrAddParamFromTag(Decay,   tag);
                    case strSus: return GetOrAddParamFromTag(Sustain, tag);
                    case strRel: return GetOrAddParamFromTag(Release, tag);
                }

                return Setting_null;
            }


            public void Delete(Clip clip, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Attack .Delete(clip, iSrc);
                Decay  .Delete(clip, iSrc);
                Sustain.Delete(clip, iSrc);
                Release.Delete(clip, iSrc);
            }


            public override string Save()
            {
                return
                      W (Tag)

                    + W (Attack .Save())
                    + W (Decay  .Save())
                    + W (Sustain.Save())
                    +    Release.Save();
            }


            public static Envelope Load(string[] data, ref int i, Instrument inst, int iSrc, Setting parent)
            {
                var tag = data[i++];

                var env = new Envelope(parent, inst, OK(iSrc) ? inst.Sources[iSrc] : Source_null);

                env.Attack  = Parameter.Load(data, ref i, inst, iSrc, env, env.Attack );
                env.Decay   = Parameter.Load(data, ref i, inst, iSrc, env, env.Decay  );
                env.Sustain = Parameter.Load(data, ref i, inst, iSrc, env, env.Sustain);
                env.Release = Parameter.Load(data, ref i, inst, iSrc, env, env.Release);

                return env;
            }


            public override string GetLabel(out float width)
            {
                width = 174;

                return
                      PrintValue(Attack .Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Decay  .Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Sustain.Value, 2, True, 0).PadLeft(4) + strEmpty
                    + PrintValue(Release.Value, 2, True, 0).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                if (!_dp.Program.TooComplex)
                {
                    base.DrawLabels(sprites, x, y, dp);

                    if (Attack .HasDeepParams(CurChannel, CurSrc)) Attack .DrawLabels(sprites, x, y, dp);
                    if (Decay  .HasDeepParams(CurChannel, CurSrc)) Decay  .DrawLabels(sprites, x, y, dp);
                    if (Sustain.HasDeepParams(CurChannel, CurSrc)) Sustain.DrawLabels(sprites, x, y, dp);
                    if (Release.HasDeepParams(CurChannel, CurSrc)) Release.DrawLabels(sprites, x, y, dp);
                }

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var tp = new TimeParams(g_time, 0, Note_null, EditedClip.EditLength, -1, _triggerDummy, EditedClip, dp.Program);

                Attack .UpdateValue(tp);
                Decay  .UpdateValue(tp);
                Sustain.UpdateValue(tp);
                Release.UpdateValue(tp);

                var isAtt = IsCurParam(strAtt);
                var isDec = IsCurParam(strDec);
                var isSus = IsCurParam(strSus);
                var isRel = IsCurParam(strRel);


                var w0 = 240f;
                var h0 = 120f;

                var x0 = x + w/2 - w0/2;
                var y0 = y + h/2 - h0/2;

                Vector2 p0, p1, p2, p3, p4;

                GetEnvelopeCoords(x0, y0, w0, h0, Math.Min(dp.Volume, 1), False, out p0, out p1, out p2, out p3, out p4);
                DrawEnvelopeSupportsAndInfo(sprites, p0, p1, p2, p3, p4, y0, h0, isAtt, isDec, isSus, isRel);

                GetEnvelopeCoords(x0, y0, w0, h0, Math.Min(dp.Volume, 1), True, out p0, out p1, out p2, out p3, out p4);
                DrawEnvelope(sprites, p0, p1, p2, p3, p4, color3, False, False, False, False, Decay.CurValue);

                GetEnvelopeCoords(x0, y0, w0, h0, Math.Min(dp.Volume, 1), False, out p0, out p1, out p2, out p3, out p4);
                DrawEnvelope(sprites, p0, p1, p2, p3, p4, color5, isAtt, isDec, isSus, isRel, Decay.Value);
            }


            void DrawEnvelopeSupportsAndInfo(List<MySprite> sprites, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float y, float h, bool isAtt, bool isDec, bool isSus, bool isRel)
            {
                var sw = 1;

                DrawLine(sprites, p0.X, p0.Y, p0.X, y + h, color3, sw);
                DrawLine(sprites, p2.X, p2.Y, p2.X, p0.Y,  color3, sw);
                DrawLine(sprites, p1.X, p1.Y, p1.X, y + h, color3, sw);
                DrawLine(sprites, p3.X, p3.Y, p3.X, p0.Y,  color3, sw);
                DrawLine(sprites, p1.X, p2.Y, p3.X, p3.Y,  color3, sw);
                                                              
                DrawLine(sprites, p0.X, p0.Y, p4.X, p4.Y,  color3, sw);


                // labels

                var a = Attack .Value;
                var d = Decay  .Value;
                var s = Sustain.Value;
                var r = Release.Value;

                var fs = 0.5f;

                DrawString(sprites, S_00(a) + (isAtt ? " s" : ""),  p0.X           +  6,  p0.Y +  3,         fs, isAtt ? color6 : color3, TA_CENTER);
                DrawString(sprites, S_00(d) + (isDec ? " s" : ""), (p1.X + p2.X)/2 + 16, (p1.Y+p2.Y)/2 - 20, fs, isDec ? color6 : color3, TA_CENTER);
                DrawString(sprites, S_00(s),                       (p2.X + p3.X)/2 -  5,  p2.Y - 20,         fs, isSus ? color6 : color3, TA_CENTER);
                DrawString(sprites, S_00(r) + (isRel ? " s" : ""), (p3.X + p4.X)/2 -  5,  p0.Y +  3,         fs, isRel ? color6 : color3, TA_CENTER);
            }


            void DrawEnvelope(List<MySprite> sprites, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color col, bool isAtt, bool isDec, bool isSus, bool isRel, float d)
            {
                var wa = isAtt ? 6 : 1;
                var wd = isDec ? 6 : 1;
                var ws = isSus ? 6 : 1;
                var wr = isRel ? 6 : 1;


                // attack
                DrawLine(sprites, p0, p1, col, wa);

                // decay
                var pPrev = Vector2.Zero;
            
                for (float f = 0; f <= 1; f += 0.01f)
                {
                    var p = new Vector2(
                        p1.X + (p2.X - p1.X) * f,
                        p1.Y + (p2.Y - p1.Y) * (1 - (float)Math.Pow(1-f, 2)));

                    if (f > 0)
                        DrawLine(sprites, pPrev, p, col, wd);

                    pPrev = p;    
                }

                if (isDec && d < 0.01)
                    FillRect(sprites, p1.X - 4, p1.Y - 4, 8, 8, color6);

                // sustain
                DrawLine(sprites, p2, p3, col, ws);

                // release
                for (float f = 0; f <= 1; f += 0.01f)
                {
                    var p = new Vector2(
                        p3.X + (p4.X - p3.X) * f,
                        p3.Y + (p4.Y - p3.Y) * (1 - (float)Math.Pow(1-f, 2)));

                    if (f > 0)
                        DrawLine(sprites, pPrev, p, col, wr);

                    pPrev = p;    
                }
            }


            void GetEnvelopeCoords(float x, float y, float w, float h, float vol, bool current, out Vector2 p0, out Vector2 p1, out Vector2 p2, out Vector2 p3, out Vector2 p4)
            {
                var a = current ? Attack .CurValue : Attack .Value;
                var d = current ? Decay  .CurValue : Decay  .Value;
                var s = current ? Sustain.CurValue : Sustain.Value;
                var r = current ? Release.CurValue : Release.Value;

                var scale = 1f;
                var fps   = FPS * scale;

                p0   = new Vector2(x, y + h);
                p0.X = Math.Min(p0.X, x + w - r*fps);

                p1   = new Vector2(p0.X + a*fps, p0.Y - h*vol);
                p1.X = Math.Min(p1.X, x + w - r*fps);

                p2   = new Vector2(p1.X + d*fps, p0.Y - h*vol * s);
                p2.X = Math.Min(p2.X, x + w);

                p3   = new Vector2(x + w - r*fps, p2.Y);
                p4   = new Vector2(x + w, p0.Y);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                DrawFuncButton(sprites, "A", 1, w, y, True, Attack .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "D", 2, w, y, True, Decay  .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "S", 3, w, y, True, Sustain.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "R", 4, w, y, True, Release.HasDeepParams(chan, -1));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                    case 1: AddNextSetting(strAtt); break;
                    case 2: AddNextSetting(strDec); break;
                    case 3: AddNextSetting(strSus); break;
                    case 4: AddNextSetting(strRel); break;
                }
            }


            public override bool CanDelete()
            {
                return True;
            }
        }
    }
}
