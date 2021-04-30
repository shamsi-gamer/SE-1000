using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        public class Arpeggio : Setting
        {
            public Clip      Clip;

            public Parameter Length,
                             Scale;


            public Arpeggio(Instrument inst) 
                : base(strArp, null, null, inst, null)
            {
                Clip = new Clip(null, "");
                Clip.Arpeggio = this;
                Clip.Patterns.Add(new Pattern(Clip));
                
                SetInstrument(inst);

                Length = (Parameter)NewSettingFromTag(strLen, this, inst, null);
                Scale  = (Parameter)NewSettingFromTag(strScl, this, inst, null);
            }


            public Arpeggio(Arpeggio arp) 
                : base(arp.Tag, null, arp.Prototype, arp.Instrument, arp.Source)
            {
                Clip   = new Clip(arp.Clip);

                Length = new Parameter(arp.Length, this);
                Scale  = new Parameter(arp.Scale,  this);

                //if (arp.Clip != null)
                //{ 
                //    Notes = new List<Note>();

                //    foreach (var note in arp.Notes)
                //        Notes.Add(note);

                //    if (arp.Length > 0)
                //        Length = arp.Length;

                //    if (arp.Scale != null)
                //    {
                //        Scale = new Parameter(arp.Scale);
                //        Scale.Parent = this;
                //    }
                //    else
                //        Scale = null;
                //}
                //else
                //{
                //    Notes  =  null;
                //    Length = -Math.Abs(Length); // turn off but keep current value
                //    Scale  =  null;
                //}
            }


            public Arpeggio Copy()
            {
                return new Arpeggio(this);
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return Scale != null;
            }


            public void SetInstrument(Instrument inst)
            {
                foreach (var pat in Clip.Patterns)
                    pat.Channels[0].Instrument = inst;
            }


            public override void Randomize(Program prog)
            {
                if (prog.TooComplex) return;

                Length.Randomize(prog);
                Scale .Randomize(prog);
            }


            public override Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strLen: return GetOrAddParamFromTag(Length, tag);
                    case strScl: return GetOrAddParamFromTag(Scale,  tag);
                }

                return null;
            }


            public void Delete(Clip song, int iSrc)
            {
                // this method removes note and channel automation associated with this setting

                Length.Delete(song, iSrc);
                Scale .Delete(song, iSrc);
            }


            public override string Save()
            {
                var arp = W(Tag);

                // save song here

                arp += W(Length.Save());
                arp +=   Scale .Save();

                return arp;
            }


            public static Arpeggio Load(string[] data, ref int i, Instrument inst, int iSrc)
            {
                var tag = data[i++];
 
                var arp = new Arpeggio(inst);

                arp.Length = Parameter.Load(data, ref i, inst, iSrc, arp, arp.Length);
                arp.Scale  = Parameter.Load(data, ref i, inst, iSrc, arp, arp.Scale );

                return arp;
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Length.HasDeepParams(g_session.CurClip.CurrentChannel, g_session.CurClip.CurSrc)) Length.DrawLabels(sprites, x, y, dp);
                if (Scale .HasDeepParams(g_session.CurClip.CurrentChannel, g_session.CurClip.CurSrc)) Scale .DrawLabels(sprites, x, y, dp);

                _dp.Next(dp);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                DrawFuncButton(sprites, strLen, 1, w, h, true, Length.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, strScl, 2, w, h, true, Scale .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "X",   5, w, h, false, false, g_mainPressed.Contains(5));                
            }


            public override void Func(int func)
            {
                switch (func)
                { 
                case 1:
                    Clip.EditPos = -1;
                    //UpdateEditLabel(lblEdit, false);

                    AddNextSetting(strLen);
                    break;

                case 2:
                    Clip.EditPos = -1;
                    //UpdateEditLabel(lblEdit, false);

                    AddNextSetting(strScl);
                    break;
                }
            }


            public override bool CanDelete()
            {
                return true;
            }
        }
    }
}
