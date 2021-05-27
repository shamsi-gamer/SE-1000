using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static Modulate ModDestConnecting =  null;
        static int      ModDestSrcIndex   = -1,
                        ModCurChan        = -1,
                        ModSelChan        = -1,
                        ModCurPat         = -1;
        static Clip     ModDestClip       =  null;
        static Channel  ModDestChannel    =  null;


        public enum ModOp { Multiply, Add };


        public class Modulate : Setting
        {
            public ModOp            Op;
                                    
            public Parameter        Amount,
                                    Attack,
                                    Release;
                                    
            public float            Phase,
                                    Delta,
                                    CurValue;

            public List<Setting>    SrcSettings; // this is the reference count for all three
            public List<Source>     SrcSources;
            public List<Instrument> SrcInstruments;


            public Modulate(Setting parent, Instrument inst, Source src) 
                : base(strMod, parent, null, inst, src)
            {
                Op             = ModOp.Add;
                               
                Amount         = (Parameter)NewSettingFromTag(strAmt, this, inst, src);
                Attack         = (Parameter)NewSettingFromTag(strAtt, this, inst, src);
                Release        = (Parameter)NewSettingFromTag(strRel, this, inst, src);

                g_mod.Add(this);

                Phase          = 0;
                Delta          = 1f/FPS;
                CurValue       = 0;

                SrcSettings    = new List<Setting>();            
                SrcSources     = new List<Source>();
                SrcInstruments = new List<Instrument>();
            }


            public Modulate(Modulate mod, Setting parent, Instrument inst, Source src) 
                : base(mod.Tag, parent, mod.Prototype, inst, src)
            {
                Op       = mod.Op;
                        
                Amount   = new Parameter(mod.Amount,  this);
                Attack   = new Parameter(mod.Attack,  this);
                Release  = new Parameter(mod.Release, this);

                g_mod.Add(this);

                Phase    = mod.Phase;
                Delta    = mod.Delta;
                CurValue = mod.CurValue;

                SrcSettings = new List<Setting>();
                foreach (var set in mod.SrcSettings)
                    SrcSettings.Add(set);

                SrcSources = new List<Source>();
                foreach (var _src in mod.SrcSources)
                    SrcSources.Add(_src);

                SrcInstruments = new List<Instrument>();
                foreach (var _inst in mod.SrcInstruments)
                    SrcInstruments.Add(_inst);
            }


            public Modulate Copy(Setting parent) 
            {
                return new Modulate(this, parent, Instrument, Source);
            }


            public void AdvanceTime()
            {
                Phase += Delta;
            }


            public float UpdateValue(TimeParams tp)
            {
                if ( /*m_valid
                    ||*/tp.Program.TooComplex) 
                    return CurValue;

                var amt = Amount .UpdateValue(tp);
                var att = Attack .UpdateValue(tp);
                var rel = Release.UpdateValue(tp);

                var val = 0f;

                for (int i = 0; i < SrcSettings.Count; i++)
                {
                    var set  = SrcSettings   [i];
                    var src  = SrcSources    [i];
                    var inst = SrcInstruments[i];

                    if (OK(set))
                    {
                             if (set.GetType() == typeof(Parameter)) val = Math.Max(val, ((Parameter)set).CurValue);
                        else if (set.GetType() == typeof(LFO      )) val = Math.Max(val, ((LFO      )set).CurValue);
                        else if (set.GetType() == typeof(Envelope )) val = Math.Max(val, ((Envelope )set).CurValue);
                        else if (set.GetType() == typeof(Modulate )) val = Math.Max(val, ((Modulate )set).CurValue);
                        // TODO add more that have CurValue
                    }

                    else if (OK(src)) 
                        val = Math.Max(val, src.CurVolume);
                    
                    else
                        val = Math.Max(val, inst.CurVolume);
                }

                var  cv  = Math.Abs(CurValue);
                var _amt = Math.Abs(amt);

                var a = Math.Min(   cv + val*_amt/FPS/(att>0?att:0.000001f), _amt);
                var d = Math.Max(0, cv -     _amt/FPS/(rel>0?rel:0.000001f));

                CurValue = Math.Sign(amt) * (d + (a - d) * val);
                m_valid = T;

                return CurValue;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return
                       Op != ModOp.Add
                    || Amount .HasDeepParams(chan, src)
                    || Attack .HasDeepParams(chan, src)
                    || Release.HasDeepParams(chan, src);
            }


            public override void Clear()
            {
                Amount .Clear();
                Attack .Clear();
                Release.Clear();
            }


            public override void Reset()
            {
                base.Reset();

                Amount .Reset();
                Attack .Reset();
                Release.Reset();
            }


            public override void Randomize(Program prog)
            {
                Amount .Randomize(prog);
                Attack .Randomize(prog);
                Release.Randomize(prog);
            }


            public override void AdjustFromController(Clip clip, Program prog)
            {
                if (g_remote.MoveIndicator    .Z != 0) prog.AdjustFromController(clip, Amount,  -g_remote.MoveIndicator    .Z/ControlSensitivity);

                if (g_remote.RotationIndicator.X != 0) prog.AdjustFromController(clip, Attack,  -g_remote.RotationIndicator.X/ControlSensitivity);
                if (g_remote.RotationIndicator.Y != 0) prog.AdjustFromController(clip, Release,  g_remote.RotationIndicator.Y/ControlSensitivity);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strAmt: return GetOrAddParamFromTag(Amount,  tag);
                    case strAtt: return GetOrAddParamFromTag(Attack,  tag);
                    case strRel: return GetOrAddParamFromTag(Release, tag);
                }

                return null;
            }


            public void Delete(Clip clip, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Amount .Delete(clip, iSrc);
                Attack .Delete(clip, iSrc);
                Release.Delete(clip, iSrc);
            }


            public override string Save()
            {
                var save = 
                      W (Tag)
                    +  S(SrcSettings.Count);

                for (int i = 0; i < SrcSettings.Count; i++)
                { 
                    var set  = SrcSettings   [i];
                    var src  = SrcSources    [i];
                    var inst = SrcInstruments[i];

                    save += 
                          P (OK(set)  ? set .GetPath(src.Index) : "")
                        + PS(OK(src)  ? src .Index              : -1)
                        + P (OK(inst) ? inst.Name               : "");
                }

                save +=
                      P(Amount .Save())
                    + P(Attack .Save())
                    + P(Release.Save());

                return save;
            }


            public static Modulate Load(string[] data, ref int d, Instrument inst, int iSrc, Setting parent)
            {
                var tag = data[d++];

                var mod = new Modulate(
                    parent, 
                    inst, 
                    iSrc > -1 ? inst.Sources[iSrc] : null);


                var nSources = int.Parse(data[d++]);

                for (int i = 0; i < nSources; i++)
                {
                    var setPath     = data[d++];
                    var modSrcIndex = int.Parse(data[d++]);
                    var modInst     = data[d++];

                    var _inst = Instruments.Find(_i => _i.Name == modInst);

                    mod.SrcInstruments.Add(_inst);
                    mod.SrcSources    .Add(modSrcIndex > -1 ? _inst.Sources[modSrcIndex] : null);
                    mod.SrcSettings   .Add(GetSettingFromPath(_inst, setPath));
                }

                
                mod.Amount  = Parameter.Load(data, ref d, inst, iSrc, mod, mod.Amount );
                mod.Attack  = Parameter.Load(data, ref d, inst, iSrc, mod, mod.Attack );
                mod.Release = Parameter.Load(data, ref d, inst, iSrc, mod, mod.Release);

                return mod;
            }


            public override string GetLabel(out float width)
            {
                width = 163;

                return
                      (Op == ModOp.Add ? "+ " : "* ")
                    + PrintValue(Amount .Value, 2, T, 0).PadLeft(5) + " "
                    + PrintValue(Attack .Value, 2, T, 0).PadLeft(4) + " "
                    + PrintValue(Release.Value, 2, T, 0).PadLeft(4);
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Amount .HasDeepParams(CurChannel, CurSrc)) Amount .DrawLabels(sprites, x, y, dp);
                if (Attack .HasDeepParams(CurChannel, CurSrc)) Attack .DrawLabels(sprites, x, y, dp);
                if (Release.HasDeepParams(CurChannel, CurSrc)) Release.DrawLabels(sprites, x, y, dp);

                _dp.Next(dp);
            }


            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var sTime = 
                    IsPlaying
                    ? g_time - EditedClip.Track.StartTime
                    : 0;

                var tp = new TimeParams(g_time, 0, sTime, null, EditedClip.EditLength, -1, _triggerDummy, dp.Program);

                Amount .UpdateValue(tp);
                Attack .UpdateValue(tp);
                Release.UpdateValue(tp);


                var w0    = 240f;
                var h0    = 120f;

                var isAmt = IsCurParam(strAmt);
                var isAtt = IsCurParam(strAtt);
                var isRel = IsCurParam(strRel);

                var x0 = x + w/2 - w0/2;
                var y0 = y + h/2 - h0/4;

                Vector2 p0, p1, p2;

                GetEnvelopeCoords(x0, y0, w0, h0, F, out p0, out p1, out p2);
                DrawEnvelopeSupportsAndInfo(sprites, p0, p1, p2, w0, y0, h0, isAmt, isAtt, isRel);

                GetEnvelopeCoords(x0, y0, w0, h0, T, out p0, out p1, out p2);
                DrawEnvelope(sprites, p0, p1, p2, color3, F, F, F);

                GetEnvelopeCoords(x0, y0, w0, h0, F, out p0, out p1, out p2);
                DrawEnvelope(sprites, p0, p1, p2, color5, isAmt, isAtt, isRel);


                var strFrom = "from\n";

                if (SrcSettings.Count == 0)
                    strFrom += "...";

                else
                {
                    for (int i = 0; i < SrcSettings.Count; i++)
                    {
                        var set  = SrcSettings   [i];
                        var src  = SrcSources    [i];
                        var inst = SrcInstruments[i];
                        
                        strFrom += "\n" + inst.Name;

                             if (OK(set)) strFrom += "/" + set.GetPath(OK(src) ? src.Index : -1);
                        else if (OK(src)) strFrom += "/" + src.Index;
                    }
                }

                DrawString(sprites, strFrom, x0 + w0/2, y + h/2 - h0/2 - 80, 0.5f, color5, TaC);
            }


            void DrawEnvelopeSupportsAndInfo(List<MySprite> sprites, Vector2 p0, Vector2 p1, Vector2 p2, float w, float y, float h, bool isAmt, bool isAtt, bool isRel)
            {
                var sw = 1;

                DrawLine(sprites, p0.X, y,       p0.X,     y + h,   color3, sw);
                DrawLine(sprites, p0.X, y + h/2, p0.X + w, y + h/2, color3, sw);
                                                              

                // labels

                var amt = Amount .Value;
                var a   = Attack .Value;
                var r   = Release.Value;

                var fs = 0.5f;

                DrawString(sprites, S_00(amt) + (isAmt ? " s" : ""),                      p1.X + 18,           p1.Y + (amt>=0?-20:4), fs, isAmt ? color6 : color3, TaC);
                DrawString(sprites, S_00(a)   + (isAtt ? " s" : ""),                     (p0.X + p1.X)/2 + 6,  p0.Y +  3,             fs, isAtt ? color6 : color3, TaC);
                DrawString(sprites, S_00(r)   + (isRel ? " s" : ""), Math.Max(p0.X + 90, (p1.X + p2.X)/2 - 5), p0.Y +  3,             fs, isRel ? color6 : color3, TaC);
            }


            void DrawEnvelope(List<MySprite> sprites, Vector2 p0, Vector2 p1, Vector2 p2, Color col, bool isAmt, bool isAtt, bool isRel)
            {
                var wa = isAtt || isAmt ? 6 : 1;
                var wr = isRel || isAmt ? 6 : 1;


                // attack
                DrawLine(sprites, p0, p1, col, wa);


                // release
                var pPrev = Vector2.Zero;

                for (float f = 0; f <= 1; f += 0.01f)
                {
                    var p = new Vector2(
                        p1.X + (p2.X - p1.X) * f,
                        p1.Y + (p2.Y - p1.Y) * (1 - (float)Math.Pow(1-f, 2)));

                    if (f > 0)
                        DrawLine(sprites, pPrev, p, col, wr);

                    pPrev = p;    
                }
            }


            void GetEnvelopeCoords(float x, float y, float w, float h, bool current, out Vector2 p0, out Vector2 p1, out Vector2 p2)
            {
                var amt = current ? Amount .CurValue : Amount .Value;
                var a   = current ? Attack .CurValue : Attack .Value;
                var r   = current ? Release.CurValue : Release.Value;

                var scale = 1f;
                var fps   = FPS * scale;

                p0   = new Vector2(x, y + h/2);
                p0.X = Math.Min(p0.X, x + w);

                p1   = new Vector2(p0.X + a*fps, y + h/2 - h/2*amt);
                p1.X = Math.Min(p1.X, x + w);

                p2   = new Vector2(p1.X + r*fps, y + h/2);
                p2.X = Math.Min(p2.X, x + w);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float y, Channel chan)
            {
                DrawFuncButton(sprites, (Op == ModOp.Add ? "Add " : "Mult") + "↕", 0, w, y, F, F);
                DrawFuncButton(sprites, strAmt, 1, w, y, T, Amount .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "A",    2, w, y, T, Attack .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "R",    3, w, y, T, Release.HasDeepParams(chan, -1));
            }


            public override void Func(int func)
            {
                switch (func)
                {
                    case 0:
                    {
                        var newOp = (int)Op + 1;
                        if (newOp > (int)ModOp.Add) newOp = 0;
                        Op = (ModOp)newOp;
                        g_lcdPressed.Add(lcdMain+func);
                        break;
                    }
                    case 1: AddNextSetting(strAmt); break;
                    case 2: AddNextSetting(strAtt); break;
                    case 3: AddNextSetting(strRel); break;
                }
            }


            public override bool CanDelete()
            {
                return T;
            }
        }



        void ResetModConnecting()
        {
            ModDestConnecting = null;
            ModDestSrcIndex   = -1;
            ModCurChan        = -1;
            ModSelChan        = -1;
            ModCurPat         = -1;
            ModDestClip       = null;
            ModDestChannel    = null;
        }
    }
}