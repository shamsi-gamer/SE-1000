using VRage.Game.GUI.TextPanel;


namespace IngameScript
{
    partial class Program
    {
        const bool  T  = true,
                    F  = false;

        const float fN = float.NaN;

        const TextAlignment TaC = TextAlignment.CENTER;

        static string W(string str, bool semi = T) { return str + (semi ? ";" : ""); }
        static string P(string str)                   { return ";" + str; }

        static string  S<T> (T      val) { return val.ToString();       }
        static string  S0   (double val) { return val.ToString("0");    }
        static string  S00  (double val) { return val.ToString("0.00"); }
        static string  S_00 (double val) { return val.ToString(".00");  }
        static string  S_000(double val) { return val.ToString(".000"); }
        static string  B    (bool   b  ) { return b ? "1" : "0";        }
        
        static string WS<T>(T    val) { return W(S(val)); }
        static string PS<T>(T    val) { return P(S(val)); }
        static string WB   (bool b  ) { return W(B(b));   }

        static string  N(string s, bool newLine = T) { return s + (newLine ? "\n" : ""); }
        static string PN(string s)                      { return "\n" + s; }
    }
}
