using Godot;
using System;

namespace Grao;

// Sons simples gerados por código (sem ficheiros de áudio): explosões, faíscas, fogo, água a chiar,
// fogo de artifício, trovão, campainha e o contador Geiger dos neutrões.
public partial class Sons : Node
{
    const int RATE = 22050;
    readonly Random rng = new(7);
    AudioStreamWav boom, bigBoom, pop, zap, sizzle, buzz, crackle, click, thunder, ui;
    readonly AudioStreamPlayer[] pool = new AudioStreamPlayer[14];
    int poolI;
    AudioStreamPlayer fireLoop, buzzLoop;
    float fireDb = -80, buzzDb = -80;
    double tBoom, tBig, tPop, tZap, tSiz, tThunder;
    public bool Muted { get; private set; }

    public override void _Ready()
    {
        boom = Make(Boom(1.3f, 55, .5f));
        bigBoom = Make(Boom(3.2f, 35, .35f));
        thunder = Make(Thunder());
        pop = Make(Pop());
        zap = Make(Zap());
        sizzle = Make(Sizzle());
        click = Make(Click(), false);
        ui = Make(UiSnd());
        buzz = Make(Buzz(), true);
        crackle = Make(Crackle(), true);
        for (int k = 0; k < pool.Length; k++) { pool[k] = new AudioStreamPlayer(); AddChild(pool[k]); }
        fireLoop = Loop(crackle); buzzLoop = Loop(buzz);
    }

    AudioStreamPlayer Loop(AudioStream s)
    {
        var p = new AudioStreamPlayer { Stream = s, VolumeDb = -80 };
        AddChild(p); p.Play();
        return p;
    }

    public void ToggleMute()
    {
        Muted = !Muted;
        AudioServer.SetBusMute(0, Muted);
    }

    float R() => (float)rng.NextDouble();
    float Nz() => (float)(rng.NextDouble() * 2 - 1);

    void Play(AudioStream s, float db, float pitch = 1)
    {
        var p = pool[poolI++ % pool.Length];
        p.Stream = s; p.VolumeDb = db; p.PitchScale = pitch; p.Play();
    }
    public void Ui() => Play(ui, -10, .9f + R() * .2f);
    public void Spark() { double now = Now(); if (now - tZap > .12) { Play(zap, -14, .9f + R() * .3f); tZap = now; } }
    static double Now() => Time.GetTicksMsec() / 1000.0;
    static float Log2(int n) => MathF.Log2(Math.Max(1, n));

    // chamado uma vez por frame com o mundo que está à vista
    public void Tick(Sim s, int steps)
    {
        double now = Now();
        if (s.evBigBoom > 0 && now - tBig > .5) { Play(bigBoom, 0); tBig = now; tBoom = now; }
        else if (s.evBoom > 0 && now - tBoom > .11) { Play(boom, MathF.Min(0, -8 + Log2(s.evBoom) * 1.6f), .8f + R() * .35f); tBoom = now; }
        if (s.evThunder > 0 && now - tThunder > .3) { Play(thunder, -2, .9f + R() * .2f); tThunder = now; }
        if (s.evPop > 0 && now - tPop > .07) { Play(pop, -7, .85f + R() * .4f); tPop = now; }
        if (s.evZap > 0) Spark();
        if (s.evSizzle > 0 && now - tSiz > .22) { Play(sizzle, MathF.Min(-6, -20 + Log2(s.evSizzle) * 2.5f), .9f + R() * .25f); tSiz = now; }
        // contador Geiger: estalidos ao acaso, mais quantos mais neutrões houver
        int nc = s.NeutronCount;
        if (nc > 0) { float pr = MathF.Min(.9f, nc / 120f + .05f); if (R() < pr) Play(click, -16, .8f + R() * .6f); }
        // fogo e campainha: sons contínuos com volume conforme o que está a acontecer
        float fire = steps > 0 ? s.fireCount / (float)steps : 0;
        float fTarget = fire <= 0 ? -80 : MathF.Min(-6, -32 + 9 * MathF.Log10(1 + fire));
        fireDb += (fTarget - fireDb) * (fTarget > fireDb ? .3f : .08f);
        fireLoop.VolumeDb = fireDb;
        float bTarget = steps > 0 && s.evBuzz > 0 ? -14 : steps == 0 ? buzzDb : -80;
        buzzDb += (bTarget - buzzDb) * .5f;
        buzzLoop.VolumeDb = buzzDb;
        s.evBoom = s.evBigBoom = s.evPop = s.evZap = s.evSizzle = s.evBuzz = s.evThunder = s.fireCount = 0;
    }

