using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        public class Track
        {
            public Clip[]  Clips;


            public long    StartTime, // in ticks
                           PlayTime;

            public int     PlayClip,
                           NextClip,
                           
                           PlayPat, // this can't be a property because it must sometimes be separate from PlayTime, for queueing
                           NextPat;


            public bool    Playing     => OK(PlayClip);
            public Clip    PlayingClip => OK(PlayClip) ? Clips[PlayClip] : Clip_null;

            public float   PlayStep    => Playing ? PlayTime /(float)TicksPerStep : float_NaN; 
            public float   StartStep   => Playing ? StartTime/(float)TicksPerStep : float_NaN; 
                        

            public float[] DspVol;



            public Track()
            {
                Clips = new Clip[g_nChans];
                for (int i = 0; i < g_nChans; i++)
                    Clips[i] = Clip_null;
                          

                Stop();
                          
                DspVol = new float[g_nChans];
            }



            public Track(Track track)
            {
                Clips = new Clip[g_nChans]; 
                for (int i = 0; i < g_nChans; i++)
                {
                    Clips[i]       = track.Clips[i];
                    Clips[i].Track = this;
                }

                StartTime = track.StartTime;
                PlayTime  = track.PlayTime;
                          
                PlayClip  = track.PlayClip;
                NextClip  = track.NextClip;
                          
                PlayPat   = track.PlayPat;
                NextPat   = track.NextPat;
                          
                DspVol    = new float[g_nChans];
            }



            public void Stop()
            { 
                PlayClip  = -1;
                NextClip  = -1;
                          
                PlayPat   = -1;
                NextPat   = -1;
                          
                PlayTime  = long_NaN;
                StartTime = long_NaN;
            }



            public void SetClip(int index, Program prog)
            { 
                if (MixerShift)
                    return;

                var clip = Clips[index];

                if (OK(ClipCopy))          
                    PlaceClip(index);

                else if (OK(clip))
                { 
                    if (   EditClip == 1  // move clip
                        || EditClip == 2) // duplicate clip
                        ClipCopy = clip; 
                    
                    else if (EditClip == 3) // delete clip
                        DeleteClip(clip, index); 

                    else if (OK(PlayClip)
                         && !OK(NextClip)
                         &&  PlayClip == index
                         && CueClip > 0)
                        NextClip = PlayClip; // cancel clip off

                    else if (index != PlayClip) // cue next clip
                    { 
                        CueNextClip(index, prog);
                        SetEditedClip(clip);
                    }
                }

                else if (!OK(clip))
                {
                    if (EditClip == 0) // set clip
                    {
                        if (!OK(Clips[index]))
                        { 
                            Clips[index] = Clip.Create(this); // set clip
                            EditClip = -1;
                        }
                    }
                    else
                    { 
                        NextClip = -1;

                        if (CueClip == 0)
                            PlayClip = -1;
                    }
                }

            }



            public void CueNextClip(int index, Program prog)
            {
                //if (OK(NextClip))
                //{
                //    NextClip =
                //        index == NextClip
                //        ? PlayClip
                //        : index;
                //}
                //else
                    NextClip = index;


                //if (EditClip == 0)
                //{ 
                //    foreach (var track in Tracks)
                //    {
                //        if (track == this) continue;
                //        track.NextClip = -1;
                //    }

                //    EditClip = -1;
                //}


                if (CueClip == 0)
                {
                    var playTime = GetAnyCurrentPlayTime();

                    PlayClip = NextClip;

                    PlayPat  =  0;
                    NextPat  = -1;

                    if (OK(playTime)) PlayTime = playTime % (Clips[NextClip].StepLength * TicksPerStep);
                    else              PlayTime = 0;

                    StartTime = g_time - PlayTime;

                    SetInstName();
                    prog.ResetLfos();
                }
            }



            public bool GetCueNextClip(Clip refClip)
            {
                if (   !OK(PlayClip)
                    && !OK(NextClip))
                    return False;

                
                if (!OK(refClip)) 
                    return OK(NextPat);


                var step = refClip.Track.PlayStep;

                var pat = 
                    CueClip == 2 
                    ? refClip.Patterns.Count-1 
                    : refClip.Track.PlayPat;


                return step >= (pat+1) * g_patSteps;
            }



            void SetEditedClip(Clip clip)
            {
                if (clip != EditedClip) 
                {
                    EditedClip = clip;
                    UpdateClipDisplay(EditedClip);
                    SetInstName();
                    SetLabelColor(EditedClip.ColorIndex);
                }
            }



            void PlaceClip(int index)
            {
                if (   EditClip == 1
                    || EditClip == 2) 
                    MoveClip(index);

                EditedClip = Clips[index];
                UpdateClipDisplay(EditedClip);

                ClipCopy = Clip_null;
                EditClip = -1;
            }



            void MoveClip(int index)
            {
                var srcTrack = ClipCopy.Track;
                var srcIndex = srcTrack.Clips.IndexOf(ClipCopy);


                if (EditClip == 1) // move
                    Swap(ref Clips[index], ref srcTrack.Clips[srcIndex]);

                else  // duplicate
                {
                    if (   OK(Clips[index]) 
                        && Clips[index] == ClipCopy)
                        ClipCopy = Clip_null;
                    else
                    { 
                        Clips[index] = new Clip(srcTrack.Clips[srcIndex], this);
                        GetNewClipName(Clips[index], Clips);
                    }
                }


                // update the clips' tracks in case they were
                // swapped between different tracks
                Clips[index].Track = this;

                if (OK(srcTrack.Clips[srcIndex]))
                    srcTrack.Clips[srcIndex].Track = srcTrack;


                if (srcTrack.PlayClip == srcIndex) // moved clip is playing
                { 
                    if (this != srcTrack)
                    { 
                        if (EditClip == 1) // move
                        { 
                            Swap(ref PlayPat,   ref srcTrack.PlayPat);
                            Swap(ref NextPat,   ref srcTrack.NextPat);

                            Swap(ref PlayTime,  ref srcTrack.PlayTime);
                            Swap(ref StartTime, ref srcTrack.StartTime);

                            if (PlayClip == srcTrack.PlayClip)
                            {
                                Swap(ref PlayClip, ref srcTrack.PlayClip);
                                Swap(ref NextClip, ref srcTrack.NextClip);
                            }
                            else
                            {
                                PlayClip  = srcTrack.PlayClip;
                                NextClip  = srcTrack.NextClip;
                            }
                        }
                        else // duplicate
                        { 
                            PlayPat   = srcTrack.PlayPat;
                            NextPat   = srcTrack.NextPat;

                            PlayTime  = srcTrack.PlayTime;
                            StartTime = srcTrack.StartTime;

                            PlayClip  = srcTrack.PlayClip;
                            NextClip  = srcTrack.NextClip;

                            if (srcTrack != this)
                                Stop();
                        }
                    }
                }
            }



            void DeleteClip(Clip clip, int index)
            {
                if (clip == EditedClip)
                    EditedClip = GetClipAfterDelete(clip);

                Clips[index] = Clip_null;

                if (!SessionHasClips)
                { 
                    Clips[index] = Clip.Create(this);
                    EditedClip = Clips[index];
                }

                UpdateClipDisplay(EditedClip);

                if (PlayClip == index)
                    Stop();

                EditClip = -1;
            }



            public void CueNextPattern(Clip clip, Program prog)
            {
                UpdateBlockPat(clip);
                UpdatePlayTime(clip, prog);

                PlayPat = (int)(PlayStep / g_patSteps);
            }



            void UpdateBlockPat(Clip clip)
            {
                if (   OK(NextPat)
                    && clip.Block)
                {
                    var b = clip.GetBlock(PlayPat);
                    if (OK(b)) PlayPat = b.Last;
                }
            }



            void UpdatePlayTime(Clip clip, Program prog)
            {
                int start, end;
                clip.GetPosLimits(PlayPat, out start, out end);
                end = start + Math.Min(end - start, clip.StepLength);

                if (OK(NextPat))
                {
                    var b = clip.GetBlock(NextPat);
                    if (clip.Block && OK(b))
                        NextPat = b.First;

                    PlayTime  = GetPatTime(NextPat);
                    StartTime = g_time - PlayTime;

                    NextPat = -1;
                }
                else if (PlayStep >= end)
                {
                    clip.WrapCurrentNotes(end - start);

                    PlayTime  -= (end - start) * TicksPerStep;
                    StartTime += (end - start) * TicksPerStep;

                    prog.ResetLfos();
                }
            }



            public string Save()
            {
                var cfg = 
                     (OK(PlayTime) ? S(PlayTime) : "?")
                    + PS(PlayPat)
                    + PS(NextPat)
                    + PS(NextClip);

                var _indices = new List<int>();

                for (int i = 0; i < g_nChans; i++)
                    if (OK(Clips[i])) _indices.Add(i);

                var indices = S(_indices.Count);

                foreach (var i in _indices)
                    indices += PS(i);

                var save =
                      cfg
                    + PN(indices);

                for (int i = 0; i < _indices.Count; i++)
                    save += PN(Clips[_indices[i]].Save());

                return save;
            }



            public static Track Load(string[] lines, ref int line)//, out string curPath)
            {
                if (line >= lines.Length) 
                    return Track_null;

                var track = new Track();

                var strCfg = lines[line++];

                if (!strCfg.Contains(';'))
                    return Track_null;

                var cfg = strCfg.Split(';');
                var c = 0;

                if (   cfg.Length < 4
                    || !long_TryParse(cfg[c++], out track.PlayTime)
                    || ! int_TryParse(cfg[c++], out track.PlayPat )
                    || ! int_TryParse(cfg[c++], out track.NextPat )
                    || ! int_TryParse(cfg[c++], out track.NextClip)) 
                    return Track_null;

                var _indices = lines[line++].Split(';');

                int nClips;
                if (!int_TryParse(_indices[0], out nClips)) return Track_null;

                var indices = new List<int>();
                for (int i = 0; i < nClips; i++)
                    indices.Add(int_Parse(_indices[i+1]));
                 
                //curPath = "";

                for (int i = 0; i < nClips; i++)
                {
                    var clip = Clip.Load(lines, ref line, track);

                    if (OK(clip)) track.Clips[indices[i]] = clip;//, out curPath));
                    else          return Track_null;
                }

                return track;
            }



            public void UpdateVolumes(Program prog)
            {
                for (int i = 0; i < g_sounds.Count; i++)
                {
                    if (prog.TooComplex) return;

                    var snd = g_sounds[i];

                    if (snd.Note.Clip.Track != this)
                        continue;

                    var lTime = g_time - snd.Time;

                    if (lTime < snd.Length + snd.ReleaseLength)
                    {
                        var instVol = snd.Source.Instrument.DisplayVolume;
                        if (!OK(instVol)) instVol = 0;

                        var playVol =
                            OK(PlayingClip)
                            ?   instVol
                              * snd.Channel.Volume
                              * PlayingClip.Volume
                            : 0;

                        DspVol[snd.iChan] = Math.Max(
                            DspVol[snd.iChan],
                            playVol);
                    }
                }
            }



            public void DampenDisplayVolumes()
            {
                for (int i = 0; i < DspVol.Length; i++)
                    DspVol[i] *= 0.2f;
            }



            public void ResetDisplayVolumes()
            {
                for (int i = 0; i < DspVol.Length; i++)
                    DspVol[i] = 0;
            }
        }


        void SetAllTrackClips(int index, int refTrack)
        {
            var tracks = new List<Track>();

            foreach (var track in Tracks)
                tracks.Add(track);

            // make the ref track last so that double clicking works when Shift is enabled
            if (refTrack < tracks.Count-1)
            { 
                var temp = tracks[refTrack];
                tracks[refTrack] = tracks.Last();
                tracks[tracks.Count-1] = temp;
            }

            foreach (var track in tracks)
            {
                if (OK(track.Clips[index])) track.SetClip(index, this);
                else                        track.NextClip = -1;
            }
        }
    }
}
