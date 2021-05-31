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

            DrawString(sprites, FullNameFromTag(setting.Tag), x + w/2, y, 1f, color5, TA_CENTER);

            var nameHeight = 40;

            setting.DrawSetting(
                sprites, 
                x,
                y + nameHeight, 
                w, 
                h - nameHeight, 
                new DrawParams(this));
        }


        static void DrawValueVertical(List<MySprite> sprites, float x, float y, float w, float h, float min, float max, float value, float v, string tag, int dec)
        {
            var wb = w/10;
            var wg = w/20;
            var wl = w - wg - wb;
            var ws = 20;

            var hk = h/50;
            var sy = 10;

            var zy = y + h/2;


            // value marks
            for (int i = 0; i < 5; i++)
            {
                FillRect(sprites, x, y + h/4 * i, wl, hk, color3);

                var val = max + i * (min - max)/4;

                DrawString(sprites, 
                    PrintValue(val, -2, val == 0, 0),
                    x, 
                    y + h/4f * i + 4, 
                    0.25f, 
                    color5);
            }


            
            FillRect(sprites, x     + ws, zy, wl - ws, -h/2 * v/64,     color4); // current value bar
            FillRect(sprites, x + w - wb, zy, wb,      -h/2 * value/64, color6); // set value bar


            // set value number
            DrawString(
                sprites, 
                PrintValue(value, dec, True, 0), 
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
                    PrintValue(val, -2, val == 0, 0),
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
                PrintValue(value, 3, True, 0), 
                x + w/2 - 20, 
                y + hl + 30, 
                1f, 
                color6);
        }


        void DrawKeysAndAuto(List<MySprite> sprites, Parameter param, float x, float y, float w, float h, float xt, float rh, Clip clip, int pat)
        {
            var path  = param.GetPath(CurSrc);

            var lf    = w/3000;
            var vf    = w/800;

            var pow   = 3f;
            var extra = 1.33f;


            // draw legend
            switch (path.Split('/').Last())
            { 
                case strVol:
                { 
                    for (float f = 0; f <= 1; f += 0.1f)
                    { 
                        var y0 = KeyPos(x, y + rh, w, h - rh, 0, new Key(CurSrc, param, (float)Math.Pow(f, pow) * 2, float_NaN), clip).Y;
                        var db = PrintValue(Math.Abs(100 * (float)Math.Log10(f * extra)), 0, True, 2);

                        DrawLine(sprites, x + xt, y0, x+w, y0, color2);
                        DrawString(sprites, db, x + xt + 3, y0 + lf*2, lf, color2);
                    }

                    break;
                }
                case strCnt: 
                {
                    for (int i = 0; i <= 20; i += 5)
                    { 
                        var y0  = KeyPos(x, y + rh, w, h - rh, 0, new Key(CurSrc, param, i, float_NaN), clip).Y;
                        var val = PrintValue(i, 0, False, 0);

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
                        var key = new Key(CurSrc, param, f, float_NaN);
                        var y0  = KeyPos(x, y + rh, w, h - rh, 0, key, clip).Y;
                        var str = PrintValue(val, -2, param.Max - param.Min > 1, 0);

                            
                        DrawLine(sprites, x + xt, y0, x+w, y0, color2);
                        DrawString(sprites, str, x + xt + 3, y0 + lf*2, lf, color2);
                    }

                    break;
                }
            }


            // draw values
                 if (EditedClip.ParamKeys) DrawParamKeys(sprites, x + xt, y + rh, w-xt, h-rh,       clip, pat, CurChan);
            else if (EditedClip.ParamAuto) DrawParamAuto(sprites, x + xt, y + rh, w-xt, h-rh, w-xt, clip, pat, CurChan);


            if (   EditedClip.ParamKeys
                || EditedClip.ParamAuto)
                FillRect(sprites, x, y + rh, xt, h - rh, color0); // masks the line above


            // draw value string
            if (EditedClip.ParamKeys)
            {

            }
            else if (EditedClip.ParamAuto)
            {
                var key = SelChannel.AutoKeys.Find(
                       k => k.Path == path
                    && k.StepTime >= (clip.EditPos % g_patSteps) 
                    && k.StepTime <  (clip.EditPos % g_patSteps) + 1);

                var strVal = "";
                    
                if (OK(key))
                {
                    strVal = GetParamValueString(key.Value, key.Path.Split('/').Last());
                }
                else
                {
                    var src = OK(CurSrc) ? SelSource : Source_null;

                    var _param = (Parameter)GetSettingFromPath(SelChannel.Instrument, path);
                    var val    = _param.Value;

                    strVal = GetParamValueString(val, path.Split('/').Last());
                }

                DrawString(sprites, strVal, x + xt - 30, y + rh + 30, 1f, color6, TextAlignment.RIGHT);
            }
        }


        string GetParamValueString(float val, string paramTag)
        {
            switch (paramTag)
            { 
            case strVol: return PrintValue(100 * Math.Log10(val), 0, True, 0) + " dB";
            case strCnt: return PrintValue(val, 0, True, 0);
            default:     return PrintValue(val, 2, True, 0);
            }
        }


        void DrawParamKeys(List<MySprite> sprites, float x, float y, float w, float h, Clip clip, int p, int ch)
        {
            var wt   = w / g_patSteps;
            var cd   = w/ 65; // circle diameter
            var dr   = w/250;

            var pat  = clip.Patterns[p];
            var chan = pat.Channels[ch];

            var path = g_settings.Last().GetPath(CurSrc);

            foreach (var note in chan.Notes)
            {
                var param = (Parameter)GetSettingFromPath(note.Instrument, path);
                var key   = note.Keys.Find(k => k.Path == path);

                var pt = new Vector2(
                    x + wt * (note.Step + note.ShOffset) + wt/2,
                    y + h);

                var p0 = KeyPos(x, y, w, h, p, AltChanKey(key ?? new Key(CurSrc, param, param.GetKeyValue(note, CurSrc), note.Step)), clip);


                // draw interpolation circle
                if (   note == clip.Inter
                    ||    OK(clip.Inter)
                       && note.SongStep >= clip.EditPos
                       && note.SongStep <  clip.EditPos+1)
                {      
                    FillCircle(sprites, p0.X, p0.Y, cd/2+dr,   color0);
                    FillCircle(sprites, p0.X, p0.Y, cd/2+dr*2, color6);
                }


                var col = OK(key) ? color6 : color3;


                // draw shadow
                if (   note.SongStep >= clip.EditPos
                    && note.SongStep <  clip.EditPos+1)
                {
                    DrawLine  (sprites, pt.X, pt.Y, p0.X, p0.Y, color0, w/250+2);
                    FillCircle(sprites, p0.X, p0.Y, cd/2+1, color0);

                    if (!OK(key)) col = color4;
                }

                // draw key
                DrawLine(sprites, pt.X, pt.Y, pt.X, p0.Y, col, w/250);
                FillCircle(sprites, p0.X, p0.Y, cd/2, col);
            }


            var curNote = chan.Notes.Find(n =>
                   n.SongStep >= clip.EditPos
                && n.SongStep <  clip.EditPos+1);

            // draw interpolation line
            if (   OK(clip.Inter)
                && OK(curNote)
                && (   clip.Inter.PatIndex == p
                    || curNote   .PatIndex == p))
            {
                var pi = ValuePos(x, y, w, h, p, clip.Inter, path, clip);
                var pc = ValuePos(x, y, w, h, p, curNote,    path, clip);

                DrawLine(sprites, pi, pc, color5);
            }
        }


        void DrawParamAuto(List<MySprite> sprites, float x, float y, float w, float h, float wTotal, Clip clip, int p, int ch)
        {
            var wt = w/g_patSteps;
            var cd = w/ 65; // circle diameter
            var dr = w/250;

            var pat   = clip.Patterns[p];
            var chan  = pat.Channels[ch];
            
            var param = CurParam;
            var path  = param.GetPath(CurSrc);

            var songKeys = clip.ChannelAutoKeys[ch].Where(k => k.Path == path).ToList();

            songKeys.Sort((a, b) => a.StepTime.CompareTo(b.StepTime));

            // draw keys
            if (songKeys.Count > 0)
            {
                // draw middle sections
                for (int i = 0; i < songKeys.Count-1; i++)
                {
                    var p0 = KeyPos(x, y, w, h, p, AltChanKey(songKeys[i  ]), clip);
                    var p1 = KeyPos(x, y, w, h, p, AltChanKey(songKeys[i+1]), clip);
                    DrawLine(sprites, p0, p1, color6);
                }


                // draw key points
                for (int i = 0; i < songKeys.Count; i++)
                {
                    var pc = KeyPos(x, y, w, h, p, AltChanKey(songKeys[i]), clip);

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
                var f1 = KeyPos(x, y, w, h, p, AltChanKey(songKeys[0]), clip);
                var prevKey = PrevClipAutoKey(songKeys[0].StepTime, p, path);
                var f0 = OK(prevKey) ? KeyPos(x, y, w, h, p, prevKey, clip) : new Vector2(x, f1.Y);
                DrawLine(sprites, f0, f1, color6);


                // draw last section
                var l0 = KeyPos(x, y, w, h, p, AltChanKey(songKeys.Last()), clip);
                var nextKey = NextClipAutoKey(songKeys.Last().StepTime, p, path);
                var l1 = OK(nextKey) ? KeyPos(x, y, w, h, p, nextKey, clip) : new Vector2(x + wTotal, l0.Y);
                DrawLine(sprites, l0, l1, color6);
            }
            else
            {
                var key = AltChanKey(new Key(CurSrc, param, param.Value, float_NaN));
                var pk  = KeyPos(x, y, w, h, p, key, clip);

                var p0 = new Vector2(x,        pk.Y);
                var p1 = new Vector2(x+wTotal, pk.Y);

                DrawLine(sprites, p0, p1, color6);
            }
        }


        Key AltChanKey(Key from)
        {
            var val = from.Value;

            switch (from.Path.Split('/').Last())
            { 
                case strCnt: val /= 4; break;
            }

            return new Key(from.SourceIndex, from.Parameter, val, from.StepTime);
        }


        Vector2 ValuePos(float x, float y, float w, float h, int p, Note note, string path, Clip clip)
        {
            var param = (Parameter)GetSettingFromPath(note.Instrument, path);
            var val   = param.GetKeyValue(note, CurSrc);

            var wt    = w/g_patSteps;
            var cd    = w/65; // circle diameter

            switch (param.Tag)
            {
                case strVol:  val /=  2; break;
                case strTune: val /= 64; break;
                case strCnt:  val /= 10; break;
            }

            return new Vector2(
                x + wt * (note.Step + (note.PatIndex - p)*g_patSteps + note.ShOffset) + wt/2, 
                y + h - h/2 * val - cd/2);
        }


        Vector2 KeyPos(float x, float y, float w, float h, int p, Key key, Clip clip)
        {
            var chan    = SelChannel;
            var inst    = chan.Instrument;
            var setting = GetSettingFromPath(inst, key.Path);
            var val     = key.Value;
            var wt      = w/g_patSteps;

            switch (setting.Tag)
            {
                case strVol:  val /=  2; break;
                case strTune: val /= 64; break;
                case strCnt:  val /= 10; break;
            }


            float yo;

            if (setting.Tag == strTune) yo = h/2;
            else                       yo = h;

            var kp = new Vector2(
                x + wt * (key.StepTime - p*g_patSteps) + wt/2, 
                y + yo - h * val);

            return kp;
        }
    }
}