    // ---------- síntese ----------
    static AudioStreamWav Make(float[] s, bool loop = false)
    {
        float pk = 1e-6f; foreach (var v in s) pk = MathF.Max(pk, MathF.Abs(v));
        float g = .9f / pk;
        var b = new byte[s.Length * 2];
        for (int i = 0; i < s.Length; i++)
        {
            short v = (short)Math.Clamp((int)(s[i] * g * 32767f), -32768, 32767);
            b[2 * i] = (byte)v; b[2 * i + 1] = (byte)(v >> 8);
        }
        var w = new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = RATE, Stereo = false, Data = b };
        if (loop) { w.LoopMode = AudioStreamWav.LoopModeEnum.Forward; w.LoopBegin = 0; w.LoopEnd = s.Length; }
        return w;
    }

    float[] Boom(float dur, float f0, float bright)
    {
        int n = (int)(dur * RATE); var s = new float[n];
        float lp = 0, lp2 = 0, ph = 0;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)RATE;
            float env = MathF.Exp(-t * 4.5f / dur) * MathF.Min(1, t / .004f);
            float a = bright * MathF.Exp(-t * 7) + .015f;
            lp += (Nz() - lp) * a; lp2 += (lp - lp2) * a;
            ph += f0 * (1 + 1.5f * MathF.Exp(-t * 9)) / RATE;
            s[i] = (lp2 * 3f + MathF.Sin(ph * MathF.Tau) * .7f * MathF.Exp(-t * 5 / dur)) * env;
        }
        return s;
    }
    float[] Thunder()
    {
        var s = Boom(3f, 30, .25f);
        float mod = 1, lp = 0;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)RATE;
            if (i % 2000 == 0) mod = .5f + R() * .8f;
            lp += (Nz() - lp) * .5f;
            s[i] = s[i] * mod + (t < .08f ? lp * (1 - t / .08f) * .8f : 0);
        }
        return s;
    }
    float[] Pop()
    {
        int n = (int)(.9f * RATE); var s = new float[n];
        float ce = 0;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)RATE, v = 0;
            if (t < .05f) v += Nz() * (1 - t / .05f);
            if (R() < .006f * MathF.Exp(-t * 3)) ce = .5f + R() * .5f;
            ce *= .92f; v += Nz() * ce * .6f;
            s[i] = v;
        }
        return s;
    }
    float[] Zap()
    {
        int n = (int)(.3f * RATE); var s = new float[n];
        float ph = 0, gate = 1;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)RATE;
            if (i % 220 == 0) gate = R() < .7f ? 1 : .2f;
            ph += (90 + 50 * MathF.Sin(t * 70)) / RATE;
            float saw = (ph - MathF.Floor(ph)) * 2 - 1;
            s[i] = (saw * .6f + Nz() * .5f) * gate * MathF.Exp(-t * 9);
        }
        return s;
    }
    float[] Sizzle()
    {
        int n = (int)(.7f * RATE); var s = new float[n];
        float prev = 0, fl = 1;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)RATE, nz = Nz(), hp = nz - prev; prev = nz;
            if (i % 300 == 0) fl = .6f + R() * .4f;
            s[i] = hp * fl * MathF.Min(1, t / .04f) * MathF.Exp(-t * 4);
        }
        return s;
    }
    float[] Click()
    {
        int n = 120; var s = new float[n];
        for (int i = 0; i < n; i++) s[i] = Nz() * MathF.Exp(-i / 18f);
        return s;
    }
    float[] UiSnd()
    {
        int n = (int)(.05f * RATE); var s = new float[n];
        for (int i = 0; i < n; i++) { float t = i / (float)RATE; s[i] = MathF.Sin(t * 1400 * MathF.Tau) * MathF.Exp(-t * 90); }
        return s;
    }
    float[] Buzz()
    {
        const int period = 110; // ~200 Hz, períodos inteiros para o loop não estalar
        int n = period * 60; var s = new float[n];
        float lp = 0;
        for (int i = 0; i < n; i++)
        {
            float sq = (i % period) < period / 2 ? 1 : -1, sq2 = (i % (period / 2)) < period / 4 ? .4f : -.4f;
            lp += (sq + sq2 - lp) * .25f;
            s[i] = lp;
        }
        return s;
    }
    float[] Crackle()
    {
        int n = 3 * RATE; var s = new float[n];
        float lp = 0, ce = 0;
        for (int i = 0; i < n; i++)
        {
            lp += (Nz() - lp) * .04f;
            if (R() < .0025f) ce = .4f + R() * .6f;
            ce *= .9f;
            float fade = MathF.Min(1, MathF.Min(i, n - i) / 800f);
            s[i] = lp * .5f * fade + Nz() * ce;
        }
        return s;
    }

    public void Dump(string dir)
    {
        foreach (var (n, w) in new[] { ("boom", boom), ("bigboom", bigBoom), ("thunder", thunder), ("pop", pop), ("zap", zap), ("sizzle", sizzle), ("click", click), ("buzz", buzz), ("crackle", crackle) })
            w.SaveToWav($"{dir}/{n}.wav");
    }
}
