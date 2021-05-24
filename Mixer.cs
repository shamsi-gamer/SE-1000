namespace IngameScript
{
    partial class Program
    {
        void MixerShift()
        {
            EditClip.MixerShift = !EditClip.MixerShift;
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
            EditClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                EditClip.Patterns[p].Channels[ch].On = on;
        }


        void EnableChannel(int pat, int ch, bool on)
        {
            var chan = EditClip.Patterns[pat].Channels[ch];
            chan.On = on;
        }


        public void SetVolume(int ch, float dv)
        {
            if (g_showSession)
            { 
                g_session.Tracks[dv > 0 ? 0 : 1].SetClip(ch);
            }
            else
            { 
                var vol = CurPattern.Channels[ch].Volume;
                var mod = (EditClip.MixerShift ? 10 : 1) * dv;

                int first, last;
                EditClip.GetPatterns(CurPat, out first, out last);

                for (int p = first; p <= last; p++)
                {
                    var chan = EditClip.Patterns[p].Channels[ch];
                    chan.Volume = MinMax(0, vol + dVol * mod, 2);
                }

                g_lcdPressed.Add(lcdMixer+ch);
            }
        }


        void Solo(int ch)
        {
            if (g_showSession)
                g_session.Tracks[2].SetClip(ch);

            else
            {
                if (EditClip.Solo >= 0)
                {
                    int _first, _last;
                    EditClip.GetCurPatterns(out _first, out _last);

                    for (int p = _first; p <= _last; p++)
                        UnsoloChannel(p, ch);
                }

                if (ch == EditClip.Solo)
                {
                    EditClip.Solo = -1;
                    return;
                }


                for (int _ch = 0; _ch < g_nChans; _ch++)
                    EditClip.ChanOn[_ch] = CurPattern.Channels[_ch].On;


                int first, last;
                EditClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    SoloChannel(p, ch);

                EditClip.Solo = EditClip.Solo == ch ? -1 : ch;
            }
        }


        void Mute(int ch)
        {
            if (g_showSession)
                g_session.Tracks[3].SetClip(ch);

            else
            { 
                var on = !CurPattern.Channels[ch].On;

                int first, last;
                EditClip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    EditClip.Patterns[p].Channels[ch].On = on;

                if (!on)
                    EditClip.TrimCurrentNotes(ch);
            }
        }


        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, F);
                EditClip.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, T);
        }


        void UnsoloChannel(int pat, int ch)
        {
            if (EditClip.Solo >= 0)
            {
                for (int i = 0; i < g_nChans; i++)
                    EnableChannel(pat, i, EditClip.ChanOn[i]);
            }
        }
    }
}
