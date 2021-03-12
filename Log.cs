using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        static List<string> g_log     = new List<string>();
        static List<long>   g_logTime = new List<long>();

        static void Log(string str)
        {
            g_log.Add(str);
            g_logTime.Add(g_time);

            pnlInfoLog.CustomData += g_time.ToString() + ": " + str + "\n";
        }
    }
}
