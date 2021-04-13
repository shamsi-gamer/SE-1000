using System;


namespace IngameScript
{
    partial class Program
    {
        //static float sqr(float x) { return x*x; }


        //static float logb(double _base, double value)
        //{
        //    return (float)(Math.Log(value) / Math.Log(_base));
        //}

        //Vector2 vector2(float angle, float dist)
        //{
        //    return new Vector2(
        //        dist * (float)Math.Cos(angle),
        //        dist * (float)Math.Sin(angle));
        //}


        static string printNoZero(double d, int dec)
        {
            return d.ToString(
                  (Math.Abs(d) < 1 ? "" : "0") 
                + "." 
                + new string('0', dec));
        }


        static string printValue(double val, int dec, bool showZero, int pad)
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
                    printNoZero(val, Math.Abs(dec))
                    .PadLeft(pad + Math.Abs(dec) + (dec != 0 ? 1 : 0));
            }

            return str;
        }


        //static void Swap<T>(ref T t1, ref T t2)
        //{
        //    var temp = t1;
        //    t1 = t2;
        //    t2 = temp;
        //}


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


        static bool OK(float f) { return !float.IsNaN(f); }


        static int    MinMax(int    min, int    val, int    max) { return Math.Min(Math.Max(min, val), max); }
        static float  MinMax(float  min, float  val, float  max) { return Math.Min(Math.Max(min, val), max); }
        //static double MinMax(double min, double val, double max) { return Math.Min(Math.Max(min, val), max); }


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


        bool ReadBytes(uint f, int b) { return (f & (1 << b)) != 0; }
        void WriteByte(ref uint f, bool c, int b) { if (c) f |= (uint)(1 << b); }


        static bool fequal(float a, float b)
        {
            return Math.Abs(a - b) < 0.000001;
        }
    }
}
