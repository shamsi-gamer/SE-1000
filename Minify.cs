namespace IngameScript
{
    partial class Program
    {
        const bool  T  = true;
        const bool  F  = false;

        const float fN = float.NaN;

        static string S<T>(T val) { return val.ToString(); }
        static string B(bool b) {  return b ? "1" : "0"; }
        static string W<T>(T val, bool semi = true) { return S(val) + (semi ? ";" : ""); }

        static string N(string s, bool newLine = true) { return s + (newLine ? "\n" : ""); }
    }
}
