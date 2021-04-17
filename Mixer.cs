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
        void MixerShift()
        {
            g_mixerShift = !g_mixerShift;
            UpdateLight(lblMixerShift, g_mixerShift);
        }


        void EnableChannels(bool on)
        {
            for (int ch = 0; ch < g_nChans; ch++)
                EnableChannel(ch, on);

            MarkLight(
                on
                ? lblMixerAll
                : lblMixerMuteAll);
        }


        void EnableChannel(int ch, bool on)
        {
            int first, last;
            GetPatterns(g_song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
                g_song.Patterns[p].Channels[ch].On = on;
        }


        void Mute(int ch)
        {
            var on = !CurrentPattern.Channels[ch].On;

            int first, last;
            GetPatterns(g_song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
                g_song.Patterns[p].Channels[ch].On = on;

            if (!on)
                g_song.TrimCurrentNotes(ch);

            //mixerPressed.Add(ch);
        }


        void EnableChannel(int pat, int ch, bool on)
        {
            var chan = g_song.Patterns[pat].Channels[ch];
            chan.On = on;
        }


        void Solo(int ch)
        {
            if (g_solo >= 0)
            {
                int _first, _last;
                GetPatterns(g_song, CurPat, out _first, out _last);

                for (int p = _first; p <= _last; p++)
                    UnsoloChannel(p, ch);
            }

            if (ch == g_solo)
            {
                g_solo = -1;
                return;
            }


            for (int _ch = 0; _ch < g_nChans; _ch++)
                g_on[_ch] = CurrentPattern.Channels[_ch].On;


            int first, last;
            GetPatterns(g_song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
                SoloChannel(p, ch);


            g_solo = 
                g_solo == ch
                ? -1
                : ch;

            //mixerPressed.Add(ch);
        }


        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, false);
                g_song.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, true);
        }


        void UnsoloChannel(int pat, int ch)
        {
            if (g_solo >= 0)
            {
                for (int i = 0; i < g_nChans; i++)
                    EnableChannel(pat, i, g_on[i]);
            }
        }
    }
}
