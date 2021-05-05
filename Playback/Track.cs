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

            
            public Track(Session session)
            {
                Session = session;

                Clips   = new List<Clip>();
                Indices = new List<int> ();

                CurIndex = -1;
            }


            public void Add(Clip clip, int index)
            {
                Clips  .Add(clip);
                Indices.Add(index);
            }


            public string Save()
            {
                var save = S(Clips.Count);

                foreach (var i in Indices)
                    save += PS(i);

                foreach (var clip in Clips)
                    save += PN(clip.Save());

                return save;
            }


            public static Track Load(Session session, string[] lines, ref int line)//, out string curPath)
            {
                var track = new Track(session);

                var indices = lines[line++].Split(';');

                int nClips;
                if (!int.TryParse(indices[0], out nClips)) return null;

                //curPath = "";

                for (int i = 0; i < nClips; i++)
                {
                    int index;
                    if (!int.TryParse(indices[i+1], out index)) return null;

                    track.Indices.Add(line);
                
                    var clip = Clip.Load(session, lines, ref line);

                    if (clip != null) track.Clips.Add(clip); //, out curPath));
                    else              return null;
                }

                return track;
            }
        }
    }
}
