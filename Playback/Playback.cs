using System;


namespace IngameScript
{
    partial class Program
    {
        void InitPlaybackAfterLoad(long playTime)
        {
            CurClip.SetCurrentPattern(CurClip.CurPat);

            CurClip.PlayTime = playTime % (CurClip.Patterns.Count * g_patSteps * g_session.TicksPerStep);

            CurClip.StartTime =
                g_playing
                ? g_time - CurClip.PlayTime        
                : long_NaN;

            if (CurClip.AutoCue)
                CurClip.SetCue();

            CurClip.CueNextPattern();
        }


        void UpdatePlayback()
        {
            UpdateTime();

            StopNotes(g_playing ? CurClip.PlayStep : 0);
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
                    if (!g_playing)
                        return;

                    clip.CueNextPattern();
            
                    if (   clip == CurClip
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
                        * CurClip.Volume);
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
