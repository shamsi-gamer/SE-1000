using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


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
                Attack      = (Parameter)NewSettingFromTag("Att", this);
                Decay       = (Parameter)NewSettingFromTag("Dec", this);
                Sustain     = (Parameter)NewSettingFromTag("Sus", this);
                Release     = (Parameter)NewSettingFromTag("Rel", this);

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


            public float GetValue(long gTime, long lTime, long sTime, int noteLen, Note note, int src, List<TriggerValue> triggerValues, Program prog)
            {
                if (prog.TooComplex) return 0;


                var trigAtt = triggerValues.Find(v => v.Path == Attack .GetPath(src));
                var trigDec = triggerValues.Find(v => v.Path == Decay  .GetPath(src));
                var trigRel = triggerValues.Find(v => v.Path == Release.GetPath(src));

                if (trigAtt == null)
                {
                    trigAtt = new TriggerValue(
                        Attack.GetPath(src),
                        Attack.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues, prog));

                    triggerValues.Add(trigAtt);
                }

                if (trigDec == null)
                {
                    trigDec = new TriggerValue(
                        Decay.GetPath(src),
                        Decay.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues, prog));

                    triggerValues.Add(trigDec);
                }

                if (trigRel == null)
                {
                    trigRel = new TriggerValue(
                        Release.GetPath(src),
                        Release.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues, prog));

                    triggerValues.Add(trigRel);
                }

                float a = trigAtt.Value,
                      d = trigDec.Value,
                      s = Sustain.GetValue(gTime, lTime, sTime, noteLen, note, src, triggerValues, prog),
                      r = trigRel.Value;
                
                if (note != null)
                    noteLen = note.FrameLength;

                return GetValue(lTime, noteLen, a, d, s, r);
            }


            public static float GetValue(long lTime, int noteLen, float a, float d, float s, float r)
            {
                var lt = lTime  /(float)FPS;
                var nl = noteLen/(float)FPS;

                if (lt >= nl + r)
                { 
                    return 0;
                }
                else if (lt >= nl) // release
                {
                         if (a   >= nl) s  = nl/a;
                    else if (a+d >= nl) s += (1 - (nl-a)/d) * (1-s);

                    return s * (1 - MinMax(0, (float)Math.Pow((lt-nl)/r, 2), 1));
                }
                else if (lt >= a + d) // sustain
                {
                    return s;
                }
                else if (lt >= a) // decay
                {
                    return s + (1 - (float)Math.Pow((lt-a)/d, 2)) * (1-s);
                }
                else if (lt >= 0) // attack 
                {
                    return lt/a;
                }

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
                    case "Att": return Attack  ?? (Attack  = (Parameter)NewSettingFromTag("Att", this));
                    case "Dec": return Decay   ?? (Decay   = (Parameter)NewSettingFromTag("Dec", this));
                    case "Sus": return Sustain ?? (Sustain = (Parameter)NewSettingFromTag("Sus", this));
                    case "Rel": return Release ?? (Release = (Parameter)NewSettingFromTag("Rel", this));
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


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                DrawFuncButton(sprites, "A", 1, w, h, true, Attack .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "D", 2, w, h, true, Decay  .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "S", 3, w, h, true, Sustain.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "R", 4, w, h, true, Release.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "X", 5, w, h, false, false, mainPressed.Contains(5));
            }


            public override void Func(int func, Program prog)
            {
                switch (func)
                {
                    case 1: prog.AddNextSetting("Att"); break;
                    case 2: prog.AddNextSetting("Dec"); break;
                    case 3: prog.AddNextSetting("Sus"); break;
                    case 4: prog.AddNextSetting("Rel"); break;
                    case 5: prog.RemoveSetting(this);                               break;
                }
            }
        }
    }
}
