namespace IngameScript
{
    partial class Program
    {
        void MixerShift()
        {
            g_session.CurClip.MixerShift = !g_session.CurClip.MixerShift;
            //UpdateLabel(lblMixerShift, g_session.CurClip.MixerShift);
        }


        void EnableChannels(bool on)
        {
            for (int ch = 0; ch < g_nChans; ch++)
                EnableChannel(ch, on);

            //MarkLabel(
            //    on
            //    ? lblMixerAll
            //    : lblMixerMuteAll);
        }


        void EnableChannel(int ch, bool on)
        {
            int first, last;
            g_session.CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                g_session.CurClip.Patterns[p].Channels[ch].On = on;
        }


        void EnableChannel(int pat, int ch, bool on)
        {
            var chan = g_session.CurClip.Patterns[pat].Channels[ch];
            chan.On = on;
        }


        public void SetVolume(int ch, float dv)
        {
            if (g_showSession)
            { 
                //SetClip(dv > 0 ? 0 : 1, ch);
            }
            else
            { 
                var vol = g_session.CurClip.CurrentPattern.Channels[ch].Volume;
                var mod = (g_session.CurClip.MixerShift ? 10 : 1) * dv;

                int first, last;
                g_session.CurClip.GetPatterns(g_session.CurClip.CurPat, out first, out last);

                for (int p = first; p <= last; p++)
                {
                    var chan = g_session.CurClip.Patterns[p].Channels[ch];
                    chan.Volume = MinMax(0, vol + dVol * mod, 2);
                }

                g_mixerPressed.Add(ch);
            }
        }


        void Solo(int ch)
        {
            if (g_showSession)
            {
                //SetClip(2, ch);
            }
            else
            {
                if (g_session.CurClip.Solo >= 0)
                {
                    int _first, _last;
                    g_session.CurClip.GetCurPatterns(out _first, out _last);

                    for (int p = _first; p <= _last; p++)
                        UnsoloChannel(p, ch);
                }

                if (ch == g_session.CurClip.Solo)
                {
                    g_session.CurClip.Solo = -1;
                    return;
                }


                for (int _ch = 0; _ch < g_nChans; _ch++)
                    g_session.CurClip.ChanOn[_ch] = g_session.CurClip.CurrentPattern.Channels[_ch].On;


                int first, last;
                g_session.CurClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    SoloChannel(p, ch);

                g_session.CurClip.Solo = g_session.CurClip.Solo == ch ? -1 : ch;
            }
        }


        void Mute(int ch)
        {
            if (g_showSession)
                g_session.SetClip(3, ch);

            else
            { 
                var on = !g_session.CurClip.CurrentPattern.Channels[ch].On;

                int first, last;
                g_session.CurClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    g_session.CurClip.Patterns[p].Channels[ch].On = on;

                if (!on)
                    g_session.CurClip.TrimCurrentNotes(ch);
            }
        }


        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, false);
                g_session.CurClip.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, true);
        }


        void UnsoloChannel(int pat, int ch)
        {
            if (g_session.CurClip.Solo >= 0)
            {
                for (int i = 0; i < g_nChans; i++)
                    EnableChannel(pat, i, g_session.CurClip.ChanOn[i]);
            }
        }
    }
}
