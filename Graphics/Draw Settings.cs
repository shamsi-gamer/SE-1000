using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void DrawCurrentSetting(List<MySprite> sprites, float x, float y, float w, float h)
        {
            if (CurSet < 0) return;

            var setting = g_settings[CurSet];

            DrawString(sprites, FullNameFromTag(setting.Tag), x + w/2, y, 1f, color5, TaC);

            var nameHeight = 40;

            setting.DrawSetting(
                sprites, 
                x,
                y + nameHeight, 
                w, 
                h - nameHeight, 
                new DrawParams(this));
        }


        static void DrawValueVertical(List<MySprite> sprites, float x, float y, float w, float h, float min, float max, float value, float v, string tag, bool mixer = true)
        {
            var wb = w/10;
            var wg = w/20;
            var wl = w - wg - wb;
            var ws = 20;

            var hk = h/50;
            var sy = 10;

            var zy = y + h/2;// - h * (value - min) / (max - min);


            // value marks
            for (int i = 0; i < 5; i++)
            {
                FillRect(sprites, x, y + h/4 * i, wl, hk, color3);

                var val = max + i * (min - max)/4;

                DrawString(sprites, 
                    printValue(val, -2, val == 0, 0),
                    x, 
                    y + h/4f * i + 4, 
                    0.25f, 
                    color5);
            }


            // current value bar
            FillRect(
                sprites, 
                x + ws,          
                zy, 
                wl - ws, 
                -h/2 * v/64, 
                color4);

            // set value bar
            FillRect(
                sprites, 
                x + w - wb, 
                zy, 
                wb,
                -h/2 * value/64,
                color6);


            // set value number
            DrawString(
                sprites, 
                printValue(v, 2, true, 0), 
                x + wl + 30, 
                zy - sy - 10, 
                1f, 
                color6);
        }


        static void DrawValueHorizontal(List<MySprite> sprites, float x, float y, float w, float h, float min, float max, float value, float v, string tag)
        {
            var hb = h/10;
            var hg = h/20;
            var hl = h - hg - hb;
            var hs = 10;

            var wk = w/50;


            // value marks
            for (int i = 0; i < 5; i++)
            {
                FillRect(sprites, x + w/4 * i, y, wk, hl, color3);

                var val = max + (4 - i) * (min - max)/4;

                DrawString(sprites, 
                    printValue(val, -2, val == 0, 0),
                    x + w/4f * i + 4, 
                    y, 
                    0.25f, 
                    color5);
            }


            // current value bar
            FillRect(
                sprites, 
                x + w/2, 
                y + hs,          
                w/2 * v, 
                hl - hs, 
                color4);

            // set value bar
            FillRect(
                sprites, 
                x + w/2, 
                y + h - hb, 
                w/2 * value,
                hb,
                color6);


            // set value number
            DrawString(
                sprites, 
                printValue(v, 2, true, 0), 
                x + w/2 - 20, 
                y + hl + 30, 
                1f, 
                color6);
        }


        void DrawValueLegend(List<MySprite> sprites, Parameter param, float x, float y, float w, float h, float xt, float rh, Song song, int pat)
        {
            var path  = param.GetPath(CurSrc);

            var lf    = w/3000;
            var vf    = w/800;

            var pow   = 3f;
            var extra = 1.33f;


            // draw legend
            switch (path.Split('/').Last())
            { 
                case "Vol":
                { 
                    for (float f = 0; f <= 1; f += 0.1f)
                    { 
                        var y0 = KeyPos(x, y + rh, w, h - rh, 0, new Key(CurSrc, param, (float)Math.Pow(f, pow) * 2, fN), song).Y;
                        var db = printValue(Math.Abs(100 * (float)Math.Log10(f * extra)), 0, true, 2);

                        DrawLine(sprites, x + xt, y0, x+w, y0, color2);
                        DrawString(sprites, db, x + xt + 3, y0 + lf*2, lf, color2);
                    }

                    break;
                }
                case "Cnt": 
                {
                    for (int i = 0; i <= 20; i += 5)
                    { 
                        var y0  = KeyPos(x, y + rh, w, h - rh, 0, new Key(CurSrc, param, i, fN), song).Y;
                        var val = printValue(i, 0, false, 0);

                        DrawLine(sprites, x+xt, y0, x+w, y0, color2);
                        DrawString(sprites, val, x + xt + 3, y0 + lf*2, lf, color2);
                    }

                    break;
                }
                default:
                {
                    for (int i = 0; i < 5; i++)
                    { 
                        var val = param.Min + i * (param.Max - param.Min)/4;
                        var f   = (val - param.Min) / (param.Max - param.Min);
                        var key = new Key(CurSrc, param, f, fN);
                        var y0  = KeyPos(x, y + rh, w, h - rh, 0, key, song).Y;
                        var str = printValue(val, -2, param.Max - param.Min > 1, 0);

                            
                        DrawLine(sprites, x + xt, y0, x+w, y0, color2);
                        DrawString(sprites, str, x + xt + 3, y0 + lf*2, lf, color2);
                    }

                    break;
                }
            }


            // draw values
                 if (g_paramKeys) DrawParamKeys(sprites, x + xt, y + rh, w-xt, h-rh,       song, pat, CurChan);
            else if (g_paramAuto) DrawParamAuto(sprites, x + xt, y + rh, w-xt, h-rh, w-xt, song, pat, CurChan);


            if (   g_paramKeys
                || g_paramAuto)
                FillRect(sprites, x, y + rh, xt, h - rh, color0); // masks the line above


            // draw value string
            if (g_paramKeys)
            {

            }
            else if (g_paramAuto)
            {
                var key = SelectedChannel.AutoKeys.Find(
                       k => k.Path == path
                    && k.StepTime >= (song.EditPos % g_nSteps) 
                    && k.StepTime <  (song.EditPos % g_nSteps) + 1);

                var strVal = "";
                    
                if (key != null)
                {
                    strVal = GetParamValueString(key.Value, key.Path.Split('/').Last());
                }
                else
                {
                    var src = CurSrc > -1 ? SelectedSource : null;

                    var _param = (Parameter)GetSettingFromPath(SelectedChannel.Instrument, path);
                    var val    = _param.CurValue;

                    strVal = GetParamValueString(val, path.Split('/').Last());
                }

                DrawString(sprites, strVal, x + xt - 30, y + rh + 30, 1f, color6, TextAlignment.RIGHT);
            }
        }


        string GetParamValueString(float val, string paramTag)
        {
            switch (paramTag)
            { 
            case "Vol": return printValue(100 * Math.Log10(val), 0, true, 0) + " dB";
            case "Cnt": return printValue(val, 0, true, 0);
            default:    return printValue(val, 2, true, 0);
            }
        }


        void DrawParamKeys(List<MySprite> sprites, float x, float y, float w, float h, Song song, int p, int ch)
        {
            var wt   = w / g_nSteps;
            var cd   = w/65; // circle diameter
            var dr   = w/250;

            var pat  = song.Patterns[p];
            var chan = pat.Channels[ch];

            var path = g_settings.Last().GetPath(CurSrc);

            foreach (var note in chan.Notes)
            {
                var param = (Parameter)GetSettingFromPath(note.Instrument, path);
                var key   = note.Keys.Find(k => k.Path == path);

                var pt = new Vector2(
                    x + wt * (note.PatStep + note.ShOffset) + wt/2,
                    y + h);

                var p0 = KeyPos(x, y, w, h, p, AltChanKey(key ?? new Key(CurSrc, param, param.GetKeyValue(note, CurSrc), note.PatStep)), song);


                // draw interpolation circle
                if (   note == song.Inter
                    ||    song.Inter != null
                       && note.SongStep >= song.EditPos
                       && note.SongStep <  song.EditPos+1)
                {      
                    FillCircle(sprites, p0.X, p0.Y, cd/2+dr,   color0);
                    FillCircle(sprites, p0.X, p0.Y, cd/2+dr*2, color6);
                }


                var col = key != null ? color6 : color3;


                // draw shadow
                if (   note.SongStep >= song.EditPos
                    && note.SongStep <  song.EditPos+1)
                {
                    DrawLine  (sprites, pt.X, pt.Y, p0.X, p0.Y, color0, w/250+2);
                    FillCircle(sprites, p0.X, p0.Y, cd/2+1, color0);

                    if (key == null) col = color4;
                }

                // draw key
                DrawLine(sprites, pt.X, pt.Y, pt.X, p0.Y, col, w/250);
                FillCircle(sprites, p0.X, p0.Y, cd/2, col);
            }


            var curNote = chan.Notes.Find(n =>
                   n.SongStep >= song.EditPos
                && n.SongStep <  song.EditPos+1);

            // draw interpolation line
            if (   song.Inter != null
                && curNote != null
                && (   song.Inter.PatIndex == p
                    || curNote   .PatIndex == p))
            {
                var pi = ValuePos(x, y, w, h, p, song.Inter, path, song);
                var pc = ValuePos(x, y, w, h, p, curNote,    path, song);

                DrawLine(sprites, pi, pc, color5);
            }
        }


        void DrawParamAuto(List<MySprite> sprites, float x, float y, float w, float h, float wTotal, Song song, int p, int ch)
        {
            var wt = w/g_nSteps;
            var cd = w/65; // circle diameter
            var dr = w/250;

            var pat   = song.Patterns[p];
            var chan  = pat.Channels[ch];
            
            var param = CurParam;
            var path  = param.GetPath(CurSrc);

            var songKeys = song.ChannelAutoKeys[ch].Where(k => k.Path == path).ToList();

            songKeys.Sort((a, b) => a.StepTime.CompareTo(b.StepTime));

            // draw keys
            if (songKeys.Count > 0)
            {
                // draw middle sections
                for (int i = 0; i < songKeys.Count-1; i++)
                {
                    var p0 = KeyPos(x, y, w, h, p, AltChanKey(songKeys[i  ]), song);
                    var p1 = KeyPos(x, y, w, h, p, AltChanKey(songKeys[i+1]), song);
                    DrawLine(sprites, p0, p1, color6);
                }


                // draw key points
                for (int i = 0; i < songKeys.Count; i++)
                {
                    var pc = KeyPos(x, y, w, h, p, AltChanKey(songKeys[i]), song);

                    // draw move circle
                    if (songKeys[i] == g_editKey)
                    {
                        FillCircle(sprites, pc.X, pc.Y, cd/2 + dr*2, color6);
                        FillCircle(sprites, pc.X, pc.Y, cd/2 + dr,   color0);
                    }

                    // draw key point
                    FillCircle(sprites, pc, cd/2, color6);
                }


                // draw first section
                var f1 = KeyPos(x, y, w, h, p, AltChanKey(songKeys[0]), song);
                var prevKey = PrevSongAutoKey(songKeys[0].StepTime, p, ch, path);
                var f0 = prevKey != null ? KeyPos(x, y, w, h, p, prevKey, song) : new Vector2(x, f1.Y);
                DrawLine(sprites, f0, f1, color6);


                // draw last section
                var l0 = KeyPos(x, y, w, h, p, AltChanKey(songKeys.Last()), song);
                var nextKey = NextSongAutoKey(songKeys.Last().StepTime, p, ch, path);
                var l1 = nextKey != null ? KeyPos(x, y, w, h, p, nextKey, song) : new Vector2(x + wTotal, l0.Y);
                DrawLine(sprites, l0, l1, color6);
            }
            else
            {
                var key = AltChanKey(new Key(CurSrc, param, param.Value, fN));
                var pk  = KeyPos(x, y, w, h, p, key, song);

                var p0 = new Vector2(x,        pk.Y);
                var p1 = new Vector2(x+wTotal, pk.Y);

                DrawLine(sprites, p0, p1, color6);
            }
        }


        Key AltChanKey(Key from)
        {
            var val = from.Value;
            //var pow = 3f;

            switch (from.Path.Split('/').Last())
            { 
                //case "Vol": val = (float)Math.Pow(val, pow); break;
                case "Cnt": val /= 4; break;
            }

            return new Key(from.SourceIndex, from.Parameter, val, from.StepTime);
        }


        Vector2 ValuePos(float x, float y, float w, float h, int p, Note note, string path, Song song)
        {
            var param = (Parameter)GetSettingFromPath(note.Instrument, path);
            var val   = param.GetKeyValue(note, CurSrc);

            var wt    = w/g_nSteps;
            var cd    = w/65; // circle diameter

            switch (param.Tag)
            {
                case "Vol":  val /=  2; break;
                case "Tune": val /= 64; break;
                case "Cnt":  val /= 10; break;
            }

            return new Vector2(
                x + wt * (note.PatStep + (note.PatIndex - p)*g_nSteps + note.ShOffset) + wt/2, 
                y + h - h/2 * val - cd/2);
        }


        Vector2 KeyPos(float x, float y, float w, float h, int p, Key key, Song song)
        {
            var chan    = SelectedChannel;
            var inst    = chan.Instrument;
            var setting = GetSettingFromPath(inst, key.Path);
            var val     = key.Value;
            var wt      = w/g_nSteps;

            switch (setting.Tag)
            {
                case "Vol":  val /=  2; break;
                case "Tune": val /= 64; break;
                case "Cnt":  val /= 10; break;
            }


            float yo;

            if (setting.Tag == "Tune") yo = h/2;
            else                       yo = h;

            var kp = new Vector2(
                x + wt * (key.StepTime - p*g_nSteps) + wt/2, 
                y + yo - h * val);

            return kp;
        }
    }
}
