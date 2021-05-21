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
                if (MemSet)
                {
                    Mems[m] = Mems[m] < 0 || Mems[m] != CurPat ? CurPat : -1;
                    MemSet = F;
                }
                else if (Mems[m] > -1)
                {
                    if (g_playing) Track.NextPat = Mems[m];
                    else           SetCurrentPattern(Mems[m]);
                }
            }


            public void Cue()
            {
                SetCue();
            }


            public void Mem()
            {
                MemSet = !MemSet;
            }


            void ValMem()
            {
                for (int m = 0; m < nMems; m++)
                    if (Mems[m] >= Patterns.Count) Mems[m] = -1;
            }


            public void PrevPattern()
            {
                if (MovePat) MovePatterns(CurPat - 1);
                else SetCurrentPattern(CurPat - 1);

                g_lcdPressed.Add(lcdClip+5);
            }


            public void NextPattern()
            {
                if (MovePat) MovePatterns(CurPat + 1);
                else SetCurrentPattern(CurPat + 1);

                g_lcdPressed.Add(lcdClip+6);
            }


            public void MovePatterns(int destPat)
            {
                var block = GetBlock(CurPat);
                if (OK(block))
                {
                    var pats = new List<Pattern>();

                    for (int i = block.First; i <= block.Last; i++)
                        pats.Add(Patterns[i]);

                    Patterns.RemoveRange(block.First, block.Len);
                    Blocks.Remove(block);

                    var newFirst = block.First;
                    if (destPat > CurPat)
                    {
                        var b = GetBlock(block.Last + 1);
                        if (OK(b))
                        {
                            b.First -= block.Len;
                            b.Last  -= block.Len;

                            newFirst = block.First + b.Len;
                            CurPat += b.Len;
                        }
                        else
                        {
                            newFirst++;
                            CurPat++;
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

                            CurPat -= b.Len;
                        }
                        else
                        {
                            newFirst--;
                            CurPat--;
                        }
                    }

                    newFirst = MinMax(0, newFirst, Patterns.Count);
                    for (int i = 0; i < pats.Count; i++)
                        Patterns.Insert(newFirst + i, pats[i]);

                    Blocks.Add(new Block(newFirst, newFirst + block.Len - 1));

                    CurPat = MinMax(destPat - block.First, CurPat, Patterns.Count - 1 - (block.Last - destPat));
                }
                else
                {
                    var pat = CurPattern;
                    Patterns.RemoveAt(CurPat);

                    var b = GetBlock(destPat);
                    if (OK(b))
                    {
                        var frw = destPat > CurPat ? 1 : -1;

                        destPat = MinMax(0, CurPat + b.Len * frw, Patterns.Count);

                        b.First -= frw;
                        b.Last  -= frw;

                        CurPat = MinMax(0, CurPat + b.Len * frw, Patterns.Count);
                    }
                    else
                    {
                        destPat = MinMax(0, destPat, Patterns.Count);
                        CurPat  = MinMax(0, destPat, Patterns.Count);
                    }

                    Patterns.Insert(destPat, pat);
                }

                if (g_playing)
                    Track.PlayTime += GetPatTime(CurPat - destPat);

                if (OK(EditPos))
                    EditPos = CurPat * g_patSteps + EditPos % g_patSteps;


                UpdateAutoKeys();


                g_lcdPressed.Add(lcdClip+7);
            }


            public void SetCurrentPattern(int p)
            {
                if (Patterns.Count == 0)
                    return;


                //var oldPat = CurPat;

                //StopEdit();

                var b = GetBlock(CurPat);

                if (    OK(b)
                    && (In || Out))
                {
                    var off = p > CurPat ? 1 : -1;

                         if (In ) b.First = MinMax(0, b.First + off, Math.Min(CurPat, b.Last));
                    else if (Out) b.Last  = MinMax(Math.Max(b.First, CurPat), b.Last + off, Patterns.Count-1);
                }
                else
                {
                    CurPat = p;

                         if (CurPat < 0)               CurPat = Patterns.Count - 1;
                    else if (CurPat >= Patterns.Count) CurPat = 0;


                    if (AutoCue)
                        Track.NextPat = CurPat;
                }

                if (OK(EditPos))
                    EditPos = CurPat * g_patSteps + EditPos % g_patSteps;


                //if (g_playing)
                //{
                //         if (CurPat > oldPat) StartTime -= nSteps * g_session.TicksPerStep;
                //    else if (CurPat < oldPat) StartTime += nSteps * g_session.TicksPerStep;
                //}


                //UpdateOctaveLabel();
                UpdateSongOff();//g_song.CurPat);
            
                UpdateInstName();
            }


            public void NewPattern()
            { 
                var pat = new Pattern(CurPattern);
                pat.Clear();

                Patterns.Insert(CurPat + 1, pat);
                SetCurrentPattern(CurPat + 1);

                MovePatternOff();
                DisableBlock();

                if (OK(EditPos))
                    EditPos = 0;

                //if (g_playing)
                //    StartTime -= nSteps * g_session.TicksPerStep;

                UpdateAutoKeys();


                g_lcdPressed.Add(lcdClip+2);
            }


            public void DeletePattern()
            {
                var block = GetBlock(CurPat);

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

                    if (CurPat >= Patterns.Count)
                        SetCurrentPattern(Patterns.Count - 1);
                }
                else
                {
                    var b = GetBlock(CurPat);

                    if (Patterns.Count > 1) Patterns.RemoveAt(CurPat);
                    else                    Patterns[0].Clear();

                    if (OK(b))
                    {
                        if (b.First == b.Last) Blocks.Remove(b);
                        else b.Last--;
                    }

                    if (CurPat >= Patterns.Count)
                        SetCurrentPattern(Patterns.Count-1);
                }


                if (OK(EditPos))
                    EditPos = Math.Min(EditPos, Patterns.Count * g_patSteps);


                //if (OK(g_song.PlayTime))
                //    g_song.StartTime += nSteps * g_session.TicksPerStep;

                if (Track.PlayPat >= Patterns.Count)
                    Track.PlayPat  = Patterns.Count - 1;


                UpdateAutoKeys();


                MovePatternOff();
                DisableBlock();

                ValMem();

                g_lcdPressed.Add(lcdClip+0);
            }


            public void DuplicatePattern()
            {
                var block = GetBlock(CurPat);

                if (   Block
                    && OK(block))
                {
                    for (int p = block.First; p <= block.Last; p++)
                        Patterns.Insert(block.Last + 1 + p - block.First, new Pattern(Patterns[p]));

                    Blocks.Add(new Block(
                    block.Last + 1,
                    block.Last + block.Len));

                    SetCurrentPattern(CurPat + block.Len);
                }
                else
                {
                    Patterns.Insert(CurPat + 1, new Pattern(CurPattern));
                    SetCurrentPattern(CurPat + 1);
                }

                MovePatternOff();
                DisableBlock();

                //if (NOK(recordPosition))
                // recordPosition = 0;

                //if (g_playing)
                //    StartTime -= nSteps * g_session.TicksPerStep;

                UpdateAutoKeys();


                g_lcdPressed.Add(lcdClip+1);
            }        
        }
    }
}
