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


            public Modulate(Setting parent) : base("Modulate", "Mod", parent)
            {
                Amount  = new Parameter("Amount",  "Att", -10, 10, -1, 1, 0.01f, 0.1f, 0,    this);
                Attack  = new Parameter("Attack",  "Att",   0, 10,  0, 1, 0.01f, 0.1f, 0,    this);
                Release = new Parameter("Release", "Rel",   0, 10,  0, 2, 0.01f, 0.1f, 0.2f, this);
            }


            public Modulate(Modulate mod, Setting parent) : base(mod.Name, mod.Tag, parent, mod.Prototype)
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
                      W(Amount .Save())
                    + W(Attack .Save())
                    +   Release.Save();
            }
        }
    }
}
