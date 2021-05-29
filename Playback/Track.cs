﻿using System;
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

            //public bool  NotesArePlaying;
            

            public Track()
            {
                Clips = new Clip[g_nChans];
                for (int i = 0; i < g_nChans; i++)
                    Clips[i] = Clip_null;
                          

                StartTime = 
                PlayTime  = long_NaN;
                          
                PlayClip  = 
                NextClip  = 
                          
                PlayPat   = 
                NextPat   = -1;
                          
                DspVol    = new float[g_nChans];

                //NotesArePlaying = F;
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

                //NotesArePlaying = track.NotesArePlaying;
            }


            public void SetClip(int index, bool force = false)
            { 
                var clip = Clips[index];

                     if (EditClip == 0 || force) SetClip(clip, index);
                else if (OK(ClipCopy))           PlaceClip(index);

                else if (OK(clip))
                { 
                         if (EditClip == 1                           // move clip
                          || EditClip == 2) ClipCopy = clip;         // duplicate clip
                    else if (EditClip == 3) DeleteClip(clip, index); // delete clip

                    else if (index != PlayClip) // queue next clip
                    { 
                        if (OK(NextClip))
                            NextClip =
                                index != NextClip
                                ? index
                                : -1;
                        else
                            NextClip = index;
                    }
                    
                    else if (OK(PlayClip)
                         && !OK(NextClip))
                        PlayClip = -1; // force mute on second press
                    
                    else
                        NextClip = -1; // queue clip off
                }
            }


            void SetClip(Clip clip, int index)
            {
                if (!OK(clip))
                {
                    clip = Clip.Create(this);
                    Clips[index] = clip;
                    EditClip = -1;
                }
                    
                if (clip != EditedClip) 
                {
                    EditedClip = clip;
                    UpdateClipDisplay();
                }
                else
                    ShowSession = False;
            }


            void PlaceClip(int index)
            {
                     if (EditClip == 0) Clips[index] = Clip.Create(this); // create clip
                else if (EditClip == 1
                      || EditClip == 2) MoveClip(index);

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
                { 
                    var swap = Clips[index];
                    Clips[index] = srcTrack.Clips[srcIndex];
                    srcTrack.Clips[srcIndex] = swap;
                }
                else // duplicate
                    Clips[index] = new Clip(srcTrack.Clips[srcIndex], this);


                // update the clips' tracks in case they were
                // swapped between different tracks
                Clips[index].Track = this;

                if (OK(srcTrack.Clips[srcIndex]))
                    srcTrack.Clips[srcIndex].Track = srcTrack;


                if (srcTrack.PlayClip == srcIndex) // moved clip is playing
                { 
                    PlayTime  = srcTrack.PlayTime;
                    StartTime = srcTrack.StartTime;

                    PlayClip  = index;
                    NextClip  = index;
                    PlayPat   = srcTrack.PlayPat;
                    NextPat   = srcTrack.NextPat;

                    if (srcTrack != this)
                    { 
                        srcTrack.PlayTime  = long_NaN;
                        srcTrack.StartTime = long_NaN;

                        srcTrack.PlayClip  = -1;
                        srcTrack.NextClip  = -1;
                        srcTrack.PlayPat   = -1;
                        srcTrack.NextPat   = -1;
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
                { 
                    PlayClip = -1;
                    NextClip = -1;

                    PlayPat  = -1;
                    NextPat  = -1;
                }

                EditClip = -1;
            }


            public bool CueNextPattern()
            {
                if (   !OK(PlayClip)
                    && !OK(NextClip))
                    return False;


                if (      !OK(PlayPat)
                       && !OK(NextPat)
                    || PlayStep < (PlayPat + 1) * g_patSteps)
                    return False;


                if (NextClip != PlayClip)
                { 
                    NextPat  = 0;
                    PlayClip = NextClip;
                }


                if (!OK(PlayClip))
                {
                    PlayPat = -1;
                    NextPat = -1;
                    return False;
                }


                //if (PlayStep >= Clips[PlayClip].Patterns.Count * g_patSteps)
                //{
                    var clip = Clips[PlayClip];

                    clip.Length = clip.Patterns.Count * g_patSteps;

                    if (NextPat > -1)
                    {
                        var b = clip.GetBlock(PlayPat);

                        if (clip.Block && OK(b))
                            PlayPat = b.Last;
                    }


                    int start, end;
                    clip.GetPosLimits(PlayPat, out start, out end);
                    end = start + Math.Min(end - start, clip.Length);

                    if (NextPat > -1)
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
                //}


                PlayPat = (int)(PlayStep / g_patSteps);

                return True;
            }


            public string Save()
            {
                var cfg = 
                     (PlayTime == long_NaN ? "?" : S(PlayTime))
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

                    var snd   = g_sounds[i];
                    var lTime = g_time - snd.Time;

                    if (lTime < snd.Length + snd.ReleaseLength)
                    {
                        var instVol = snd.Source.Instrument.DisplayVolume;
                        if (!OK(instVol)) instVol = 0;

                        DspVol[snd.iChan] = Math.Max(
                            DspVol[snd.iChan],
                              instVol
                            * snd.Channel.Volume
                            * EditedClip.Volume);
                    }
                }
            }


            public void DampenDisplayVolumes()
            {
                for (int i = 0; i < DspVol.Length; i++)
                    DspVol[i] *= 0.6f;
            }


            public void ResetDisplayVolumes()
            {
                for (int i = 0; i < DspVol.Length; i++)
                    DspVol[i] = 0;
            }


            //public void UpdateNotesArePlaying()
            //{
            //    if (OK(g_notes.Find(n => n.Channel.Pattern.Clip.Track == this)))
            //        NotesArePlaying = T;
            //}
        }
    }
}
