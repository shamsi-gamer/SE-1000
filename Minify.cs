namespace IngameScript
{
    partial class Program
    {
        static string S<T>(T val) { return val.ToString(); }
        static string W<T>(T val, bool semi = true) { return S(val) + (semi ? ";" : ""); }
    }
}
