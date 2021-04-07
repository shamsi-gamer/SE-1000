using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        public class Arpeggio : Setting
        {
            public Song      Song;

            public Parameter Length,
                             Scale;


            public Arpeggio(Instrument inst) : base("Arp", null)
            {
                Song = new Song("");
                Song.Arpeggio = this;
                Song.Patterns.Add(new Pattern(Song));
                
                SetInstrument(inst);

                Length = (Parameter)NewFromTag("Len", this);
                Scale  = (Parameter)NewFromTag("Scl", this);
            }


            public Arpeggio(Arpeggio arp) : base(arp.Tag, null, arp.Prototype)
            {
                Song   = new Song(arp.Song);

                Length = new Parameter(arp.Length, this);
                Scale  = new Parameter(arp.Scale,  this);

                //if (arp.Song != null)
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


            public override void Remove(Setting setting)
            {
                if (setting == Scale) Scale  = null;
            }


            public void SetInstrument(Instrument inst)
            {
                foreach (var pat in Song.Patterns)
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
                    case "Len": return Length ?? (Length = (Parameter)NewFromTag("Len", this));
                    case "Scl": return Scale  ?? (Scale  = (Parameter)NewFromTag("Scl", this));
                }

                return null;
            }


            public void Delete(Song song, int iSrc)
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

                arp.Length = Parameter.Load(data, ref i, inst, iSrc, arp);
                arp.Scale  = Parameter.Load(data, ref i, inst, iSrc, arp);

                return arp;
            }


            public override void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams _dp)
            {
                x += _dp.OffX;
                y += _dp.OffY;

                var dp = new DrawParams(_dp);

                base.DrawLabels(sprites, x, y, dp);

                if (Length.HasDeepParams(CurrentChannel, CurSrc)) Length.DrawLabels(sprites, x, y, dp);
                if (Scale .HasDeepParams(CurrentChannel, CurSrc)) Scale .DrawLabels(sprites, x, y, dp);

                _dp.Next(dp);
            }


            public override void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                DrawFuncButton(sprites, "Len", 1, w, h, true, Length.HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "Scl", 2, w, h, true, Scale .HasDeepParams(chan, -1));
                DrawFuncButton(sprites, "X",   5, w, h, false, false, mainPressed.Contains(5));                
            }


            public override void Func(int func)
            {
                switch (func)
                { 
                case 1:
                    Song.EditPos = -1;
                    UpdateEditLight(lblEdit, false);

                    AddNextSetting("Len");
                    break;

                case 2:
                    Song.EditPos = -1;
                    UpdateEditLight(lblEdit, false);

                    AddNextSetting("Scl");
                    break;

                case 5: RemoveSetting(this); break;
                }
            }
        }
    }
}
