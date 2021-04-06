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
                             TrigRelease;


            public Envelope(Setting parent) : base("Env", parent)
            {
                Attack      = (Parameter)NewFromTag("Att", this);
                Decay       = (Parameter)NewFromTag("Dec", this);
                Sustain     = (Parameter)NewFromTag("Sus", this);
                Release     = (Parameter)NewFromTag("Rel", this);

                TrigAttack  = 
                TrigDecay   = 
                TrigRelease = fN;
            }


            public Envelope(Envelope env, Setting parent) : base(env.Tag, parent, env)
            {
                Attack      = new Parameter(env.Attack,  this);
                Decay       = new Parameter(env.Decay,   this);
                Sustain     = new Parameter(env.Sustain, this);
                Release     = new Parameter(env.Release, this);

                TrigAttack  = env.TrigAttack;
                TrigDecay   = env.TrigDecay;
                TrigRelease = env.TrigRelease;
            }


            public Envelope Copy(Setting parent)
            {
                return new Envelope(this, parent);
            }


            public float GetValue(TimeParams tp)
            {
                if (tp.Program.TooComplex) return 0;


                var trigAtt = tp.TriggerValues.Find(v => v.Path == Attack .GetPath(tp.SourceIndex));
                var trigDec = tp.TriggerValues.Find(v => v.Path == Decay  .GetPath(tp.SourceIndex));
                var trigRel = tp.TriggerValues.Find(v => v.Path == Release.GetPath(tp.SourceIndex));

                if (trigAtt == null)
                {
                    trigAtt = new TriggerValue(
                        Attack.GetPath(tp.SourceIndex),
                        Attack.GetValue(tp));

                    tp.TriggerValues.Add(trigAtt);
                }

                if (trigDec == null)
                {
                    trigDec = new TriggerValue(
                        Decay.GetPath(tp.SourceIndex),
                        Decay.GetValue(tp));

                    tp.TriggerValues.Add(trigDec);
                }

                if (trigRel == null)
                {
                    trigRel = new TriggerValue(
                        Release.GetPath(tp.SourceIndex),
                        Release.GetValue(tp));

                    tp.TriggerValues.Add(trigRel);
                }

                float a = trigAtt.Value,
                      d = trigDec.Value,
                      s = Sustain.GetValue(tp),
                      r = trigRel.Value;
                
                if (tp.Note != null)
                    tp.NoteLength = tp.Note.FrameLength;

                return GetValue(tp.LocalTime, tp.NoteLength, a, d, s, r);
            }


            public static float GetValue(long lTime, int noteLen, float a, float d, float s, float r)
            {
                var lt = lTime  /(float)FPS;
                var nl = noteLen/(float)FPS;

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


            public override void Remove(Setting setting)
            {
                     if (setting == Attack)  Attack  = null;
                else if (setting == Decay)   Decay   = null;
                else if (setting == Sustain) Sustain = null;
                else if (setting == Release) Release = null;
            }


            public override void Clear()
            {
                Attack .Clear();
                Decay  .Clear();
                Sustain.Clear();
                Release.Clear();
            }


            public override void Randomize(Program prog)
            {
                Attack .Randomize(prog);
                Decay  .Randomize(prog);
                Release.Randomize(prog);
                Sustain.Randomize(prog);
            }


            public override void AdjustFromController(Song song, Program prog)
            {
                if (g_remote.MoveIndicator    .Z != 0) prog.AdjustFromController(song, Attack,  -g_remote.MoveIndicator    .Z/ControlSensitivity);
                if (g_remote.MoveIndicator    .X != 0) prog.AdjustFromController(song, Decay,    g_remote.MoveIndicator    .X/ControlSensitivity);

                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(song, Sustain, -g_remote.RotationIndicator.X/ControlSensitivity);
                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(song, Release,  g_remote.RotationIndicator.Y/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case "Att": return Attack  ?? (Attack  = (Parameter)NewFromTag("Att", this));
                    case "Dec": return Decay   ?? (Decay   = (Parameter)NewFromTag("Dec", this));
                    case "Sus": return Sustain ?? (Sustain = (Parameter)NewFromTag("Sus", this));
                    case "Rel": return Release ?? (Release = (Parameter)NewFromTag("Rel", this));
                }

                return null;
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

                var env = new Envelope(parent);

                env.Attack  = Parameter.Load(data, ref i, inst, iSrc, env);
                env.Decay   = Parameter.Load(data, ref i, inst, iSrc, env);
                env.Sustain = Parameter.Load(data, ref i, inst, iSrc, env);
                env.Release = Parameter.Load(data, ref i, inst, iSrc, env);

                return env;
            }


            public override void GetLabel(out string str, out float width)
            {
                width = 186;

                str =
                      printValue(Attack .CurValue, 2, true, 0).PadLeft(4) + "  "
                    + printValue(Decay  .CurValue, 2, true, 0).PadLeft(4) + "  "
                    + printValue(Sustain.CurValue, 2, true, 0).PadLeft(4) + "  "
                    + printValue(Release.CurValue, 2, true, 0).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Attack .HasDeepParams(CurrentChannel, CurSrc)) Attack .DrawLabels(sprites, x, y, dp);
                if (Decay  .HasDeepParams(CurrentChannel, CurSrc)) Decay  .DrawLabels(sprites, x, y, dp);
                if (Sustain.HasDeepParams(CurrentChannel, CurSrc)) Sustain.DrawLabels(sprites, x, y, dp);
                if (Release.HasDeepParams(CurrentChannel, CurSrc)) Release.DrawLabels(sprites, x, y, dp);

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var sTime = 
                    g_song.StartTime > -1
                    ? g_time - g_song.StartTime
                    : 0;

                var tp  = new TimeParams(g_time, 0, sTime, null, EditLength, -1, _triggerDummy, dp.Program);

                var a = Attack .GetValue(tp);
                var d = Decay  .GetValue(tp);
                var s = Sustain.GetValue(tp);
                var r = Release.GetValue(tp);
                                                                       

                var w0 = 240f;
                var h0 = 120f;


                var v     = Math.Min(dp.Volume, 1);

                var fs    = 0.5f;
                var scale = 1f;
                var fps   = FPS * scale;

                var x0    = x + w/2 - w0/2;

                var p0    = new Vector2(x0, y + h/2 + h0/2);
                    p0.X  = Math.Min(p0.X, x0 + w0);

                var p1    = new Vector2(p0.X + a*fps, p0.Y - h0*v);
                    p1.X  = Math.Min(p1.X, x0 + w0);

                var p2    = new Vector2(p1.X + d*fps, p0.Y - h0*v * s);
                    p2.X  = Math.Min(p2.X, x0 + w0);

                var p3    = new Vector2(x0 + w0 - r*fps, p2.Y);
                var p4    = new Vector2(x0 + w0, p0.Y);


                var isAtt = IsCurParam("Att");
                var isDec = IsCurParam("Dec");
                var isSus = IsCurParam("Sus");
                var isRel = IsCurParam("Rel");


                var wa = isAtt ? 6 : 1;
                var wd = isDec ? 6 : 1;
                var ws = isSus ? 6 : 1;
                var wr = isRel ? 6 : 1;


                // draw envelope supports and info

                var sw = 1;

                DrawLine(sprites, p0.X, p0.Y, p0.X, y + h0, color3, sw);
                DrawLine(sprites, p2.X, p2.Y, p2.X, p0.Y,   color3, sw);
                DrawLine(sprites, p1.X, p1.Y, p1.X, y + h0, color3, sw);
                DrawLine(sprites, p3.X, p3.Y, p3.X, p0.Y,   color3, sw);
                DrawLine(sprites, p1.X, p2.Y, p3.X, p3.Y,   color3, sw);
                                                              
                DrawLine(sprites, p0.X, p0.Y, p4.X, p4.Y,   color3, sw);


                // draw labels

                DrawString(sprites, S_00(a) + (isAtt ? " s" : ""),  p0.X           +  6,  p0.Y +  3,         fs, isAtt ? color6 : color3, TaC);
                DrawString(sprites, S_00(d) + (isDec ? " s" : ""), (p1.X + p2.X)/2 + 16, (p1.Y+p2.Y)/2 - 20, fs, isDec ? color6 : color3, TaC);
                DrawString(sprites, S_00(s),                       (p2.X + p3.X)/2 -  5,  p2.Y - 20,         fs, isSus ? color6 : color3, TaC);
                DrawString(sprites, S_00(r) + (isRel ? " s" : ""), (p3.X + p4.X)/2 -  5,  p0.Y +  3,         fs, isRel ? color6 : color3, TaC);


                // draw the envelope


                // attack
                DrawLine(sprites, p0, p1, color6, wa);

                // decay
                var pPrev = Vector2.Zero;
            
                for (float f = 0; f <= 1; f += 0.01f)
                {
                    var p = new Vector2(
                        p1.X + (p2.X - p1.X) * f,
                        p1.Y + (p2.Y - p1.Y) * (1 - (float)Math.Pow(1-f, 2)));

                    if (f > 0)
                        DrawLine(sprites, pPrev, p, color6, wd);

                    pPrev = p;    
                }

                // sustain
                DrawLine(sprites, p2, p3, color6, ws);

                // release
                for (float f = 0; f <= 1; f += 0.01f)
                {
                    var p = new Vector2(
                        p3.X + (p4.X - p3.X) * f,
                        p3.Y + (p4.Y - p3.Y) * (1 - (float)Math.Pow(1-f, 2)));

                    if (f > 0)
                        DrawLine(sprites, pPrev, p, color6, wr);

                    pPrev = p;    
                }


                if (isDec && d < 0.01)
                    FillRect(sprites, p1.X-4, p1.Y-4, 8, 8, color6);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                DrawFuncButton(sprites, "A", 1, w, y, true, Attack .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "D", 2, w, y, true, Decay  .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "S", 3, w, y, true, Sustain.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "R", 4, w, y, true, Release.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "X", 5, w, y, false, false, mainPressed.Contains(5));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                    case 1: AddNextSetting("Att"); break;
                    case 2: AddNextSetting("Dec"); break;
                    case 3: AddNextSetting("Sus"); break;
                    case 4: AddNextSetting("Rel"); break;
                    case 5: RemoveSetting(this);   break;
                }
            }
        }
    }
}
