using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void High(int h)
        {
            var tune = SelSource    ?.Tune
                    ?? SelInstrument?.Tune;

            if (h == 11) // here and not in BeatHigh() because TogglePiano() needs to know about tune
                TogglePiano(tune);

            else if (IsCurParam(strTune)
                  && (tune?.UseChord ?? False)
                  && !(EditedClip.ParamKeys || EditedClip.ParamAuto))
                UpdateFinalTuneChord(tune, HighToNote(h));

            else if (EditedClip.ChordEdit
                  && OK(EditedClip.Chord))
                EditChord(HighToNote(h));

            else if (EditedClip.Piano)
                PlayNote(
                    EditedClip,
                    HighToNote(h), 
                       OK(EditedClip.Chord) 
                    && EditedClip.ChordMode 
                    ? EditedClip.Chords[EditedClip.Chord] 
                    : null,
                    CurChan);
            else
                BeatHigh(h);
        }



        void BeatHigh(int h)
        {
                 if (h == 0) Copy();
            else if (h == 1) Paste(); 
                          
            else if (h == 2) PickNote();  
            else if (h == 3) ToggleAllChannels();
            else if (h == 4) RandomInstrument();
                          
            else if (h == 5) ClearNotes();
            else if (h == 6) Random();
                                   
            else if (h == 7) Common1();
            else if (h == 8) Common2();
            else if (h == 9) Common3();

            else if (h == 10) EditedClip.Accent = !EditedClip.Accent;

            if (  (h < 2 || h > 4) 
                && h != 10)
                lblHigh[h].Mark();
        }



        void Common1()
        {
            if (!ShowPianoView)
                Flip(CurChan, 4);
        }



        void Common2()
        {
            if (ShowPianoView) ReverseNotes();
            else               Flip(CurChan, 8);
        }



        void Common3()
        {
            if (ShowPianoView) FlipNotes();
            else               Flip(CurChan, 16);
        }



        void TogglePiano(Tune tune)
        { 
            if (   IsCurParam(strTune)
                && (tune?.UseChord ?? False))
            {
                EditedClip.Settings.RemoveLast();
                CurSet--;
                EditedClip.Piano = False;
            }
            else
                EditedClip.Piano = !EditedClip.Piano;

            if (EditedClip.Piano)
                EditedClip.Pick = False;
        }



        void Low(int l)
        {
            var tune = SelSource    ?.Tune
                    ?? SelInstrument?.Tune;

            if (   IsCurParam(strTune)
                && (tune?.UseChord ?? False))
                UpdateFinalTuneChord(tune, LowToNote(l));

            else if (EditedClip.ChordEdit
                  && OK(EditedClip.Chord))
                EditChord(LowToNote(l));

            else if (EditedClip.Piano)
            {
                if (l == 15)
                    EditedClip.HalfSharp = !EditedClip.HalfSharp;

                else PlayNote( // l < 15
                    EditedClip,
                    LowToNote(l),
                       OK(EditedClip.Chord)
                    && EditedClip.ChordMode 
                    ? EditedClip.Chords[EditedClip.Chord]
                    : null,
                    CurChan);
            }
            else
                Tick(CurChan, l);
        }



        void Tick(int ch, float step)
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                Tick(p, ch, step);
        }



        void Tick(int pat, int ch, float step)
        {
            var _chan = EditedClip.Patterns[pat].Channels[ch];
            var  chan = EditedClip.Patterns[pat].Channels[ch];

            if (EditedClip.Accent)
            {
                chan.Accents[(int)step] = !chan.Accents[(int)step];
            }
            else
            {
                var dStep = Math.Min(Math.Max(0.5f, EditedClip.EditStep), 1);

                var found = chan.Notes.Where(n => 
                       n.Step >= step
                    && n.Step <  step + dStep).ToArray();

                if (found.Length == 0)
                {
                    if (!EditedClip.Pick)
                    {
                        var notes = GetChordNotes(EditedClip.CurNote);

                        for (int n = 0; n < notes.Count; n++)
                            chan.AddNote(new Note(chan, ch, 1, notes[n], step + ChordStrum(n), EditedClip.EditStepLength));
                    }
                }
                else if (EditedClip.Pick)
                {
                    EditedClip.CurNote = found[0].Number;
                    EditedClip.Pick    = False;

                    TriggerNote(
                        EditedClip,
                        found[0].Number, 
                        ch, 
                        found[0].StepLength,
                        0);
                }
                else
                {
                    EditedClip.Pick = False;
                
                    foreach (var n in found)
                        chan.Notes.Remove(n);
                }
            }
        }



        void Shift(bool fwd)
        {
            if (EditedClip.AllChan)
            {
                for (int i = 0; i < g_nChans; i++)
                    Shift(i, fwd);
            }
            else
                Shift(CurChan, fwd);
        }



        void Shift(int ch, bool fwd)
        {
            var pats = EditedClip.Patterns;

            var spill = new List<Note>();

            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            if (fwd)
            {
                for (int p = last; p >= first; p--)
                {
                    var chan = pats[p].Channels[ch];

                    for (int n = chan.Notes.Count - 1; n >= 0; n--)
                    {
                        var note = chan.Notes[n];

                        note.Step += EditedClip.EditStep;

                        if (note.Step >= g_patSteps)
                            spill.Add(note);
                    }
                }

                foreach (var n in spill)
                {
                    var spillPat  = n.PatIndex == last ? first : n.PatIndex+1;
                    var spillChan = pats[spillPat].Channels[ch];

                    MoveSpillNotes(n, spillChan, -g_patSteps);
                }
            }
            else
            {
                for (int p = first; p <= last; p++)
                {
                    var chan = pats[p].Channels[ch];

                    for (int n = chan.Notes.Count - 1; n >= 0; n--)
                    {
                        var note = chan.Notes[n];

                        note.Step -= EditedClip.EditStep;

                        if (note.Step < 0)
                            spill.Add(note);
                    }
                }

                foreach (var n in spill)
                {
                    var spillPat  = n.PatIndex == first ? last : n.PatIndex-1;
                    var spillChan = pats[spillPat].Channels[ch];

                    MoveSpillNotes(n, spillChan, g_patSteps);
                }
            }
        }



        void MoveSpillNotes(Note note, Channel spillChan, float dSteps)
        {
            note.Channel.Notes.Remove(note);
            spillChan.Notes.Add(note);

            note.Channel  = spillChan;
            note.Step += dSteps;
        }



        void RandomInstrument()
        {
            EditedClip.RndInst = !EditedClip.RndInst;
        }



        void ToggleAllChannels()
        {
            EditedClip.AllChan = !EditedClip.AllChan;
        }



        void Flip(int ch, int frac)
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            float dStep = g_patSteps / frac;

            if (frac == 16)
                dStep = Math.Min(Math.Max(0.5f, dStep), 1);

            for (int p = first; p <= last; p++)
            { 
                for (float step = 0; step < g_patSteps; step += dStep)
                    Tick(p, ch, step);
            }
        }



        void Flip(int pat, int ch, int frac)
        {
            for (int step = 0; step < g_patSteps; step += g_patSteps / frac)
                Tick(pat, ch, step);
        }



        void ClearNotes()
        {
            if (   EditedClip.RndInst
                && OK(SelChan))
                return;

            if (EditedClip.AllChan)
            {
                for (int i = 0; i < g_nChans; i++)
                    ClearNotes(i);
            }
            else
                ClearNotes(CurChan);
        }



        void ClearNotes(int ch)
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                var chan = EditedClip.Patterns[p].Channels[ch];

                if (EditedClip.ParamKeys)
                {
                    foreach (var note in chan.Notes)
                        note.Keys.RemoveAll(k => k.Path == CurrentParam.Path);
                }

                else if (EditedClip.ParamAuto)
                    chan.AutoKeys.RemoveAll(k => k.Path == CurrentParam.Path);

                else if (EditedClip.Accent)
                {
                    for (int i = 0; i < g_patSteps; i++)
                        chan.Accents[i] = False;
                }

                else
                    chan.Notes.Clear();
            }

            if (EditedClip.ParamAuto)
                EditedClip.UpdateAutoKeys();
        }



        void PickNote()
        {
            EditedClip.Pick = !EditedClip.Pick;
        }



        int HighToNote(int high)
        {
            var h = high * NoteScale + 1;

            if (high > 1) h++;
            if (high > 4) h++;
            if (high > 6) h++;

            return 
                  (60 + CurChannel.Transpose * 12 + h) * NoteScale 
                + (EditedClip.HalfSharp ? 1 : 0);
        }



        int LowToNote(int low)
        {
            var l = low * NoteScale;

            if (low >  2) l--;
            if (low >  6) l--;
            if (low >  9) l--;
            if (low > 13) l--;

            return 
                  (60 + CurChannel.Transpose * 12 + l) * NoteScale
                + (EditedClip.HalfSharp ? 1 : 0);
        }
    }
}
