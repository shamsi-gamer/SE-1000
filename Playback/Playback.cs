using System;


namespace IngameScript
{
    partial class Program
    {
        void InitPlaybackAfterLoad(long playTime)
        {
            g_clip.SetCurrentPattern(g_clip.CurPat);

            g_clip.PlayTime = playTime % (g_clip.Patterns.Count * g_nSteps * g_ticksPerStep);

            g_clip.StartTime =
                OK(g_clip.PlayTime)
                ? g_time - g_clip.PlayTime        
                : long_NaN;

            if (g_clip.AutoCue)
                g_clip.SetCue();

            g_clip.CueNextPattern();
        }


        void UpdatePlayback()
        {
            UpdateTime();

            StopNotes(g_clip?.PlayStep ?? 0);
            DeleteSounds(StopSounds());

            UpdateSounds();
            UpdateVolumes();

            ResetValues();
        }


        void UpdateTime()
        {
            foreach (var track in g_tracks)
            {
                foreach (var clip in track.Clips)
                {
                    if (!OK(clip.PlayTime))
                        return;

                    clip.CueNextPattern();
            
                    if (   clip == g_clip
                        && clip.Follow) 
                        clip.SetCurrentPattern(clip.PlayPat);

                    AddPlaybackNotes(clip);
                }
            }

            UpdateOctaveLight();
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
                        * g_clip.Volume);
                }
            }
        }


        void ResetValues()
        {
            foreach (var inst in g_inst)
                inst.ResetValues();
        }


        void DampenDisplayVolumes()
        {
            for (int i = 0; i < g_dspVol.Length; i++)
                g_dspVol[i] *= 0.7f;
        }
    }
}
