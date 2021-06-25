using System;


namespace IngameScript
{
    partial class Program
    {
        void Play(bool play = True)
        {
            if (!OK(Tracks))
                return;


            var anyCued = !OK(Tracks.Find(t => 
                   OK(t.NextClip) 
                || OK(t.PlayClip)));


            if (  !Playing // play
                && play) 
            {
                if (      ShowClip 
                       && ShowMixer == 2
                    || anyCued) // everything stopped, nothing cued
                {
                    var track = EditedClip.Track;

                    var saved = CueClip;
                    CueClip = 0;

                    track.NextClip = EditedClip.Index;
                    track.CueNextClip(EditedClip.Index);

                    if (anyCued)
                        track.NextPat = 0;

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

                        var b = playingClip.GetBlock(playingClip.EditPat);

                        var _block =
                               playingClip.Block
                            && OK(b)
                            && playingClip.EditPat > b.First;

                        playingClip.SetEditPattern(_block ? b.First : 0);
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
                    {
                        var playClip = track.Clips[track.PlayClip];

                        if (   CueClip == 1
                            && OK(refClip))
                        {
                            var nextPat = refClip.Track.PlayPat + 1;

                            if (playClip.Patterns.Count < refClip.Patterns.Count)
                            { 
                                if (nextPat >= playClip.Patterns.Count) 
                                    nextPat = 0;

                                track.NextPat = nextPat % playClip.Patterns.Count;
                            }
                            else if (playClip.Patterns.Count > refClip.Patterns.Count)
                            {
                                if (nextPat >= refClip.Patterns.Count)
                                    nextPat = 0;

                                track.NextPat = nextPat % refClip.Patterns.Count;
                            }
                            else
                            {
                                if (nextPat >= playClip.Patterns.Count)
                                    nextPat = 0;

                                track.NextPat = nextPat;
                            }
                        }
                        else 
                            track.NextPat = 0;
                    }
                }


                if (!OK(track.PlayClip)) // stop clip
                {
                    track.PlayPat = -1;
                    track.NextPat = -1;

                    track.PlayTime  = long_NaN;
                    track.StartTime = long_NaN;
                    
                    continue;
                }
            }


            foreach (var track in Tracks)
            {
                var clip = track.PlayingClip;
                if (!OK(clip)) continue;

                track.CueNextPattern(clip);

                if (   clip == EditedClip
                    && clip.Follow)
                { 
                    clip.SetEditPattern(track.PlayPat);
                    clip.FindAndSetActiveOctave();
                }

                AddPlaybackNotes(clip);
            }


            StopNotes();
            DeleteSounds(StopSounds());
            UpdateSounds();


            foreach (var track in Tracks)
                track.UpdateVolumes();

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
        }
    }
}
