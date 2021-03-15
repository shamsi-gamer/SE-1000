using System;


namespace IngameScript
{
    partial class Program
    {
        public class Modulate : Setting
        {
            public Parameter Amount,
                             Attack,
                             Release;


            public Modulate(Setting parent) : base("Mod", parent)
            {
                Amount  = NewParamFromTag("Amt", this);
                Attack  = NewParamFromTag("Att", this);
                Release = NewParamFromTag("Rel", this);
            }


            public Modulate(Modulate mod, Setting parent) : base(mod.Tag, parent, mod.Prototype)
            {
                Amount  = new Parameter(mod.Amount,  this);
                Attack  = new Parameter(mod.Attack,  this);
                Release = new Parameter(mod.Release, this);
            }


            public Modulate Copy(Setting parent) 
            {
                return new Modulate(this, parent);
            }


            public void MakeValid()
            {
                Prototype = this;
            }


            public float GetValue(long gTime, long lTime, long sTime, int noteLen, Note note, int src)
            {
                return 0;
            }


            public override bool HasDeepParams(Channel chan, int src)
            {
                return F;
            }


            public override string Save()
            {
                return
                      W(Tag)
                    + W(Amount .Save())
                    + W(Attack .Save())
                    +   Release.Save();
            }


            public static Modulate Load(string[] data, ref int i, Setting parent)
            {
                var tag = data[i++];

                var mod = new Modulate(parent);

                mod.Amount  = Parameter.Load(data, ref i, mod);
                mod.Attack  = Parameter.Load(data, ref i, mod);
                mod.Release = Parameter.Load(data, ref i, mod);

                return mod;
            }
        }
    }
}