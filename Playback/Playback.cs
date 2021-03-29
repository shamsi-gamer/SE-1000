using System;


namespace IngameScript
{
    partial class Program
    {
        void InitPlaybackAfterLoad(long playTime)
        {
            SetCurrentPattern(CurPat);

            if (g_autoCue)
                Cue();

            PlayTime = playTime % (g_song.Patterns.Count * nSteps * g_ticksPerStep);

            StartTime = 
                PlayTime > -1 
                ? g_time - PlayTime        
                : -1;

            CueNextPattern();
        }


        void UpdateTime()
        {
            if (PlayTime < 0)
                return;

            CueNextPattern();

            if (g_follow) 
                SetCurrentPattern(PlayPat);

            AddPlaybackNotes();

            UpdateOctaveLight();
        }


        void UpdatePlayback()
        {
            UpdateTime();

            StopNotes(PlayStep);

            var delete = StopSounds();
            DeleteSounds(delete);

            UpdateSounds();
            UpdateVolumes();
        }


        void CueNextPattern()
        {
            //var noteLen = (int)(EditLength * g_ticksPerStep);

            g_song.Length = g_song.Patterns.Count * nSteps;
            //    g_song.Arpeggio != null
            //    ? (int)Math.Round(g_song.Arpeggio.Length.GetValue(g_time, 0, g_song.StartTime, noteLen, null, g_song.CurSrc))
            /*:*/


            if (g_cue > -1)
            {
                var b = g_song.GetBlock(PlayPat);

                if (g_block && b != null)
                    PlayPat = b.Last;
            }


            if (PlayStep >= (PlayPat + 1) * nSteps)
            { 
                int start, end;
                GetPosLimits(g_song, PlayPat, out start, out end);
                end = start + Math.Min(end - start, g_song.Length);

                if (g_cue > -1)
                {
                    var b = g_song.GetBlock(g_cue);
                    if (g_block && b != null)
                        g_cue = b.First;

                    PlayTime  = GetPatTime(g_cue);
                    StartTime = g_time - PlayTime;

                    g_cue = -1;
                    UpdateLight(lblCue, g_cue > -1);
                }
                else if (PlayStep >= end)
                {
                    StopCurrentNotes(g_song);

                    PlayTime  -= (end - start) * g_ticksPerStep;
                    StartTime += (end - start) * g_ticksPerStep;
                }
            }


            PlayPat = 
                PlayTime > -1 
                ? (int)(PlayStep / nSteps) 
                : -1;
        }


        void GetPosLimits(Song song, int pat, out int start, out int end)
        {
            int first, last;
            GetPlayPatterns(song, pat, out first, out last);

            start =  first     * nSteps;
            end   = (last + 1) * nSteps;
        }


        void GetPlayPatterns(Song song, int p, out int f, out int l)
        {
            if (g_loop)
            {
                f = p;
                l = p;

                var b = song.GetBlock(p);

                if (   g_block
                    && b != null)
                {
                    f = b.First;
                    l = b.Last;
                }
            }
            else
            {
                f = 0;
                l = song.Patterns.Count-1;
            }
        }


        void FinalizePlayback(Song song)
        {
            //var pat = song.Patterns[song.PlayPat];

            //for (int ch = 0; ch < nChans; ch++)
            //{
            //    var chan = pat.Channels[ch];

            //    var arpNotes = chan.Notes.FindAll(n =>
            //                n.Instrument.Arpeggio != null
            //            && (int)(song.PlayStep * g_ticksPerStep) >= (int)((song.PlayPat * nSteps + n.StepTime               ) * g_ticksPerStep)
            //            && (int)(song.PlayStep * g_ticksPerStep) <  (int)((song.PlayPat * nSteps + n.StepTime + n.StepLength) * g_ticksPerStep));

            //    var noteLen = (int)(EditLength * g_ticksPerStep);

            //    foreach (var n in arpNotes)
            //    {
            //        var arp = n.Instrument.Arpeggio;

            //        n.FramePlayTime += arp.Scale .GetValue(g_time, 0, song.StartTime, noteLen, n, -1);
            //        var maxLength    = arp.Length.GetValue(g_time, 0, song.StartTime, noteLen, n, -1);

            //        while (n.FramePlayTime >= maxLength * g_ticksPerStep)
            //            n.FramePlayTime -= maxLength * g_ticksPerStep;
            //    }
            //}


            g_time++;

            if (PlayTime > -1)
                PlayTime++;
        }


        void UpdateVolumes()
        {
            for (int i = 0; i < g_sounds.Count; i++)
            {
                if (TooComplex) return;

                var snd   = g_sounds[i];
                var lTime = g_time - snd.FrameTime;

                if (lTime < snd.FrameLength + snd.ReleaseLength)
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
