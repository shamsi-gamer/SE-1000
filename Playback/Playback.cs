using System;


namespace IngameScript
{
    partial class Program
    {
        void InitPlaybackAfterLoad(long playTime)
        {
            SetCurrentPattern(CurPat);

            g_song.PlayTime = playTime % (g_song.Patterns.Count * g_nSteps * g_ticksPerStep);

            g_song.StartTime =
                OK(g_song.PlayTime)
                ? g_time - g_song.PlayTime        
                : long_NaN;

            if (g_autoCue)
                g_song.SetCue();

            g_song.CueNextPattern();
        }


        void UpdatePlayback()
        {
            UpdateTime();

            StopNotes(g_song.PlayStep);
            DeleteSounds(StopSounds());

            UpdateSounds();
            UpdateVolumes();

            ResetValues();
        }


        void UpdateTime()
        {
            if (!OK(g_song.PlayTime))
                return;

            g_song.CueNextPattern();
            
            if (g_follow) 
                SetCurrentPattern(g_song.PlayPat);

            AddPlaybackNotes();

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
                        * g_volume);
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
