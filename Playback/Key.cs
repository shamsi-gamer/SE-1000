namespace IngameScript
{
    partial class Program
    {
        public class Key
        {
            public int       SourceIndex;

            public Parameter Parameter;
            public string    Path => Parameter.GetPath(SourceIndex);

            public float     Value,
                             StepTime;

            public Channel   Channel;


            public Key(int srcIndex, Parameter param, float val, float stepTime, Channel chan = null)
            {
                SourceIndex = srcIndex;
                Parameter   = param;
                Value       = val;
                StepTime    = stepTime;
                Channel     = chan;
            }


            public Key(Key key)
            {
                SourceIndex = key.SourceIndex;
                Parameter   = key.Parameter;
                Value       = key.Value;
                StepTime    = key.StepTime;
                Channel     = key.Channel;
            }


            public string Save()
            {
                return
                      WS(SourceIndex)
                    + W (Path)
                    + WS(Value)
                    +  S(StepTime);
            }


            public static Key Load(string[] data, ref int i, Instrument inst)
            {
                var srcIndex = int_Parse(data[i++]);
                var path     = data[i++];
                var value    = float.Parse(data[i++]);
                var stepTime = float.Parse(data[i++]);

                return new Key(
                    srcIndex,
                    (Parameter)GetSettingFromPath(inst, path),
                    value,
                    stepTime);
            }
        }
    }
}
