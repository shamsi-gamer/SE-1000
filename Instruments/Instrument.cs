using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        public class Instrument
        {
            public string       Name;

            public Parameter    Volume;
            public Tune         Tune;
            public Parameter    Glide;
            public Filter       Filter;
            public Delay        Delay;

            public List<Source> Sources;

            public float        DisplayVolume;

            public Program      Program;



            public float CurVolume { get 
            { 
                var vol = 0f;

                foreach (var src in Sources)
                    vol = Math.Max(vol, src.CurVolume);

                return vol;
            } }



            public Instrument(Program prog)
            {
                Name          = "New Sound";
                              
                Volume        = (Parameter)NewSettingFromTag(strVol, Setting_null, this, Source_null);
                              
                Tune          = Tune_null;
                Glide         = Parameter_null;
                Filter        = Filter_null;
                Delay         = Delay_null;
                              
                Sources       = new List<Source>();

                DisplayVolume = float.NaN;

                Program       = prog;
            }



            public Instrument(string name, Program prog) : this(prog)
            {
                Name = name;
            }



            public Instrument(Instrument inst)
            {
                Name          = inst.Name;
                             
                Volume        = new Parameter(inst.Volume, Setting_null);
                             
                Tune          = inst.Tune  ?.Copy();
                Glide         = new Parameter(inst.Glide, Setting_null);
                Filter        = inst.Filter?.Copy();
                Delay         = inst.Delay ?.Copy();

                Sources = new List<Source>();
                foreach (var src in inst.Sources)
                    Sources.Add(new Source(src, this));

                DisplayVolume = inst.DisplayVolume;

                Program       = inst.Program;
            }



            public void Randomize()
            {
                Sources.Clear();

                var nSrc = Math.Max(1, (int)Math.Round(Math.Pow(RND, 1.5) * MaxSources/2));

                var used = new List<Oscillator>();

                for (int i= 0; i < nSrc; i++)
                { 
                    var src = new Source(this);
                 
                    Sources.Add(src);
                    src.Randomize(used);
                }

                //Volume.SetValue(1, Note_null);//.Randomize(Program prog);

                //if (RND > 0.9f)
                //{
                //    Tune = new Tune(this, Source_null);
                //    Tune.Randomize();
                //}
                //else
                //    Tune = Tune_null;
                
                //if (   RND > 0.9f
                //    && OK(Sources.Find(s => OK(s.Harmonics))))
                //{
                //    Filter = new Filter(this, Source_null);
                //    Filter.Randomize();
                //}
                //else
                //    Filter = Filter_null;

                //if (RND > 0.9f)
                //{
                //    Delay = new Delay(this, Source_null);
                //    Delay.Randomize();
                //}
                //else
                //    Delay = Delay_null;
            }



            public void ResetValues()
            {
                DisplayVolume = float.NaN;

                Volume .Reset();
                Tune  ?.Reset();
                Glide ?.Reset();
                Filter?.Reset();
                Delay ?.Reset();

                foreach (var src in Sources)
                    src.ResetValues();
            }



            public Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strVol:   return Volume;
                    case strTune:  return Tune   ?? (Tune   = new Tune  (this, Source_null));
                    case strGlide: return Glide  ?? (Glide  = (Parameter)NewSettingFromTag(strGlide, Setting_null, this, Source_null));
                    case strFlt:   return Filter ?? (Filter = new Filter(this, Source_null));
                    case strDel:   return Delay  ?? (Delay  = new Delay (this, Source_null));
                }

                return Setting_null;
            }



            public void Delete()
            {
                // this method removes note and channel automation associated with this instrument

                Volume .Delete(-1);
                Tune  ?.Delete(-1);
                Glide ?.Delete(-1);
                Filter?.Delete(-1);
                Delay ?.Delete(-1);

                foreach (var src in Sources)
                    src.Delete();
            }



            public string Save()
            {
                var inst = N(
                      W (Name)
                    + WS(Sources.Count)

                    + Volume.Save()

                    + SaveSetting(Tune)
                    + SaveSetting(Glide)
                    + SaveSetting(Filter)
                    + SaveSetting(Delay));

                for (int i = 0; i < Sources.Count; i++)
                    inst += N(Sources[i].Save());

                return inst;
            }



            public static Instrument Load(string[] lines, ref int line, Program prog)
            {
                var data = lines[line++].Split(';');
                var d    = 0;

                var inst = new Instrument(data[d++], prog);

                var nSources = int_Parse(data[d++]);

                inst.Volume = Parameter.Load(data, ref d, inst, -1, Setting_null);

                while (d < data.Length
                    && (   data[d] == strTune 
                        || data[d] == strFlt 
                        || data[d] == strDel))
                {
                    switch (data[d])
                    { 
                        case strTune:  inst.Tune   = Tune     .Load(data, ref d, inst, -1); break;
                        case strGlide: inst.Glide  = Parameter.Load(data, ref d, inst, -1, Setting_null); break;
                        case strFlt:   inst.Filter = Filter   .Load(data, ref d, inst, -1); break;
                        case strDel:   inst.Delay  = Delay    .Load(data, ref d, inst, -1); break;
                    }
                }

                for (int j = 0; j < nSources; j++)
                    Source.Load(lines, ref line, inst, j);

                return inst;
            }

            

            public void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams dp)
            {
                if (dp.Program.TooComplex)
                    return;

                Volume .DrawLabels(sprites, x, y, dp);
                Tune  ?.DrawLabels(sprites, x, y, dp);
                Glide ?.DrawLabels(sprites, x, y, dp);
                Filter?.DrawLabels(sprites, x, y, dp);
                Delay ?.DrawLabels(sprites, x, y, dp);
            }                                           



            public void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                if (OK(CurSet))
                    CurSetting.DrawFuncButtons(sprites, w, h, chan);
                
                else
                {
                    DrawFuncButton(sprites, strVol,   1, w, h, True, Volume.HasDeepParams(chan, -1));
                    DrawFuncButton(sprites, strTune,  2, w, h, True, OK(Tune  ));
                    DrawFuncButton(sprites, strGlide, 3, w, h, True, OK(Glide ));
                    DrawFuncButton(sprites, strFlt,   4, w, h, True, OK(Filter));
                    DrawFuncButton(sprites, strDel,   5, w, h, True, OK(Delay ));
                }
            }
        }
    }
}
