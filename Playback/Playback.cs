using System;


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
        //            g_playing
        //            ? g_time - track.PlayTime        
        //            : long_NaN;

        //        if (playClip.AutoCue)
        //            playClip.SetCue();

        //        track.CueNextPattern();
        //    }
        //}


        void Play(bool play = T)
        {
            if (  !g_playing // play
                && play) 
            {
                g_playing = T;

                var nextPat   = 0;
                var playTime  = GetPatTime(nextPat);
                var startTime = g_time - playTime;

                foreach (var track in g_session.Tracks)
                {
                    track.PlayTime  = playTime;
                    track.StartTime = startTime;

                    //if (track.NextClip < 0) continue;
                    //var nextClip = track.Clips[track.NextClip];

                    //var nextPat = 
                    //    track.NextPat > -1 
                    //    ? track.NextPat 
                    //    : nextClip.CurPat;

                    //track.PlayTime = GetPatTime(nextPat);
                    //track.NextPat  = -1;

                    //track.StartTime = g_time - track.PlayTime;
                }
            }

            else // stop
            { 
                if (NO(g_session))
                    return;

                g_playing = F;


                foreach (var track in g_session.Tracks)
                {
                    if (track.PlayClip < 0) continue;
                    var playClip = track.Clips[track.PlayClip];

                    var b = playClip.GetBlock(playClip.CurPat);

                    var _block =
                           playClip.Block
                        && OK(b)
                        && CurPat > b.First;

                    playClip.SetCurrentPattern(_block ? b.First : 0);
                    playClip.Track.NextPat = -1;


                    playClip.TrimCurrentNotes();


                    playClip.Track.PlayTime  = long_NaN;
                    playClip.Track.StartTime = long_NaN;
                }


                lastNotes.Clear();
            }
        }


        void UpdatePlayback()
        {
            if (g_playing)
            {
                foreach (var track in g_session.Tracks)
                {
                    if (   track.PlayClip < 0
                        && track.NextClip < 0)
                        continue;

                    track.CueNextPattern();

                    var clip = track.Clips[track.PlayClip];

                    if (   clip == CurClip
                        && clip.Follow) 
                        clip.SetCurrentPattern(track.PlayPat);

                    AddPlaybackNotes(clip);
                }
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
