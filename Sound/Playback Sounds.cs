﻿using Sandbox.Game.EntityComponents;
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
        void AddSourceSounds(List<Sound> sounds, Source src, Note note)
        {
            if (!src.On) return;
            src.CreateSounds(sounds, src, note, this);
        }


        void AddSoundEchos(List<Sound> sounds, Sound snd, Delay del, int iSrc)
        {
            if (del == null) return;

            var lTime = g_time - snd.Note.PatTime;
            var sTime = g_time - g_song.StartTime;

            var dc = del.Count.GetValue(g_time, lTime, sTime, snd.FrameLength, snd.Note, snd.SourceIndex, snd.TriggerValues);
            var dt = del.Time .GetValue(g_time, lTime, sTime, snd.FrameLength, snd.Note, snd.SourceIndex, snd.TriggerValues);

            for (int i = 0; i < dc; i++)
            {
                var echoVol = (float)Math.Pow(
                    del.GetVolume(
                        i,
                        g_time,
                        lTime,
                        sTime,
                        snd.FrameLength,
                        snd.Note,
                        iSrc,
                        snd.TriggerValues),
                    2);

                var echo = new Sound(
                    snd.Sample,
                    snd.Note.Channel,
                    snd.Note.iChan,
                    snd.FrameTime,
                    snd.FrameLength,
                    snd.ReleaseLength,
                    snd.TriggerVolume,
                    snd.Instrument,
                    snd.SourceIndex,
                    snd.Note,
                    snd.TriggerValues,
                    true,
                    snd,
                    echoVol,
                    snd.Harmonic,
                    snd.HrmSound,
                    snd.HrmPos);

                echo.FrameTime += (int)((i + 1) * dt * FPS);

                sounds.Add(echo);
            }
        }


        void PlaySounds()
        {
            for (int i = 0; i < g_sounds.Count; i++)
            {
                var snd = g_sounds[i];
                var lTime = g_time - snd.FrameTime;

                if (   lTime >= 0
                    && lTime < snd.FrameLength + snd.ReleaseLength) //+ (snd.Instrument.Volume.Envelope?.Release.GetKeyValue(snd.Note, snd.Source.Index) ?? 0)*FPS
                    UpdateSound(snd);
            }
        }


        void UpdateSound(Sound snd)
        {
            var song = snd.Note.Channel.Pattern.Song;

            var lTime = g_time - snd.FrameTime;
            var sTime = g_time - song.StartTime;


            float vol = 0;

            if (snd.Cache != null) // not echo
            {
                var updateVol = 
                    song.PlayTime < snd.FrameTime + snd.FrameLength + snd.ReleaseLength
                    ? snd.GetVolume(g_time, song.StartTime)
                    : 1;

                vol = snd.TriggerVolume
                    * updateVol
                    * snd.Channel.Volume
                    * g_volume;

                if (snd.Harmonic != null)
                {
                    snd.Harmonic.CurValue = ApplyFilter(
                        snd.Harmonic.CurValue, 
                        snd.Source,
                        snd.HrmPos, 
                        g_time,
                        lTime,
                        sTime,
                        snd.FrameLength, 
                        snd.Note, 
                        snd.SourceIndex,
                        snd.TriggerValues);

                    vol *= snd.Harmonic.CurValue;
                }


                if (snd.HrmSound != null)
                    snd.HrmSound.DisplayVolume = Math.Max(vol, snd.HrmSound.DisplayVolume);
                else
                    snd.DisplayVolume = vol;


                if (lTime < snd.Cache.Length)
                    snd.Cache[lTime] = vol;
            }
            else if (lTime < snd.EchoSource.Cache.Length)
            {
                vol = snd.EchoSource.Cache[lTime]
                    * snd.EchoVolume;
            }


            UpdateSoundSpeakers(snd, vol);


            snd.ElapsedFrameTime = g_time - snd.FrameTime;

            while (snd.ElapsedFrameTime >= MaxSampleLength)
                   snd.ElapsedFrameTime -= MaxSampleLength;
        }


        void UpdateSoundSpeakers(Sound snd, float vol)
        {
            if (snd.Speakers.Count == 0)
            {
                var v = vol;

                while (v-- > 0)
                { 
                    var spk = g_sm.GetSpeaker();

                    if (spk != null)
                    { 
                        spk.Block.SelectedSound = snd.Sample;
                        spk.Block.LoopPeriod = 10;
                        spk.Block.Play();

                        snd.Speakers.Add(spk);
                    }
                }
            }


            foreach (var spk in snd.Speakers)
            {
                spk.Block.Volume = Math.Min(vol--, 1);

                // if sample is ending, restart it //TODO make this smooth
                if (snd.ElapsedFrameTime >= MaxSampleLength)
                {
                    spk.Block.Stop();
                    spk.Block.Play();
                }
            }        
        }


        List<int> StopSounds()
        {
            var delete = new List<int>();

            for (int i = 0; i < g_sounds.Count; i++)
            {
                var snd = g_sounds[i];

                if (g_time > snd.FrameTime + snd.FrameLength + snd.ReleaseLength)// + (snd.Instrument.Volume.Envelope?.Release.GetKeyValue(snd.Note, snd.Source.Index) ?? 0) * FPS)
                {
                    snd.Stop();
                    delete.Add(i);
                }
            }

            return delete;
        }


        void DeleteSounds(List<int> delete)
        {
            for (int i = delete.Count - 1; i >= 0; i--)
            { 
                var snd = g_sounds[delete[i]];

                snd.Note.Sounds.Remove(snd);

                if (snd.Note.Sounds.Count == 0)
                    g_notes.Remove(snd.Note);

                g_sounds.RemoveAt(delete[i]);
            }
        }
    }
}
