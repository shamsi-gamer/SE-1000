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

            if (h == 10) // here and not in BeatHigh() because TogglePiano() needs to know about tune
                TogglePiano(tune);

            else if (IsCurParam(strTune)
                  && (tune?.UseChord ?? F)
                  && !(CurClip.ParamKeys || CurClip.ParamAuto))
                UpdateFinalTuneChord(tune, HighToNote(h));

            else if (CurClip.ChordEdit
                  && CurClip.Chord > -1)
                EditChord(HighToNote(h));

            else if (CurClip.Piano)
                PlayNote(
                    CurClip,
                    HighToNote(h), 
                       CurClip.Chord > -1 
                    && CurClip.ChordMode 
                    ? CurClip.Chords[CurClip.Chord] 
                    : null,
                    CurChan);
            else
                BeatHigh(h);
        }


        void BeatHigh(int h)
        {
                 if (h == 0) ClearNotes();
            else if (h == 1) Random();
                                   
            else if (h == 2) RandomInstrument();
            else if (h == 3) ToggleAllChannels();
            else if (h == 4) PickNote();  
                          
            else if (h == 5) Shift(F);
            else if (h == 6) Shift(T); 
                          
            else if (h == 7) Flip(CurChan, 4); 
            else if (h == 8) Flip(CurChan, 8); 
            else if (h == 9) Flip(CurChan, 16);

            if (h < 2 || h > 4)
                lblHigh[h].Mark();
        }


        void TogglePiano(Tune tune)
        { 
            if (   IsCurParam(strTune)
                && (tune?.UseChord ?? F))
            { 
                g_settings.RemoveLast();
                CurSet--;
                CurClip.Piano = F;
            }
            else
                CurClip.Piano = !CurClip.Piano;

            if (CurClip.Piano)
                CurClip.Pick = F;
        }


        void Low(int l)
        {
            var tune = SelSource    ?.Tune
                    ?? SelInstrument?.Tune;

            if (   IsCurParam(strTune)
                && (tune?.UseChord ?? F))
                UpdateFinalTuneChord(tune, LowToNote(l));

            else if (CurClip.ChordEdit
                  && CurClip.Chord > -1)
                EditChord(LowToNote(l));

            else if (CurClip.Piano)
            {
                if (l == 15)
                    CurClip.HalfSharp = !CurClip.HalfSharp;

                else PlayNote( // l < 15
                    CurClip,
                    LowToNote(l),
                       CurClip.Chord > -1 
                    && CurClip.ChordMode 
                    ? CurClip.Chords[CurClip.Chord]
                    : null,
                    CurChan);
            }
            else
                Tick(CurChan, l);
        }


        void Tick(int ch, int step)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                Tick(p, ch, step);
        }


        void Tick(int pat, int ch, int step)
        {
            var _chan = CurClip.Patterns[pat].Channels[ch];
            var  chan = CurClip.Patterns[pat].Channels[ch];

            var found = chan.Notes.Where(n => 
                   n.Step >= step
                && n.Step <  step+1).ToArray();

            if (found.Length == 0)
            {
                if (!CurClip.Pick)
                {
                    var notes = GetChordNotes(CurClip.CurNote);

                    for (int n = 0; n < notes.Count; n++)
                        chan.AddNote(new Note(chan, ch, 1, notes[n], step + ChordSpread(n), CurClip.EditStepLength));
                }
            }
            else if (CurClip.Pick)
            {
                CurClip.CurNote = found[0].Number;

                CurClip.Pick = F;

                TriggerNote(
                    CurClip,
                    found[0].Number, 
                    ch, 
                    found[0].StepLength,
                    0);
            }
            else
            {
                CurClip.Pick = F;
                
                foreach (var n in found)
                    chan.Notes.Remove(n);
            }
        }


        void Shift(bool fwd)
        {
            if (CurClip.AllChan)
            {
                for (int i = 0; i < g_nChans; i++)
                    Shift(i, fwd);
            }
            else
                Shift(CurChan, fwd);
        }


        void Shift(int ch, bool fwd)
        {
            var pats = CurClip.Patterns;

            var spill = new List<Note>();

            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            if (fwd)
            {
                for (int p = last; p >= first; p--)
                {
                    var chan = pats[p].Channels[ch];

                    for (int n = chan.Notes.Count - 1; n >= 0; n--)
                    {
                        var note = chan.Notes[n];

                        note.Step += Math.Min(CurClip.EditStepIndex, 1);

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

                        note.Step -= Math.Min(CurClip.EditStepIndex, 1);

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
            CurClip.RndInst = !CurClip.RndInst;
        }


        void ToggleAllChannels()
        {
            CurClip.AllChan = !CurClip.AllChan;
        }


        void Flip(int ch, int frac)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                for (int step = 0; step < g_patSteps; step += g_patSteps / frac)
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
            if (CurClip.AllChan)
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
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                var chan = CurClip.Patterns[p].Channels[ch];

                if (CurClip.ParamKeys)
                {
                    foreach (var note in chan.Notes)
                    { 
                        var param = GetCurrentParam(note.Instrument);

                        var index = 0;
                        while ((index = note.Keys.FindIndex(k => k.Path == param.GetPath(CurSrc))) > -1)
                            note.Keys.RemoveAt(index);
                    }
                }
                else if (CurClip.ParamAuto)
                {
                    var param = GetCurrentParam(chan.Instrument);
                    var index = 0;
                        
                    while ((index = chan.AutoKeys.FindIndex(k => k.Path == param.GetPath(CurSrc))) > -1)
                        chan.AutoKeys.RemoveAt(index);
                }
                else
                { 
                    chan.Notes.Clear();
                }
            }

            if (CurClip.ParamAuto)
                CurClip.UpdateAutoKeys();
        }


        void PickNote()
        {
            CurClip.Pick = !CurClip.Pick;
            //UpdateLabel(lblHigh[7], CurClip.Pick);
        }


        int HighToNote(int high)
        {
            var h = high * NoteScale + 1;

            if (high > 1) h++;
            if (high > 4) h++;
            if (high > 6) h++;

            return 
                  (60 + CurChannel.Transpose * 12 + h) * NoteScale 
                + (CurClip.HalfSharp ? 1 : 0);
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
                + (CurClip.HalfSharp ? 1 : 0);
        }
    }
}
