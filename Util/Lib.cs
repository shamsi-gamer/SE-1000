using System;


namespace IngameScript
{
    partial class Program
    {
        static float Nozero(float x) { return x != 0 ? x : 0.000001f; }


        static string PrintNoZero(double d, int dec)
        {
            return d.ToString(
                  (Math.Abs(d) < 1 ? "" : "0") 
                + "." 
                + new string('0', dec));
        }


        static string PrintValue(double val, int dec, bool showZero, int pad)
        {
            string str;

                 if (double.IsNegativeInfinity(val)) str = "-∞";
            else if (double.IsPositiveInfinity(val)) str =  "∞";
            else if (double.IsNaN(val))              str = "NaN";
            else if (showZero)
            {
                string format = 
                    (showZero ? "0" : "") 
                    + "." 
                    + new string(dec >= 0 ? '0' : '#', Math.Abs(dec));

                str = val
                    .ToString(format)
                    .PadLeft(pad + Math.Abs(dec) + (dec != 0 ? 1 : 0));
            }
            else
            {
                str =
                    PrintNoZero(val, Math.Abs(dec))
                    .PadLeft(pad + Math.Abs(dec) + (dec != 0 ? 1 : 0));
            }

            return str;
        }


        int GetNumLength(string name)
        {
            var numLength = 0;

            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (IsDigit(name[i])) numLength++;
                else break;
            }

            return numLength;
        }

        static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }


        static bool OK(int    i) { return i > -1; }
        static bool NO(int    i) { return i <  0; }
                              
        static bool OK(float  f) { return !float.IsNaN(f); }
        static bool NO(float  f) { return !OK(f); }

        static bool OK(object o) { return o != null; }
        static bool NO(object o) { return !OK(o); }

        static int   MinMax(int   min, int   val, int   max) { return Math.Min(Math.Max(min, val), max); }
        static float MinMax(float min, float val, float max) { return Math.Min(Math.Max(min, val), max); }


        static int GetInt(string str, string pre, string suf = "")
        {
            if (   str.Length > pre.Length + suf.Length
                && str.Substring(0, pre.Length) == pre
                && str.Substring(str.Length - suf.Length) == suf)
            {
                int val;
                if (int.TryParse(str.Substring(pre.Length, str.Length - pre.Length - suf.Length), out val))
                    return val;
            }

            return -1;
        }


        static bool ReadBit(uint f, int b) { return (f & (1 << b)) != 0; }
        static void WriteBit(ref uint f, bool c, int b) { if (c) f |= (uint)(1 << b); }


        static bool fequal(float a, float b)
        {
            return Math.Abs(a - b) < 0.000001;
        }
    }
}
