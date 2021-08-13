using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        static Modulate ModDestConnecting =  Modulate_null;
        static int      ModDestSrcIndex   = -1,
                        ModCurChan        = -1,
                        ModSelChan        = -1,
                        ModCurPat         = -1;
        static Clip     ModDestClip       =  Clip_null;
        static Channel  ModDestChannel    =  Channel_null;


        public enum ModOp { Set, Add, Multiply };


        public class Modulate : Setting
        {
            public ModOp            Op;
                                    
            public Parameter        Amount,
                                    Attack,
                                    Release;
                                    
            public float            Phase,
                                    Delta,
                                    CurValue;

            public List<Setting>    ModSettings;    // this is the reference count for all three
            public List<Source>     ModSources;     
            public List<Instrument> ModInstruments; 


            public List<string>     LoadSetPath,
                                    LoadInstName;
            public List<int>        LoadSrcIndex;



            public Modulate(Setting parent, Instrument inst, Source src) 
                : base(strMod, parent, Setting_null, inst, src)
            {
                Op             = ModOp.Add;
                               
                Amount         = (Parameter)NewSettingFromTag(strAmt, this, inst, src);
                Attack         = (Parameter)NewSettingFromTag(strAtt, this, inst, src);
                Release        = (Parameter)NewSettingFromTag(strRel, this, inst, src);

                g_mod.Add(this);

                Phase          = 0;
                Delta          = 1f/FPS;
                CurValue       = 0;

                InitLists();                               
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

                InitLists();

                foreach (var  set  in mod.ModSettings   ) ModSettings   .Add( set);
                foreach (var _src  in mod.ModSources    ) ModSources    .Add(_src);
                foreach (var _inst in mod.ModInstruments) ModInstruments.Add(_inst);
            }



            void InitLists()
            {
                ModSettings    = new List<Setting>();            
                ModSources     = new List<Source>();            
                ModInstruments = new List<Instrument>();            

                LoadSetPath    = new List<string>();            
                LoadSrcIndex   = new List<int>();            
                LoadInstName   = new List<string>();            
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
                if (tp.Program.TooComplex) 
                    return 0;

                //var param = (Parameter)Parent;


                // get connected value

                var val = 0f;

                for (int i = 0; i < ModSettings.Count; i++)
                {
                    var set  = ModSettings   [i];
                    var src  = ModSources    [i];
                    var inst = ModInstruments[i];

                    if (OK(set))
                    {
                             if (set.GetType() == typeof(Parameter)) val = Math.Max(val, ((Parameter)set).CurValue);
                        else if (set.GetType() == typeof(LFO      )) val = Math.Max(val, ((LFO      )set).CurValue);
                        else if (set.GetType() == typeof(Envelope )) val = Math.Max(val, ((Envelope )set).CurValue);
                        else if (set.GetType() == typeof(Modulate )) val = Math.Max(val, ((Modulate )set).CurValue);
                        // TODO add more that have CurValue
                    }

                    else if (OK(src))  val = Math.Max(val, src .CurVolume);
                    else if (OK(inst)) val = Math.Max(val, inst.CurVolume);
                    else               val = 0;
                }


                if (Op == ModOp.Set)
                {
                    // replace value with connected

                    CurValue = val;
                }
                else
                { 
                    // modify value with connected
                
                    var amt = Amount .UpdateValue(tp);
                    var att = Attack .UpdateValue(tp);
                    var rel = Release.UpdateValue(tp);
                
                    if (att == 0) att = 0.000001f;
                    if (rel == 0) rel = 0.000001f;

                    var  cv  = Math.Abs(CurValue);
                    var _amt = Math.Abs(amt);

                    var a = Math.Min(   cv + val*_amt/FPS/att, _amt);
                    var r = Math.Max(0, cv -     _amt/FPS/rel);

                    CurValue = Math.Sign(amt) * (r + (a - r) * val);
                }

                m_valid  = True;
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



            public override void Randomize()
            {
                Amount .Randomize();
                Attack .Randomize();
                Release.Randomize();
            }



            public override void AdjustFromController(Clip clip)
            {
                Program.AdjustFromController(clip, Amount, -g_remote.MoveIndicator    .Z*ControlSensitivity);

                Program.AdjustFromController(clip, Attack, -g_remote.RotationIndicator.X*ControlSensitivity);
                Program.AdjustFromController(clip, Release, g_remote.RotationIndicator.Y*ControlSensitivity);
            }



            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strAmt: return GetOrAddParamFromTag(Amount,  tag);
                    case strAtt: return GetOrAddParamFromTag(Attack,  tag);
                    case strRel: return GetOrAddParamFromTag(Release, tag);
                }

                return Setting_null;
            }



            public void Delete(int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Amount .Delete(iSrc);
                Attack .Delete(iSrc);
                Release.Delete(iSrc);
            }



            public override string Save()
            {
                var save = 
                      Tag
                    + PS((int)Op)
                    + PS(ModSettings.Count);

                for (int i = 0; i < ModSettings.Count; i++)
                { 
                    var set  = ModSettings   [i];
                    var src  = ModSources    [i];
                    var inst = ModInstruments[i];

                    save += 
                          P (OK(set) ? set.Path : "")
                        + P (inst.Name)
                        + PS(OK(src) ? src.Index : -1);
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
                    OK(iSrc) ? inst.Sources[iSrc] : Source_null);


                int modOp;
                if (!int_TryParse(data[d++], out modOp)) return Modulate_null;

                mod.Op = (ModOp)modOp;


                int nSources;
                if (!int_TryParse(data[d++], out nSources)) return Modulate_null;

                for (int i = 0; i < nSources; i++)
                {
                    mod.LoadSetPath .Add(data[d++]);
                    mod.LoadInstName.Add(data[d++]);
                    
                    int srcIndex;
                    if (!int_TryParse(data[d++], out srcIndex)) return Modulate_null;
                    mod.LoadSrcIndex.Add(srcIndex);
                }


                mod.Amount  = Parameter.Load(data, ref d, inst, iSrc, mod, mod.Amount );
                mod.Attack  = Parameter.Load(data, ref d, inst, iSrc, mod, mod.Attack );
                mod.Release = Parameter.Load(data, ref d, inst, iSrc, mod, mod.Release);

                return mod;
            }



            public override string GetLabel(out float width)
            {
                width = 163;

                var op = "=";

                     if (Op == ModOp.Add     ) op = "+";
                else if (Op == ModOp.Multiply) op = "*";

                return
                      op
                    + PrintValue(Amount .Value, 2, True, 0).PadLeft(5) + strEmpty
                    + PrintValue(Attack .Value, 2, True, 0).PadLeft(4) + strEmpty
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

                    if (Amount .HasDeepParams(CurChannel, CurSrc)) Amount .DrawLabels(sprites, x, y, dp);
                    if (Attack .HasDeepParams(CurChannel, CurSrc)) Attack .DrawLabels(sprites, x, y, dp);
                    if (Release.HasDeepParams(CurChannel, CurSrc)) Release.DrawLabels(sprites, x, y, dp);
                }

                _dp.Next(dp);
            }



            public override void DrawSetting(List<MySprite> sprites, float x, float y, float w, float h, DrawParams dp)
            {
                var isAmt = IsCurParam(strAmt);
                var isAtt = IsCurParam(strAtt);
                var isRel = IsCurParam(strRel);


                var w0 = 240f;
                var h0 = 120f;

                var x0 = x + w/2 - w0/2;
                var y0 = y + h/2 - h0/4;

                Vector2 p0, p1, p2;

                GetEnvelopeCoords(x0, y0, w0, h0, False, out p0, out p1, out p2);
                DrawEnvelopeSupports(sprites, p0, w0, y0, h0);


                FillRect(sprites, p0.X, y0 + h0/2, w0, -CurValue*h/4, color3);


                GetEnvelopeCoords(x0, y0, w0, h0, True, out p0, out p1, out p2);
                DrawEnvelope(sprites, p0, p1, p2, color3, False, False, False);

                GetEnvelopeCoords(x0, y0, w0, h0, False, out p0, out p1, out p2);
                DrawEnvelope(sprites, p0, p1, p2, color5, isAmt, isAtt, isRel);


                var strFrom = "from\n";

                if (ModSettings.Count == 0)
                    strFrom += "...";

                else
                {
                    for (int i = 0; i < ModSettings.Count; i++)
                    {
                        var set  = ModSettings   [i];
                        var src  = ModSources    [i];
                        var inst = ModInstruments[i];
                        
                        strFrom += "\n";

                             if (OK(set))  strFrom += set.Path;
                        else if (OK(src))  strFrom += inst.Name + "/" + src.Index;
                        else if (OK(inst)) strFrom += inst.Name;
                    }
                }

                DrawString(sprites, strFrom, x0 + w0/2, y + h/2 - h0/2 - 80, 0.5f, color5, TA_CENTER);
            }



            void DrawEnvelopeSupports(List<MySprite> sprites, Vector2 p0, float w, float y, float h)
            {
                var sw = 1;

                DrawLine(sprites, p0.X, y,       p0.X,     y + h,   color3, sw);
                DrawLine(sprites, p0.X, y + h/2, p0.X + w, y + h/2, color3, sw);
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

                                                              

                // labels

                var amt = Amount .Value;
                var a   = Attack .Value;
                var r   = Release.Value;

                var fs = 0.5f;

                DrawString(sprites, S_00(amt) + (isAmt ? " s" : ""),                      p1.X + 18,           p1.Y + (amt>=0?-20:4), fs, isAmt ? color6 : color3, TA_CENTER);
                DrawString(sprites, S_00(a)   + (isAtt ? " s" : ""),                     (p0.X + p1.X)/2 + 6,  p0.Y +  3,             fs, isAtt ? color6 : color3, TA_CENTER);
                DrawString(sprites, S_00(r)   + (isRel ? " s" : ""), Math.Max(p0.X + 90, (p1.X + p2.X)/2 - 5), p0.Y +  3,             fs, isRel ? color6 : color3, TA_CENTER);
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
                var strOp = "Set ";

                     if (Op == ModOp.Add     ) strOp = "Add ";
                else if (Op == ModOp.Multiply) strOp = "Mult";

                DrawFuncButton(sprites, strOp + "↕", 0, w, y, False, False);
                DrawFuncButton(sprites, strAmt, 1, w, y, True, Amount .HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, "A",    2, w, y, True, Attack .HasDeepParams(chan, CurSrc));
                DrawFuncButton(sprites, "R",    3, w, y, True, Release.HasDeepParams(chan, CurSrc));
            }



            public override void Func(int func)
            {
                switch (func)
                {
                    case 0:
                    {
                        var newOp = (int)Op + 1;
                        if (newOp > (int)ModOp.Multiply) newOp = 0;
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
                return True;
            }
        }



        static void ResetModConnecting()
        {
            ModDestConnecting = Modulate_null;
            ModDestSrcIndex   = -1;
            ModCurChan        = -1;
            ModSelChan        = -1;
            ModCurPat         = -1;
            ModDestClip       = Clip_null;
            ModDestChannel    = Channel_null;
        }
    }
}