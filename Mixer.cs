namespace IngameScript
{
    partial class Program
    {
        void MixerShift()
        {
            EditedClip.MixerShift = !EditedClip.MixerShift;
        }


        void EnableChannels(bool on)
        {
            if (g_session.ShowSession)
            { 
                if (on)
                {
                    g_session.EditClip = 
                        g_session.EditClip != 1
                        ?  1
                        : -1;
                }
                else    
                {
                    g_session.EditClip = 
                        g_session.EditClip != 2
                        ?  2
                        : -1;
                }
            }
            else
            { 
                for (int ch = 0; ch < g_nChans; ch++)
                    EnableChannel(ch, on);

                (on
                 ? lblMixerAll
                 : lblMixerMuteAll).Mark();
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
            if (g_session.ShowSession)
            { 
                g_session.Tracks[dv > 0 ? 0 : 1].SetClip(ch);
                UpdatePlaybackStatus();
            }
            else
            { 
                var vol = CurPattern.Channels[ch].Volume;
                var mod = (EditedClip.MixerShift ? 10 : 1) * dv;

                int first, last;
                EditedClip.GetPatterns(CurPat, out first, out last);

                for (int p = first; p <= last; p++)
                {
                    var chan = EditedClip.Patterns[p].Channels[ch];
                    chan.Volume = MinMax(0, vol + dVol * mod, 2);
                }

                g_lcdPressed.Add(lcdMixer+ch);
            }
        }


        void Solo(int ch)
        {
            if (g_session.ShowSession)
            { 
                g_session.Tracks[2].SetClip(ch);
                UpdatePlaybackStatus();
            }
            else
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
                    EditedClip.ChanOn[_ch] = CurPattern.Channels[_ch].On;


                int first, last;
                EditedClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    SoloChannel(p, ch);

                EditedClip.Solo = EditedClip.Solo == ch ? -1 : ch;
            }
        }


        void Mute(int ch)
        {
            if (g_session.ShowSession)
            { 
                g_session.Tracks[3].SetClip(ch);
                UpdatePlaybackStatus();
            }
            else
            { 
                var on = !CurPattern.Channels[ch].On;

                int first, last;
                EditedClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    EditedClip.Patterns[p].Channels[ch].On = on;

                if (!on)
                    EditedClip.TrimCurrentNotes(ch);
            }
        }


        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, F);
                EditedClip.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, T);
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
