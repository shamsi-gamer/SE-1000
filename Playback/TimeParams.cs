using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class TimeParams
        {
            public long               GlobalTime;
            public long               LocalTime;
                                      
            public Note               Note;
            public int                NoteLength;
                                      
            public int                SourceIndex;
            public List<TriggerValue> TriggerValues;

            public Clip               Clip;
            public Program            Program;


            public TimeParams(long gTime, long lTime, Clip clip, Program prog)
            {
                GlobalTime    = gTime;
                LocalTime     = lTime;
                              
                Note          = Note_null;
                NoteLength    = 0;
                              
                SourceIndex   = -1;
                TriggerValues = null;

                Clip          = clip;
                Program       = prog;
            }


            public TimeParams(TimeParams tp)
            {
                GlobalTime    = tp.GlobalTime;
                LocalTime     = tp.LocalTime;
                              
                Note          = tp.Note;
                NoteLength    = tp.NoteLength;
                              
                SourceIndex   = tp.SourceIndex;

                if (OK(tp.TriggerValues))
                {
                    TriggerValues = new List<TriggerValue>();
                    foreach (var tv in tp.TriggerValues)
                        TriggerValues.Add(new TriggerValue(tv));
                }
                else
                    TriggerValues = null;

                Clip          = tp.Clip;
                Program       = tp.Program;
            }


            public TimeParams(long gTime, long lTime, Note note, int noteLen, int iSrc, List<TriggerValue> triggerValues, Clip clip, Program prog)
                : this(gTime, lTime, clip, prog)
            {
                Note          = note;
                NoteLength    = noteLen;
                              
                SourceIndex   = iSrc;
                TriggerValues = triggerValues;
            }


            public float GetTriggerValue(Parameter param)
            {
                var path    = param.GetPath(SourceIndex);
                var trigVal = TriggerValues.Find(v => v.Path == path);

                if (!OK(trigVal))
                {
                    trigVal = new TriggerValue(path, param.UpdateValue(this));
                    TriggerValues.Add(trigVal);
                }

                return trigVal.Value;
            }
        }
    }
}
