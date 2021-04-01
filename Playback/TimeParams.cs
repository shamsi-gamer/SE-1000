using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class TimeParams
        {
            public long               Global;
            public long               Local;
            public long               Song;
                                      
            public Note               Note;
            public int                NoteLength;
                                      
            public int                SourceIndex;
            public List<TriggerValue> TriggerValues;

            public Program            Program;


            public TimeParams(long gTime, long lTime, long sTime, Program prog)
            {
                Global        = gTime;
                Local         = lTime;
                Song          = sTime;
                              
                Note          = null;
                NoteLength    = 0;
                              
                SourceIndex   = -1;
                TriggerValues = null;

                Program       = prog;
            }


            public TimeParams(long gTime, long lTime, long sTime, Note note, int noteLen, int iSrc, List<TriggerValue> triggerValues, Program prog)
                : this(gTime, lTime, sTime, prog)
            {
                Note          = note;
                NoteLength    = noteLen;
                              
                SourceIndex   = iSrc;
                TriggerValues = triggerValues;
            }
        }
    }
}
