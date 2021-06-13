using System;


namespace IngameScript
{
    partial class Program
    {
        void Play(bool play = True)
        {
            if (!OK(Tracks))
                return;


            if (  !Playing // play
                && play) 
            {
                if (ShowClip && ShowMixer)
                {
                    var saved = CueClip;
                    CueClip = 0;
                    EditedClip.Track.NextClip = EditedClip.Index;
                    EditedClip.Track.CueNextClip(EditedClip.Index, this);
                    CueClip = saved;
                }
                else
                { 
                    foreach (var track in Tracks)
                    {
                        if (OK(track.NextClip))
                            track.NextPat = 0;
                    }
                }
            }

            else // stop
            { 
                foreach (var track in Tracks)
                {
                    if (!OK(track.PlayClip))
                        continue;

                    if (   OK(track.NextClip)
                        && CueClip > 0)
                        track.NextClip = -1;
                    else
                    {
                        var playingClip = track.PlayingClip;

                        var b = playingClip.GetBlock(playingClip.CurPat);

                        var _block =
                               playingClip.Block
                            && OK(b)
                            && playingClip.CurPat > b.First;

                        playingClip.SetCurrentPattern(_block ? b.First : 0);
                        playingClip.TrimCurrentNotes();

                        track.Stop();

                        if (CueClip == 0) // stop is not cancellable, a double click is like a panic button
                            track.NextClip = -1;
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

            if (    Playing
                && !playing)
                Stop();
        }



        void UpdatePlayback()
        {
            var refClip = GetLongestPlayingClip();

            foreach (var track in Tracks)
            {
                if (   track.NextClip != track.PlayClip // set cue
                    && track.GetCueNextClip(refClip))
                { 
                    track.PlayClip = track.NextClip;

                    if (OK(track.PlayClip))
                        track.NextPat = 0; // prime next pat
                }


                if (!OK(track.PlayClip)) // stop clip
                {
                    track.PlayPat = -1;
                    track.NextPat = -1;
                    continue;
                }


                var clip = track.PlayingClip;
                //if (!OK(clip)) continue; // commenting this out because it shouldn't be necessary

                track.CueNextPattern(clip, this);

                if (   clip == EditedClip
                    && clip.Follow)
                    clip.SetCurrentPattern(track.PlayPat);


                AddPlaybackNotes(clip);
            }


            StopNotes();
            DeleteSounds(StopSounds());
            UpdateSounds();


            foreach (var track in Tracks)
                track.UpdateVolumes(this);

            foreach (var inst in Instruments)
                inst.ResetValues();
        }


        static Clip GetLongestPlayingClip()
        {
            // figure out which clip will serve as the reference
            // for cueing up the next clip

            var longestPlaying = Clip_null;

            foreach (var track in Tracks)
            { 
                if (   OK(track.PlayClip)
                    && (  !OK(longestPlaying)
                        || track.PlayingClip.Patterns.Count > longestPlaying.Patterns.Count))
                    longestPlaying = track.PlayingClip;
            }

            return longestPlaying;

            //return 
            //       !OK(longestPlaying)
            //    || !OK(PlayClip)
            //    ||  longestPlaying.Patterns.Count > PlayingClip.Patterns.Count
            //    ? longestPlaying
            //    : PlayingClip;

            //return 
            //return Math.Min(
            //    Clips[NextClip].Patterns.Count, 
            //    (maxOtherPat % Clips[NextClip].Patterns.Count)) - 1;
        }
    }
}
