using System;
using static Grao.Mat;

namespace Grao;

// Motor da simulação (CPU): grelha de células + temperatura + eletricidade + neutrões + pessoas.
// Só processa chunks 16x16 acordados.
public sealed class Sim
{
    public static float NPfast = .012f, NPslow = .07f, NPheat = 400f, NPspont = .0001f;

    public readonly int W, H, N;
    public readonly byte[] T, Sh, L, E, S;
    public readonly float[] Tm;
    const int CS = 16;
    readonly int CW, CH, NCh;
    readonly ushort[] chunkOf;
    readonly byte[] wakeA, act;
    int tick = 1;
    uint rs;

    // neutrões
    public const int NMAX = 6000;
    public readonly float[] nX = new float[NMAX], nY = new float[NMAX];
    readonly float[] nVX = new float[NMAX], nVY = new float[NMAX];
    readonly ushort[] nL = new ushort[NMAX];
    readonly byte[] nS = new byte[NMAX];
    int nc;
    public int NeutronCount => nc;

    // pessoas: bonecos 3x7 (pX = coluna do centro, pY = linha dos pés)
    public const int PMAX = 400;
    public readonly float[] pX = new float[PMAX], pY = new float[PMAX], pHP = new float[PMAX], pAir = new float[PMAX],
        pTemp = new float[PMAX], pBurn = new float[PMAX], pWalk = new float[PMAX], pVirus = new float[PMAX];
    readonly float[] pVX = new float[PMAX], pVY = new float[PMAX];
    public readonly sbyte[] pDir = new sbyte[PMAX];
    public readonly byte[] pLook = new byte[PMAX * 4];
    int pc;
    public int PeopleCount => pc;
    readonly short[] occ;
    readonly int[] occList = new int[PMAX * 21];
    int occN;

    // eventos para os sons (o Main lê e põe a zero)
    public int evBoom, evBigBoom, evPop, evZap, evSizzle, evBuzz, evThunder, fireCount;

