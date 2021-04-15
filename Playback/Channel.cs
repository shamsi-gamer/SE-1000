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


            public Channel(Channel chan, Pattern pat = null)
            {
                Pattern    = pat ?? chan.Pattern;

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
                        && _n.PatStep > note.PatStep
                        && _n.PatStep < note.PatStep + note.StepLength);

                    if (   ahead >= 0
                        && ahead < Notes.Count)
                        note.StepLength = Notes[ahead].PatStep - note.PatStep;

                    var behind = Notes.FindIndex(_n =>
                           note.Number == _n.Number
                        && note.PatStep > _n.PatStep
                        && note.PatStep < _n.PatStep + _n.StepLength);

                    if (   behind >= 0
                        && behind < Notes.Count)
                        Notes[behind].StepLength = note.PatStep - Notes[behind].PatStep;
                }
            }


            public void UpdateNotes()
            {
                var iChan = Pattern.Channels.IndexOf(this);

                foreach (var note in Notes)
                { 
                    note.Channel = this;
                    note.iChan   = iChan;
                }
            }


            public bool HasNoteKeys(string path) { return Notes   .Find(n => n.Keys.Find(k => k.Path == path) != null) != null; }
            public bool HasAutoKeys(string path) { return AutoKeys.Find(k => k.Path == path) != null; }
            public bool HasKeys    (string path) { return HasNoteKeys(path) || HasAutoKeys(path); }


            public bool IsDefault { get
            {
                return
                       Notes   .Count == 0
                    && AutoKeys.Count == 0
                    && On
                    && Volume  == 1
                    && Shuffle == 0;
            } }


            public string Save()
            {
                var save =
                      WS(Pattern.Channels.IndexOf(this))
                    + W (Instrument.Name)

                    + WB(On)
                    + WS(Volume)
                    + WS(Shuffle)
                    + WS(Transpose)

                    + SaveNotes()
                    + SaveAutoKeys();

                return save;
            }


            string SaveNotes()
            {
                var save = S(Notes.Count);

                foreach (var n in Notes)
                    save += ";" + n.Save();

                return save;
            }


            string SaveAutoKeys()
            {
                var save = ";" + S(AutoKeys.Count);

                foreach (var k in AutoKeys)
                    save += ";" + k.Save();

                return save;
            }


            public static Channel Load(string[] data, ref int i, out int index, Pattern pat)
            {
                var chan = new Channel(pat);
                
                index           = int.Parse(data[i++]);

                var instName    = data[i++];
                chan.Instrument = g_inst.Find(inst => inst.Name == instName);

                chan.On         = int  .Parse(data[i++]) > 0;
                chan.Volume     = float.Parse(data[i++]);
                chan.Shuffle    = int  .Parse(data[i++]);
                chan.Transpose  = int  .Parse(data[i++]);

                chan.LoadNotes   (data, ref i, index);
                chan.LoadAutoKeys(data, ref i);

                return chan;
            }


            void LoadNotes(string[] data, ref int i, int iChan)
            {
                var nNotes = int.Parse(data[i++]);

                for (int n = 0; n < nNotes; n++)
                {
                    var note = Note.Load(data, ref i, Instrument);

                    note.Channel = this;
                    note.iChan   = iChan;

                    Notes.Add(note);
                }
            }


            void LoadAutoKeys(string[] data, ref int i)
            {
                var nKeys = int.Parse(data[i++]);

                for (int k = 0; k < nKeys; k++)
                    AutoKeys.Add(Key.Load(data, ref i, Instrument));
            }
        }
    }
}
