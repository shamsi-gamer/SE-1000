using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        //void InitPlaybackAfterLoad(long playTime)
        //{
        //    foreach (var track in g_session.Tracks)
        //    {
        //        if (track.PlayClip < 0) continue;
        //        var playClip = track.Clips[track.PlayClip];

        //        playClip.SetCurrentPattern(nextClip.CurPat);

        //        track.PlayTime = playTime % (playClip.Patterns.Count * g_patSteps * g_session.TicksPerStep);

        //        track.StartTime =
        //            Playing
        //            ? g_time - track.PlayTime        
        //            : long_NaN;

        //        if (playClip.AutoCue)
        //            playClip.SetCue();

        //        track.CueNextPattern();
        //    }
        //}


        void Play(bool play = True)
        {
            if (!OK(Tracks))
                return;


            if (  !IsPlaying // play
                && play) 
            {
                foreach (var track in Tracks)
                {
                    if (OK(track.NextClip))
                        track.NextPat = 0;
                }
            }

            else // stop
            { 
                foreach (var track in Tracks)
                {
                    if (!OK(track.PlayClip))
                        continue;

                    if (OK(track.NextClip))
                        track.NextClip = -1;
                    else
                    {
                        var playClip = track.Clips[track.PlayClip];

                        var b = playClip.GetBlock(playClip.CurPat);

                        var _block =
                               playClip.Block
                            && OK(b)
                            && CurPat > b.First;

                        playClip.SetCurrentPattern(_block ? b.First : 0);

                        track.NextPat   = -1;
                        track.PlayPat   = -1;

                        playClip.TrimCurrentNotes();

                        track.PlayTime  = long_NaN;
                        track.StartTime = long_NaN;

                        track.PlayClip  = -1;
                    }
                }

                lastNotes.Clear();
            }
        }


        void Stop() { Play(False); }


        void CheckIfMustStop()
        {
            var playing = OK(Tracks.Find(track => 
                   OK(track.PlayClip) 
                || OK(track.NextClip)));

            if (    IsPlaying
                && !playing)
                Stop();
        }


        void UpdatePlayback()
        {
            var cueNext = False;

            foreach (var track in Tracks)
            {
                if (   !OK(track.PlayClip)
                    && !OK(track.NextClip))
                    continue;

                cueNext |= track.CueNextPattern();

                if (!OK(track.PlayClip))
                    continue;

                var clip = track.Clips[track.PlayClip];

                if (   clip == EditedClip
                    && clip.Follow) 
                    clip.SetCurrentPattern(track.PlayPat);

                AddPlaybackNotes(clip);
            }


            if (cueNext)
            {
                foreach (var track in Tracks)
                {
                    if (OK(track.NextClip))
                        track.NextPat = 0;
                }
            }


            StopNotes();
            DeleteSounds(StopSounds());
            UpdateSounds();


            foreach (var track in Tracks)
                track.UpdateVolumes(this);

            foreach (var inst in Instruments)
                inst.ResetValues();
        }
    }
}
