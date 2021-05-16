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

            public int        CurIndex;
            public int        NextIndex;

            public long       StartTime, // in ticks
                              PlayTime;

            public int        PlayPat; // this can't be a property because it must sometimes be separate from PlayTime, for queueing

            public float      PlayStep { get 
                              {
                                  return
                                      g_playing 
                                      ? PlayTime / (float)Session.TicksPerStep 
                                      : fN; 
                              } }

            
            public Track(Session session)
            {
                Session   = session;
                          
                Clips     = new List<Clip>();
                Indices   = new List<int> ();

                CurIndex  = 
                NextIndex = -1;

                PlayTime  =
                StartTime = long_NaN;
                          
                PlayPat   = -1;
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

                CurIndex  = track.CurIndex;
                NextIndex = track.NextIndex;

                PlayTime  = track.PlayTime;
                StartTime = track.StartTime;
                          
                PlayPat   = track.PlayPat;
            }


            public void Add(Clip clip, int index)
            {
                Clips  .Add(clip);
                Indices.Add(index);
            }


            public void CueNextPattern()
            {
                var clip = Clips[CurIndex];


                clip.Length = clip.Patterns.Count * g_patSteps;


                if (clip.CueNext > -1)
                {
                    var b = clip.GetBlock(PlayPat);

                    if (clip.Block && b != null)
                        PlayPat = b.Last;
                }


                if (PlayStep >= (PlayPat + 1) * g_patSteps)
                { 
                    int start, end;
                    clip.GetPosLimits(PlayPat, out start, out end);
                    end = start + Math.Min(end - start, clip.Length);

                    if (clip.CueNext > -1)
                    {
                        var b = clip.GetBlock(clip.CueNext);
                        if (clip.Block && b != null)
                            clip.CueNext = b.First;

                        PlayTime  = GetPatTime(clip.CueNext);
                        StartTime = g_time - PlayTime;

                        clip.CueNext = -1;
                    }
                    else if (PlayStep >= end)
                    {
                        clip.WrapCurrentNotes(end - start);

                        PlayTime  -= (end - start) * g_session.TicksPerStep;
                        StartTime += (end - start) * g_session.TicksPerStep;
                    }

                    if (NextIndex > -1)
                        CurIndex = NextIndex;
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
                //                n.Instrument.Arpeggio != null
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
                    + PS(NextIndex);

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
                if (!int .TryParse(cfg[c++], out track.NextIndex)) return null;

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

                    if (clip != null) track.Clips.Add(clip);//, out curPath));
                    else              return null;
                }

                return track;
            }
        }
    }
}
