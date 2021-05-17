﻿using System;


namespace IngameScript
{
    partial class Program
    {
        void InitPlaybackAfterLoad(long playTime)
        {
            CurClip.SetCurrentPattern(CurPat);

            CurClip.Track.PlayTime = playTime % (CurClip.Patterns.Count * g_patSteps * g_session.TicksPerStep);

            CurClip.Track.StartTime =
                g_playing
                ? g_time - CurClip.Track.PlayTime        
                : long_NaN;

            if (CurClip.AutoCue)
                CurClip.SetCue();

            CurClip.Track.CueNextPattern();
        }


        void UpdatePlayback()
        {
            UpdateTime();

            StopNotes(g_playing ? CurClip.Track.PlayStep : 0);
            DeleteSounds(StopSounds());

            UpdateSounds();
            UpdateVolumes();

            g_session.Instruments.ForEach(inst => inst.ResetValues());
        }


        void UpdateTime()
        {
            if (!g_playing)
                return;

            foreach (var track in g_session.Tracks)
            {
                if (track.PlayClip < 0)
                    continue;

                var clip = track.Clips[track.PlayClip];

                clip.Track.CueNextPattern();

                if (   clip == CurClip
                    && clip.Follow) 
                    clip.SetCurrentPattern(clip.Track.PlayPat);

                AddPlaybackNotes(clip);
            }
        }


        void UpdateVolumes()
        {
            for (int i = 0; i < g_sounds.Count; i++)
            {
                if (TooComplex) return;

                var snd   = g_sounds[i];
                var lTime = g_time - snd.Time;

                if (lTime < snd.Length + snd.ReleaseLength)
                {
                    var instVol = snd.Source.Instrument.DisplayVolume;
                    if (NOK(instVol)) instVol = 0;

                    g_dspVol[snd.iChan] = Math.Max(
                        g_dspVol[snd.iChan],
                          instVol
                        * snd.Channel.Volume
                        * CurClip.Volume);
                }
            }
        }


        void DampenDisplayVolumes()
        {
            for (int i = 0; i < g_dspVol.Length; i++)
                g_dspVol[i] *= 0.7f;
        }
    }
}
