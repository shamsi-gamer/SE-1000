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
                g_song.PlayTime > -1 
                ? g_time - g_song.PlayTime        
                : -1;

            if (g_autoCue)
            { 
                g_song.SetCue();
                UpdateLight(lblCue, g_song.Cue > -1);
            }

            g_song.CueNextPattern();

            UpdateLight(lblCue, g_song.Cue > -1);
        }


        void UpdateTime()
        {
            if (g_song.PlayTime < 0)
                return;

            g_song.CueNextPattern();
            
            if (g_follow) 
                SetCurrentPattern(g_song.PlayPat);

            AddPlaybackNotes();

            UpdateLight(lblCue, g_song.Cue > -1);
            UpdateOctaveLight();
        }


        void UpdatePlayback()
        {
            UpdateTime();

            StopNotes(g_song.PlayStep);

            var delete = StopSounds();
            DeleteSounds(delete);

            UpdateSounds();
            UpdateVolumes();
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
                    g_dspVol[snd.iChan] = Math.Max(
                        g_dspVol[snd.iChan],
                          snd.DisplayVolume
                        * snd.Channel.Volume
                        * g_volume);
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
