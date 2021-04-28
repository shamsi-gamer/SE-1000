namespace IngameScript
{
    partial class Program
    {
        static bool g_session = true;
        static bool g_setClip = false;

        
        void Clips()
        {
            if (g_session) g_setClip = true;
            else           g_session = true;   

            UpdateClipsLight();
            MarkLight(lblClips, g_setClip);
        }
    }
}
