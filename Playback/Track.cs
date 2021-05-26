using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        public class Track
        {
            public Session    Session;

            public Clip[]     Clips;


            public long       StartTime, // in ticks
                              PlayTime;

            public int        PlayClip,
                              NextClip,
                              
                              PlayPat, // this can't be a property because it must sometimes be separate from PlayTime, for queueing
                              NextPat;


            public bool       IsPlaying { get { return OK(PlayClip); } }

            public float      PlayStep { get 
                              {
                                  return
                                      IsPlaying 
                                      ? PlayTime / (float)Session.TicksPerStep 
                                      : fN; 
                              } }

            
            public float[]    DspVol;

            //public bool       NotesArePlaying;
            

            public Track(Session session)
            {
                Session   = session;
                          
                Clips = new Clip[g_nChans];
                for (int i = 0; i < g_nChans; i++)
                    Clips[i] = null;
                          
                StartTime = long_NaN;
                PlayTime  = long_NaN;
                          
                PlayClip  = -1;
                NextClip  = -1;
                          
                PlayPat   = -1;
                NextPat   = -1;
                          
                DspVol    = new float[g_nChans];

                //NotesArePlaying = F;
            }


            public Track(Track track)
            {
                Session   = track.Session;
                            
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

                if (   g_session.EditClip == 0
                    || force)
                {
                    if (!OK(clip))
                    {
                        clip = new Clip(this);
                        clip.Patterns.Add(new Pattern(Session.Instruments[0], clip));

                        Clips[index] = clip;
                    }
                    
                    if (clip != Session.EditedClip) Session.EditedClip = clip;
                    else                            g_session.ShowSession = F;
                }

                else if (OK(g_session.ClipCopy))
                {
                    Clips[index] = new Clip(g_session.ClipCopy, this);
                    g_session.ClipCopy = null;
                }

                else if (OK(clip))
                { 
                    if (g_session.EditClip == 1)
                    {
                        g_session.ClipCopy = clip;
                        g_session.EditClip = -1;
                    }

                    else if (g_session.EditClip == 2)
                    {
                        if (clip == Session.EditedClip)
                            Session.EditedClip = Session.GetClipAfterDelete(clip);

                        if (Session.Tracks.Sum(t => t.Clips.Count(c => OK(c))) > 1) // check that this isn't the only clip
                            Clips[index] = null;

                        g_session.EditClip = -1;
                    }

                    else if (index != PlayClip)
                        NextClip = !OK(NextClip) ? index : -1;
                    
                    else if (OK(PlayClip)
                         && !OK(NextClip))
                        PlayClip = -1; // force mute on second press
                    
                    else
                        NextClip = -1;
                }
            }


            public void CueNextPattern()
            {
                if (   !OK(PlayClip)
                    && !OK(NextClip))
                    return;


                if (  !OK(PlayPat)
                    || PlayStep < (PlayPat + 1) * g_patSteps)
                    return;


                if (NextClip != PlayClip)
                { 
                    NextPat  = 0;
                    PlayClip = NextClip;
                }


                if (!OK(PlayClip))
                {
                    PlayPat = -1;
                    return;
                }


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

                    PlayTime  -= (end - start) * g_session.TicksPerStep;
                    StartTime += (end - start) * g_session.TicksPerStep;
                }


                PlayPat = (int)(PlayStep / g_patSteps);
            }


            public void FinalizePlayback()
            {
                //var pat = clip.Patterns[clip.PlayPat];

                //for (int ch = 0; ch < nChans; ch++)
                //{
                //    var chan = pat.Channels[ch];

                //    var arpNotes = chan.Notes.FindAll(n =>
                //               OK(n.Instrument.Arpeggio)
                //            && (int)(clip.PlayStep * g_session.TicksPerStep) >= (int)((clip.PlayPat * nSteps + n.StepTime               ) * g_session.TicksPerStep)
                //            && (int)(clip.PlayStep * g_session.TicksPerStep) <  (int)((clip.PlayPat * nSteps + n.StepTime + n.StepLength) * g_session.TicksPerStep));

                //    var noteLen = (int)(EditLength * g_session.TicksPerStep);

                //    foreach (var n in arpNotes)
                //    {
                //        var arp = n.Instrument.Arpeggio;

                //        n.FramePlayTime += arp.Scale .UpdateValue(g_time, 0, clip.StartTime, noteLen, n, -1);
                //        var maxLength    = arp.Length.UpdateValue(g_time, 0, clip.StartTime, noteLen, n, -1);

                //        while (n.FramePlayTime >= maxLength * g_session.TicksPerStep)
                //            n.FramePlayTime -= maxLength * g_session.TicksPerStep;
                //    }
                //}


                if (IsPlaying)
                    PlayTime++;
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

                foreach (var clip in Clips)
                    save += PN(clip.Save());

                return save;
            }


            public static Track Load(Session session, string[] lines, ref int line)//, out string curPath)
            {
                var track = new Track(session);

                var cfg = lines[line++].Split(';');
                var c = 0;

                if (!long_TryParse(cfg[c++], out track.PlayTime )) return null;
                if (!int .TryParse(cfg[c++], out track.PlayPat  )) return null;
                if (!int .TryParse(cfg[c++], out track.NextPat  )) return null;
                if (!int .TryParse(cfg[c++], out track.NextClip )) return null;

                var _indices = lines[line++].Split(';');

                int nClips;
                if (!int .TryParse(_indices[0], out nClips)) return null;

                var indices = new List<int>();
                for (int i = 0; i < nClips; i++)
                    indices.Add(int.Parse(_indices[i+1]));
                 
                //curPath = "";

                for (int i = 0; i < nClips; i++)
                {
                    var clip = Clip.Load(session, lines, ref line);

                    if (OK(clip)) track.Clips[indices[i]] = clip;//, out curPath));
                    else          return null;
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
