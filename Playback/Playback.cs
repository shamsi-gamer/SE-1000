using System.Linq;


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
                    CueClip = False;
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
                        && CueClip)
                        track.NextClip = -1;
                    else
                    {
                        var playClip = track.Clips[track.PlayClip];

                        var b = playClip.GetBlock(playClip.CurPat);

                        var _block =
                               playClip.Block
                            && OK(b)
                            && playClip.CurPat > b.First;

                        playClip.SetCurrentPattern(_block ? b.First : 0);
                        playClip.TrimCurrentNotes();

                        track.Stop();

                        if (!CueClip) // stop is not cancellable, a double click is like a panic button
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
            var cueNext = False;

            foreach (var track in Tracks)
                cueNext |= track.GetCueNextPattern();


            if (cueNext)
            {
                foreach (var track in Tracks)
                {
                    if (track.NextClip != track.PlayClip)
                    { 
                        track.PlayClip = track.NextClip;

                        if (OK(track.PlayClip))
                            track.NextPat = 0;
                    }
                }
            }


            foreach (var track in Tracks)
            {                
                if (!OK(track.PlayClip))
                {
                    track.PlayPat = -1;
                    track.NextPat = -1;
                    continue;
                }

                var clip = track.Clips[track.PlayClip];
                if (!OK(clip)) continue;

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
    }
}
