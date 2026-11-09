using System;
using System.Collections.Generic;

namespace Grao;

public sealed class Def
{
    public string K, N, C, Desc = "";
    public int Kind, Cat = -1;
    public bool Hidden;
    public float D = 1, Slip, Cond = .3f, Cap = 1, Temp = 20, Flam, BurnT, Flame = 800, Elec, Absorb = -1, Fiss, Alpha = 1;
    public float Air = -1, Vary = -1, Fixed = float.NaN;
    public int Spread, Trig, Boom, Mod, Act, Indes;
    public (int, int)? Burn, Life;
    public (float, string)? Hi, Lo;
}

public static partial class Mat
{
    public const int K_NONE = 0, K_SOLID = 1, K_POWDER = 2, K_LIQUID = 3, K_GAS = 4, K_FIRE = 5;
    public const int NM = 128;

    public static readonly byte[] Kind = new byte[NM], Spread = new byte[NM], BurnMin = new byte[NM], BurnMax = new byte[NM],
        HiTo = new byte[NM], LoTo = new byte[NM], Trig = new byte[NM], Boom = new byte[NM], ModA = new byte[NM], ActA = new byte[NM],
        FixedA = new byte[NM], LifeMin = new byte[NM], LifeMax = new byte[NM], IndesA = new byte[NM];
    public static readonly float[] Dens = new float[NM], Slip = new float[NM], Cond = new float[NM], Cap = new float[NM], Air = new float[NM],
        TempA = new float[NM], Flam = new float[NM], BurnT = new float[NM], Flame = new float[NM], HiT = new float[NM], LoT = new float[NM],
        Elec = new float[NM], Absorb = new float[NM], Fiss = new float[NM], FixedT = new float[NM], Alpha = new float[NM], Vary = new float[NM];
    public static readonly float[] Pal = new float[NM * 3];
    public static readonly Dictionary<string, int> Id = new();

    static Mat()
    {
        float[] airBy = { 0, .0015f, .0015f, .002f, .003f, 0 };
        Array.Fill(Cap, 1f); Array.Fill(HiT, float.PositiveInfinity); Array.Fill(LoT, float.NegativeInfinity);
        for (int i = 0; i < Defs.Length; i++) Id[Defs[i].K] = i;
        for (int i = 0; i < Defs.Length; i++)
        {
            var d = Defs[i];
            Kind[i] = (byte)d.Kind; Dens[i] = d.D; Slip[i] = d.Slip; Spread[i] = (byte)d.Spread;
            Cond[i] = d.Cond; Cap[i] = d.Cap; Air[i] = d.Air >= 0 ? d.Air : airBy[d.Kind]; TempA[i] = d.Temp;
            Flam[i] = d.Flam; BurnT[i] = d.BurnT;
            BurnMin[i] = (byte)(d.Burn?.Item1 ?? 20); BurnMax[i] = (byte)(d.Burn?.Item2 ?? 50); Flame[i] = d.Flame;
            if (d.Hi is { } hi) { HiT[i] = hi.Item1; HiTo[i] = (byte)Id[hi.Item2]; }
            if (d.Lo is { } lo) { LoT[i] = lo.Item1; LoTo[i] = (byte)Id[lo.Item2]; }
            Elec[i] = d.Elec; Trig[i] = (byte)d.Trig; Boom[i] = (byte)d.Boom;
            Absorb[i] = d.Absorb >= 0 ? d.Absorb : (d.Kind == K_GAS || d.Kind == K_NONE ? 0 : .02f);
            ModA[i] = (byte)d.Mod; Fiss[i] = d.Fiss; ActA[i] = (byte)d.Act;
            if (!float.IsNaN(d.Fixed)) { FixedA[i] = 1; FixedT[i] = d.Fixed; }
            if (d.Life is { } lf) { LifeMin[i] = (byte)lf.Item1; LifeMax[i] = (byte)lf.Item2; }
            IndesA[i] = (byte)d.Indes;
            var c = Hex(d.C); Pal[i * 3] = c.r; Pal[i * 3 + 1] = c.g; Pal[i * 3 + 2] = c.b;
            Alpha[i] = d.Alpha;
            Vary[i] = d.Vary >= 0 ? d.Vary : (d.Kind == K_LIQUID ? .1f : .22f);
        }
    }

    public static (float r, float g, float b) Hex(string h)
    {
        int n = Convert.ToInt32(h.Substring(1), 16);
        return (((n >> 16) & 255) / 255f, ((n >> 8) & 255) / 255f, (n & 255) / 255f);
    }
}
