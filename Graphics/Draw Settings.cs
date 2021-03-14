using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void DrawSettings(List<MySprite> sprites, float x, float y, float w, float h)
        {
            Setting setting;

            if (curSet > -1)
            { 
                setting = g_settings[curSet];

                DrawString(sprites, setting.Name, x + 15, y + 30, 1f, color5);

                var bx = 40;
                var by = 55;

                if (IsParam(setting))
                {
                    var param = (Parameter)setting;

                    if (   param.Tag == "Att"
                        || param.Tag == "Dec"
                        || param.Tag == "Sus"
                        || param.Tag == "Rel")
                    {
                        DrawEnvelope(sprites, (Envelope)g_settings[curSet-1], x + 15, y + 110, w - 15, 140, 1);
                    }
                    else if (param.Tag == "Amp"
                          || param.Tag == "Freq"
                          ||    param.Parent != null // LFO offset has a parent
                             && param.Tag == "Off")
                    {
                        DrawLfo(sprites, (LFO)g_settings[curSet-1], x + bx, y + by + 90, w - bx * 2, 110, true, true);
                    }
                    else if (param.Tag == "Cnt"
                          || param.Tag == "Time"
                          || param.Tag == "Lvl"
                          || param.Tag == "Pow")
                    {
                        DrawDelay(sprites, (Delay)g_settings[curSet-1], x + 15, y + 140, w - 30, 110, true);
                    }
                    else if (param.Tag == "Cut"
                          || param.Tag == "Res")
                    {
                        DrawFilter(sprites, (Filter)g_settings[curSet-1], x + 15, y + 140, w - 30, 110, true);
                    }
                    else if (param.Tag == "Vol")
                    {
                        DrawSoundLevel(sprites, x + bx + 20, y + by + 90, 60, 120, 
                            param.Value,
                            param.CurValue);

                        var db = 100 * Math.Log10(param.Value);
                        var strDb = printValue(db, 0, true, 0) + " dB";

                        // draw value 
                        DrawString(sprites, strDb, x + bx + 100, y + by + 180, 1f, color6);
                    }
                    else if (param.Parent == null // source offset has no parent
                          && param.Tag    == "Off")
                    {
                        DrawValueHorizontal(sprites, x + bx + 5, y + by + 90, 180, 50, param.Min, param.Max, 
                            param.Value,
                            param.CurValue,
                            param.Tag);
                    }
                    else
                    { 
                        DrawValueVertical(sprites, x + bx + 20, y + by + 90, 60, 120, param.Min, param.Max,
                            param.Value,
                            param.CurValue,
                            param.Tag,
                            false);
                    }
                }
                else if (setting.GetType() == typeof(Envelope))
                {
                    DrawEnvelope(sprites, (Envelope)setting, x + 15, y + 110, w - 15, 140, 1);
                }
                else if (setting.GetType() == typeof(Delay))
                {
                    DrawDelay(sprites, (Delay)setting, x + 15, y + 120, w - 30, 110, true);
                }
                else if (setting.GetType() == typeof(Filter))
                {
                    DrawFilter(sprites, (Filter)setting, x + 15, y + 120, w - 30, 110, true);
                }
                else if (setting.GetType() == typeof(LFO)) 
                {
                    DrawLfo(sprites, (LFO)setting, x + bx, y + by + 90, w - bx*2, 110, true, true);
                }
            }
        }


        void DrawSetting(List<MySprite> sprites, Setting setting, float x, float y, ref float yo, bool active)
        {
            bool draw = sprites != null;


            var  sh = 18f;

            var  xo =  8f;
            var _yo =  0f;


            float ew = 0;

            if (draw)
            { 
                bool thisSetting = CurSetting == setting;

                var textCol = 
                    active
                    ? color6
                    : color0;

                var boxCol = 
                    active
                    ? (thisSetting ? color1 : color4)
                    : (thisSetting ? color6 : color3);


                if (   setting != null
                    && !HasTag(setting, "Vol")
                    && !HasTag(setting, "Off")
                    && setting.GetType() != typeof(Tune)
                    && setting.GetType() != typeof(Harmonics)
                    && setting.GetType() != typeof(Filter)
                    && setting.GetType() != typeof(Delay)
                    && setting.GetType() != typeof(Arpeggio))
                { 
                    if (yo == 0)
                        DrawLine(sprites, x, y + sh/2, x + xo, y + sh/2, boxCol);
                    else
                    {
                        DrawLine(sprites, x - ew/2, y - yo+sh - 3, x - ew/2, y + sh/2, boxCol);
                        DrawLine(sprites, x - ew/2, y + sh/2,      x + xo,   y + sh/2, boxCol);
                    }
                }


                if (IsParam(setting))
                {
                    if (setting.Tag == "Vol") ew = 72f;
                    else                      ew = 85f; 
                }
                else
                    ew = 30f;


                x += xo;


                if (IsParam(setting))
                {
                    var param = (Parameter)setting;

                    // setting name
                    FillRect(sprites, x, y, ew, 15, boxCol);

                    DrawString(sprites, setting.Tag, x +  5, y + 1, 0.36f, textCol);
                 
                    string str;
                    
                    if (param.Tag == "Vol")
                    {
                        var db = 100 * Math.Log10(param.Value);
                        str = printValue(db, 0, true, 0).PadLeft(4);
                    }
                    else
                        str = printValue(param.Value, 2, true, 1).PadLeft(6);

                    DrawString(sprites, str, x + 40, y + 1, 0.36f, textCol);
                }
                else
                {
                    // setting name
                    FillRect(sprites, x, y, ew, 15, boxCol);
                    DrawString(sprites, setting.Tag, x + 5, y + 1, 0.36f, textCol);
                }

                x += ew;
            }


            bool children = false;

                 if (setting.GetType() == typeof(Parameter)) DrawParamSetting    (sprites, (Parameter)setting, x, y, ref _yo, active, ref children);
            else if (setting.GetType() == typeof(Envelope )) DrawEnvelopeSetting (sprites, (Envelope )setting, x, y, ref _yo, active, ref children);
            else if (setting.GetType() == typeof(LFO      )) DrawLfoSetting      (sprites, (LFO      )setting, x, y, ref _yo, active, ref children);
            else if (setting.GetType() == typeof(Harmonics)) DrawHarmonicsSetting(sprites, (Harmonics)setting, x, y, ref _yo, active, ref children);
            else if (setting.GetType() == typeof(Filter   )) DrawFilterSetting   (sprites, (Filter   )setting, x, y, ref _yo, active, ref children);
            else if (setting.GetType() == typeof(Delay    )) DrawDelaySetting    (sprites, (Delay    )setting, x, y, ref _yo, active, ref children);
            else if (setting.GetType() == typeof(Arpeggio )) DrawArpeggioSetting (sprites, (Arpeggio )setting, x, y, ref _yo, active, ref children);


            if (!children)
                _yo += sh;

            yo += _yo;
        }


        void DrawParamSetting(List<MySprite> sprites, Parameter param, float x, float y, ref float yo, bool active, ref bool children)
        { 
            if (param.Envelope != null) { DrawSetting(sprites, param.Envelope, x, y + yo, ref yo, active); children = true; }
            if (param.Lfo      != null) { DrawSetting(sprites, param.Lfo,      x, y + yo, ref yo, active); children = true; }
        }


        void DrawEnvelopeSetting(List<MySprite> sprites, Envelope env, float x, float y, ref float yo, bool active, ref bool children)
        { 
            if (env.Attack .HasDeepParams(null, CurSrc)) { DrawSetting(sprites, env.Attack,  x, y + yo, ref yo, active); children = true; }
            if (env.Decay  .HasDeepParams(null, CurSrc)) { DrawSetting(sprites, env.Decay,   x, y + yo, ref yo, active); children = true; }
            if (env.Sustain.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, env.Sustain, x, y + yo, ref yo, active); children = true; }
            if (env.Release.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, env.Release, x, y + yo, ref yo, active); children = true; }
        }


        void DrawLfoSetting(List<MySprite> sprites, LFO lfo, float x, float y, ref float yo, bool active, ref bool children)
        { 
            if (lfo.Frequency.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, lfo.Frequency, x, y + yo, ref yo, active); children = true; }                
            if (lfo.Amplitude.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, lfo.Amplitude, x, y + yo, ref yo, active); children = true; }
            if (lfo.Offset   .HasDeepParams(null, CurSrc)) { DrawSetting(sprites, lfo.Offset,    x, y + yo, ref yo, active); children = true; }
        }


        void DrawHarmonicsSetting(List<MySprite> sprites, Harmonics hrm, float x, float y, ref float yo, bool active, ref bool children)
        { 
            for (int i = 0; i < hrm.Tones.Length; i++)
                if (hrm.Tones[i].HasDeepParams(null, CurSrc)) { DrawSetting(sprites, hrm.Tones[i], x, y + yo, ref yo, active); children = true; }
        }


        void DrawFilterSetting(List<MySprite> sprites, Filter flt, float x, float y, ref float yo, bool active, ref bool children)
        { 
            if (flt.Cutoff   .HasDeepParams(null, CurSrc)) { DrawSetting(sprites, flt.Cutoff,    x, y + yo, ref yo, active); children = true; }
            if (flt.Resonance.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, flt.Resonance, x, y + yo, ref yo, active); children = true; }
        }


        void DrawDelaySetting(List<MySprite> sprites, Delay del, float x, float y, ref float yo, bool active, ref bool children)
        {
            if (del.Count.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, del.Count, x, y + yo, ref yo, active); children = true; }
            if (del.Time .HasDeepParams(null, CurSrc)) { DrawSetting(sprites, del.Time,  x, y + yo, ref yo, active); children = true; }
            if (del.Level.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, del.Level, x, y + yo, ref yo, active); children = true; }
            if (del.Power.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, del.Power, x, y + yo, ref yo, active); children = true; }
        }


        void DrawArpeggioSetting(List<MySprite> sprites, Arpeggio arp, float x, float y, ref float yo, bool active, ref bool children)
        { 
            if (arp.Length.HasDeepParams(null, CurSrc)) { DrawSetting(sprites, arp.Length, x, y + yo, ref yo, active); children = true; }
            if (arp.Scale .HasDeepParams(null, CurSrc)) { DrawSetting(sprites, arp.Scale,  x, y + yo, ref yo, active); children = true; }
        }


        void DrawValueVertical(List<MySprite> sprites, float x, float y, float w, float h, float min, float max, float value, float v, string tag, bool mixer = true)
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


        void DrawValueHorizontal(List<MySprite> sprites, float x, float y, float w, float h, float min, float max, float value, float v, string tag)
        {
            var hb = h/10;
            var hg = h/20;
            var hl = h - hg - hb;
            var hs = 10;

            var wk = w/50;
            var sx = 10;

            var zx = x + w/2;


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
                zx, 
                y + hs,          
                w/2 * v, 
                hl - hs, 
                color4);

            // set value bar
            FillRect(
                sprites, 
                zx, 
                y + h - hb, 
                w/2 * value,
                hb,
                color6);


            // set value number
            DrawString(
                sprites, 
                printValue(v, 2, true, 0), 
                zx - sx - 10, 
                y + hl + 30, 
                1f, 
                color6);
        }


        void DrawLfo(List<MySprite> sprites, LFO lfo, float x, float y, float w, float h, bool active, bool on)
        {
            var pPrev = new Vector2(float.NaN, float.NaN);

            var fs = 0.5f;


            var amp  = lfo.Amplitude.CurValue;
            var freq = lfo.Frequency.CurValue;
            var off  = lfo.Offset   .CurValue;


            var isAmp  = IsCurParam("Amp");
            var isFreq = IsCurParam("Freq");
            var isOff  = IsCurParam("Off");


            DrawLine(sprites, x, y,   x,   y+h, isAmp  ? color6 : color3);
            DrawLine(sprites, x, y+h, x+w, y+h, isFreq ? color6 : color3);



            // draw current value
            var startTime = PlayTime > -1 ? StartTime : 0;
            var lTime     = PlayTime > -1 ? g_time - startTime : 0;

            var val  = lfo.CurValue;
            var blur = lfo.Type == LFO.LfoType.Noise ? Math.Pow(freq, 4) : 1;

            var ty   = (float)Math.Max(y,   y + h/2 - val*h/2 - blur);
            var by   = (float)Math.Min(y+h, y + h/2 - val*h/2 + blur*2);

            var col  = new Color(
                color4.R,
                color4.G,
                color4.B,
                (int)(Math.Pow(1/freq, 2.5)*0xFF));

            FillRect(sprites, x, ty, w, Math.Max(2, by-ty), col);


            // draw the waveform
            for (long f = 0; f < FPS; f++)
            {
                var v = lfo.GetValue(
                    g_time + f, 
                    lTime + f, 
                    g_time - startTime + f, 
                    (int)(EditLength * g_ticksPerStep), 
                    null, 
                    -1,
                    _triggerDummy);

                var p = new Vector2(
                    x + w * f/(float)FPS,
                    y + h/2 - v*h/2);

                if (   OK(pPrev.X)
                    && OK(pPrev.Y))
                    DrawLine(sprites, pPrev, p, color4, 2);

                pPrev = p;
            }


            // amplitude label
            DrawString(
                sprites, 
                amp.ToString(".00"), 
                x + w/2, 
                y + h/2 - h/2*amp - 20, 
                fs, 
                isAmp ? color6 : color3,
                TextAlignment.CENTER);


            // frequency label
            DrawString(
                sprites, 
                (Math.Pow(2, freq)-1).ToString(".00") + (isFreq ? " Hz" : ""),
                x + w/2,
                y + h + 3,
                fs,
                isFreq ? color6 : color3,
                TextAlignment.CENTER);


            // offset label
            DrawString(
                sprites, 
                off.ToString(".00") + (isOff ? " s" : ""),
                x,
                y + h + 3,
                fs,
                isOff ? color6 : color3,
                TextAlignment.CENTER);
        }


        void DrawEnvelope(List<MySprite> sprites, Envelope env, float x, float y, float w, float h, float vol)
        {
            var sTime = 
                StartTime > -1
                ? g_time - StartTime
                : 0;

            var len = (int)(EditLength * g_ticksPerStep);

            var a = env.Attack .GetValue(g_time, 0, sTime, len, null, -1, _triggerDummy);
            var d = env.Decay  .GetValue(g_time, 0, sTime, len, null, -1, _triggerDummy);
            var s = env.Sustain.GetValue(g_time, 0, sTime, len, null, -1, _triggerDummy);
            var r = env.Release.GetValue(g_time, 0, sTime, len, null, -1, _triggerDummy);
                                                                           

            var xoff  = 20;
            var b     = 18;
            var v     = Math.Min(vol, 1);

            var fs    = 0.5f;
            var scale = 1f;
            var fps   = FPS * scale;

            var x0    = x;// + xoff + b + 1;

            var p0    = new Vector2(x0/* + fps * Offset*/, y + h - b);
                p0.X  = Math.Min(p0.X, x0 + w - b * 2);

            var p1    = new Vector2(p0.X + fps * a, p0.Y - (h - b * 2) * v);
                p1.X  = Math.Min(p1.X, x0 + w - b * 2);

            var p2    = new Vector2(p1.X + fps * d, p0.Y - (h - b * 2) * v * s);
                p2.X  = Math.Min(p2.X, x0 + w - b * 2);

            var p3    = new Vector2(x + xoff + w - b * 2 - fps * r, p2.Y);
            var p4    = new Vector2(x + xoff + w - b * 2, p0.Y);


            var isAtt = IsCurParam("Att");
            var isDec = IsCurParam("Dec");
            var isSus = IsCurParam("Sus");
            var isRel = IsCurParam("Rel");



            var wa = isAtt ? 6 : 1;
            var wd = isDec ? 6 : 1;
            var ws = isSus ? 6 : 1;
            var wr = isRel ? 6 : 1;


            // draw envelope supports and info

            var sw = 1;

            DrawLine(sprites, p0.X, p0.Y, p0.X, y,         color3, sw);
            DrawLine(sprites, p2.X, p2.Y, p2.X, y + h - b, color3, sw);
            DrawLine(sprites, p1.X, p1.Y, p1.X, y + h - b, color3, sw);
            DrawLine(sprites, p3.X, p3.Y, p3.X, y + h - b, color3, sw);
            DrawLine(sprites, p1.X, p2.Y, p3.X, p3.Y,      color3, sw);
                                                              
            DrawLine(sprites, p0.X, p0.Y, p4.X, p4.Y,      color3, sw);


            // draw labels

            DrawString(sprites, a.ToString(".00") + (isAtt ? " s" : ""),  p0.X + 6,             p0.Y +  3,         fs, isAtt ? color6 : color3, TextAlignment.CENTER);
            DrawString(sprites, d.ToString(".00") + (isDec ? " s" : ""), (p1.X + p2.X)/2 + 16, (p1.Y+p2.Y)/2 - 20, fs, isDec ? color6 : color3, TextAlignment.CENTER);
            DrawString(sprites, s.ToString(".00"),                       (p2.X + p3.X)/2 - 5,   p2.Y - 20,         fs, isSus ? color6 : color3, TextAlignment.CENTER);
            DrawString(sprites, r.ToString(".00") + (isRel ? " s" : ""), (p3.X + p4.X)/2 - 5,   p0.Y +  3,         fs, isRel ? color6 : color3, TextAlignment.CENTER);


            // draw the envelope

            DrawLine(sprites, p0, p1, color6, wa);
            DrawLine(sprites, p1, p2, color6, wd);
            DrawLine(sprites, p2, p3, color6, ws);
            DrawLine(sprites, p3, p4, color6, wr);

            if (isDec && d < 0.01)
                FillRect(sprites, p1.X-4, p1.Y-4, 8, 8, color6);
        }


        void DrawDelay(List<MySprite> sprites, Delay del, float x, float y, float w, float h, bool active)
        {
            var b = 30;


            FillRect(sprites, x, y + h - b - 1, w, 2, color3);

            var dc = del.Count.CurValue;
            var dt = del.Time .CurValue * 100;
            var dl = del.Level.CurValue;
            var dp = del.Power.CurValue;


            var fs = 0.5f;

            var dx = 0f;


            FillRect(sprites, x, y + h - b, 4, -(h - b*2), color3);


            for (int i = 0; i < (int)dc && dx < w - dt; i++)
            {
                dx = (i+1) * dt;

                FillRect(sprites, 
                    x + dx, 
                    y + h - b, 
                    4, 
                    -(h - b*2) * del.GetVolume(i, g_time, 0, 0, (int)(EditLength * g_ticksPerStep), null, CurSrc, _triggerDummy),
                    color4);
            }


            // count
            DrawString(
                sprites, 
                S(Math.Round(dc)), 
                x, // + Math.Min(dt * dc, w), 
                y + h - b + 8, 
                fs, 
                IsCurParam("Cnt") ? color6 : color3);


            if (dc > 0)
            { 
                // time
                DrawString(
                    sprites, 
                    Math.Round(dt*10).ToString("0") + " ms", 
                    x + 60, 
                    y + h - b + 8, 
                    fs,
                    IsCurParam("Time") ? color6 : color3, 
                    TextAlignment.CENTER);


                // level
                var lx = x + dt + 2;

                DrawString(
                    sprites, 
                    dl.ToString("0.00"), 
                    lx, 
                    y + h - b - (h - b*2) * dl - 24, 
                    fs,
                    IsCurParam("Lvl") ? color6 : color3, 
                    TextAlignment.CENTER);


                // power
                var px  = x + MinMax(70, dt*dc/2, w);
                var dim = dc > 1 && Math.Abs(px - lx) > 20 ? color6 : color3;

                var vol = del.GetVolume(Math.Max(0, (int)dc/2 - 1), 0, 0, 0, (int)(EditLength * g_ticksPerStep), null, CurSrc, _triggerDummy);

                DrawString(
                    sprites, 
                    dp.ToString("0.00"),
                    px,
                    y + h - b - (h - b*2) * vol - 24,
                    fs,
                    IsCurParam("Pow") ? color6 : color3,
                    TextAlignment.CENTER);
            }
        }


        void DrawFilter(List<MySprite> sprites, Filter flt, float x, float y, float w, float h, bool active)
        {
            var cut = flt.Cutoff   .CurValue;
            var res = flt.Resonance.CurValue;


            var fs = 0.5f;

        //    var dx = 0f;


            FillRect(sprites, x, y + h, 2, -h, color3);
            FillRect(sprites, x, y + h, w,  2, color3);


            DrawFilter(sprites, x, y, w, h, color5, 4, cut, res);


            // cutoff
            DrawString(sprites, "Cutoff",                    x, y - 40, fs, IsCurParam("Cut") ? color6 : color3);
            DrawString(sprites, printValue(cut, 2, true, 0), x, y - 25, fs, IsCurParam("Cut") ? color6 : color3);

            // resonance
            DrawString(sprites, "Resonance",                 x + 100, y - 40, fs, IsCurParam("Res") ? color6 : color3);
            DrawString(sprites, printValue(res, 2, true, 0), x + 100, y - 25, fs, IsCurParam("Res") ? color6 : color3);
        }


        void DrawParamValues(List<MySprite> sprites, Parameter param, float x, float y, float w, float h, float xt, float rh, Song song, int pat)
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
                        var y0 = KeyPos(x, y + rh, w, h - rh, 0, new Key(CurSrc, param, (float)Math.Pow(f, pow) * 2, float.NaN), song).Y;
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
                        var y0  = KeyPos(x, y + rh, w, h - rh, 0, new Key(CurSrc, param, i, float.NaN), song).Y;
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
                        var key = new Key(CurSrc, param, f, float.NaN);
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
                    && k.StepTime >= (song.EditPos % nSteps) 
                    && k.StepTime <  (song.EditPos % nSteps) + 1);

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
            var wt   = w / nSteps;
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
                       && song.GetStep(note) >= song.EditPos
                       && song.GetStep(note) <  song.EditPos+1)
                {      
                    FillCircle(sprites, p0.X, p0.Y, cd/2+dr,   color0);
                    FillCircle(sprites, p0.X, p0.Y, cd/2+dr*2, color6);
                }


                var col = key != null ? color6 : color3;


                // draw shadow
                if (   song.GetStep(note) >= song.EditPos
                    && song.GetStep(note) <  song.EditPos+1)
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
                   song.GetStep(n) >= song.EditPos
                && song.GetStep(n) <  song.EditPos+1);

            // draw interpolation line
            if (   song.Inter != null
                && curNote != null
                && (   song.GetNotePat(song.Inter) == p
                    || song.GetNotePat(curNote) == p))
            {
                var pi = ValuePos(x, y, w, h, p, song.Inter, path, song);
                var pc = ValuePos(x, y, w, h, p, curNote,    path, song);

                DrawLine(sprites, pi, pc, color5);
            }
        }


        void DrawParamAuto(List<MySprite> sprites, float x, float y, float w, float h, float wTotal, Song song, int p, int ch)
        {
            var wt = w/nSteps;
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
                var key = AltChanKey(new Key(CurSrc, param, param.Value, float.NaN));
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

            var wt    = w/nSteps;
            var cd    = w/65; // circle diameter

            switch (param.Tag)
            {
                case "Vol":  val /=  2; break;
                case "Tune": val /= 64; break;
                case "Cnt":  val /= 10; break;
            }

            return new Vector2(
                x + wt * (note.PatStep + (song.GetNotePat(note) - p)*nSteps + note.ShOffset) + wt/2, 
                y + h - h/2 * val - cd/2);
        }


        Vector2 KeyPos(float x, float y, float w, float h, int p, Key key, Song song)
        {
            var chan    = SelectedChannel;
            var inst    = chan.Instrument;
            var setting = GetSettingFromPath(inst, key.Path);
            var val     = key.Value;
            var wt      = w/nSteps;

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
                x + wt * (key.StepTime - p*nSteps) + wt/2, 
                y + yo - h * val);

            return kp;
        }
    }
}
