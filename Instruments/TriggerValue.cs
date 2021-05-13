namespace IngameScript
{
    partial class Program
    {
        public class TriggerValue
        {
            public string Path;
            public float  Value;


            public TriggerValue(string path, float value)
            {
                Path  = path;
                Value = value;
            }


            public TriggerValue(TriggerValue val)
            {
                Path  = val.Path;
                Value = val.Value;
            }


            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(TriggerValue))
                    return false;

                var trig = (TriggerValue)obj;

                return Path  == trig.Path;
            }


            public override int GetHashCode()
            {
                return Path.GetHashCode() * 17;
            }
        }
    }
}