    public Sim(int w, int h, int seed)
    {
        W = w; H = h; N = w * h;
        T = new byte[N]; Sh = new byte[N]; L = new byte[N]; E = new byte[N]; S = new byte[N];
        Tm = new float[N]; Array.Fill(Tm, 20f);
        CW = (W + CS - 1) / CS; CH = (H + CS - 1) / CS; NCh = CW * CH;
        chunkOf = new ushort[N];
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) chunkOf[y * W + x] = (ushort)((y >> 4) * CW + (x >> 4));
        wakeA = new byte[NCh]; Array.Fill(wakeA, (byte)1); act = new byte[NCh];
        occ = new short[N]; Array.Fill(occ, (short)-1);
        eQ = new int[N]; eSeen = new int[N]; eW = new byte[N];
        rs = seed == 0 ? 1u : (uint)seed;
    }

    float Rnd() { rs ^= rs << 13; rs ^= rs >> 17; rs ^= rs << 5; return rs * 2.3283064365386963e-10f; }
    void Wake(int i) => wakeA[chunkOf[i]] = 1;
    int LifeFor(int t) { int a = LifeMin[t]; return a != 0 ? a + (int)(Rnd() * (LifeMax[t] - a + 1)) : 0; }
    byte RB() => (byte)(Rnd() * 256);

    void Set(int i, int t)
    {
        T[i] = (byte)t; Sh[i] = t == FOGO || t == VIRUS || t == CLONADOR || t == INTERRUPTOR ? (byte)0 : RB(); L[i] = (byte)LifeFor(t); E[i] = 0; Tm[i] = TempA[t]; S[i] = (byte)tick; Wake(i);
    }
    void Become(int i, int t) { T[i] = (byte)t; Sh[i] = RB(); L[i] = (byte)LifeFor(t); E[i] = 0; S[i] = (byte)tick; Wake(i); }
    void Move(int i, int j)
    {
        (T[i], T[j]) = (T[j], T[i]); (Sh[i], Sh[j]) = (Sh[j], Sh[i]); (L[i], L[j]) = (L[j], L[i]);
        E[i] = 0; E[j] = 0; (Tm[i], Tm[j]) = (Tm[j], Tm[i]);
        S[i] = (byte)tick; S[j] = (byte)tick; Wake(i); Wake(j);
    }
    int Nb(int i, int x, int y, int n)
    {
        if (n == 0) return y > 0 ? i - W : -1;
        if (n == 1) return y < H - 1 ? i + W : -1;
        if (n == 2) return x > 0 ? i - 1 : -1;
        return x < W - 1 ? i + 1 : -1;
    }
    bool CanSink(int t, int j)
    {
        int u = T[j]; if (u == 0) return true; if (S[j] == tick) return false;
        return Kind[u] >= K_LIQUID && Dens[t] > Dens[u];
    }
    bool CanRise(int t, int j)
    {
        int u = T[j]; if (u == 0) return true; if (S[j] == tick) return false;
        int k = Kind[u]; return (k == K_LIQUID || k == K_GAS) && Dens[u] > Dens[t];
    }

    // ---------- fogo e explosões ----------
    void Ignite(int j)
    {
        int u = T[j]; if (u == FOGO || u == 0) return;
        if (u == BOMBA_A) { T[j] = 0; Nuke(j % W, j / W); return; }
        if (u == FOGUETE) { Become(j, FOGUETE_V); Tm[j] = 300; return; }
        if (u == DINAMITE) { Become(j, DINAMITE_A); return; }
        if (u == DINAMITE_A) return;
        if (u == BOMBA_GELO) { T[j] = 0; Wake(j); Freeze(j % W, j / W); return; }
        if (Boom[u] > 0) { Explode(j % W, j / W, Boom[u]); return; }
        T[j] = FOGO; Sh[j] = (byte)u; E[j] = 0;
        int a = BurnMin[u], b = BurnMax[u]; L[j] = (byte)(a + (int)(Rnd() * (b - a + 1)));
        if (Tm[j] < Flame[u]) Tm[j] = Flame[u];
        S[j] = (byte)tick; Wake(j);
    }
    void Nuke(int cx, int cy)
    {
        evBigBoom++;
        Explode(cx, cy, 30);
        const int R2 = 45;
        for (int dy = -R2; dy <= R2; dy++)
        {
            int y = cy + dy; if (y < 0 || y >= H) continue;
            for (int dx = -R2; dx <= R2; dx++)
            {
                int x = cx + dx, q = dx * dx + dy * dy; if (x < 0 || x >= W || q > R2 * R2) continue;
                int k = y * W + x, u = T[k]; if (u == 0 || IndesA[u] != 0) continue;
                float f = 1 - MathF.Sqrt(q) / R2;
                Tm[k] += 2500 * f; Wake(k);
                if (Rnd() < .04f * f && Kind[u] != K_GAS && u != FOGO) Become(k, RESIDUO);
            }
        }
        for (int q = 0; q < 400; q++) AddN(cx + .5f, cy + .5f, 0);
        HurtArea(cx, cy, 70, 1500, 6, 250);
    }
    void Freeze(int cx, int cy)
    {
        const int R = 24;
        evBoom++;
        for (int dy = -R; dy <= R; dy++)
        {
            int y = cy + dy; if (y < 0 || y >= H) continue;
            for (int dx = -R; dx <= R; dx++)
            {
                int x = cx + dx, q = dx * dx + dy * dy; if (x < 0 || x >= W || q > R * R) continue;
                int k = y * W + x, u = T[k];
                float f = 1 - MathF.Sqrt(q) / R;
                if (u == 0) { if (Rnd() < .12f * f) { Set(k, Rnd() < .5f ? NEVE : AZOTO); Tm[k] = -20 - 150 * f; } continue; }
                if (IndesA[u] != 0 || FixedA[u] != 0) continue;
                Tm[k] = MathF.Max(-250, Tm[k] - 400 * f); Wake(k);
            }
        }
        for (int p = 0; p < pc; p++)
        {
            float dx = pX[p] - cx, dy = pY[p] - 3 - cy, d = MathF.Sqrt(dx * dx + dy * dy);
            if (d < R + 6) { float f = 1 - d / (R + 6); pTemp[p] -= 60 * f; pHP[p] -= 90 * f; }
        }
    }
    void Explode(int cx, int cy, int r)
    {
        int r2 = r * r;
        if (r < 20) evBoom++;
        for (int dy = -r; dy <= r; dy++)
        {
            int y = cy + dy; if (y < 0 || y >= H) continue;
            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx, q = dx * dx + dy * dy; if (x < 0 || x >= W || q > r2) continue;
                int k = y * W + x, u = T[k]; if (IndesA[u] != 0) continue;
                float d = MathF.Sqrt(q) / r;
                if (u == BOMBA_A || u == FOGUETE || u == BOMBA_GELO) { Tm[k] = 3000; Wake(k); continue; }
                if (u == DINAMITE || u == DINAMITE_A)
                {
                    if (u == DINAMITE) Become(k, DINAMITE_A);
                    if (L[k] > 6) L[k] = (byte)(2 + (int)(Rnd() * 5));
                    continue;
                }
                if (u != 0 && Boom[u] > 0)
                {
                    if (d < .5f) { T[k] = FOGO; Sh[k] = 0; L[k] = (byte)(5 + (int)(Rnd() * 15)); Tm[k] = 2000; } else Tm[k] = 3000;
                    Wake(k); continue;
                }
                if (Rnd() < 1.2f - d)
                {
                    int nt;
                    if (u == AGUA || u == AGUA_S) nt = VAPOR;
                    else { float r1 = Rnd(); nt = r1 < .35f ? FOGO : r1 < .65f ? FUMO : 0; }
                    T[k] = (byte)nt; Sh[k] = nt == FOGO ? (byte)0 : RB(); E[k] = 0;
                    L[k] = (byte)(nt == FOGO ? 5 + (int)(Rnd() * 20) : LifeFor(nt));
                    float ht = 1500 * (1 - d) + 200; if (Tm[k] < ht) Tm[k] = ht;
                    S[k] = (byte)tick;
                }
                else Tm[k] += 800 * (1 - d);
                Wake(k);
            }
        }
        HurtArea(cx, cy, r + 4, 400, 3, 120);
    }

    // ---------- eletricidade ----------
    // Corrente contínua: em cada passo espalha-se a partir das fontes (bateria, gerador, painel solar,
    // faíscas e raios) por tudo o que conduz. A água doce conduz mal: só leva a corrente até perto.
    // E = 3 (tem corrente) | brilho<<2 (a lâmpada usa o brilho para acender). As fontes só têm brilho.
    readonly int[] eQ, eSeen; readonly byte[] eW;
    int eQn, eStamp, eSrcN, pulseN;
    readonly int[] eSrc = new int[16384], pulseI = new int[4096]; readonly byte[] pulseT = new byte[4096];

    public int Powered => eQn;
    void AddSrc(int i) { if (eSrcN < eSrc.Length) eSrc[eSrcN++] = i; }
    void AddPulse(int i, int ttl) { if (pulseN < pulseI.Length) { pulseI[pulseN] = i; pulseT[pulseN++] = (byte)ttl; } }
    bool Conducts(int u, int k)
    {
        if (Elec[u] <= 0) return false;
        if (u == INTERRUPTOR) return Sh[k] >= 128;
        if (u == TERMOSTATO) return Tm[k] < 50;
        return true;
    }
    void EPush(int j, int w) { eSeen[j] = eStamp; eW[j] = (byte)w; eQ[eQn++] = j; }
    void ElecPass()
    {
        for (int k = 0; k < eQn; k++) E[eQ[k]] = 0;
        eQn = 0;
        int m = 0;
        for (int k = 0; k < pulseN; k++)
        {
            int i = pulseI[k]; if (Conducts(T[i], i)) AddSrc(i);
            if (--pulseT[k] > 0) { pulseI[m] = i; pulseT[m++] = pulseT[k]; }
        }
        pulseN = m;
        if (eSrcN == 0) return;
        if (++eStamp == int.MaxValue) { Array.Clear(eSeen); eStamp = 1; }
        for (int k = 0; k < eSrcN; k++) { int i = eSrc[k]; if (eSeen[i] != eStamp) EPush(i, 0); }
        eSrcN = 0;
        for (int h = 0; h < eQn; h++)
        {
            int i = eQ[h], t = T[i], x = i % W, y = i / W;
            E[i] = Elec[t] > 0 ? (byte)(3 | ((t == LAMPADA ? 24 : 2) << 2)) : (byte)(2 << 2);
            switch (t)
            {
                case RESIST: if (Tm[i] < 950) { Tm[i] += 8; Wake(i); } break;
                case LAMPADA: if (Tm[i] < 70) { Tm[i] += .2f; Wake(i); } break;
                case CAMPAINHA: evBuzz++; break;
                case AGUA:
                case AGUA_S:
                    Tm[i] += .1f; Wake(i);
                    // eletrólise: a água dá hidrogénio e oxigénio; a salgada dá hidrogénio e cloro
                    if (Rnd() < .0005f) { Become(i, Rnd() < .67f ? HIDROG : t == AGUA ? OXIGENIO : CLORO); continue; }
                    break;
            }
            int w0 = eW[i];
            for (int n = 0; n < 4; n++)
            {
                int j = Nb(i, x, y, n); if (j < 0 || eSeen[j] == eStamp) continue;
                int u = T[j]; if (u == 0) continue;
                if (Conducts(u, j))
                {
                    float p = Elec[u]; int w = w0 + (p >= .9f ? 0 : (int)MathF.Ceiling(1 / p));
                    if (w <= 24) EPush(j, w);
                }
                else if (Trig[u] != 0 && Elec[u] == 0 && Rnd() < .3f) Ignite(j);
            }
        }
    }
    public void ToggleSwitch(int cx, int cy, int r)
    {
        int to = -1;
        ForBrush(cx, cy, Math.Max(1, r), (i, _, _) =>
        {
            if (T[i] != INTERRUPTOR) return;
            if (to < 0) to = Sh[i] >= 128 ? 0 : 255;
            Sh[i] = (byte)to; Wake(i);
        });
    }
    public void SetSwitches(byte v) { for (int i = 0; i < N; i++) if (T[i] == INTERRUPTOR) Sh[i] = v; }

    // ---------- neutrões ----------
    void AddN(float x, float y, byte slow)
    {
        if (nc >= NMAX) return;
        float a = Rnd() * 6.2831853f, sp = slow != 0 ? .6f : 1.5f;
        nX[nc] = x; nY[nc] = y; nVX[nc] = MathF.Cos(a) * sp; nVY[nc] = MathF.Sin(a) * sp; nL[nc] = 220; nS[nc] = slow; nc++;
    }
    void KillN(int n)
    {
        nc--; nX[n] = nX[nc]; nY[n] = nY[nc]; nVX[n] = nVX[nc]; nVY[n] = nVY[nc]; nL[n] = nL[nc]; nS[n] = nS[nc];
    }
    void Fission(int c, float x, float y)
    {
        Tm[c] += NPheat; Wake(c);
        int k = Rnd() < .5f ? 2 : 3;
        for (int q = 0; q < k; q++) AddN(x, y, 0);
        if ((T[c] == URANIO || T[c] == PLUTONIO) && Rnd() < .1f) { int f = L[c] - 1; if (f <= 0) Become(c, RESIDUO); else L[c] = (byte)f; }
    }
    void NeutronPass()
    {
        int n = 0;
        while (n < nc)
        {
            float x = nX[n] + nVX[n], y = nY[n] + nVY[n];
            if (x < 0 || y < 0 || x >= W || y >= H || --nL[n] == 0) { KillN(n); continue; }
            nX[n] = x; nY[n] = y;
            int c = (int)y * W + (int)x, t = T[c];
            if (occ[c] >= 0 && Rnd() < .3f) { pHP[occ[c]] -= 6; KillN(n); continue; }
            if (t != 0)
            {
                float fs = Fiss[t];
                if (fs > 0 && Rnd() < (nS[n] != 0 ? NPslow : NPfast) * fs) { Fission(c, x, y); KillN(n); continue; }
                if (Rnd() < Absorb[t]) { Tm[c] += 2; Wake(c); KillN(n); continue; }
                int m = ModA[t];
                if (m > 0)
                {
                    if (nS[n] == 0) { nS[n] = 1; nVX[n] *= .4f; nVY[n] *= .4f; }
                    if (Rnd() < (m == 2 ? .3f : .12f)) { float a = Rnd() * 6.2831853f; nVX[n] = MathF.Cos(a) * .6f; nVY[n] = MathF.Sin(a) * .6f; }
                }
                else if (fs == 0 && Kind[t] != K_GAS && Kind[t] != K_FIRE && Rnd() < .3f) { nVX[n] = -nVX[n]; nVY[n] = -nVY[n]; }
            }
            n++;
        }
    }

    // ---------- movimento ----------
    void Powder(int i, int x, int y, int t)
    {
        if (y + 1 >= H) return;
        int d = i + W;
        if (CanSink(t, d)) { Move(i, d); return; }
        if (Rnd() < Slip[t])
        {
            int dir = Rnd() < .5f ? -1 : 1;
            if (x + dir >= 0 && x + dir < W && CanSink(t, d + dir)) { Move(i, d + dir); return; }
            if (x - dir >= 0 && x - dir < W && CanSink(t, d - dir)) Move(i, d - dir);
        }
    }
    void Liquid(int i, int x, int y, int t)
    {
        if (y > 0)
        {
            int u = i - W;
            if (T[u] == t && S[u] != tick && Tm[i] > Tm[u] + .3f && Rnd() < .5f) { Move(i, u); return; }
        }
        if (y + 1 < H)
        {
            int d = i + W;
            if (CanSink(t, d)) { Move(i, d); return; }
            int dir = Rnd() < .5f ? -1 : 1;
            if (x + dir >= 0 && x + dir < W && CanSink(t, d + dir)) { Move(i, d + dir); return; }
            if (x - dir >= 0 && x - dir < W && CanSink(t, d - dir)) { Move(i, d - dir); return; }
        }
        if (Rnd() < Slip[t])
        {
            int dir = Rnd() < .5f ? -1 : 1, sp = Spread[t];
            for (int pass = 0; pass < 2; pass++, dir = -dir)
            {
                int best = -1;
                for (int s = 1; s <= sp; s++)
                {
                    int nx = x + dir * s; if (nx < 0 || nx >= W) break;
                    int j = i + dir * s;
                    int u = T[j];
                    // o gás não trava o líquido: troca de lugar com ele como se fosse ar
                    if (u == 0 || (Kind[u] == K_GAS && S[j] != tick))
                    {
                        best = j;
                        if (y + 1 < H) { int b = T[j + W]; if (b == 0 || Kind[b] == K_GAS) break; }
                    }
                    else { if (Kind[u] == K_LIQUID && CanSink(t, j)) best = j; break; }
                }
                if (best >= 0) { Move(i, best); return; }
            }
        }
    }
    bool HeavyFall(int t, int j)
    {
        int u = T[j]; if (u == 0) return true; if (S[j] == tick) return false;
        return Kind[u] == K_GAS && Dens[u] < Dens[t];
    }
    void Gas(int i, int x, int y, int t)
    {
        if (LifeMin[t] != 0 || (t == VAPOR && L[i] > 0))
        {
            int l = L[i] - 1;
            if (l <= 0) { T[i] = 0; L[i] = 0; E[i] = 0; S[i] = (byte)tick; Wake(i); return; }
            L[i] = (byte)l; Wake(i);
        }
        if (Dens[t] > .0013f)
        {
            // gás pesado continua mais leve que qualquer líquido: debaixo de água sobe em bolhas
            if (y > 0) { int u = i - W; if (Kind[T[u]] == K_LIQUID && S[u] != tick && Rnd() < .5f) { Move(i, u); return; } }
            if (y + 1 < H)
            {
                int d = i + W;
                if (HeavyFall(t, d)) { Move(i, d); return; }
                int dir = Rnd() < .5f ? -1 : 1, nx = x + dir;
                if (nx >= 0 && nx < W && HeavyFall(t, d + dir)) { Move(i, d + dir); return; }
            }
            int dir2 = Rnd() < .5f ? -1 : 1, nx2 = x + dir2;
            if (nx2 >= 0 && nx2 < W && T[i + dir2] == 0 && Rnd() < .5f) Move(i, i + dir2);
            return;
        }
        if (y > 0)
        {
            int u = i - W;
            if ((Rnd() < Slip[t] || Kind[T[u]] == K_LIQUID) && CanRise(t, u)) { Move(i, u); return; }
            int dir = Rnd() < .5f ? -1 : 1, nx = x + dir;
            if (nx >= 0 && nx < W && CanRise(t, u + dir)) { Move(i, u + dir); return; }
        }
        int dir3 = Rnd() < .5f ? -1 : 1, nx3 = x + dir3;
        if (nx3 >= 0 && nx3 < W && T[i + dir3] == 0) Move(i, i + dir3);
    }
    void Fire(int i, int x, int y)
    {
        fireCount++;
        int o = Sh[i]; float ft = Flame[o];
        if (Tm[i] < ft) Tm[i] = ft;
        for (int n = 0; n < 4; n++)
        {
            int j = Nb(i, x, y, n); if (j < 0) continue;
            int u = T[j]; if (u == 0) continue;
            if (u == AGUA || u == AGUA_S)
            {
                if (Rnd() < .2f) { evSizzle++; T[i] = FUMO; Sh[i] = RB(); L[i] = (byte)LifeFor(FUMO); Tm[i] = 150; S[i] = (byte)tick; Wake(i); return; }
                continue;
            }
            if (u == CO2) { if (Rnd() < .25f) { Become(i, FUMO); return; } continue; }
            float f = Flam[u]; if (f > 0 && Rnd() < f) Ignite(j);
        }
        int l = L[i] - 1;
        if (l <= 0)
        {
            int nt = 0; float r = Rnd();
            if (o == MADEIRA && r < .08f) nt = CARVAO;
            else if ((o == MADEIRA || o == PLANTA) && r < .45f) nt = CINZA;
            else if (o == TERMITE && r < .6f) nt = METAL_F;
            else if (r < .3f) nt = FUMO;
            if (nt != 0) { Become(i, nt); if (nt == METAL_F) Tm[i] = 2000; } else { T[i] = 0; L[i] = 0; S[i] = (byte)tick; Wake(i); }
            return;
        }
        L[i] = (byte)l; Wake(i);
        if (y > 0 && Rnd() < .3f)
        {
            int dir = (int)(Rnd() * 3) - 1, nx = x + dir;
            if (nx >= 0 && nx < W)
            {
                int j = i - W + dir;
                if (T[j] == 0) { T[j] = FOGO; Sh[j] = 0; L[j] = (byte)(Math.Min(l >> 2, 12) + 1); Tm[j] = 650; E[j] = 0; S[j] = (byte)tick; Wake(j); }
            }
        }
    }

    // ---------- pessoas ----------
    bool Blk(int x, int y)
    {
        if (x < 0 || x >= W || y >= H) return true; if (y < 0) return false;
        int u = T[y * W + x]; if (u == 0) return false;
        int k = Kind[u]; return k == K_SOLID || k == K_POWDER;
    }
    bool RowBlocked(int cx, int y) => Blk(cx - 1, y) || Blk(cx, y) || Blk(cx + 1, y);
    public bool SpawnPerson(int x, int y)
    {
        if (pc >= PMAX || x < 1 || x > W - 2 || y < 7 || y >= H) return false;
        for (int yy = y - 6; yy <= y; yy++) if (RowBlocked(x, yy)) return false;
        int p = pc++;
        pX[p] = x; pY[p] = y + .5f; pVX[p] = 0; pVY[p] = 0; pVirus[p] = 0; pHP[p] = 200; pAir[p] = 200; pTemp[p] = 37; pBurn[p] = 0; pWalk[p] = 0;
        pDir[p] = (sbyte)(Rnd() < .5f ? -1 : 1);
        for (int k = 0; k < 4; k++) pLook[p * 4 + k] = RB();
        return true;
    }
    void RemovePerson(int p)
    {
        pc--;
        if (p != pc)
        {
            pX[p] = pX[pc]; pY[p] = pY[pc]; pVX[p] = pVX[pc]; pVY[p] = pVY[pc]; pHP[p] = pHP[pc]; pAir[p] = pAir[pc];
            pTemp[p] = pTemp[pc]; pBurn[p] = pBurn[pc]; pWalk[p] = pWalk[pc]; pDir[p] = pDir[pc]; pVirus[p] = pVirus[pc];
            for (int k = 0; k < 4; k++) pLook[p * 4 + k] = pLook[pc * 4 + k];
        }
    }
    void KillPerson(int p)
    {
        int cx = (int)MathF.Round(pX[p]), fy = (int)MathF.Floor(pY[p]);
        bool burnt = pBurn[p] > 0 || pTemp[p] > 120, infected = pVirus[p] > 0 && !burnt;
        for (int y = fy - 6; y <= fy; y++) for (int x = cx - 1; x <= cx + 1; x++)
        {
            if (x < 0 || x >= W || y < 0 || y >= H) continue;
            int i = y * W + x;
            if (infected) { if (T[i] == 0 && Rnd() < .7f) { Set(i, VIRUS); Sh[i] = SANGUE; } continue; }
            if (T[i] == 0 && Rnd() < (burnt ? .4f : .3f)) { Set(i, burnt ? CINZA : SANGUE); Tm[i] = burnt ? 200 : 37; }
        }
        RemovePerson(p);
    }
    void HurtArea(int cx, int cy, float r, float dmg, float push, float burn)
    {
        for (int p = 0; p < pc; p++)
        {
            float dx = pX[p] - cx, dy = pY[p] - 3 - cy, d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > r) continue;
            float f = 1 - d / r;
            pHP[p] -= dmg * f; pVX[p] += (dx >= 0 ? 1 : -1) * push * f; pVY[p] -= push * f;
            if (burn > 0) pBurn[p] = MathF.Max(pBurn[p], burn * f);
        }
    }
    void PeoplePass()
    {
        for (int p = 0; p < pc;)
        {
            int cx = (int)MathF.Round(pX[p]), fy = (int)MathF.Floor(pY[p]);
            float hot = 20, cold = 20, hp = pHP[p];
            int liquid = 0, headLiq = 0, solidIn = 0; bool shock = false, cure = false;
            for (int y = fy - 7; y <= fy + 1; y++)
            {
                if (y < 0 || y >= H) continue;
                for (int x = cx - 2; x <= cx + 2; x++)
                {
                    if (x < 0 || x >= W) continue;
                    int i = y * W + x, u = T[i]; if (u == 0) continue;
                    float tm = Tm[i]; if (tm > hot) hot = tm; if (tm < cold) cold = tm;
                    if (u == VIRUS && pVirus[p] == 0 && Rnd() < .05f) pVirus[p] = 1;
                    else if (u == CURA) cure = true;
                    if (u == ACIDO) hp -= 1.5f; else if (u == CLORO) hp -= .4f; else if (u == LAVA || u == CORIO) hp -= 8;
                    else if (u == FOGO && Rnd() < .03f) pBurn[p] = MathF.Max(pBurn[p], 150);
                    if ((E[i] & 3) != 0 && Elec[u] > 0) shock = true;
                    if (y >= fy - 6 && y <= fy && x >= cx - 1 && x <= cx + 1)
                    {
                        int k = Kind[u];
                        if (k == K_LIQUID) { liquid++; if (y <= fy - 5) headLiq++; } else if (k == K_SOLID || k == K_POWDER) solidIn++;
                    }
                }
            }
            float target = hot > 45 ? 37 + (hot - 45) * .6f : cold < 5 ? 37 - (5 - cold) * .4f : 37;
            pTemp[p] += (target - pTemp[p]) * .05f;
            if (hot > 55) hp -= (hot - 55) * .04f;
            if (cold < -5) hp -= .2f + (-5 - cold) * .01f;
            if (shock) { hp -= 6; evZap++; }
            if (pBurn[p] > 0)
            {
                pBurn[p]--; hp -= .6f;
                if (Rnd() < .3f && fy - 7 >= 0) { int j = (fy - 7) * W + cx; if (T[j] == 0) { T[j] = FOGO; Sh[j] = 0; L[j] = (byte)(5 + (int)(Rnd() * 8)); Tm[j] = 700; S[j] = (byte)tick; Wake(j); } }
                if (liquid > 4) pBurn[p] = 0;
            }
            if (headLiq >= 4) { pAir[p] -= 1; if (pAir[p] <= 0) hp -= 1.5f; } else pAir[p] = MathF.Min(200, pAir[p] + 3);
            if (solidIn > 12) hp -= .5f;
            if (cure) pVirus[p] = 0;
            if (pVirus[p] > 0)
            {
                hp -= .25f; pVirus[p] += 1;
                if (Rnd() < .01f && fy + 1 < H) { int j = (fy + 1) * W + cx; int u = T[j]; if (u != 0 && u != VIRUS && IndesA[u] == 0) { T[j] = VIRUS; Sh[j] = (byte)u; E[j] = 0; Wake(j); } }
            }
            if (RowBlocked(cx, fy) && !RowBlocked(cx, fy - 7)) { pY[p] -= 1; fy--; }
            bool inLiq = liquid >= 6;
            float vy = pVY[p];
            if (inLiq) vy = Math.Clamp(vy * .8f + (headLiq >= 2 ? -.09f : .03f), -.5f, .4f);
            else vy = MathF.Min(4, vy + .12f);
            if (vy > 0)
            {
                float t = pY[p] + vy; bool landed = false;
                while (MathF.Floor(pY[p]) < MathF.Floor(t)) { if (RowBlocked(cx, (int)MathF.Floor(pY[p]) + 1)) { landed = true; break; } pY[p] += 1; }
                if (!landed && RowBlocked(cx, (int)MathF.Floor(t) + 1) && vy > .5f) landed = true;
                if (landed) { if (vy > 2.4f && !inLiq) hp -= (vy - 2.4f) * 70; vy = 0; if (pY[p] > H - .01f) pY[p] = H - .01f; }
                else pY[p] = t;
                if (RowBlocked(cx, (int)MathF.Floor(pY[p]) + 1)) vy = 0;
            }
            else if (vy < 0)
            {
                float t = pY[p] + vy; bool stop = false;
                while (MathF.Floor(t) < MathF.Floor(pY[p])) { if (RowBlocked(cx, (int)MathF.Floor(pY[p]) - 7)) { stop = true; break; } pY[p] -= 1; }
                if (stop) vy = 0; else pY[p] = t;
            }
            pVY[p] = vy;
            if (pY[p] >= H) pY[p] = H - .01f;
            fy = (int)MathF.Floor(pY[p]);
            bool ground = RowBlocked(cx, fy + 1);
            float vx = pVX[p] * .9f; if (vx > -.05f && vx < .05f) vx = 0; pVX[p] = vx;
            float ddx = vx;
            if (vx == 0 && (ground || inLiq))
            {
                if (Rnd() < .004f) pDir[p] = (sbyte)-pDir[p];
                ddx = pDir[p] * (inLiq ? .06f : pBurn[p] > 0 ? .3f : .14f);
            }
            if (ddx != 0)
            {
                float nx = pX[p] + ddx; int ncx = (int)MathF.Round(nx);
                if (ncx == cx) pX[p] = nx;
                else
                {
                    int d = ncx > cx ? 1 : -1, col = ncx + d, top = -1;
                    for (int y = fy - 6; y <= fy; y++) if (Blk(col, y)) { top = y; break; }
                    if (top < 0) pX[p] = nx;
                    else if (top >= fy - 1 && ground)
                    {
                        int h = fy - top + 1; bool ok = true;
                        for (int y = fy - 6 - h; y < top && ok; y++) if (Blk(col, y)) ok = false;
                        for (int y = fy - 6 - h; y < fy - 6 && ok; y++) if (RowBlocked(cx, y)) ok = false;
                        if (ok) { pY[p] -= h; pX[p] = nx; } else { pDir[p] = (sbyte)-d; pVX[p] = 0; }
                    }
                    else { pDir[p] = (sbyte)-d; pVX[p] = 0; }
                }
                pWalk[p] += MathF.Abs(ddx);
            }
            pHP[p] = hp;
            if (hp <= 0) { KillPerson(p); continue; }
            p++;
        }
        for (int k = 0; k < occN; k++) occ[occList[k]] = -1;
        occN = 0;
        for (int p = 0; p < pc; p++)
        {
            int cx = (int)MathF.Round(pX[p]), fy = (int)MathF.Floor(pY[p]);
            for (int y = fy - 6; y <= fy; y++) for (int x = cx - 1; x <= cx + 1; x++)
            {
                if (x < 0 || x >= W || y < 0 || y >= H) continue;
                int i = y * W + x; occ[i] = (short)p; occList[occN++] = i;
            }
        }
    }
    public float VirusOf(int p) => pVirus[p];
    public int PersonAt(int x, int y) => x < 0 || y < 0 || x >= W || y >= H ? -1 : occ[y * W + x];

    void Lightning(int cx)
    {
        evThunder++;
        int x = Math.Clamp(cx, 0, W - 1), y = 0;
        for (; y < H; y++)
        {
            int i = y * W + x, u = T[i];
            if (occ[i] >= 0) { pHP[occ[i]] = -1; pBurn[occ[i]] = 200; break; }
            if (u != 0 && Kind[u] != K_GAS) break;
            if (u == 0) { T[i] = FOGO; Sh[i] = 0; L[i] = (byte)(2 + (int)(Rnd() * 3)); Tm[i] = 3000; E[i] = 0; S[i] = (byte)tick; Wake(i); }
            if (Rnd() < .35f) x = Math.Clamp(x + (Rnd() < .5f ? -1 : 1), 0, W - 1);
        }
        if (y >= H) return;
        ForBrush(x, y, 3, (i, _, _) =>
        {
            int u = T[i]; if (u == 0 || IndesA[u] != 0) return;
            Tm[i] += 1500;
            if (Elec[u] > 0) AddPulse(i, 10); else if (Trig[u] != 0 || Flam[u] > 0) Ignite(i);
            Wake(i);
        });
    }

    void Burst(int i, int x, int y)
    {
        T[i] = 0; L[i] = 0; Wake(i); evPop++;
        byte hue = RB();
        int n = 50 + (int)(Rnd() * 40);
        for (int q = 0; q < n; q++)
        {
            float a = Rnd() * 6.2831853f, d = 2 + Rnd() * 9;
            int sx = x + (int)MathF.Round(MathF.Cos(a) * d), sy = y + (int)MathF.Round(MathF.Sin(a) * d);
            if (sx < 0 || sx >= W || sy < 0 || sy >= H) continue;
            int k = sy * W + sx; if (T[k] != 0) continue;
            Set(k, FAGULHA); Sh[k] = (byte)(hue + (int)(Rnd() * 50)); Tm[k] = 20;
        }
        HurtArea(x, y, 8, 30, 1, 30);
    }

    // ---------- reações especiais (true = a célula mudou) ----------
    bool Special(int i, int x, int y, int t)
    {
        switch (t)
        {
            case AGUA:
            case AGUA_S:
                if (y > 0 && T[i - W] == 0)
                {
                    float tm = Tm[i], p = .00002f + (tm > 20 ? (tm - 20) * .00005f : 0);
                    if (Rnd() < p)
                    {
                        if (t == AGUA_S && Rnd() < .15f) Become(i, SAL);
                        else { Become(i, VAPOR); L[i] = (byte)(100 + (int)(Rnd() * 100)); }
                        if (y < H - 1 && T[i + W] == t) Tm[i + W] -= 3;
                        return true;
                    }
                }
                return false;
            case SAL:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4)); if (j < 0) return false;
                int u = T[j];
                if (u == AGUA) { Wake(i); if (Rnd() < .05f) { T[j] = AGUA_S; Wake(j); T[i] = 0; S[i] = (byte)tick; return true; } }
                else if ((u == GELO || u == NEVE) && Rnd() < .03f) { Become(j, AGUA_S); Tm[j] = 1; }
                return false;
            }
            case CIMENTO:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4)); if (j < 0) return false;
                if (T[j] == AGUA) { Wake(i); if (Rnd() < .02f) { T[j] = 0; Wake(j); Become(i, BETAO); return true; } }
                return false;
            }
            case ACIDO:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4)); if (j < 0) return false;
                int u = T[j];
                if (u == 0 || u == ACIDO || u == VIDRO || u == VIDRO_F || u == OURO || u == OURO_F || IndesA[u] != 0 || Kind[u] == K_GAS || u == FOGO) return false;
                Wake(i);
                bool metal = u == METAL || u == FIO || u == MERCURIO || u == RESIST || u == INTERRUPTOR || u == TERMOSTATO || u == CAMPAINHA;
                float p = metal ? .05f : (u == PEDRA || u == BETAO || u == CHUMBO) ? .01f : .12f;
                if (Rnd() < p)
                {
                    if (metal) Become(j, HIDROG); else if (Rnd() < .25f) Become(j, FUMO); else { T[j] = 0; E[j] = 0; Wake(j); }
                    Tm[i] += 5;
                    if (Rnd() < .35f) { T[i] = 0; S[i] = (byte)tick; return true; }
                }
                return false;
            }
            case PLANTA:
            {
                bool wet = false;
                for (int n = 0; n < 4; n++) { int j = Nb(i, x, y, n); if (j >= 0 && T[j] == AGUA) { wet = true; break; } }
                if (!wet) return false;
                Wake(i);
                if (Rnd() < .05f)
                {
                    int j = Nb(i, x, y, (int)(Rnd() * 4));
                    if (j >= 0 && T[j] == AGUA)
                    {
                        T[j] = 0; Wake(j);
                        int r = (int)(Rnd() * 5), nx = x, ny = y - 1;
                        if (r == 1) nx--; else if (r == 2) nx++; else if (r == 3) { nx--; ny = y; } else if (r == 4) { nx++; ny = y; }
                        if (nx >= 0 && nx < W && ny >= 0) { int k = ny * W + nx; if (T[k] == 0) Set(k, PLANTA); }
                    }
                }
                return false;
            }
            case BATERIA: AddSrc(i); Wake(i); return false;
            case SOLAR:
                Wake(i);
                if (((tick + i) & 15) == 0)
                {
                    bool sun = true;
                    for (int k = i - W; k >= 0; k -= W) { int u = T[k]; if (u != 0 && u != SOLAR && Kind[u] != K_GAS && Alpha[u] > .6f) { sun = false; break; } }
                    L[i] = (byte)(sun ? 1 : 0);
                }
                if (L[i] != 0) AddSrc(i);
                return false;
            case GERADOR:
            {
                bool found = false;
                for (int n = 0; n < 4; n++) { int j = Nb(i, x, y, n); if (j >= 0 && T[j] == VAPOR && L[j] == 0 && Tm[j] > 60) { Tm[j] -= 6; Wake(j); found = true; } }
                if (found) L[i] = 15;
                if (L[i] > 0) { L[i]--; AddSrc(i); Wake(i); }
                return false;
            }
            case PLUTONIO: Tm[i] += .08f; if (Rnd() < NPspont * 6) AddN(x + .5f, y + .5f, 0); Wake(i); return false;
            case URANIO: Tm[i] += .04f; if (Rnd() < NPspont) AddN(x + .5f, y + .5f, 0); Wake(i); return false;
            case RESIDUO: Tm[i] += .06f; if (Rnd() < .0002f) AddN(x + .5f, y + .5f, 0); Wake(i); return false;
            case CORIO: Tm[i] += .4f; if (Rnd() < .002f) AddN(x + .5f, y + .5f, 0); Wake(i); return false;
            case AQUECEDOR: case ARREF: Wake(i); return false;
            case TERRA:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4));
                if (j >= 0 && T[j] == AGUA) { Wake(i); if (Rnd() < .01f) { T[j] = 0; Wake(j); Become(i, LAMA); return true; } }
                return false;
            }
            case METAL:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4));
                if (j >= 0 && (T[j] == AGUA || T[j] == AGUA_S)) { Wake(i); if (Rnd() < (T[j] == AGUA_S ? .004f : .001f)) Become(i, FERRUGEM); }
                return false;
            }
            case SEMENTE:
                if (y < H - 1) { int u = T[i + W]; if (u == TERRA || u == LAMA) { Wake(i); if (Rnd() < .004f) { Become(i, PLANTA); return true; } } }
                return false;
            case GELO_SECO:
                Wake(i);
                if (Rnd() < .01f) { int j = Nb(i, x, y, (int)(Rnd() * 4)); if (j >= 0 && T[j] == 0) { Set(j, CO2); Tm[j] = Tm[i]; } }
                return false;
            case SODIO:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4));
                if (j >= 0 && (T[j] == AGUA || T[j] == AGUA_S)) { Wake(i); if (Rnd() < .3f) { Explode(x, y, 3); return true; } }
                return false;
            }
            case NUVEM:
                Wake(i);
                if (y < H - 1 && T[i + W] == 0 && Rnd() < .003f) Set(i + W, Tm[i] < 0 ? NEVE : AGUA);
                return false;
            case TOCHA:
                Wake(i);
                if (y > 0 && T[i - W] == 0 && Rnd() < .4f) { int j = i - W; T[j] = FOGO; Sh[j] = 0; L[j] = (byte)(4 + (int)(Rnd() * 8)); Tm[j] = 700; E[j] = 0; S[j] = (byte)tick; Wake(j); }
                return false;
            case TORNEIRA:
                Wake(i); if (y < H - 1 && T[i + W] == 0 && Rnd() < .25f) Set(i + W, AGUA); return false;
            case VIRUS:
                Wake(i);
                if (Rnd() < .02f)
                {
                    int j = Nb(i, x, y, (int)(Rnd() * 4));
                    if (j >= 0)
                    {
                        int u = T[j];
                        if (u != 0 && u != VIRUS && u != CURA && u != FOGO && IndesA[u] == 0 && Kind[u] != K_GAS && u != FAGULHA && u != FOGUETE_V)
                        { T[j] = VIRUS; Sh[j] = (byte)u; L[j] = 0; E[j] = 0; S[j] = (byte)tick; Wake(j); }
                    }
                }
                return false;
            case CURA:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4));
                if (j >= 0 && T[j] == VIRUS)
                {
                    int orig = Sh[j];
                    if (orig == 0 || orig >= Defs.Length || orig == VIRUS) { T[j] = 0; L[j] = 0; Wake(j); }
                    else { float tm = Tm[j]; Become(j, orig); Tm[j] = tm; }
                    if (Rnd() < .2f) { T[i] = 0; S[i] = (byte)tick; Wake(i); return true; }
                }
                return false;
            }
            case CLONADOR:
                Wake(i);
                if (Sh[i] == 0)
                {
                    for (int n = 0; n < 4; n++)
                    {
                        int j = Nb(i, x, y, n); if (j < 0) continue;
                        int u = T[j];
                        if (u != 0 && u != CLONADOR && u != BURACO && u != FOGUETE_V && u != FAGULHA) { Sh[i] = (byte)u; break; }
                    }
                }
                else if (Rnd() < .25f) { int j = Nb(i, x, y, (int)(Rnd() * 4)); if (j >= 0 && T[j] == 0) Set(j, Sh[i]); }
                return false;
            case BURACO:
            {
                Wake(i);
                const int PULL = 14;
                for (int dy = -PULL; dy <= PULL; dy++)
                {
                    int yy = y + dy; if (yy < 0 || yy >= H) continue;
                    for (int dx = -PULL; dx <= PULL; dx++)
                    {
                        int xx = x + dx; if (xx < 0 || xx >= W) continue;
                        int q = dx * dx + dy * dy; if (q > PULL * PULL) continue;
                        int k = yy * W + xx, u = T[k]; if (u == 0 || u == BURACO || IndesA[u] != 0) continue;
                        if (q <= 9) { T[k] = 0; L[k] = 0; E[k] = 0; Wake(k); continue; }
                        if (Kind[u] >= K_POWDER && Rnd() < .4f)
                        {
                            int tx = xx - Math.Sign(dx), ty = yy - Math.Sign(dy), tk = ty * W + tx;
                            if (T[tk] == 0) Move(k, tk);
                        }
                    }
                }
                for (int p = 0; p < pc; p++)
                {
                    float ddx = x - pX[p], ddy = y - (pY[p] - 3), d2 = ddx * ddx + ddy * ddy;
                    if (d2 < 16) pHP[p] = -1000;
                    else if (d2 < 900) { float f = .02f / MathF.Sqrt(d2); pVX[p] += ddx * f; pVY[p] += ddy * f; }
                }
                return false;
            }
            case FOGUETE_V:
            {
                int l = L[i] - 1; L[i] = (byte)Math.Max(0, l);
                int cur = i, cx = x, cy = y;
                for (int k = 0; k < 2; k++)
                {
                    int nx = cx + (Rnd() < .15f ? (Rnd() < .5f ? -1 : 1) : 0);
                    if (cy == 0 || nx < 0 || nx >= W || l <= 0) { Burst(cur, cx, cy); return true; }
                    int up = (cy - 1) * W + nx, u = T[up];
                    if (u == FOGUETE_V || u == FOGUETE) return true; // espera pelo de cima
                    if (u != 0 && Kind[u] != K_GAS && Kind[u] != K_FIRE) { Burst(cur, cx, cy); return true; }
                    Move(cur, up);
                    if (T[cur] == 0 && Rnd() < .6f) { T[cur] = FOGO; Sh[cur] = 0; L[cur] = (byte)(2 + (int)(Rnd() * 4)); Tm[cur] = 700; S[cur] = (byte)tick; }
                    cur = up; cx = nx; cy--;
                }
                return true;
            }
            case FAGULHA:
            {
                int l = L[i] - 1;
                if (l <= 0) { T[i] = 0; L[i] = 0; S[i] = (byte)tick; Wake(i); return true; }
                L[i] = (byte)l; Wake(i);
                if (y + 1 < H && Rnd() < .35f) { int d = i + W; if (T[d] == 0) { Move(i, d); return true; } }
                if (Rnd() < .2f) { int dir = Rnd() < .5f ? -1 : 1; if (x + dir >= 0 && x + dir < W && T[i + dir] == 0) Move(i, i + dir); }
                return true;
            }
            case ANTIMAT:
            {
                int j = Nb(i, x, y, (int)(Rnd() * 4)); if (j < 0) return false;
                int u = T[j];
                if (u == 0 || u == ANTIMAT || IndesA[u] != 0 || Kind[u] == K_GAS || u == FOGO) return false;
                T[j] = 0; L[j] = 0; E[j] = 0; Wake(j);
                T[i] = FOGO; Sh[i] = 0; L[i] = (byte)(3 + (int)(Rnd() * 4)); Tm[i] = 3000; E[i] = 0; S[i] = (byte)tick; Wake(i);
                HurtArea(x, y, 5, 60, 1, 60);
                return true;
            }
            case PLASMA:
                for (int n = 0; n < 4; n++)
                {
                    int j = Nb(i, x, y, n); if (j < 0) continue;
                    int u = T[j]; if (u == 0 || u == PLASMA) continue;
                    if (Flam[u] > 0 || Trig[u] != 0) Ignite(j);
                    else if (IndesA[u] == 0 && FixedA[u] == 0) { Tm[j] += 60; Wake(j); }
                }
                return false;
            case DINAMITE_A:
            {
                Wake(i);
                int l = L[i] - 1;
                if (l <= 0) { T[i] = 0; L[i] = 0; Explode(x, y, 10); return true; }
                L[i] = (byte)l;
                if (y > 0 && Rnd() < .25f && T[i - W] == 0) { int j = i - W; T[j] = FOGO; Sh[j] = 0; L[j] = (byte)(2 + (int)(Rnd() * 3)); Tm[j] = 700; E[j] = 0; S[j] = (byte)tick; Wake(j); }
                return false;
            }
            case MINA:
            {
                Wake(i);
                if (y == 0) return false;
                int a = i - W, u = T[a], prev = Sh[i];
                Sh[i] = (byte)u;
                bool step = occ[a] >= 0 || (y > 1 && occ[a - W] >= 0);
                bool landed = prev == 0 && u != 0 && u != MINA && Kind[u] != K_GAS && Kind[u] != K_FIRE;
                if (step || landed) { Ignite(i); return true; }
                return false;
            }
            case PEIXE:
            {
                Wake(i);
                bool wet = false;
                for (int n = 0; n < 4 && !wet; n++) { int j = Nb(i, x, y, n); if (j >= 0 && (T[j] == AGUA || T[j] == AGUA_S)) wet = true; }
                if (!wet)
                {
                    if (++L[i] > 220) { Become(i, SANGUE); return true; }
                    if (Rnd() < .04f && y > 0) { int nx = x + (Rnd() < .5f ? -1 : 1); if (nx >= 0 && nx < W && T[i - W + nx - x] == 0) { Move(i, i - W + nx - x); return true; } }
                    return false;
                }
                L[i] = 0;
                if (Rnd() < .35f)
                {
                    int f = (Sh[i] & 1) != 0 ? 1 : -1, j = -1; float r = Rnd();
                    if (r < .7f) { if (x + f >= 0 && x + f < W) j = i + f; }
                    else j = Nb(i, x, y, r < .85f ? 0 : 1);
                    if (j >= 0 && (T[j] == AGUA || T[j] == AGUA_S) && S[j] != tick) { Move(i, j); return true; }
                    if (r < .7f) Sh[i] ^= 1;
                }
                return true;
            }
            case MINHOCA:
            {
                Wake(i);
                if (Rnd() < .2f)
                {
                    int j = Nb(i, x, y, (int)(Rnd() * 4));
                    if (j >= 0 && (T[j] == TERRA || T[j] == LAMA) && S[j] != tick) { Move(i, j); return true; }
                }
                return false;
            }
            case PASSARO:
            {
                Wake(i);
                if (Rnd() < .5f) return false;
                if (Rnd() < .01f) { Sh[i] ^= 1; return false; }
                int f = (Sh[i] & 1) != 0 ? 1 : -1, nx = x + f; float r = Rnd();
                int ny = y + (r < .3f ? -1 : r < .55f ? 1 : 0);
                if (y + 4 < H && T[i + 4 * W] != 0) ny = y - 1;
                if (nx < 0 || nx >= W || ny < 0 || ny >= H) { Sh[i] ^= 1; return false; }
                int j = ny * W + nx, u = T[j];
                if ((u == 0 || Kind[u] == K_GAS) && S[j] != tick) { Move(i, j); return true; }
                Sh[i] ^= 1;
                return false;
            }
            case RALO:
                Wake(i);
                for (int n = 0; n < 4; n++) { int j = Nb(i, x, y, n); if (j >= 0 && Kind[T[j]] >= K_POWDER) { T[j] = 0; L[j] = 0; E[j] = 0; Wake(j); } }
                return false;
        }
        return false;
    }

    void Cell(int i, int x, int y)
    {
        int t = T[i]; if (t == 0 || S[i] == tick) return;
        if (ActA[t] != 0 && Special(i, x, y, t)) return;
        int kd = Kind[t];
        if (kd == K_POWDER) Powder(i, x, y, t);
        else if (kd == K_LIQUID) Liquid(i, x, y, t);
        else if (kd == K_GAS) Gas(i, x, y, t);
        else if (kd == K_FIRE) Fire(i, x, y);
    }

    // ---------- calor ----------
    void HeatPass()
    {
        for (int cy = 0; cy < CH; cy++)
        {
            int y0 = cy * CS, y1 = Math.Min(H, y0 + CS);
            for (int cx = 0; cx < CW; cx++)
            {
                if (act[cy * CW + cx] == 0) continue;
                int x0 = cx * CS, x1 = Math.Min(W, x0 + CS);
                for (int y = y0; y < y1; y++)
                {
                    int i = y * W + x0;
                    for (int x = x0; x < x1; x++, i++)
                    {
                        int t = T[i]; if (t == 0) continue;
                        float t0 = Tm[i], ki = Cond[t], ci = Cap[t], ti = t0;
                        if (x < W - 1)
                        {
                            int j = i + 1, u = T[j];
                            if (u != 0)
                            {
                                float k = ki < Cond[u] ? ki : Cond[u];
                                if (k > 0) { float f = (Tm[j] - ti) * k * .25f; if (f > .02f || f < -.02f) { ti += f / ci; Tm[j] -= f / Cap[u]; wakeA[chunkOf[j]] = 1; } }
                            }
                            else ti += (20 - ti) * Air[t];
                        }
                        if (y < H - 1)
                        {
                            int j = i + W, u = T[j];
                            if (u != 0)
                            {
                                float k = ki < Cond[u] ? ki : Cond[u];
                                if (k > 0) { float f = (Tm[j] - ti) * k * .25f; if (f > .02f || f < -.02f) { ti += f / ci; Tm[j] -= f / Cap[u]; wakeA[chunkOf[j]] = 1; } }
                            }
                            else ti += (20 - ti) * Air[t];
                        }
                        if (y > 0 && T[i - W] == 0) ti += (20 - ti) * Air[t];
                        if (FixedA[t] != 0) ti = FixedT[t];
                        Tm[i] = ti;
                        float dd = ti - t0; if (dd > .03f || dd < -.03f) wakeA[chunkOf[i]] = 1;
                        if (ti > HiT[t]) { if (Rnd() < .2f) Phase(i, t, HiTo[t]); }
                        else if (ti < LoT[t]) { if (t == VAPOR ? (L[i] == 0 && Rnd() < .03f) : Rnd() < .08f) Phase(i, t, LoTo[t]); }
                        if (BurnT[t] > 0 && ti > BurnT[t] && Rnd() < .2f) Ignite(i);
                    }
                }
            }
        }
    }
    void Phase(int i, int t, int to)
    {
        if (t == AGUA_S && to == VAPOR && Rnd() < .15f) to = SAL;
        if (to == VAPOR && Tm[i] > 140) evSizzle++;
        Become(i, to);
    }

    public void Step()
    {
        tick = (tick % 255) + 1;
        if (tick == 1) Array.Clear(S); // senão, células paradas há 255 passos eram saltadas uma vez
        Array.Clear(act);
        for (int cy = 0; cy < CH; cy++) for (int cx = 0; cx < CW; cx++)
        {
            if (wakeA[cy * CW + cx] == 0) continue;
            for (int yy = Math.Max(0, cy - 1); yy <= Math.Min(CH - 1, cy + 1); yy++)
                for (int xx = Math.Max(0, cx - 1); xx <= Math.Min(CW - 1, cx + 1); xx++) act[yy * CW + xx] = 1;
        }
        Array.Clear(wakeA);
        HeatPass();
        for (int y = H - 1; y >= 0; y--)
        {
            int crow = (y >> 4) * CW, row = y * W; bool ltr = ((y + tick) & 1) == 0;
            for (int c = 0; c < CW; c++)
            {
                int cx = ltr ? c : CW - 1 - c; if (act[crow + cx] == 0) continue;
                int x0 = cx * CS, x1 = Math.Min(W, x0 + CS);
                if (ltr) for (int x = x0; x < x1; x++) Cell(row + x, x, y);
                else for (int x = x1 - 1; x >= x0; x--) Cell(row + x, x, y);
            }
        }
        ElecPass();
        NeutronPass();
        PeoplePass();
    }

    public float AwakeFraction() { int c = 0; for (int k = 0; k < NCh; k++) c += act[k]; return (float)c / NCh; }

    // ---------- ferramentas ----------
    void ForBrush(int cx, int cy, int r, Action<int, int, int> fn)
    {
        float r2 = r * r + r * .8f;
        for (int dy = -r; dy <= r; dy++)
        {
            int y = cy + dy; if (y < 0 || y >= H) continue;
            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx; if (x < 0 || x >= W || dx * dx + dy * dy > r2) continue;
                fn(y * W + x, x, y);
            }
        }
    }
    public void Paint(int cx, int cy, int r, int t, bool replace)
    {
        if (t == PESSOA_B) { SpawnPerson(cx, cy); return; }
        int k = Kind[t];
        float prob = k == K_SOLID ? 1 : (k == K_FIRE || k == K_GAS) ? .3f : .5f;
        ForBrush(cx, cy, r, (i, _, _) =>
        {
            if (!replace && T[i] != 0) return;
            if (T[i] == t) return;
            if (r == 0 || Rnd() < prob) Set(i, t);
        });
    }
    void Erase(int cx, int cy, int r)
    {
        ForBrush(cx, cy, r, (i, _, _) => { T[i] = 0; E[i] = 0; L[i] = 0; Tm[i] = 20; Wake(i); });
        for (int p = pc - 1; p >= 0; p--) { float dx = pX[p] - cx, dy = pY[p] - 3 - cy; if (dx * dx + dy * dy <= (r + 3) * (r + 3)) RemovePerson(p); }
    }
    public void Tool(int cx, int cy, int r, string name)
    {
        switch (name)
        {
            case "erase": Erase(cx, cy, r); break;
            case "heat":
            case "cool":
                float d = name == "heat" ? 12 : -12;
                ForBrush(cx, cy, r, (i, _, _) => { if (T[i] != 0 && FixedA[T[i]] == 0) { Tm[i] += d; Wake(i); } });
                break;
            case "spark":
                ForBrush(cx, cy, r, (i, _, _) =>
                {
                    int u = T[i]; if (u == 0) return;
                    if (Elec[u] > 0) { if (u != INTERRUPTOR) AddPulse(i, 4); }
                    else if ((Trig[u] != 0 || Flam[u] > 0) && Rnd() < .2f) Ignite(i);
                });
                HurtArea(cx, cy, r + 3, 40, 0, 0);
                break;
            case "raio": Lightning(cx); break;
            case "neutron":
                for (int q = 0; q < Math.Max(1, r); q++) AddN(cx + (Rnd() - .5f) * r * 2, cy + (Rnd() - .5f) * r * 2, 0);
                break;
        }
    }

    public void Rain(int n, int[] types)
    {
        for (int k = 0; k < n; k++) { int x = (int)(Rnd() * W); if (T[x] == 0) Set(x, types[(int)(Rnd() * types.Length)]); }
    }
    public void Drain() { for (int i = N - W; i < N; i++) if (T[i] != 0) { T[i] = 0; Wake(i); } }
    public void Clear()
    {
        Array.Clear(T); Array.Clear(L); Array.Clear(E); Array.Fill(Tm, 20f);
        eQn = 0; eSrcN = 0; pulseN = 0;
        nc = 0; pc = 0; Array.Fill(occ, (short)-1); occN = 0; Array.Fill(wakeA, (byte)1);
    }
    public int Count() { int c = 0; for (int i = 0; i < N; i++) if (T[i] != 0) c++; return c; }

    // ---------- cenários ----------
    int X(double f) => (int)Math.Round(f * W);
    int Y(double f) => (int)Math.Round(f * H);
    void R(double x0, double y0, double x1, double y1, int t)
    {
        int ax = Math.Max(0, (int)Math.Round(x0)), bx = Math.Min(W, (int)Math.Round(x1));
        int ay = Math.Max(0, (int)Math.Round(y0)), by = Math.Min(H, (int)Math.Round(y1));
        for (int y = ay; y < by; y++) for (int x = ax; x < bx; x++) Set(y * W + x, t);
    }
    void SetXY(int x, int y, int t) { if (x >= 0 && x < W && y >= 0 && y < H) Set(y * W + x, t); }

    public string Preset(string name)
    {
        Clear();
        R(0, H - 3, W, H, PAREDE);
        if (name == "reator")
        {
            int a = X(.06), b = X(.54), top = Y(.14);
            R(a, top, a + 4, H - 3, BETAO); R(b - 4, top, b, H - 3, BETAO);
            R(a, H - 7, b, H - 3, BETAO); R(a, top, b, top + 4, BETAO);
            R(a + 4, Y(.5), b - 4, H - 7, AGUA);
            int xi0 = a + 4, wi = b - 4 - xi0, nr = 5, ry0 = Y(.6), ry1 = H - 12;
            for (int k = 0; k < nr; k++)
            {
                double xc = xi0 + wi * (k + 1.0) / (nr + 1);
                R(xc - 2, ry0, xc + 2, ry1, URANIO);
                if (k < nr - 1) { double xm = xi0 + wi * (k + 1.5) / (nr + 1); R(xm - 1.5, ry0 - 12, xm + 1.5, ry1 + 3, BORO); }
            }
            R(xi0, ry0 - 8, xi0 + 1, ry1, BORO); R(b - 5, ry0 - 8, b - 4, ry1, BORO);
            R(X(.22), top, X(.38), top + 6, GERADOR);
            int wx = X(.3), lx = X(.8), wy = top - 7;
            R(wx, wy, wx + 2, top, FIO); R(wx, wy, lx + 2, wy + 2, FIO);
            R(lx, wy, lx + 2, Y(.4), FIO);
            R(lx - 10, Y(.4), lx + 12, Y(.4) + 7, LAMPADA);
            R(lx, Y(.4) + 7, lx + 2, Y(.74), FIO);
            int t0 = X(.66), t1 = X(.94);
            R(t0, Y(.62), t0 + 2, H - 3, VIDRO); R(t1 - 2, Y(.62), t1, H - 3, VIDRO); R(t0, H - 5, t1, H - 3, VIDRO);
            R(t0 + 2, Y(.66), t1 - 2, H - 5, AGUA);
            R(X(.7), Y(.74), X(.9), Y(.74) + 2, RESIST);
            return "Reator: apaga as barras de controlo amarelas (botão direito) para arrancar. O vapor faz girar o gerador e acende as lâmpadas. Se aquecer de mais, funde (cório)!";
        }
        if (name == "circuito")
        {
            int by = Y(.45);
            R(X(.03), by - 8, X(.03) + 14, by + 10, BATERIA);
            R(X(.03) + 14, by, X(.9), by + 2, FIO);
            R(X(.22), by - 3, X(.3), by + 5, LAMPADA);
            int t0 = X(.36), t1 = X(.6);
            R(t0, Y(.6), t0 + 2, H - 3, VIDRO); R(t1 - 2, Y(.6), t1, H - 3, VIDRO); R(t0, H - 5, t1, H - 3, VIDRO);
            R(t0 + 2, Y(.66), t1 - 2, H - 5, AGUA);
            R(X(.48), by + 2, X(.48) + 2, H - 12, FIO);
            R(t0 + 5, H - 12, t1 - 5, H - 10, RESIST);
            R(X(.9), by, X(.9) + 2, Y(.68), FIO);
            R(X(.85), Y(.72), X(.95), Y(.8), C4);
            R(X(.82), Y(.8), X(.98), H - 3, PEDRA);
            R(X(.66), Y(.3), X(.76), Y(.36), MADEIRA);
            R(X(.66), Y(.36), X(.76), by, RESIST);
            return "Circuito: a bateria acende a lâmpada e a resistência ferve a água. O fio pára antes do C4: liga-o com Fio de cobre para rebentar.";
        }
        if (name == "vulcao")
        {
            int px = X(.38), py = Y(.3), half = X(.32);
            for (int x = 0; x < W; x++)
            {
                double h = py + Math.Abs(x - px) / (double)half * (H - py);
                for (int y = Math.Max(0, (int)Math.Round(h)); y < H - 3; y++) Set(y * W + x, Rnd() < .15f ? CASCALHO : PEDRA);
            }
            R(px - 3, py - 2, px + 3, Y(.75), LAVA);
            for (int dy = -12; dy <= 12; dy++) for (int dx = -22; dx <= 22; dx++)
                if (dx * dx / 484.0 + dy * dy / 144.0 <= 1) SetXY(px + dx, Y(.78) + dy, LAVA);
            for (int dy = 0; dy < 40; dy++) R(px - 34 + dy * .6, py - 42 + dy, px + 34 - dy * .6, py - 41 + dy, LAVA);
            R(X(.74), Y(.62), W, H - 3, AGUA); R(X(.72), Y(.62), X(.74), H - 3, PEDRA);
            R(0, Y(.86), X(.12), H - 3, TERRA);
            for (int k = 0; k < 8; k++) R(X(.01 + k * .013), Y(.86) - 2, X(.01 + k * .013) + 1, Y(.86), PLANTA);
            R(X(.03), Y(.6), X(.13), Y(.62), AGUA);
            R(px - 40, py - 60, px - 20, py - 56, NEVE);
            return "Vulcão: a lava escorre, aquece tudo e a água à volta ferve em vapor.";
        }
        if (name == "explosivos")
        {
            R(X(.1), Y(.8), X(.9), H - 3, PEDRA);
            R(X(.15), Y(.7), X(.25), Y(.8), C4);
            R(X(.35), Y(.72), X(.45), Y(.8), POLVORA);
            R(X(.55), Y(.74), X(.65), Y(.8), NITRO);
            R(X(.75), Y(.72), X(.85), Y(.8), TERMITE);
            R(X(.3), Y(.4), X(.7), Y(.42), METAL);
            R(X(.4), Y(.3), X(.6), Y(.4), HIDROG);
            return "Explosivos: usa a Faísca (Ferramentas) ou o Fogo para os acender.";
        }
        if (name == "pessoas")
        {
            R(0, Y(.8), W, H - 3, TERRA);
            for (int k = 0; k < 8; k++) R(X(.08) + k * 3, Y(.8) - (k + 1) * 2, X(.08) + k * 3 + 3, Y(.8), PEDRA);
            R(X(.08) + 24, Y(.8) - 16, X(.3), Y(.8) - 14, PEDRA);
            R(X(.4), Y(.7), X(.42), Y(.8), PEDRA); R(X(.58), Y(.7), X(.6), Y(.8), PEDRA);
            R(X(.42), Y(.72), X(.58), Y(.8), AGUA);
            R(X(.45), Y(.1), X(.55), Y(.14), NUVEM);
            R(X(.75), Y(.8) - 6, X(.75) + 2, Y(.8), TOCHA);
            R(X(.85), Y(.8) - 4, X(.95), Y(.8), GELO_SECO);
            for (int k = 0; k < 14; k++) SpawnPerson(X(.05 + k * .065), Y(.8) - 22);
            return "Pessoas: andam sozinhas, sobem degraus e caem. Experimenta cloro, fogo, raios ou água funda.";
        }
        if (name == "virus")
        {
            R(0, Y(.8), W, H - 3, TERRA);
            R(X(.47), Y(.8), X(.53), Y(.8) + 3, VIRUS);
            R(X(.1), Y(.8) - 10, X(.2), Y(.8) - 8, PEDRA);
            R(X(.8), Y(.25), X(.83), Y(.27), TORNEIRA);
            for (int k = 0; k < 14; k++) SpawnPerson(X(.05 + k * .065), Y(.8) - 22);
            return "Vírus: come o chão e infeta as pessoas, que se tornam vírus ao morrer. Trava-o com Cura (separador Vida) ou fogo.";
        }
        if (name == "artificio")
        {
            R(0, Y(.85), W, H - 3, PEDRA);
            R(X(.04), Y(.85) - 4, X(.04) + 2, Y(.85), TOCHA);
            R(X(.04) + 2, Y(.85) - 1, X(.96), Y(.85), POLVORA);
            R(X(.04) + 2, Y(.85) - 3, X(.04) + 4, Y(.85) - 1, FOGO);
            for (int k = 0; k < 9; k++) { int fx = X(.12 + k * .095); R(fx, Y(.85) - 6, fx + 2, Y(.85) - 1, FOGUETE); }
            return "Fogo de artifício: a tocha acende o rastilho de pólvora e os foguetes sobem e rebentam em cores.";
        }
        if (name == "eletrico")
        {
            // 1) bateria -> interruptor -> lâmpadas -> interruptor -> campainha
            int y1 = Y(.18);
            R(X(.02), y1 - 6, X(.02) + 8, y1 + 8, BATERIA);
            R(X(.02) + 8, y1, X(.12), y1 + 2, FIO);
            R(X(.12), y1 - 1, X(.12) + 4, y1 + 3, INTERRUPTOR);
            R(X(.12) + 4, y1, X(.36), y1 + 2, FIO);
            R(X(.18), y1 - 4, X(.23), y1, LAMPADA); R(X(.27), y1 - 4, X(.32), y1, LAMPADA);
            R(X(.36), y1 - 1, X(.36) + 4, y1 + 3, INTERRUPTOR);
            R(X(.36) + 4, y1, X(.44), y1 + 2, FIO);
            R(X(.44), y1 - 2, X(.44) + 4, y1 + 4, CAMPAINHA);
            for (int yy = y1 - 1; yy < y1 + 3; yy++) for (int xx = X(.12); xx < X(.12) + 4; xx++) Sh[yy * W + xx] = 255;
            // 2) painel solar -> tubo de néon
            int sy = Y(.06), ty = Y(.3);
            R(X(.58), sy, X(.74), sy + 3, SOLAR);
            R(X(.6), ty, X(.96), ty + 7, VIDRO);
            R(X(.6) + 1, ty + 1, X(.96) - 1, ty + 6, NEON);
            R(X(.65), sy + 3, X(.65) + 2, ty + 2, FIO);
            // 3) aquecedor com termóstato dentro da água: a água fica à volta dos 50 °C
            int t0 = X(.55), t1 = X(.8), top = Y(.6);
            R(t0, top, t0 + 2, H - 3, VIDRO); R(t1 - 2, top, t1, H - 3, VIDRO); R(t0, H - 5, t1, H - 3, VIDRO);
            R(t0 + 2, Y(.64), t1 - 2, H - 5, AGUA);
            R(t0 + 6, H - 10, t1 - 6, H - 8, RESIST);
            int wx = X(.68), ty0 = Y(.78);
            R(X(.3), Y(.4), X(.36), Y(.52), BATERIA);
            R(X(.36), Y(.45), wx + 2, Y(.45) + 2, FIO);
            R(wx, Y(.45), wx + 2, ty0, FIO);
            R(wx - 3, ty0, wx + 5, ty0 + 6, TERMOSTATO);
            R(wx, ty0 + 6, wx + 2, H - 10, FIO);
            return "Elétrico: clica nos interruptores com a Faísca (verde = ligado). O painel solar acende o néon enquanto tiver céu por cima. O termóstato corta a resistência quando a água passa dos 50 °C.";
        }
        if (name == "natureza")
        {
            int g = Y(.72);
            R(0, g, W, H - 3, TERRA);
            int l0 = X(.4), l1 = X(.72), lb = Y(.93);
            for (int y = g; y < lb; y++)
            {
                double f = (y - g) / (double)(lb - g);
                int a0 = (int)(l0 + (l1 - l0) * .5 * f * f), a1 = (int)(l1 - (l1 - l0) * .5 * f * f);
                for (int x = a0; x < a1; x++) Set(y * W + x, AGUA);
            }
            for (int k = 0; k < 14; k++) { int x = l0 + 6 + (int)(Rnd() * (l1 - l0 - 12)), y = g + 3 + (int)(Rnd() * (lb - g - 8)); if (T[y * W + x] == AGUA) Set(y * W + x, PEIXE); }
            for (int k = 0; k < 14; k++) { int x = (int)(Rnd() * W), y = g + 4 + (int)(Rnd() * (H - 8 - g)); if (T[y * W + x] == TERRA) Set(y * W + x, MINHOCA); }
            for (int k = 0; k < 10; k++) SetXY((int)(Rnd() * W), Y(.1 + Rnd() * .3), PASSARO);
            for (int k = 0; k < 12; k++) { int x = (int)(Rnd() * W); if (x < l0 - 2 || x > l1 + 2) R(x, g - 3 - (int)(Rnd() * 6), x + 1, g, PLANTA); }
            R(X(.1), Y(.08), X(.3), Y(.11), NUVEM);
            R(X(.78), g - 10, X(.8), g, MADEIRA); R(X(.75), g - 14, X(.83), g - 10, PLANTA);
            return "Natureza: peixes no lago, minhocas a escavar e pássaros no céu. Tira um peixe da água e ele sufoca.";
        }
        FillBench();
        return "";
    }

    public void FillBench()
    {
        Clear(); rs = 777; tick = 1; Array.Clear(S);
        for (int y = (int)(H * .35); y < H; y++) for (int x = 0; x < W; x++)
        {
            float r = Rnd();
            int t = r < .4f ? AREIA : r < .62f ? AGUA : r < .72f ? OLEO : r < .74f ? ACIDO : 0;
            if (t != 0) Set(y * W + x, t);
        }
        for (int p = 0; p < 6; p++)
        {
            int py = (int)(H * (.15 + .12 * p)), px = (int)(Rnd() * W * .7f), len = (int)(W * .25);
            for (int x = px; x < px + len && x < W; x++) Set(py * W + x, p % 2 == 1 ? MADEIRA : PEDRA);
        }
        ForBrush((int)(W * .3), (int)(H * .4), Math.Max(2, (int)(H * .06)), (i, _, _) => Set(i, LAVA));
        ForBrush((int)(W * .7), (int)(H * .3), Math.Max(2, (int)(H * .04)), (i, _, _) => Set(i, FOGO));
    }
}
