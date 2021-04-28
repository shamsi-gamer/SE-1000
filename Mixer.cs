namespace IngameScript
{
    partial class Program
    {
        void MixerShift()
        {
            g_clip.MixerShift = !g_clip.MixerShift;
            UpdateLight(lblMixerShift, g_clip.MixerShift);
        }


        void EnableChannels(bool on)
        {
            for (int ch = 0; ch < g_nChans; ch++)
                EnableChannel(ch, on);

            MarkLight(
                on
                ? lblMixerAll
                : lblMixerMuteAll);
        }


        void EnableChannel(int ch, bool on)
        {
            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                g_clip.Patterns[p].Channels[ch].On = on;
        }


        void EnableChannel(int pat, int ch, bool on)
        {
            var chan = g_clip.Patterns[pat].Channels[ch];
            chan.On = on;
        }


        public void SetVolume(int ch, float dv)
        {
            if (g_session)
                SetClip(dv > 0 ? 0 : 1, ch);

            else
            { 
                var vol = g_clip.CurrentPattern.Channels[ch].Volume;
                var mod = (g_clip.MixerShift ? 10 : 1) * dv;

                int first, last;
                g_clip.GetPatterns(g_clip.CurPat, out first, out last);

                for (int p = first; p <= last; p++)
                {
                    var chan = g_clip.Patterns[p].Channels[ch];
                    chan.Volume = MinMax(0, vol + dVol * mod, 2);
                }

                g_mixerPressed.Add(ch);
            }
        }


        void Solo(int ch)
        {
            if (g_session)
                SetClip(2, ch);

            else
            {
                if (g_clip.Solo >= 0)
                {
                    int _first, _last;
                    g_clip.GetCurPatterns(out _first, out _last);

                    for (int p = _first; p <= _last; p++)
                        UnsoloChannel(p, ch);
                }

                if (ch == g_clip.Solo)
                {
                    g_clip.Solo = -1;
                    return;
                }


                for (int _ch = 0; _ch < g_nChans; _ch++)
                    g_on[_ch] = g_clip.CurrentPattern.Channels[_ch].On;


                int first, last;
                g_clip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    SoloChannel(p, ch);

                g_clip.Solo = g_clip.Solo == ch ? -1 : ch;
            }
        }


        void Mute(int ch)
        {
            if (g_session)
                SetClip(3, ch);

            else
            { 
                var on = !g_clip.CurrentPattern.Channels[ch].On;

                int first, last;
                g_clip.GetCurPatterns(out first, out last);

                for (int p = first; p <= last; p++)
                    g_clip.Patterns[p].Channels[ch].On = on;

                if (!on)
                    g_clip.TrimCurrentNotes(ch);
            }
        }


        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, false);
                g_clip.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, true);
        }


        void UnsoloChannel(int pat, int ch)
        {
            if (g_clip.Solo >= 0)
            {
                for (int i = 0; i < g_nChans; i++)
                    EnableChannel(pat, i, g_on[i]);
            }
        }


        void SetClip(int ch, int index)
        { 
            var track = g_tracks[ch];

            if (g_setClip)
            {
                var found = track.Indices.FindIndex(i => i == index);

                if (found < 0)
                {
                    g_clip = new Clip(track);
                    g_clip.Patterns.Add(new Pattern(g_clip, g_inst[0]));
                    g_clip.Name = "New Clip";

                    track.Clips  .Add(g_clip);
                    track.Indices.Add(index);
                    track.CurIndex = index;
                }
                else
                {
                    g_clip = track.Clips[found];
                }

                g_setClip = false;
                g_session = false;

                UpdateLights();
            }
            else
            {
                track.CurIndex = 
                    track.CurIndex != index
                    ? index
                    : -1;
            }
        }


        void SetClip(Clip clip)
        { 
            clip.Track.CurIndex = clip.Track.Clips.IndexOf(clip);

            g_clip = clip;

            g_setClip = false;
            g_session = false;

            UpdateLights();
        }
    }
}
