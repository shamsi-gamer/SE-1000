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


            public Channel(Pattern pat, Instrument inst)
            {
                Pattern    = pat;

                On         = True;
                Instrument = inst;
                Volume     = 1;
                Shuffle    = 0;
                Transpose  = 0;

                Notes      = new List<Note>();
                AutoKeys   = new List<Key>();
            }


            public Channel(Channel chan, Pattern pat = Pattern_null)
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
                    var note = new Note(n) { Channel = this };
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
                        && _n.Step > note.Step
                        && _n.Step < note.Step + note.StepLength);

                    if (   ahead >= 0
                        && ahead < Notes.Count)
                        note.StepLength = Notes[ahead].Step - note.Step;

                    var behind = Notes.FindIndex(_n =>
                           note.Number == _n.Number
                        && note.Step > _n.Step
                        && note.Step < _n.Step + _n.StepLength);

                    if (   behind >= 0
                        && behind < Notes.Count)
                        Notes[behind].StepLength = note.Step - Notes[behind].Step;
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


            public bool HasNoteKeys(string path) { return OK(Notes   .Find(n => OK(n.Keys.Find(k => k.Path == path)))); }
            public bool HasAutoKeys(string path) { return OK(AutoKeys.Find(k => k.Path == path)); }
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
                var save = "";

                save += WS(Pattern.Channels.IndexOf(this));
                save += W (Instrument.Name);

                save += WB(On);
                save += WS(Volume);
                save += WS(Shuffle);
                save += WS(Transpose);

                save += SaveNotes();
                save += SaveAutoKeys();

                return save;
            }


            string SaveNotes()
            {
                var save = S(Notes.Count);

                foreach (var n in Notes)
                    save += P(n.Save());

                return save;
            }


            string SaveAutoKeys()
            {
                var save = PS(AutoKeys.Count);

                foreach (var k in AutoKeys)
                    save += P(k.Save());

                return save;
            }


            public static Channel Load(string[] data, ref int i, out int index, Pattern pat)
            {
                index           = int_Parse(data[i++]);

                var instName    = data[i++];
                var inst        = Instruments.Find(_inst => _inst.Name == instName);

                var chan = new Channel(pat, inst);
                
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
                var nNotes = int_Parse(data[i++]);

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
                var nKeys = int_Parse(data[i++]);

                for (int k = 0; k < nKeys; k++)
                    AutoKeys.Add(Key.Load(data, ref i, Instrument));
            }
        }
    }
}
