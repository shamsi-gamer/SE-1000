namespace IngameScript
{
    partial class Program
    {
        const float fN = float.NaN;

        static string W(string str, bool semi = true) { return str + (semi ? ";" : ""); }

        static string  S<T>(T val) { return val.ToString(); }
        static string WS<T>(T val) { return W(S(val));      }

        static string  B(bool b) { return b ? "1" : "0"; }
        static string WB(bool b) { return W(B(b));       }

        static string N(string s, bool newLine = true) { return s + (newLine ? "\n" : ""); }
    }
}
