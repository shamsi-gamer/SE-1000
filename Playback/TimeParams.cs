using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class TimeParams
        {
            public long               GlobalTime;
            public long               LocalTime;
            public long               SongTime;
                                      
            public Note               Note;
            public int                NoteLength;
                                      
            public int                SourceIndex;
            public List<TriggerValue> TriggerValues;

            public Program            Program;


            public TimeParams(long gTime, long lTime, long sTime, Program prog)
            {
                GlobalTime    = gTime;
                LocalTime     = lTime;
                SongTime      = sTime;
                              
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
