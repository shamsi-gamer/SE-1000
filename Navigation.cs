using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
        int[] g_mem = new int[nMems];
        bool setMem = false;

        IMyTextPanel[] lMems = new IMyTextPanel[nMems];


        void Play()
        {
            if (OK(g_song.PlayStep))
                return;

            if (g_cue > -1)
            {
                g_song.PlayStep = g_cue * nSteps;
                g_cue = -1;
            }
            else
                g_song.PlayStep = g_song.CurPat * nSteps;

            g_song.StartTime = g_time;
            UpdatePlayStopLights();
        }


        void Stop()
        {
            if (!OK(g_song.PlayStep))
            {
                var b = g_song.GetBlock(g_song.CurPat);

                var _block =
                       g_block
                    && b != null
                    && g_song.CurPat > b.First;

                SetCurrentPattern(_block ? b.First : 0);

                g_cue = -1;
            }


            StopCurrentNotes(g_song);


            g_song.PlayStep = float.NaN;
            //CurSong.StartTime = float.NaN; // don't clear start time here

            lastNotes.Clear();

            UpdatePlayStopLights();
        }


        void Cue()
        {
            g_cue = g_cue == g_song.CurPat ? -1 : g_song.CurPat;
            UpdateLight(lblCue, g_cue > -1);
        }


        void Mem(int m)
        {
            if (setMem)
            {
                g_mem[m] = g_mem[m] < 0 || g_mem[m] != g_song.CurPat ? g_song.CurPat : -1;
                setMem = false;

                UpdateMemoryLights();
            }
            else if (g_mem[m] > -1)
            {
                if (OK(g_song.PlayStep))
                {
                    g_cue = g_mem[m];
                    UpdateLight(lblCue, g_cue > -1);
                }
                else
                    SetCurrentPattern(g_mem[m]);
            }

            MarkLight(lblMem[m]);
        }


        void ClearAllMem()
        {
            for (int m = 0; m < nMems; m++)
                g_mem[m] = -1;
        }


        void Mem()
        {
            setMem = !setMem;
            UpdateLight(lblMemory, setMem);
        }


        void ValMem()
        {
            for (int m = 0; m < nMems; m++)
                if (g_mem[m] >= g_song.Patterns.Count) g_mem[m] = -1;

            UpdateMemoryLights();
        }


        public void PrevPattern(bool movePat)
        {
            if (movePat) MovePatterns(g_song.CurPat - 1);
            else SetCurrentPattern(g_song.CurPat - 1);
            
            MarkLight(lblPrevPat, !movePat);
            songPressed.Add(5);
        }


        public void NextPattern(bool movePat)
        {
            if (movePat) MovePatterns(g_song.CurPat + 1);
            else SetCurrentPattern(g_song.CurPat + 1);

            MarkLight(lblNextPat, !movePat);
            songPressed.Add(6);
        }


        public void MovePatterns(int p)
        {
            var cp = g_song.CurPat;

            var block = g_song.GetBlock(g_song.CurPat);
            if (block != null)
            {
                var pats = new List<Pattern>();

                for (int i = block.First; i <= block.Last; i++)
                    pats.Add(g_song.Patterns[i]);

                g_song.Patterns.RemoveRange(block.First, block.Len);
                g_song.Blocks.Remove(block);

                var newFirst = block.First;
                if (p > g_song.CurPat)
                {
                    var b = g_song.GetBlock(block.Last + 1);
                    if (b != null)
                    {
                        b.First -= block.Len;
                        b.Last  -= block.Len;

                        newFirst = block.First + b.Len;
                        g_song.CurPat += b.Len;
                    }
                    else
                    {
                        newFirst++;
                        g_song.CurPat++;
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
                        b.Last += block.Len;

                        g_song.CurPat -= b.Len;
                    }
                    else
                    {
                        newFirst--;
                        g_song.CurPat--;
                    }
                }

                newFirst = MinMax(0, newFirst, g_song.Patterns.Count);
                for (int i = 0; i < pats.Count; i++)
                    g_song.Patterns.Insert(newFirst + i, pats[i]);

                g_song.Blocks.Add(new Block(newFirst, newFirst + block.Len - 1));

                g_song.CurPat = MinMax(cp - block.First, g_song.CurPat, g_song.Patterns.Count - 1 - (block.Last - cp));
            }
            else
            {
                var pat = CurrentPattern(g_song);
                g_song.Patterns.RemoveAt(g_song.CurPat);

                var b = g_song.GetBlock(p);
                if (b != null)
                {
                    var frw = p > g_song.CurPat ? 1 : -1;

                    p = MinMax(0, g_song.CurPat + b.Len * frw, g_song.Patterns.Count);

                    b.First -= frw;
                    b.Last -= frw;

                    g_song.CurPat = MinMax(0, g_song.CurPat + b.Len * frw, g_song.Patterns.Count);
                }
                else
                {
                    p = MinMax(0, p, g_song.Patterns.Count);
                    g_song.CurPat = MinMax(0, p, g_song.Patterns.Count);
                }

                g_song.Patterns.Insert(p, pat);
            }

            if (OK(g_song.PlayStep))
            {
                g_song.PlayStep += (g_song.CurPat - cp) * nSteps;
                g_song.PlayPat   = (int)(g_song.PlayStep / nSteps);
            }

            if (OK(g_song.EditPos))
                g_song.EditPos = g_song.CurPat * nSteps + g_song.EditPos % nSteps;


            g_song.UpdateAutoKeys();


            songPressed.Add(7);
        }


        public void SetCurrentPattern(int p)
        {
            if (g_song.Patterns.Count == 0)
                return;


            //StopEdit();

            var b = g_song.GetBlock(g_song.CurPat);

            if (    b != null
                && (g_in || g_out))
            {
                var off = p > g_song.CurPat ? 1 : -1;

                     if (g_in ) b.First = MinMax(0, b.First + off, Math.Min(g_song.CurPat, b.Last));
                else if (g_out) b.Last  = MinMax(Math.Max(b.First, g_song.CurPat), b.Last + off, g_song.Patterns.Count - 1);
            }
            else
            {
                g_song.CurPat = p;

                     if (g_song.CurPat < 0)                      g_song.CurPat = g_song.Patterns.Count - 1;
                else if (g_song.CurPat >= g_song.Patterns.Count) g_song.CurPat = 0;


                if (g_autoCue)
                {
                    g_cue = g_song.CurPat;
                    UpdateLight(lblCue, g_cue > -1);
                }
            }

            if (OK(g_song.EditPos))
                g_song.EditPos = g_song.CurPat * nSteps + g_song.EditPos % nSteps;

            UpdateOctaveLight();
            UpdateSongOff();//g_song.CurPat);
            
            UpdateInstName();
        }


        void NewPattern()
        { 
            var pat = new Pattern(CurrentPattern(g_song));
            pat.Clear();

            g_song.Patterns.Insert(g_song.CurPat + 1, pat);
            SetCurrentPattern(g_song.CurPat + 1);

            MovePatternOff();
            DisableBlock();

            if (OK(g_song.EditPos))
                g_song.EditPos = 0;


            g_song.UpdateAutoKeys();


            songPressed.Add(2);
        }


        void DeletePattern()
        {
            var block = g_song.GetBlock(g_song.CurPat);

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

                if (g_song.CurPat >= g_song.Patterns.Count)
                    SetCurrentPattern(g_song.Patterns.Count - 1);
            }
            else
            {
                var b = g_song.GetBlock(g_song.CurPat);

                if (g_song.Patterns.Count > 1) g_song.Patterns.RemoveAt(g_song.CurPat);
                else g_song.Patterns[0].Clear();

                if (b != null)
                {
                    if (b.First == b.Last) g_song.Blocks.Remove(b);
                    else b.Last--;
                }

                if (g_song.CurPat >= g_song.Patterns.Count)
                    SetCurrentPattern(g_song.Patterns.Count - 1);
            }

            if (g_song.PlayPat >= g_song.Patterns.Count)
                g_song.PlayPat  = g_song.Patterns.Count - 1;


            if (OK(g_song.EditPos))
                g_song.EditPos = Math.Min(g_song.EditPos, g_song.Patterns.Count * nSteps);


            g_song.UpdateAutoKeys();


            MovePatternOff();
            DisableBlock();

            ValMem();

            songPressed.Add(0);
        }


        void DuplicatePattern()
        {
            var block = g_song.GetBlock(g_song.CurPat);

            if (g_block
            && block != null)
            {
                for (int p = block.First; p <= block.Last; p++)
                    g_song.Patterns.Insert(block.Last + 1 + p - block.First, new Pattern(g_song.Patterns[p]));

                g_song.Blocks.Add(new Block(
                block.Last + 1,
                block.Last + block.Len));

                SetCurrentPattern(g_song.CurPat + block.Len);
            }
            else
            {
                g_song.Patterns.Insert(g_song.CurPat + 1, new Pattern(CurrentPattern(g_song)));
                SetCurrentPattern(g_song.CurPat + 1);
            }

            MovePatternOff();
            DisableBlock();

            //if (!OK(recordPosition))
            // recordPosition = 0;


            g_song.UpdateAutoKeys();


            songPressed.Add(1);
        }
    }
}
