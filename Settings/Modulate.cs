using System;


namespace IngameScript
{
    partial class Program
    {
    //    public class Modulate : Setting
    //    {
    //        public Parameter Amount;
    //        public Parameter Delay;


    //        public Modulate(float val = 0) : base("Modulate", "Mod")
    //        {
    //            Amount = new Parameter("Amount", "Amt", -10, 10, -1, 1, 0);
    //            Amount.Parent = this;

    //            Delay = new Parameter("Delay", "Del", -10, 10, -1, 1, 0);
    //            Amount.Parent = this;
    //        }


    //        public Modulate(Modulate mod) : base(mod.Name, mod.Tag, mod.Prototype)
    //        {
    //            m_value = mod.m_value;
    //        }


    //        public void MakeValid()
    //        {
    //            Prototype = this;
    //        }


    //        public float Value
    //        { 
    //            get { return m_value;  }
    //            set { m_value = value; }
    //        }


    //        public float GetValue(long gTime, long lTime, long sTime, int noteLen, Note note, int src)
    //        {
    //            return m_value;
    //        }


    //        public override bool HasDeepParams(Channel chan, int src)
    //        {
    //            return
    //                   Envelope != null
    //                || Lfo      != null
    //                || (chan?.HasKeys(GetPath(src)) ?? false)
    //                || _IsCurrent;
    //        }


    //        public override void Remove(Setting setting)
    //        {
    //                 if (setting == Envelope) Envelope = null;
    //            else if (setting == Lfo)      Lfo      = null;
    //        }
    //    }
    }
}
