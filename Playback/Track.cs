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


            public bool    Playing => OK(PlayClip);


            public float   PlayStep { get 
                           {
                               return
                                   Playing 
                                   ? PlayTime / (float)TicksPerStep 
                                   : float_NaN; 
                           } }

            
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


            public void SetClip(int index)
            { 
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
                                                 
                    else if (index != PlayClip)
                        CueNextClip(index);
                    
                    else if (OK(PlayClip)
                         &&  OK(NextClip)
                         &&  NextClip != PlayClip)
                    { 
                        NextClip = PlayClip; // cancel clip cue
                        CueNextClip(index);
                        UpdateClipName();
                    }
                    else if (OK(PlayClip)
                         && !OK(NextClip)
                         &&  CueClip)
                    { 
                        NextClip = PlayClip; // cancel clip off
                        CueNextClip(index);
                        UpdateClipName();
                    }

                    else
                    { 
                        NextClip = -1; // cue clip off
                        
                        if (!CueClip)
                            Stop();
                    }
                }
                else if (EditClip == 0)
                { 
                    Clips[index] = Clip.Create(this);
                    CueNextClip(index);
                }


                if (   OK(NextClip)
                    && OK(Clips[NextClip])) 
                    SetEditedClip(Clips[NextClip]);
            }


            void CueNextClip(int index)
            {
                if (OK(NextClip))
                {
                    NextClip =
                        index == NextClip
                        ? PlayClip
                        : index;
                }
                else
                    NextClip = index;


                if (EditClip == 0)
                { 
                    foreach (var track in Tracks)
                    {
                        if (track == this) continue;
                        track.NextClip = -1;
                    }

                    EditClip = -1;
                }


                if (!CueClip)
                {
                    var playTime = GetAnyCurrentPlayTime();

                    PlayClip = NextClip;

                    PlayPat  =  0;
                    NextPat  = -1;

                    if (OK(playTime)) PlayTime = playTime % (Clips[NextClip].Patterns.Count * g_patSteps * TicksPerStep);
                    else              PlayTime = 0;

                    UpdateInstName();
                }


                StartTime = g_time - PlayTime;
            }


            void SetEditedClip(Clip clip)
            {
                if (clip != EditedClip) 
                {
                    EditedClip = clip;
                    UpdateClipDisplay();
                    UpdateInstName();
                }
            }


            void PlaceClip(int index)
            {
                if (   EditClip == 1
                    || EditClip == 2) 
                    MoveClip(index);

                EditedClip = Clips[index];
                UpdateClipDisplay();

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
                        UpdateClipName(Clips[index], Clips);
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

                UpdateClipDisplay();

                if (PlayClip == index)
                    Stop();

                EditClip = -1;
            }


            public bool GetCueNextPattern()
            {
                if (   !OK(PlayClip)
                    && !OK(NextClip))
                    return False;

                if (   !OK(PlayPat)
                    && !OK(NextPat))
                    return False;

                if (PlayStep < (PlayPat + 1) * g_patSteps)
                    return False;

                //if (NextClip != PlayClip)
                //{ 
                //    NextPat  = 0;
                //    PlayClip = NextClip;
                //}

                //if (!OK(PlayClip))
                //{
                //    PlayPat = -1;
                //    NextPat = -1;
                //    return False;
                //}

                return True;
            }


            public void CueNextPattern(Clip clip)
            {
                clip.Length = clip.Patterns.Count * g_patSteps;

                UpdateBlockPat(clip);
                UpdatePlayTime(clip);

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


            void UpdatePlayTime(Clip clip)
            {
                int start, end;
                clip.GetPosLimits(PlayPat, out start, out end);
                end = start + Math.Min(end - start, clip.Length);

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

                    if (snd.Channel.Pattern.Clip.Track != this)
                        continue;

                    var lTime = g_time - snd.Time;

                    if (lTime < snd.Length + snd.ReleaseLength)
                    {
                        var instVol = snd.Source.Instrument.DisplayVolume;
                        if (!OK(instVol)) instVol = 0;

                        var playVol =
                               OK(PlayClip)
                            && OK(Clips[PlayClip])
                            ?   instVol
                              * snd.Channel.Volume
                              * Clips[PlayClip].Volume
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


        void SetAllTrackClips(int col)
        {
            foreach (var track in Tracks)
            {
                if (OK(track.Clips[col])) track.SetClip(col);
                else                      track.NextClip = -1;
            }
        }
    }
}
