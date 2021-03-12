using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Song
        {
            public string        Name;

            public Arpeggio      Arpeggio; // indicates that this song is an arpeggio

            public List<Pattern> Patterns;
            public List<Block>   Blocks;

            public List<Key>[]   ChannelAutoKeys = new List<Key>[nChans];

            public long          StartTime; // in ticks

            public int           Length,

                                 CurChan,
                                 SelChan,
                                 CurSrc,
                                 
                                 PlayPat,
                                 CurPat;

            public float         PlayStep,
                                 EditPos,
                                 LastEditPos;
            
            public List<Note>    EditNotes;
            public Note          Inter;


            public long PlayTime { get { return (int)(PlayStep * g_ticksPerStep); } }


            public Song(string name = "Song 1")
            {
                Name     = name;

                Arpeggio = null;
                Length   = -1;

                Patterns = new List<Pattern>();
                Blocks   = new List<Block>();

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>();

                EditNotes = new List<Note>();
                
                ResetState();
            }


            public Song(Song song)
            {
                Name     = song.Name;

                Arpeggio = song.Arpeggio;
                Length   = song.Length;

                Patterns = new List<Pattern>();
                foreach (var pat in song.Patterns)
                { 
                    Patterns.Add(new Pattern(pat));
                    Patterns.Last().Song = this;
                }

                Blocks = new List<Block>();
                foreach (var b in song.Blocks)
                    Blocks.Add(new Block(b));

                for (int i = 0; i < ChannelAutoKeys.Length; i++)
                    ChannelAutoKeys[i] = new List<Key>(song.ChannelAutoKeys[i]);

                EditNotes = new List<Note>();
                
                ResetState();
            }


            public void ClearAudoKeys()
            {
                foreach (var keys in ChannelAutoKeys)
                    keys.Clear();
            }


            public void UpdateAutoKeys()
            {
                for (int ch = 0; ch < nChans; ch++)
                { 
                    var chanKeys = ChannelAutoKeys[ch];

                    chanKeys.Clear();

                    for (int p = 0; p < Patterns.Count; p++)
                    {
                        var keys = Patterns[p].Channels[ch].AutoKeys;

                        for (int k = 0; k < keys.Count; k++)
                        { 
                            chanKeys.Add(new Key(
                                keys[k].SourceIndex,
                                keys[k].Parameter,
                                keys[k].Value, 
                                keys[k].StepTime + p*nSteps,
                                keys[k].Channel));
                        }
                    }

                    chanKeys.Sort((a, b) => a.StepTime.CompareTo(b.StepTime));
                }
            }


            public void Clear()
            {
                Name = "";

                Patterns.Clear();
                Blocks.Clear();

                foreach (var keys in ChannelAutoKeys)
                    keys.Clear();

                ResetState();
            }


            void ResetState()
            {
                CurChan     =  0;
                SelChan     = -1;
                CurSrc      = -1;
                            
                PlayStep    =  float.NaN;
                PlayPat     =  0;
                CurPat      =  0;

                StartTime   = -1;

                EditPos     = float.NaN;
                LastEditPos = float.NaN;

                Inter       = null;
                EditNotes.Clear();
            }


            public int   GetNotePat(Note note) { return Patterns.FindIndex(p => p.Channels.Contains(note.Channel)); }
            public float GetStep   (Note note) { return GetNotePat(note) * nSteps + note.PatStepTime; }

            public int   GetKeyPat (Key key)   { return Patterns.FindIndex(p => p.Channels.Find(c => c.AutoKeys.Contains(key)) != null); }
            public float GetStep   (Key key)   { return GetKeyPat(key) * nSteps + key.StepTime; }


            public Block GetBlock(int pat)
            {
                return Blocks.Find(b =>
                       pat >= b.First
                    && pat <= b.Last);
            }
        }


        /////////////////////////////////////////////////////////////////////////////


        void InitSong()
        {
            //if (!LoadSong(Me.CustomData))
                NewSong();
        }


        void NewSong()
        {
            ClearSong();

            g_volume = 1;

            g_ticksPerStep = 7;

            g_inst.Clear();

            g_inst.Add(new Instrument());
            g_inst[0].Sources.Add(new Source(g_inst[0]));
            g_song.Patterns.Add(new Pattern(g_song, g_inst[0]));

            g_song.Name = "New Song";

            UpdateSongDsp();

            g_volume      = 1;
            g_chord       = -1;
            g_chordEdit   = false;
            g_chordSpread = 0;
            g_editStep    = 2;
            g_editLength  = 2;
            g_curNote     = 69 * NoteScale;
            showNote      = g_curNote;
                         
            loopPat       = false;
            g_block       = false;
            g_in          = false;
            g_out         = false;
            movePat       = false;
            allPats       = false;
            g_autoCue     = false;
            g_halfSharp   = false;
            g_follow      = false;
            allChan       = false;
            g_piano       = false;
            g_move        = false;
            g_shift       = false;
            g_mixerShift  = false;
            g_hold        = false;
            g_chordMode   = false;
            g_chordAll    = false;
            g_pick        = false;
            rndInst       = false;
            g_paramKeys   = false;
            g_paramAuto   = false;


            g_chords = new List<int>[4];

            for (int i = 0; i < g_chords.Length; i++)
                g_chords[i] = new List<int>();
            

            ClearAllMem();

            SetLightColor(4);
            UpdateLights();
        }


        void ClearSong()
        {
            g_sm.StopAll();

            g_notes.Clear();
            g_sounds.Clear();

            g_song.Clear();
        }


        void ToggleMovePattern()
        {
            movePat = !movePat;

            UpdateLight(lblPrevPat, movePat);
            UpdateLight(lblNextPat, movePat);
            UpdateLight(lblMovePat, movePat);

            if (movePat)
                DisableBlock();
        }
    }
}
