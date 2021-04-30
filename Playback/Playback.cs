using System;


namespace IngameScript
{
    partial class Program
    {
        void InitPlaybackAfterLoad(long playTime)
        {
            g_session.CurClip.SetCurrentPattern(g_session.CurClip.CurPat);

            g_session.CurClip.PlayTime = playTime % (g_session.CurClip.Patterns.Count * g_nSteps * g_session.TicksPerStep);

            g_session.CurClip.StartTime =
                OK(g_session.CurClip.PlayTime)
                ? g_time - g_session.CurClip.PlayTime        
                : long_NaN;

            if (g_session.CurClip.AutoCue)
                g_session.CurClip.SetCue();

            g_session.CurClip.CueNextPattern();
        }


        void UpdatePlayback()
        {
            UpdateTime();

            StopNotes(g_session.CurClip?.PlayStep ?? 0);
            DeleteSounds(StopSounds());

            UpdateSounds();
            UpdateVolumes();

            ResetValues();
        }


        void UpdateTime()
        {
            foreach (var track in g_session.Tracks)
            {
                foreach (var clip in track.Clips)
                {
                    if (!OK(clip.PlayTime))
                        return;

                    clip.CueNextPattern();
            
                    if (   clip == g_session.CurClip
                        && clip.Follow) 
                        clip.SetCurrentPattern(clip.PlayPat);

                    AddPlaybackNotes(clip);
                }
            }

            //UpdateOctaveLabel();
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
                    if (!OK(instVol)) instVol = 0;

                    g_dspVol[snd.iChan] = Math.Max(
                        g_dspVol[snd.iChan],
                          instVol
                        * snd.Channel.Volume
                        * g_session.CurClip.Volume);
                }
            }
        }


        void ResetValues()
        {
            foreach (var inst in g_session.Instruments)
                inst.ResetValues();
        }


        void DampenDisplayVolumes()
        {
            for (int i = 0; i < g_dspVol.Length; i++)
                g_dspVol[i] *= 0.7f;
        }
    }
}
