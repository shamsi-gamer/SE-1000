namespace IngameScript
{
    partial class Program
    {
        const bool  T  = true;
        const bool  F  = false;

        const float fN = float.NaN;

        static string S<T>(T val) { return val.ToString(); }
        static string W(string str, bool semi = true) { return str + (semi ? ";" : ""); }
        static string WS<T>(T val, bool semi = true) { return W(S(val)); }

        static string B(bool b) { return b ? "1" : "0"; }

        static string N(string s, bool newLine = true) { return s + (newLine ? "\n" : ""); }
    }
}
