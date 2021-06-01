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

            //public bool  NotesArePlaying;
            

            public Track()
            {
                Clips = new Clip[g_nChans];
                for (int i = 0; i < g_nChans; i++)
                    Clips[i] = Clip_null;
                          

                Stop();
                          
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

                     if (OK(ClipCopy))          PlaceClip(index);

                else if (OK(clip))
                { 
                         if (EditClip == 0  // move clip
                          || EditClip == 1) // duplicate clip    
                        ClipCopy = clip; 
                    
                    else if (EditClip == 2)     
                        DeleteClip(clip, index); 
                                                 
                    else if (PlayClip != index)
                        CueNextClip(index);
                    
                    else if (OK(PlayClip)
                         && !OK(NextClip)
                         &&  CueClip)
                    { 
                        NextClip = PlayClip; // cancel cue
                        CueNextClip(index);
                    }

                    else
                    { 
                        NextClip = -1; // cue clip off
                        
                        if (!CueClip)
                            Stop();
                    }
                }
                else
                { 
                    Clips[index] = Clip.Create(this);
                    CueNextClip(index);
                }


                if (OK(NextClip)) 
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
                     if (EditClip == 0
                      || EditClip == 1) MoveClip(index);

                EditedClip = Clips[index];
                UpdateClipDisplay();

                ClipCopy = Clip_null;
                EditClip = -1;
            }


            void MoveClip(int index)
            {
                var srcTrack = ClipCopy.Track;
                var srcIndex = srcTrack.Clips.IndexOf(ClipCopy);


                if (EditClip == 0) // move
                { 
                    var swap = Clips[index];
                    Clips[index] = srcTrack.Clips[srcIndex];
                    srcTrack.Clips[srcIndex] = swap;
                }
                else // duplicate
                { 
                    Clips[index] = new Clip(srcTrack.Clips[srcIndex], this);
                    UpdateClipName(Clips[index], Clips);
                }


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
                        Stop();
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

                    //if (   EditClip == 0
                    //    && OK(PlayClip)) 
                    //    SetClip(Clips[PlayClip], PlayClip);

                    //UpdateInstName();
                }

                if (!OK(PlayClip))
                {
                    PlayPat = -1;
                    NextPat = -1;
                    return False;
                }


                var clip = Clips[PlayClip];

                clip.Length = clip.Patterns.Count * g_patSteps;

                UpdateBlockPat(clip);
                UpdatePlayTime(clip);


                PlayPat = (int)(PlayStep / g_patSteps);

                return True;
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
