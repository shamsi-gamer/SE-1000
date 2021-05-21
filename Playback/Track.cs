using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Track
        {
            public Session    Session;

            public List<Clip> Clips;
            public List<int>  Indices;


            public long       StartTime, // in ticks
                              PlayTime;

            public int        PlayClip,
                              NextClip,
                              
                              PlayPat, // this can't be a property because it must sometimes be separate from PlayTime, for queueing
                              NextPat;


            public float      PlayStep { get 
                              {
                                  return
                                      g_playing 
                                      ? PlayTime / (float)Session.TicksPerStep 
                                      : fN; 
                              } }

            
            public float[]    DspVol;
                                         

            public Track(Session session)
            {
                Session   = session;
                          
                Clips     = new List<Clip>();
                Indices   = new List<int> ();

                StartTime = 
                PlayTime  = long_NaN;

                PlayClip  = 
                NextClip  =

                PlayPat   =
                NextPat   = -1;

                DspVol    = new float[g_nChans];
            }


            public Track(Track track)
            {
                Session   = track.Session;
                          
                Clips = new List<Clip>(); 

                foreach (var clip in track.Clips)
                {
                    Clips.Add(clip);
                    clip.Track = this;
                }

                Indices = new List<int>();

                foreach (var i in track.Indices)
                    Indices.Add(i);

                StartTime = track.StartTime;
                PlayTime  = track.PlayTime;
                          
                PlayClip  = track.PlayClip;
                NextClip  = track.NextClip;

                PlayPat   = track.PlayPat;
                NextPat   = track.NextPat;

                DspVol    = new float[g_nChans];
            }


            public void Add(Clip clip, int index)
            {
                Clips  .Add(clip);
                Indices.Add(index);
            }


            public void SetClip(int index)
            { 
                var found = Indices.FindIndex(i => i == index);

                if (g_editClip > 0)
                {
                    if (found < 0)
                    {
                        Session.CurClip = new Clip(this);

                        CurClip.Patterns.Add(new Pattern(Session.Instruments[0], CurClip));
                        CurClip.Name = "Clip";
                        
                        Clips.Add(CurClip);
                        Indices.Add(index);

                        //if (   g_playing
                        //    && PlayTime == long_NaN)
                        //    PlayTime = 0;
                    }

                    NextClip = index;

                    Session.CurClip = Clips[index];

                    if (g_editClip == 2)
                        g_showSession = F;

                    //g_editClip = 0;
                }
                else
                {
                    if (   PlayClip != index
                        && OK(found))
                        NextClip = index;
                }
            }


            //void SetClip(Clip clip)
            //{ 
            //    clip.Track.CurIndex = clip.Track.Clips.IndexOf(clip);

            //    CurClip = clip;

            //    g_setClip = F;
            //    g_showSession = F;

            //    //UpdateLabels();
            //}


            public void CueNextPattern()
            {
                var trackIndex = Session.Tracks.IndexOf(this);


                var clip = Clips[PlayClip > -1 ? PlayClip : NextClip];
                clip.Length = clip.Patterns.Count * g_patSteps;


                if (NextPat > -1)
                {
                    var b = clip.GetBlock(PlayPat);

                    if (clip.Block && OK(b))
                        PlayPat = b.Last;
                }


                if (PlayTime == long_NaN)
                    PlayTime = 0;

                if (PlayStep >= (PlayPat + 1) * g_patSteps)
                {
                    if (NextClip > -1)
                    {
                        PlayClip = NextClip;
                        NextClip = -1;
                    }

                    clip = Clips[PlayClip];


                    int start, end;
                    clip.GetPosLimits(PlayPat, out start, out end);
                    end = start + Math.Min(end - start, clip.Length);

                    if (NextPat > -1)
                    {
                        var b = clip.GetBlock(NextPat);
                        if (clip.Block && OK(b))
                            NextPat = b.First;

                        PlayTime = GetPatTime(NextPat);
                        StartTime = g_time - PlayTime;

                        NextPat = -1;
                    }
                    else if (PlayStep >= end)
                    {
                        clip.WrapCurrentNotes(end - start);

                        PlayTime  -= (end - start) * g_session.TicksPerStep;
                        StartTime += (end - start) * g_session.TicksPerStep;
                    }
                }


                PlayPat =
                    g_playing
                    ? (int)(PlayStep / g_patSteps)
                    : -1;
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


                if (g_playing)
                    PlayTime++;
            }


            public string Save()
            {
                var cfg = 
                        (PlayTime == long_NaN ? "?" : S(PlayTime))
                    + PS(PlayPat)
                    + PS(NextPat)
                    + PS(NextClip);

                var indices = S(Clips.Count);

                foreach (var i in Indices)
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

                var indices = lines[line++].Split(';');

                int nClips;
                if (!int .TryParse(indices[0], out nClips)) return null;

                //curPath = "";

                for (int i = 0; i < nClips; i++)
                {
                    int index;
                    if (!int.TryParse(indices[i+1], out index)) return null;

                    track.Indices.Add(line);
                
                    var clip = Clip.Load(session, lines, ref line);

                    if (OK(clip)) track.Clips.Add(clip);//, out curPath));
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
                        if (NO(instVol)) instVol = 0;

                        DspVol[snd.iChan] = Math.Max(
                            DspVol[snd.iChan],
                              instVol
                            * snd.Channel.Volume
                            * CurClip.Volume);
                    }
                }
            }


            public void DampenDisplayVolumes(float f = 0.6f)
            {
                for (int i = 0; i < DspVol.Length; i++)
                    DspVol[i] *= f;
            }


            public void ResetDisplayVolumes()
            {
                for (int i = 0; i < DspVol.Length; i++)
                    DspVol[i] = 0;
            }
        }
    }
}
