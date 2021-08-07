namespace IngameScript
{
    partial class Program
    {
        void ToggleMixerShift()
        {
            MixerShift = !MixerShift;
        }



        void SetVolumeAll(float dv)
        {
            if (ShowMixer > 0)
            { 
                var mod = (MixerShift ? 10 : 1) * dv;
                EditedClip.Volume = MinMax(0, EditedClip.Volume + dVol * mod, 2);

                (dv > 0
                 ? lblMixerVolumeUp
                 : lblMixerVolumeDown).Mark();
            }
            else
            {
                if (dv > 0)
                {
                    EditClip = EditClip != 0 ? 0 : -1;
                }
                else
                {
                    if (EditClip == 2)
                        ClipCopy = Clip_null;

                    EditClip = EditClip != 1 ? 1 : -1;
                }
            }
        }



        void EnableChannels(bool on)
        {
            if (ShowMixer > 0)
            { 
                for (int ch = 0; ch < g_nChans; ch++)
                    EnableChannel(ch, on);

                (on
                 ? lblMixerAll
                 : lblMixerMuteAll).Mark();
            }
            else
            { 
                if (on) 
                {
                    if (EditClip == 1)
                        ClipCopy = Clip_null;

                    EditClip = EditClip != 2 ? 2 : -1;
                }
                else 
                    EditClip = EditClip != 3 ? 3 : -1;
            }
        }



        void EnableChannel(int ch, bool on)
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                EditedClip.Patterns[p].Channels[ch].On = on;
        }



        void EnableChannel(int pat, int ch, bool on)
        {
            var chan = EditedClip.Patterns[pat].Channels[ch];
            chan.On = on;
        }



        public void SetVolume(int ch, float dv)
        {
            if (ShowMixer == 2)
            { 
                var vol = EditPattern.Channels[ch].Volume;
                var mod = (MixerShift ? 10 : 1) * dv;

                int first, last;
                EditedClip.GetPatterns(EditPat, out first, out last);

                for (int p = first; p <= last; p++)
                {
                    var chan = EditedClip.Patterns[p].Channels[ch];
                    chan.Volume = MinMax(0, vol + dVol * mod, 2);
                }

                g_lcdPressed.Add(lcdMixer+ch);
            }
            else
            { 
                if (MixerShift) SetAllTrackClips(ch, dv>0 ? 0 : 1);
                else            Tracks[dv>0 ? 0 : 1].SetClip(ch);

                CheckIfMustStop();
            }
        }



        void Solo(int ch)
        {
            if (ShowMixer == 2)
            { 
                if (EditedClip.Solo >= 0)
                {
                    int _first, _last;
                    EditedClip.GetCurPatterns(out _first, out _last);

                    for (int p = _first; p <= _last; p++)
                        UnsoloChannel(p, ch);
                }

                if (ch == EditedClip.Solo)
                {
                    EditedClip.Solo = -1;
                    return;
                }

                for (int _ch = 0; _ch < g_nChans; _ch++)
                    EditedClip.ChanOn[_ch] = EditPattern.Channels[_ch].On;

                int first, last;
                EditedClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    SoloChannel(p, ch);

                EditedClip.Solo = EditedClip.Solo == ch ? -1 : ch;
            }
            else
            {
                if (MixerShift) SetAllTrackClips(ch, 2);
                else            Tracks[2].SetClip(ch);

                CheckIfMustStop();
            }
        }



        void Mute(int ch)
        {
            if (ShowMixer == 2)
            { 
                var on = !EditPattern.Channels[ch].On;

                int first, last;
                EditedClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    EditedClip.Patterns[p].Channels[ch].On = on;

                if (!on)
                    EditedClip.TrimCurrentNotes(ch);
            }
            else
            {
                if (MixerShift) SetAllTrackClips(ch, 3);
                else            Tracks[3].SetClip(ch);

                CheckIfMustStop();
            }
        }



        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, False);
                EditedClip.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, True);
        }



        void UnsoloChannel(int pat, int ch)
        {
            if (EditedClip.Solo >= 0)
            {
                for (int i = 0; i < g_nChans; i++)
                    EnableChannel(pat, i, EditedClip.ChanOn[i]);
            }
        }
    }
}
