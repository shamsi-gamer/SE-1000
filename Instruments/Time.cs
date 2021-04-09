using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Time
        {
            public float Value,
                         Delta;

            public Queue<float> Cache;

                  int m_count;
            const int MaxCount = 4;

 
            public Time(float value, float delta = 1f/FPS)
            {
                Value = value;
                Delta = delta;

                Cache = new Queue<float>();
                for (int i = 0; i <= FPS; i++)
                    Cache.Enqueue(0);

                m_count = 0;
            }


            public void Move()
            {
                if (++m_count < MaxCount) 
                    return;

                Value += Delta;

                Cache.Dequeue();
                Cache.Enqueue(Value);

                m_count = 0;
            }
        }
    }
}
