using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public class Channel
        {
            public Pattern    Pattern;

            public bool       On;

            public Instrument Instrument;

            public float      Volume;

            public int        Shuffle,
                              Transpose;

            public List<Note> Notes;
            public List<Key>  AutoKeys;


            public Channel(Pattern pat = null)
            {
                Pattern    = pat;

                On         = true;
                Instrument = null;
                Volume     = 1;
                Shuffle    = 0;
                Transpose  = 0;

                Notes      = new List<Note>();
                AutoKeys   = new List<Key>();
            }


            public Channel(Channel chan)
            {
                Pattern    = chan.Pattern;

                On         = chan.On;
                Instrument = chan.Instrument;
                Volume     = chan.Volume;
                Shuffle    = chan.Shuffle;
                Transpose  = chan.Transpose;


                Notes = new List<Note>();
                foreach (var n in chan.Notes)
                {
                    var note = new Note(n);
                    note.Channel = this;
                    Notes.Add(note);
                }


                AutoKeys = new List<Key>();
                foreach (var k in chan.AutoKeys)
                    AutoKeys.Add(k);
            }


            public void AddNote(Note note)
            {
                Notes.Add(note);

                foreach (var n in Notes)
                {
                    var ahead = Notes.FindIndex(_n =>
                           _n.Number == note.Number
                        && _n.PatStepTime > note.PatStepTime
                        && _n.PatStepTime < note.PatStepTime + note.StepLength);

                    if (   ahead >= 0
                        && ahead < Notes.Count)
                        note.StepLength = Notes[ahead].PatStepTime - note.PatStepTime;

                    var behind = Notes.FindIndex(_n =>
                           note.Number == _n.Number
                        && note.PatStepTime > _n.PatStepTime
                        && note.PatStepTime < _n.PatStepTime + _n.StepLength);

                    if (   behind >= 0
                        && behind < Notes.Count)
                        Notes[behind].StepLength = note.PatStepTime - Notes[behind].PatStepTime;
                }
            }


            public bool HasNoteKeys(string path) { return Notes.Find(n => n.Keys.Find(k => k.Path == path) != null) != null; }
            public bool HasAutoKeys(string path) { return AutoKeys.Find(k => k.Path == path) != null; }
            public bool HasKeys    (string path) { return HasNoteKeys(path) || HasAutoKeys(path); }
        }
    }
}
