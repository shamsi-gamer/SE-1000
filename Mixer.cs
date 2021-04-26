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
            g_clip.MixerShift = !g_clip.MixerShift;
            UpdateLight(lblMixerShift, g_clip.MixerShift);
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
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                g_clip.Patterns[p].Channels[ch].On = on;
        }


        void Mute(int ch)
        {
            var on = !g_clip.CurrentPattern.Channels[ch].On;

            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                g_clip.Patterns[p].Channels[ch].On = on;

            if (!on)
                g_clip.TrimCurrentNotes(ch);
        }


        void EnableChannel(int pat, int ch, bool on)
        {
            var chan = g_clip.Patterns[pat].Channels[ch];
            chan.On = on;
        }


        void Solo(int ch)
        {
            if (g_clip.Solo >= 0)
            {
                int _first, _last;
                g_clip.GetCurPatterns(out _first, out _last);

                for (int p = _first; p <= _last; p++)
                    UnsoloChannel(p, ch);
            }

            if (ch == g_clip.Solo)
            {
                g_clip.Solo = -1;
                return;
            }


            for (int _ch = 0; _ch < g_nChans; _ch++)
                g_on[_ch] = g_clip.CurrentPattern.Channels[_ch].On;


            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                SoloChannel(p, ch);

            g_clip.Solo = g_clip.Solo == ch ? -1 : ch;
        }


        void SoloChannel(int pat, int ch)
        {
            for (int i = 0; i < g_nChans; i++)
            {
                if (i == ch) continue;
                EnableChannel(pat, i, false);
                g_clip.TrimCurrentNotes(i);
            }

            EnableChannel(pat, ch, true);
        }


        void UnsoloChannel(int pat, int ch)
        {
            if (g_clip.Solo >= 0)
            {
                for (int i = 0; i < g_nChans; i++)
                    EnableChannel(pat, i, g_on[i]);
            }
        }
    }
}
