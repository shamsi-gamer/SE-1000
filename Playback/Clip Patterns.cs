using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        public partial class Clip
        {
            public void SetMem(int m)
            {
                if (SetMemPat)
                {
                    Mems[m] = Mems[m] < 0 || Mems[m] != EditPat ? EditPat : -1;
                    SetMemPat = False;
                }
                else if (OK(Mems[m]))
                {
                    if (Playing) Track.NextPat = Mems[m];
                    else         SetEditPattern(Mems[m]);
                }
            }



            public void Cue()
            {
                SetCue();
            }



            public void MemPat()
            {
                if (SetOrPat) SetOrPat  = False;
                else          SetMemPat = !SetMemPat;

                SetMemSet = False;
            }



            void ValMem()
            {
                for (int m = 0; m < nMems; m++)
                    if (Mems[m] >= Patterns.Count) Mems[m] = -1;
            }



            public void PrevPattern()
            {
                if (MovePat) MovePatterns(EditPat - 1);
                else SetEditPattern(EditPat - 1);

                g_lcdPressed.Add(lcdClip+5);
            }



            public void NextPattern()
            {
                if (MovePat) MovePatterns(EditPat + 1);
                else         SetEditPattern(EditPat + 1);

                g_lcdPressed.Add(lcdClip+6);
            }



            public void MovePatterns(int destPat)
            {
                var block = GetBlock(EditPat);
                if (OK(block))
                {
                    var pats = new List<Pattern>();

                    for (int i = block.First; i <= block.Last; i++)
                        pats.Add(Patterns[i]);

                    Patterns.RemoveRange(block.First, block.Len);
                    Blocks.Remove(block);

                    var newFirst = block.First;
                    if (destPat > EditPat)
                    {
                        var b = GetBlock(block.Last + 1);
                        if (OK(b))
                        {
                            b.First -= block.Len;
                            b.Last  -= block.Len;

                            newFirst = block.First + b.Len;
                            EditPat += b.Len;
                        }
                        else
                        {
                            newFirst++;
                            EditPat++;
                        }
                    }
                    else
                    {
                        newFirst = block.First;

                        var b = GetBlock(block.First - 1);
                        if (OK(b))
                        {
                            newFirst = b.First;

                            b.First += block.Len;
                            b.Last  += block.Len;

                            EditPat -= b.Len;
                        }
                        else
                        {
                            newFirst--;
                            EditPat--;
                        }
                    }

                    newFirst = MinMax(0, newFirst, Patterns.Count);
                    for (int i = 0; i < pats.Count; i++)
                        Patterns.Insert(newFirst + i, pats[i]);

                    Blocks.Add(new Block(newFirst, newFirst + block.Len - 1));

                    EditPat = MinMax(destPat - block.First, EditPat, Patterns.Count - 1 - (block.Last - destPat));
                }
                else
                {
                    var pat = CurPattern;
                    Patterns.RemoveAt(EditPat);

                    var b = GetBlock(destPat);
                    if (OK(b))
                    {
                        var frw = destPat > EditPat ? 1 : -1;

                        destPat = MinMax(0, EditPat + b.Len * frw, Patterns.Count);

                        b.First -= frw;
                        b.Last  -= frw;

                        EditPat = MinMax(0, EditPat + b.Len * frw, Patterns.Count);
                    }
                    else
                    {
                        destPat = MinMax(0, destPat, Patterns.Count);
                        EditPat  = MinMax(0, destPat, Patterns.Count);
                    }

                    Patterns.Insert(destPat, pat);
                }

                if (Playing)
                    Track.PlayTime += GetPatTime(EditPat - destPat);

                if (OK(EditPos))
                    EditPos = EditPat * g_patSteps + EditPos % g_patSteps;


                UpdateAutoKeys();


                g_lcdPressed.Add(lcdClip+7);
            }



            public void SetEditPattern(int p)
            {
                if (Patterns.Count == 0)
                    return;


                //var oldPat = EditPat;

                //StopEdit();

                var b = GetBlock(EditPat);

                if (    OK(b)
                    && (In || Out))
                {
                    var off = p > EditPat ? 1 : -1;

                         if (In ) b.First = MinMax(0, b.First + off, Math.Min(EditPat, b.Last));
                    else if (Out) b.Last  = MinMax(Math.Max(b.First, EditPat), b.Last + off, Patterns.Count-1);
                }
                else
                {
                    EditPat = p;

                         if (EditPat < 0)               EditPat = Patterns.Count - 1;
                    else if (EditPat >= Patterns.Count) EditPat = 0;


                    if (AutoCue)
                        Track.NextPat = EditPat;
                }

                if (OK(EditPos))
                    EditPos = EditPat * g_patSteps + EditPos % g_patSteps;


                //if (Playing)
                //{
                //         if (EditPat > oldPat) StartTime -= nSteps * g_session.TicksPerStep;
                //    else if (EditPat < oldPat) StartTime += nSteps * g_session.TicksPerStep;
                //}


                UpdateClipOff();//g_song.EditPat);
            
                SetInstName();
            }



            public void FindAndSetActiveOctave()
            {
                int first, last;
                GetPatterns(Track.PlayPat, out first, out last);

                var chan = Patterns[Track.PlayPat].Channels[CurChan];

                var minNote = int.MaxValue;
                var maxNote = int.MinValue;

                foreach (var note in chan.Notes)
                {
                    minNote = Math.Min(minNote, note.Number);
                    maxNote = Math.Max(maxNote, note.Number);
                }

                for (var p = first; p <= last; p++)
                { 
                    var transpose = 
                          (int)Math.Round((minNote+maxNote)/2f / NoteScale / 12f) - 6
                        - Patterns[p].Channels[CurChan].Transpose;

                    SetTranspose(this, CurChan, transpose);
                }
            }



            public void NewPattern(Program prog)
            { 
                var pat = new Pattern(CurPattern);
                pat.Clear();

                Patterns.Insert(EditPat + 1, pat);
                SetEditPattern(EditPat + 1);

                MovePatternOff();
                DisableBlock();

                if (OK(EditPos))
                    EditPos = 0;


                Track.SyncPlayTime(prog);

                UpdateAutoKeys();


                g_lcdPressed.Add(lcdClip+2);
            }



            public void DeletePattern(Program prog)
            {
                var block = GetBlock(EditPat);

                if (   Block
                    && OK(block))
                {
                    var first = Patterns[block.First];

                    Patterns.RemoveRange(block.First, block.Len);
                    Blocks.Remove(block);

                    foreach (var b in Blocks)
                    {
                        if (b.First > block.Last)
                        {
                            b.First -= block.Len;
                            b.Last  -= block.Len;
                        }
                    }

                    if (Patterns.Count == 0)
                    {
                        first.Clear();
                        Patterns.Add(first);
                    }

                    if (EditPat >= Patterns.Count)
                        SetEditPattern(Patterns.Count - 1);
                }
                else
                {
                    var b = GetBlock(EditPat);

                    if (Patterns.Count > 1) Patterns.RemoveAt(EditPat);
                    else                    Patterns[0].Clear();

                    if (OK(b))
                    {
                        if (b.First == b.Last) Blocks.Remove(b);
                        else b.Last--;
                    }

                    if (EditPat >= Patterns.Count)
                        SetEditPattern(Patterns.Count-1);
                }


                if (OK(EditPos))
                    EditPos = Math.Min(EditPos, Patterns.Count * g_patSteps);


                Track.SyncPlayTime(prog);

                if (Track.PlayPat >= Patterns.Count)
                    Track.PlayPat  = Patterns.Count - 1;


                UpdateAutoKeys();


                MovePatternOff();
                DisableBlock();

                ValMem();

                g_lcdPressed.Add(lcdClip+0);
            }



            public void DuplicatePattern(Program prog)
            {
                var block = GetBlock(EditPat);

                if (   Block
                    && OK(block))
                {
                    for (int p = block.First; p <= block.Last; p++)
                        Patterns.Insert(block.Last + 1 + p - block.First, new Pattern(Patterns[p]));

                    Blocks.Add(new Block(
                        block.Last + 1,
                        block.Last + block.Len));

                    SetEditPattern(EditPat + block.Len);
                }
                else
                {
                    Patterns.Insert(EditPat + 1, new Pattern(CurPattern));
                    SetEditPattern(EditPat + 1);
                }

                MovePatternOff();
                DisableBlock();


                Track.SyncPlayTime(prog);

                UpdateAutoKeys();


                g_lcdPressed.Add(lcdClip+1);
            }        
        }
    }
}
