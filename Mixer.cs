namespace IngameScript
{
    partial class Program
    {
        void MixerShift()
        {
            CurClip.MixerShift = !CurClip.MixerShift;
            //UpdateLabel(lblMixerShift, CurClip.MixerShift);
        }


        void EnableChannels(bool on)
        {
            for (int ch = 0; ch < g_nChans; ch++)
                EnableChannel(ch, on);

            (on
             ? lblMixerAll
             : lblMixerMuteAll).Mark();
        }


        void EnableChannel(int ch, bool on)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                CurClip.Patterns[p].Channels[ch].On = on;
        }


        void EnableChannel(int pat, int ch, bool on)
        {
            var chan = CurClip.Patterns[pat].Channels[ch];
            chan.On = on;
        }


        public void SetVolume(int ch, float dv)
        {
            if (g_showSession)
            { 
                g_session.SetClip(dv > 0 ? 0 : 1, ch);
            }
            else
            { 
                var vol = CurPattern.Channels[ch].Volume;
                var mod = (CurClip.MixerShift ? 10 : 1) * dv;

                int first, last;
                CurClip.GetPatterns(CurPat, out first, out last);

                for (int p = first; p <= last; p++)
                {
                    var chan = CurClip.Patterns[p].Channels[ch];
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
                if (CurClip.Solo >= 0)
                {
                    int _first, _last;
                    CurClip.GetCurPatterns(out _first, out _last);

                    for (int p = _first; p <= _last; p++)
                        UnsoloChannel(p, ch);
                }

                if (ch == CurClip.Solo)
                {
                    CurClip.Solo = -1;
                    return;
                }


                for (int _ch = 0; _ch < g_nChans; _ch++)
                    CurClip.ChanOn[_ch] = CurPattern.Channels[_ch].On;


                int first, last;
                CurClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    SoloChannel(p, ch);

                CurClip.Solo = CurClip.Solo == ch ? -1 : ch;
            }
        }


        void Mute(int ch)
        {
            if (g_showSession)
                g_session.SetClip(3, ch);

            else
            { 
                var on = !CurPattern.Channels[ch].On;

                int first, last;
                CurClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    CurClip.Patterns[p].Channels[ch].On = on;

                if (!on)
                    CurClip.TrimCurrentNotes(ch);
            }
        }


        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, F);
                CurClip.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, T);
        }


        void UnsoloChannel(int pat, int ch)
        {
            if (CurClip.Solo >= 0)
            {
                for (int i = 0; i < g_nChans; i++)
                    EnableChannel(pat, i, CurClip.ChanOn[i]);
            }
        }
    }
}
