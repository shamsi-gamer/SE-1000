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
            public Filter       Filter;
            public Delay        Delay;
            public Arpeggio     Arpeggio;

            public List<Source> Sources;

            public float        CurVolume,
                                DisplayVolume;


            public Instrument()
            {
                Name          = "New Sound";
                              
                Volume        = (Parameter)NewSettingFromTag(strVol, null, this, null);
                              
                Tune          = null;
                Filter        = null;
                              
                Delay         = null;
                Arpeggio      = null;
                              
                Sources       = new List<Source>();

                CurVolume     = 0;
                DisplayVolume = float.NaN;
            }


            public Instrument(string name) : this()
            {
                Name = name;
            }


            public Instrument(Instrument inst)
            {
                Name     = inst.Name;

                Volume   = new Parameter(inst.Volume, null);

                Tune     = inst.Tune    ?.Copy();
                Filter   = inst.Filter  ?.Copy();

                Delay    = inst.Delay   ?.Copy();
                Arpeggio = inst.Arpeggio?.Copy();

                Sources = new List<Source>();
                foreach (var src in inst.Sources)
                    Sources.Add(new Source(src, this));

                CurVolume     = inst.CurVolume;
                DisplayVolume = inst.DisplayVolume;
            }


            public void Randomize(Program prog)
            {
                Sources.Clear();
                var nSrc = Math.Max(1, (int)Math.Round(Math.Pow(RND, 1.5) * MaxSources/2));

                var used = new List<Oscillator>();

                for (int i= 0; i < nSrc; i++)
                { 
                    var src = new Source(this);
                 
                    Sources.Add(src);
                    src.Randomize(used, prog);
                }

                Volume.SetValue(1, null, -1);//.Randomize(Program prog);

                if (RND > 0.7f)
                {
                    Tune = new Tune(this, null);
                    Tune.Randomize(prog);
                }
                else
                    Tune = null;
                
                if (   RND > 0.7f
                    && Sources.Find(s => s.Harmonics != null) != null)
                {
                    Filter = new Filter(this, null);
                    Filter.Randomize(prog);
                }
                else
                    Filter = null;

                if (RND > 0.7f)
                {
                    Delay = new Delay(this, null);
                    Delay.Randomize(prog);
                }
                else
                    Delay = null;

                //if (RND > 0.7f)
                //{
                //    Arpeggio = new Arpeggio(this);
                //    Arpeggio.Randomize(Program prog);
                //}
                //else
                    Arpeggio = null;
            }


            public void ResetVolumes()
            {
                DisplayVolume = float.NaN;
                CurVolume     = 0;

                foreach (var src in Sources)
                    src.CurVolume = 0;
            }


            public Setting GetOrAddSettingFromTag(string tag)
            {
                switch (tag)
                {
                    case strVol:  return Volume;
                    case strTune: return Tune     ?? (Tune     = new Tune    (this, null));
                    case strFlt:  return Filter   ?? (Filter   = new Filter  (this, null));
                    case strDel:  return Delay    ?? (Delay    = new Delay   (this, null));
                    case strArp:  return Arpeggio ?? (Arpeggio = new Arpeggio((Instrument)null));
                }

                return null;
            }


            public void Delete(Song song)
            {
                // this method removes note and channel automation associated with this instrument

                Volume   .Delete(song, -1);
                Tune    ?.Delete(song, -1);
                Filter  ?.Delete(song, -1);
                Delay   ?.Delete(song, -1);
                Arpeggio?.Delete(song, -1);

                foreach (var src in Sources)
                    src.Delete(song);
            }


            public string Save()
            {
                var inst = N(
                      W (Name)
                    + WS(Sources.Count)

                    + Volume.Save()

                    + Program.Save(Tune)
                    + Program.Save(Filter)
                    + Program.Save(Delay)
                    + Program.Save(Arpeggio));

                for (int i = 0; i < Sources.Count; i++)
                    inst += N(Sources[i].Save());

                return inst;
            }


            public static Instrument Load(string[] lines, ref int line)
            {
                var data = lines[line++].Split(';');
                var i    = 0;

                var inst = new Instrument();

                inst.Name = data[i++];

                var nSources = int.Parse(data[i++]);

                inst.Volume = Parameter.Load(data, ref i, inst, -1, null);

                while (i < data.Length
                    && (   data[i] == strTune 
                        || data[i] == strFlt 
                        || data[i] == strDel 
                        || data[i] == strArp))
                {
                    switch (data[i])
                    { 
                        case strTune: inst.Tune     = Tune    .Load(data, ref i, inst, -1); break;
                        case strFlt:  inst.Filter   = Filter  .Load(data, ref i, inst, -1); break;
                        case strDel:  inst.Delay    = Delay   .Load(data, ref i, inst, -1); break;
                        case strArp:  inst.Arpeggio = Arpeggio.Load(data, ref i, inst, -1); break;
                    }
                }

                for (int j = 0; j < nSources; j++)
                    Source.Load(lines, ref line, inst, j);

                return inst;
            }


            public void DrawLabels(List<MySprite> sprites, float x, float y, DrawParams dp)
            {
                Volume   .DrawLabels(sprites, x, y, dp);
                Tune    ?.DrawLabels(sprites, x, y, dp);
                Filter  ?.DrawLabels(sprites, x, y, dp);
                Delay   ?.DrawLabels(sprites, x, y, dp);
                Arpeggio?.DrawLabels(sprites, x, y, dp);
            }                                           


            public void DrawFuncButtons(List<MySprite> sprites, float w, float h, Channel chan)
            {
                if (CurSet > -1)
                { 
                    var setting = g_settings[CurSet];
                    setting.DrawFuncButtons(sprites, w, h, chan);
                }
                else
                {
                    DrawFuncButton(sprites, strVol,  1, w, h, true, Volume.HasDeepParams(chan, -1));
                    DrawFuncButton(sprites, strTune, 2, w, h, true, Tune     != null);
                    DrawFuncButton(sprites, strFlt,  3, w, h, true, Filter   != null);
                    DrawFuncButton(sprites, strDel,  4, w, h, true, Delay    != null);
                    DrawFuncButton(sprites, strArp,  5, w, h, true, Arpeggio != null);
                }
            }
        }
    }
}
