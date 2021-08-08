using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        public class Key
        {
            public Parameter Parameter;
            public int       SourceIndex;

            public string    Path => Parameter.Path;

            public float     Value,
                             Step;

            public Channel   Channel;


            public Key(int srcIndex, Parameter param, float val, float step, Channel chan = Channel_null)
            {
                SourceIndex = srcIndex;
                Parameter   = param;
                Value       = val;
                Step        = step;
                Channel     = chan;
            }


            public Key(Key key)
            {
                SourceIndex = key.SourceIndex;
                Parameter   = key.Parameter;
                Value       = key.Value;
                Step        = key.Step;
                Channel     = key.Channel;
            }


            public string Save()
            {
                return
                      WS(SourceIndex)
                    + W (Path)
                    + WS(Value)
                    +  S(Step);
            }


            public static Key Load(string[] data, ref int i)
            {
                var srcIndex = int_Parse(data[i++]);
                var path     = data[i++];
                var value    = float.Parse(data[i++]);
                var stepTime = float.Parse(data[i++]);

                return new Key(
                    srcIndex,
                    (Parameter)GetSettingFromPath(path),
                    value,
                    stepTime);
            }
        }


        static Key PrevClipAutoKey(Clip clip, float clipStep, int ch, string path)
        {
            var prevKeys = clip.ChannelAutoKeys[ch]
                .Where(k => 
                       (   path == ""
                        || path == k.Path)
                    && k.Step < clipStep - clip.Track.StartStep)
                .ToList();
            
            return 
                prevKeys.Count > 0
                ? prevKeys.Last()
                : Key_null;
        }


        static Key NextClipAutoKey(Clip clip, float clipStep, int ch, string path, bool forDisplay = False)
        {
            var nextKeys = clip.ChannelAutoKeys[ch]
                .Where(k => 
                       (   path == ""
                        || path == k.Path)
                    && (    forDisplay && k.Step >  clipStep
                        || !forDisplay && k.Step >= clipStep - clip.Track.StartStep))
                .ToList();

            return
                nextKeys.Count > 0
                ? nextKeys[0]
                : Key_null;
        }
    }
}
