using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        void AddSoundAndEchos(List<Sound> sounds, Sound snd, Delay del)
        {
            if (!OK(del)) return;

            var lTime = g_time - snd.Note.Time;
            var sTime = g_time - EditedClip.Track.StartTime;

            var tp = new TimeParams(g_time, lTime, sTime, snd.Note, snd.Length, snd.SourceIndex, snd.TriggerValues, this);

            var dc = del.Count.UpdateValue(tp);
            var dt = del.Time .UpdateValue(tp);


            Sound echoSrc = Sound_null;

            for (int i = 0; i < dc; i++)
            {
                if (TooComplex) return;
                
                var echoVol = del.GetVolume(i, tp);

                var echo = new Sound(snd, i > 0, echoSrc, echoVol);
                if (i == 0) echoSrc = echo;

                echo.Time += (int)(i*dt * FPS);
                sounds.Add(echo);
            }
        }


        void UpdateSounds()
        {
            for (int i = 0; i < g_sounds.Count; i++)
            {
                var snd   = g_sounds[i];
                var lTime = g_time - snd.Time;

                if (   lTime >= 0
                    && lTime < snd.Length + snd.ReleaseLength)
                    snd.Update(this);
            }
        }


        List<int> StopSounds()
        {
            var delete = new List<int>();

            for (int i = 0; i < g_sounds.Count; i++)
            {
                var snd = g_sounds[i];

                if (g_time > snd.Time + snd.Length + snd.ReleaseLength)
                {
                    snd.Stop();
                    delete.Add(i);
                }
            }

            return delete;
        }


        void DeleteSounds(List<int> delete)
        {
            for (int i = delete.Count - 1; i >= 0; i--)
            { 
                var snd = g_sounds[delete[i]];

                snd.Note.Sounds.Remove(snd);

                if (snd.Note.Sounds.Count == 0)
                    g_notes.Remove(snd.Note);

                g_sounds.RemoveAt(delete[i]);
            }
        }
    }
}
