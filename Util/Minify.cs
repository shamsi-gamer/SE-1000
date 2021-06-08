using System;
using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        const bool  True  = true,
                    False = false;

        const float float_NaN = float.NaN;


        const TextAlignment TA_CENTER = TextAlignment.CENTER;


        const Track           Track_null = null;
        const Clip             Clip_null = null;
        const Pattern       Pattern_null = null;
        const Channel       Channel_null = null;
        const Note             Note_null = null;
        const Parameter   Parameter_null = null;
        const Envelope     Envelope_null = null;
        const LFO               LFO_null = null;
        const Instrument Instrument_null = null;
        const Source         Source_null = null;
        const Setting       Setting_null = null;
        const Tune             Tune_null = null;
        const Harmonics   Harmonics_null = null;
        const Filter         Filter_null = null;
        const Delay           Delay_null = null;
        const Modulate     Modulate_null = null;
        const Key               Key_null = null;
        const Sound           Sound_null = null;
        const Oscillator Oscillator_null = null;
                           
        const Label.CondFunc     CF_null = null;
        const Action<Label>      AL_null = null;


        static string W(string str, bool semi = True) { return str + (semi ? ";" : ""); }
        static string P(string str)                { return ";" + str; }

        static string S<T> (T      val) { return val.ToString();       }
        static string S0   (double val) { return val.ToString("0");    }
        static string S00  (double val) { return val.ToString("0.00"); }
        static string S_00 (double val) { return val.ToString(".00");  }
        static string S_000(double val) { return val.ToString(".000"); }
        static string B    (bool   b  ) { return b ? "1" : "0";        }
        
        static string WS<T>(T    val) { return W(S(val)); }
        static string PS<T>(T    val) { return P(S(val)); }
        static string WB   (bool b  ) { return W(B(b));   }

        static string  N(string s, bool newLine = True) { return s + (newLine ? "\n" : ""); }
        static string PN(string s)                      { return "\n" + s; }


        static int        CurPat        => EditedClip.CurPat;

        static int        CurChan       { get { return EditedClip.CurChan; } set { EditedClip.CurChan = value; } }
        static int        SelChan       { get { return EditedClip.SelChan; } set { EditedClip.SelChan = value; } }
        static int        CurSrc        { get { return EditedClip.CurSrc;  } set { EditedClip.CurSrc  = value; } }
        static int        CurSet        { get { return EditedClip.CurSet;  } set { EditedClip.CurSet  = value; } }

        static Pattern    CurPattern    => EditedClip.CurPattern;
        static Channel    CurChannel    => EditedClip.CurChannel;

        static Source     SelSource     => EditedClip.SelSource;
        static Instrument SelInstrument => EditedClip.SelInstrument;
        static Channel    SelChannel    => EditedClip.SelChannel;
    }
}
