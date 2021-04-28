using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Track
        {
            public List<Clip> Clips;
            public List<int>  Indices;

            public int        CurIndex;

            
            public Track()
            {
                Clips   = new List<Clip>();
                Indices = new List<int> ();

                CurIndex = -1;
            }


            public string Save()
            {
                var save = S(Clips.Count);

                foreach (var i in Indices)
                    save += PS(i);

                foreach (var clip in Clips)
                    save += PN(g_clip.Save());

                return save;
            }


            public static Track Load(string[] lines, ref int line, out string curPath)
            {
                var track = new Track();

                var indices = lines[line++].Split(';');
                var nClips  = int.Parse(indices[0]);

                curPath = "";

                for (int i = 0; i < nClips; i++)
                {
                    track.Clips.Add(Clip.Load(
                        lines, 
                        ref line, 
                        out curPath));

                    track.Indices.Add(int.Parse(indices[i+1]));
                }

                return track;
            }
        }
    }
}
