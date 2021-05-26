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


        void Play(bool play = T)
        {
            if (!OK(g_session))
                return;

            if (  !(g_session?.IsPlaying ?? F) // play
                && play) 
            {
                //var nextPat   = 0;
                //var playTime  = GetPatTime(nextPat);
                //var startTime = g_time - playTime;

                foreach (var track in g_session.Tracks)
                {
                    track.PlayPat = 0;
                //    //track.PlayTime  = playTime;
                //    //track.StartTime = startTime;

                //    //if (track.NextClip < 0) continue;
                //    //var nextClip = track.Clips[track.NextClip];

                //    //var nextPat = 
                //    //    track.NextPat > -1 
                //    //    ? track.NextPat 
                //    //    : nextClip.CurPat;

                //    //track.PlayTime = GetPatTime(nextPat);
                //    //track.NextPat  = -1;

                //    //track.StartTime = g_time - track.PlayTime;
                }
            }

            else // stop
            { 
                foreach (var track in g_session.Tracks)
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


        void Stop() { Play(F); }


        void UpdatePlaybackStatus()
        {
            var playing = OK(g_session.Tracks.Find(track => 
                   OK(track.PlayClip) 
                || OK(track.NextClip)));

            if (    g_session.IsPlaying
                && !playing)
                Stop();
            //else if (!g_session.IsPlaying
            //       && playing)
            //    Play();
        }


        void UpdatePlayback()
        {
            foreach (var track in g_session.Tracks)
            {
                if (   !OK(track.PlayClip)
                    && !OK(track.NextClip))
                    continue;

                track.CueNextPattern();

                if (!OK(track.PlayClip))
                    continue;

                var clip = track.Clips[track.PlayClip];

                if (   clip == EditedClip
                    && clip.Follow) 
                    clip.SetCurrentPattern(track.PlayPat);

                AddPlaybackNotes(clip);
            }


            StopNotes();
            DeleteSounds(StopSounds());
            UpdateSounds();


            foreach (var track in g_session.Tracks)
                track.UpdateVolumes(this);

            foreach (var inst in g_session.Instruments)
                inst.ResetValues();
        }
    }
}
