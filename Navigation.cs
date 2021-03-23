using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        IMyTextPanel[] lMems = new IMyTextPanel[nMems];


        void Play()
        {
            if (PlayTime > -1)
                return;

            if (g_cue > -1)
            {
                PlayTime = GetPatTime(g_cue);
                g_cue = -1;
            }
            else
                PlayTime = GetPatTime(CurPat);

            StartTime = g_time - PlayTime;

            UpdatePlayStopLights();
        }


        void Stop()
        {
            if (PlayTime < 0)
            {
                var b = g_song.GetBlock(CurPat);

                var _block =
                       g_block
                    && b != null
                    && CurPat > b.First;

                SetCurrentPattern(_block ? b.First : 0);

                g_cue = -1;
            }


            StopCurrentNotes(g_song);


            PlayTime  = -1;
            StartTime = -1;
            PlayPat   = -1;


            lastNotes.Clear();

            UpdatePlayStopLights();
        }


        void Cue()
        {
            g_cue = g_cue == CurPat ? -1 : CurPat;
            UpdateLight(lblCue, g_cue > -1);
        }


        void Mem(int m)
        {
            if (g_setMem)
            {
                g_mem[m] = g_mem[m] < 0 || g_mem[m] != CurPat ? CurPat : -1;
                g_setMem = false;

                UpdateMemoryLights();
            }
            else if (g_mem[m] > -1)
            {
                if (PlayTime > -1)
                {
                    g_cue = g_mem[m];
                    UpdateLight(lblCue, g_cue > -1);
                }
                else
                    SetCurrentPattern(g_mem[m]);
            }

            MarkLight(lblMem[m]);
        }


        void Mem()
        {
            g_setMem = !g_setMem;
            UpdateLight(lblMemory, g_setMem);
        }


        void ValMem()
        {
            for (int m = 0; m < nMems; m++)
                if (g_mem[m] >= g_song.Patterns.Count) g_mem[m] = -1;

            UpdateMemoryLights();
        }


        public void PrevPattern(bool movePat)
        {
            if (movePat) MovePatterns(CurPat - 1);
            else SetCurrentPattern(CurPat - 1);

            MarkLight(lblPrevPat, !movePat);
            songPressed.Add(5);
        }


        public void NextPattern(bool movePat)
        {
            if (movePat) MovePatterns(CurPat + 1);
            else SetCurrentPattern(CurPat + 1);

            MarkLight(lblNextPat, !movePat);
            songPressed.Add(6);
        }


        public void MovePatterns(int destPat)
        {
            var block = g_song.GetBlock(CurPat);
            if (block != null)
            {
                var pats = new List<Pattern>();

                for (int i = block.First; i <= block.Last; i++)
                    pats.Add(g_song.Patterns[i]);

                g_song.Patterns.RemoveRange(block.First, block.Len);
                g_song.Blocks.Remove(block);

                var newFirst = block.First;
                if (destPat > CurPat)
                {
                    var b = g_song.GetBlock(block.Last + 1);
                    if (b != null)
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

                    var b = g_song.GetBlock(block.First - 1);
                    if (b != null)
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

                newFirst = MinMax(0, newFirst, g_song.Patterns.Count);
                for (int i = 0; i < pats.Count; i++)
                    g_song.Patterns.Insert(newFirst + i, pats[i]);

                g_song.Blocks.Add(new Block(newFirst, newFirst + block.Len - 1));

                CurPat = MinMax(destPat - block.First, CurPat, g_song.Patterns.Count - 1 - (block.Last - destPat));
            }
            else
            {
                var pat = CurrentPattern;
                g_song.Patterns.RemoveAt(CurPat);

                var b = g_song.GetBlock(destPat);
                if (b != null)
                {
                    var frw = destPat > CurPat ? 1 : -1;

                    destPat = MinMax(0, CurPat + b.Len * frw, g_song.Patterns.Count);

                    b.First -= frw;
                    b.Last  -= frw;

                    CurPat = MinMax(0, CurPat + b.Len * frw, g_song.Patterns.Count);
                }
                else
                {
                    destPat = MinMax(0, destPat, g_song.Patterns.Count);
                    CurPat  = MinMax(0, destPat, g_song.Patterns.Count);
                }

                g_song.Patterns.Insert(destPat, pat);
            }

            if (PlayTime > -1)
            {
                PlayTime += GetPatTime(CurPat - destPat);
                PlayPat   = (int)(PlayStep / nSteps);
            }

            if (OK(g_song.EditPos))
                g_song.EditPos = CurPat * nSteps + g_song.EditPos % nSteps;


            g_song.UpdateAutoKeys();


            songPressed.Add(7);
        }


        public void SetCurrentPattern(int p)
        {
            if (g_song.Patterns.Count == 0)
                return;


            var oldPat = CurPat;

            //StopEdit();

            var b = g_song.GetBlock(CurPat);

            if (    b != null
                && (g_in || g_out))
            {
                var off = p > CurPat ? 1 : -1;

                     if (g_in ) b.First = MinMax(0, b.First + off, Math.Min(CurPat, b.Last));
                else if (g_out) b.Last  = MinMax(Math.Max(b.First, CurPat), b.Last + off, g_song.Patterns.Count - 1);
            }
            else
            {
                CurPat = p;

                     if (CurPat < 0)                      CurPat = g_song.Patterns.Count - 1;
                else if (CurPat >= g_song.Patterns.Count) CurPat = 0;


                if (g_autoCue)
                {
                    g_cue = CurPat;
                    UpdateLight(lblCue, g_cue > -1);
                }
            }

            if (OK(g_song.EditPos))
                g_song.EditPos = CurPat * nSteps + g_song.EditPos % nSteps;


            if (PlayTime > -1)
            {
                     if (CurPat > oldPat) StartTime -= nSteps * g_ticksPerStep;
                else if (CurPat < oldPat) StartTime += nSteps * g_ticksPerStep;
            }


            //UpdateOctaveLight();
            UpdateSongOff();//g_song.CurPat);
            
            UpdateInstName();
        }


        void NewPattern()
        { 
            var pat = new Pattern(CurrentPattern);
            pat.Clear();

            g_song.Patterns.Insert(CurPat + 1, pat);
            SetCurrentPattern(CurPat + 1);

            MovePatternOff();
            DisableBlock();

            if (OK(g_song.EditPos))
                g_song.EditPos = 0;

            //if (PlayTime > -1)
            //    StartTime -= nSteps * g_ticksPerStep;

            g_song.UpdateAutoKeys();


            songPressed.Add(2);
        }


        void DeletePattern()
        {
            var block = g_song.GetBlock(CurPat);

            if (   g_block
                && block != null)
            {
                var first = g_song.Patterns[block.First];

                g_song.Patterns.RemoveRange(block.First, block.Len);
                g_song.Blocks.Remove(block);

                foreach (var b in g_song.Blocks)
                {
                    if (b.First > block.Last)
                    {
                        b.First -= block.Len;
                        b.Last  -= block.Len;
                    }
                }

                if (g_song.Patterns.Count == 0)
                {
                    first.Clear();
                    g_song.Patterns.Add(first);
                }

                if (CurPat >= g_song.Patterns.Count)
                    SetCurrentPattern(g_song.Patterns.Count - 1);
            }
            else
            {
                var b = g_song.GetBlock(CurPat);

                if (g_song.Patterns.Count > 1) g_song.Patterns.RemoveAt(CurPat);
                else g_song.Patterns[0].Clear();

                if (b != null)
                {
                    if (b.First == b.Last) g_song.Blocks.Remove(b);
                    else b.Last--;
                }

                if (CurPat >= g_song.Patterns.Count)
                    SetCurrentPattern(g_song.Patterns.Count-1);
            }


            if (OK(g_song.EditPos))
                g_song.EditPos = Math.Min(g_song.EditPos, g_song.Patterns.Count * nSteps);

            //if (PlayTime > -1)
            //    StartTime += nSteps * g_ticksPerStep;

            if (PlayPat >= g_song.Patterns.Count)
                PlayPat  = g_song.Patterns.Count-1;


            g_song.UpdateAutoKeys();


            MovePatternOff();
            DisableBlock();

            ValMem();

            songPressed.Add(0);
        }


        void DuplicatePattern()
        {
            var block = g_song.GetBlock(CurPat);

            if (g_block
            && block != null)
            {
                for (int p = block.First; p <= block.Last; p++)
                    g_song.Patterns.Insert(block.Last + 1 + p - block.First, new Pattern(g_song.Patterns[p]));

                g_song.Blocks.Add(new Block(
                block.Last + 1,
                block.Last + block.Len));

                SetCurrentPattern(CurPat + block.Len);
            }
            else
            {
                g_song.Patterns.Insert(CurPat + 1, new Pattern(CurrentPattern));
                SetCurrentPattern(CurPat + 1);
            }

            MovePatternOff();
            DisableBlock();

            //if (!OK(recordPosition))
            // recordPosition = 0;

            //if (PlayTime > -1)
            //    StartTime -= nSteps * g_ticksPerStep;

            g_song.UpdateAutoKeys();


            songPressed.Add(1);
        }
    }
}
