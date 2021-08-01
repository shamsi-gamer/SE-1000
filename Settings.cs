using System;


namespace IngameScript
{
    partial class Program
    {
        void SwitchToSetting(Clip clip, Setting set)
        {
            SwitchToSetting(clip, set.Path);
        }



        static void SwitchToSetting(Clip clip, string path)
        {
            BackOut();
            
            EditedClip = clip;
            

            var tags = path.Split('/');

            var inst = Instruments.Find(i => i.Name == tags[0]);


            CurChan =
            SelChan = Array.FindIndex(
                EditPattern.Channels, 
                chan => chan.Instrument == inst);

            if (!OK(CurChan))
                CurChan = 0;

            UpdateInstOff(CurChan);

            SetInstName(True);
            g_inputValid = False;


            int iSrc = -1;

            for (int i = 1; i < tags.Length; i++)
            { 
                var tag = tags[i];

                if (   i == 1
                    && IsDigit(tag[0]))
                { 
                    iSrc = int_Parse(tag);
                    continue;
                }

                AddNextSetting(tag, inst, iSrc);
            }
        }



        static void AddNextSetting(string tag, Instrument inst = Instrument_null, int iSrc = -2)
        {
            if (!OK(inst)) inst = SelInstrument;
            
                 if (OK(iSrc))   CurSrc = iSrc;
            else if (iSrc == -2) iSrc   = CurSrc;

            if (OK(CurSetting))
                CurSetting._IsCurrent = False;

            Setting setting;

                 if (OK(CurSetting)) setting = CurSetting        .GetOrAddSettingFromTag(tag);
            else if (OK(iSrc))       setting = inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
            else                     setting = inst              .GetOrAddSettingFromTag(tag);

            EditedClip.Settings.Add(setting);

            CurSet++;

            if (IsCurParam())
                CurSetting._IsCurrent = True;
        }



        public void DeleteCurSetting(Clip clip)
        {
            var set     = clip.CurSet;
            var setting = clip.CurSetting;

            if (   HasTag(setting, strAtt)
                || HasTag(setting, strDec)
                || HasTag(setting, strSus)
                || HasTag(setting, strRel))
                set--;

            if (clip.CurSet > 0)
            { 
                clip.Settings[clip.CurSet-1].DeleteSetting(setting);
            }
            else 
            {
                var inst = clip.SelInstrument;
                var src  = clip.SelSource;

                switch (setting.Tag)
                {
                    case strOff:  if (OK(src)) { src.Offset   .Delete(src.Index); src.Offset    = Parameter_null; } else {                                                    } break;
                    case strVol:  if (OK(src)) { src.Volume   .Delete(src.Index);                                 } else { inst.Volume.Delete(-1);                            } break;
                    case strDel:  if (OK(src)) { src.Delay    .Delete(src.Index); src.Delay     =     Delay_null; } else { inst.Delay .Delete(-1); inst.Delay  =  Delay_null; } break;
                    case strTune: if (OK(src)) { src.Tune     .Delete(src.Index); src.Tune      =      Tune_null; } else { inst.Tune  .Delete(-1); inst.Tune   =   Tune_null; } break;
                    case strHrm:  if (OK(src)) { src.Harmonics.Delete(src.Index); src.Harmonics = Harmonics_null; }      {                                                    } break;
                    case strFlt:  if (OK(src)) { src.Filter   .Delete(src.Index); src.Filter    =    Filter_null; } else { inst.Filter.Delete(-1); inst.Filter = Filter_null; } break;
                }
            }

            clip.Settings.RemoveAt(set);

            clip.CurSet -= clip.CurSet - set + 1;
        }
    }
}
