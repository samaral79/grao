// Tabela de materiais (começou gerada a partir do sim.js da versão web).
namespace Grao;

public static partial class Mat
{
    public static readonly string[] Cats = { "Pós", "Líquidos", "Gases", "Sólidos", "Eletricidade", "Explosivos", "Nuclear", "Vida", "Especiais" };

    public const int VAZIO = 0;
    public const int AREIA = 1;
    public const int TERRA = 2;
    public const int CASCALHO = 3;
    public const int NEVE = 4;
    public const int SAL = 5;
    public const int CIMENTO = 6;
    public const int CINZA = 7;
    public const int AGUA = 8;
    public const int AGUA_S = 9;
    public const int OLEO = 10;
    public const int ACIDO = 11;
    public const int LAVA = 12;
    public const int AZOTO_L = 13;
    public const int MERCURIO = 14;
    public const int VIDRO_F = 15;
    public const int METAL_F = 16;
    public const int VAPOR = 17;
    public const int FUMO = 18;
    public const int METANO = 19;
    public const int HIDROG = 20;
    public const int AZOTO = 21;
    public const int PEDRA = 22;
    public const int MADEIRA = 23;
    public const int METAL = 24;
    public const int VIDRO = 25;
    public const int GELO = 26;
    public const int BETAO = 27;
    public const int PLANTA = 28;
    public const int PAREDE = 29;
    public const int FIO = 30;
    public const int BATERIA = 31;
    public const int RESIST = 32;
    public const int LAMPADA = 33;
    public const int GERADOR = 34;
    public const int AQUECEDOR = 35;
    public const int ARREF = 36;
    public const int FOGO = 37;
    public const int POLVORA = 38;
    public const int NITRO = 39;
    public const int C4 = 40;
    public const int TERMITE = 41;
    public const int URANIO = 42;
    public const int GRAFITE = 43;
    public const int BORO = 44;
    public const int CHUMBO = 45;
    public const int RESIDUO = 46;
    public const int CORIO = 47;
    public const int FERRUGEM = 48;
    public const int LAMA = 49;
    public const int MEL = 50;
    public const int GASOLINA = 51;
    public const int CLORO = 52;
    public const int CO2 = 53;
    public const int GELO_SECO = 54;
    public const int SODIO = 55;
    public const int PESSOA_B = 56;
    public const int SEMENTE = 57;
    public const int SANGUE = 58;
    public const int NUVEM = 59;
    public const int TOCHA = 60;
    public const int TORNEIRA = 61;
    public const int RALO = 62;
    public const int VIRUS = 63;
    public const int CURA = 64;
    public const int CLONADOR = 65;
    public const int BURACO = 66;
    public const int FOGUETE = 67;
    public const int FOGUETE_V = 68;
    public const int FAGULHA = 69;
    public const int BOMBA_A = 70;
    public const int ANTIMAT = 71;
    public const int OXIGENIO = 72;
    public const int HELIO = 73;
    public const int PROPANO = 74;
    public const int NEON = 75;
    public const int PLASMA = 76;
    public const int CERA = 77;
    public const int CERA_L = 78;
    public const int OURO = 79;
    public const int OURO_F = 80;
    public const int TIJOLO = 81;
    public const int CARVAO = 82;
    public const int SERRADURA = 83;
    public const int ALCOOL = 84;
    public const int PEIXE = 85;
    public const int MINHOCA = 86;
    public const int PASSARO = 87;
    public const int DINAMITE = 88;
    public const int DINAMITE_A = 89;
    public const int MINA = 90;
    public const int BOMBA_GELO = 91;
    public const int PLUTONIO = 92;
    public const int INTERRUPTOR = 93;
    public const int TERMOSTATO = 94;
    public const int SOLAR = 95;
    public const int CAMPAINHA = 96;

    public static readonly Def[] Defs =
    {
        new() { K = "VAZIO", N = "Ar", Kind = 0, C = "#000000", Hidden = true, Flame = 650f },
        new() { K = "AREIA", N = "Areia", Kind = 2, C = "#dcc184", Cat = 0, D = 1.6f, Slip = 0.9f, Cap = 0.8f, Hi = (1700f, "VIDRO_F"), Desc = "Cai e faz montes. Afunda na água. A 1700 °C derrete em vidro." },
        new() { K = "TERRA", N = "Terra", Kind = 2, C = "#6e4c2f", Cat = 0, D = 1.3f, Slip = 0.3f, Cond = 0.25f, Act = 1, Hi = (1400f, "LAVA"), Desc = "Mais pegajosa que a areia: faz montes íngremes. Com água fica lama." },
        new() { K = "CASCALHO", N = "Cascalho", Kind = 2, C = "#8f8b86", Cat = 0, D = 1.9f, Slip = 0.25f, Cap = 0.8f, Vary = 0.5f, Hi = (1200f, "LAVA"), Desc = "Pedrinhas pesadas. Derretem em lava a 1200 °C." },
        new() { K = "NEVE", N = "Neve", Kind = 2, C = "#eef4fb", Cat = 0, D = 0.3f, Slip = 0.2f, Cond = 0.15f, Cap = 2f, Temp = -5f, Vary = 0.08f, Hi = (1f, "AGUA"), Desc = "Fria (-5 °C). Derrete acima de 0 °C. O sal derrete-a." },
        new() { K = "SAL", N = "Sal", Kind = 2, C = "#efece6", Cat = 0, D = 2.1f, Slip = 0.6f, Act = 1, Vary = 0.08f, Desc = "Dissolve-se na água (fica água salgada). Derrete gelo e neve." },
        new() { K = "CIMENTO", N = "Cimento", Kind = 2, C = "#b2afa6", Cat = 0, D = 1.5f, Slip = 0.4f, Act = 1, Desc = "Pó que endurece em betão quando toca em água." },
        new() { K = "CINZA", N = "Cinza", Kind = 2, C = "#8b8885", Cat = 0, D = 0.5f, Slip = 0.5f, Cond = 0.1f, Desc = "O que sobra da madeira e das plantas. Leve, flutua na água." },
        new() { K = "AGUA", N = "Água", Kind = 3, C = "#2f74e0", Cat = 1, D = 1f, Slip = 1f, Spread = 10, Cond = 0.6f, Cap = 4f, Air = 0.0008f, Elec = 0.15f, Absorb = 0.006f, Mod = 1, Act = 1, Vary = 0.1f, Hi = (100f, "VAPOR"), Lo = (0f, "GELO"), Desc = "Ferve a 100 °C, congela a 0 °C e evapora devagar à superfície. Conduz mal a corrente (a salgada conduz bem) e abranda neutrões." },
        new() { K = "AGUA_S", N = "Água salgada", Kind = 3, C = "#3a86b8", Cat = 1, D = 1.03f, Slip = 1f, Spread = 6, Cond = 0.6f, Cap = 4f, Elec = 0.95f, Absorb = 0.01f, Mod = 1, Act = 1, Vary = 0.1f, Hi = (102f, "VAPOR"), Lo = (-21f, "GELO"), Desc = "Conduz muito bem a eletricidade. Quando ferve, deixa sal." },
        new() { K = "OLEO", N = "Óleo", Kind = 3, C = "#5e4119", Cat = 1, D = 0.85f, Slip = 0.9f, Spread = 4, Cond = 0.2f, Cap = 2f, Flam = 0.35f, BurnT = 300f, Burn = (60, 120), Desc = "Flutua na água e arde bem (acende a 300 °C)." },
        new() { K = "ACIDO", N = "Ácido", Kind = 3, C = "#8cff40", Cat = 1, D = 1.1f, Slip = 1f, Spread = 5, Cond = 0.5f, Cap = 3f, Act = 1, Hi = (300f, "FUMO"), Desc = "Corrói quase tudo (vidro e parede não). Com metal liberta hidrogénio." },
        new() { K = "LAVA", N = "Lava", Kind = 3, C = "#ff5a14", Cat = 1, D = 2.6f, Slip = 0.2f, Spread = 2, Cond = 0.25f, Cap = 3f, Air = 0.0003f, Temp = 1200f, Lo = (700f, "PEDRA"), Desc = "Rocha derretida a 1200 °C. Arrefece em pedra e faz a água ferver." },
        new() { K = "AZOTO_L", N = "Azoto líquido", Kind = 3, C = "#bfe6ff", Cat = 1, D = 0.8f, Slip = 1f, Spread = 6, Cond = 0.5f, Temp = -196f, Vary = 0.06f, Hi = (-190f, "AZOTO"), Desc = "A -196 °C congela tudo em que toca e evapora depressa." },
        new() { K = "MERCURIO", N = "Mercúrio", Kind = 3, C = "#c7ccd3", Cat = 1, D = 13.5f, Slip = 1f, Spread = 3, Cond = 0.7f, Cap = 0.2f, Elec = 0.95f, Vary = 0.06f, Desc = "Metal líquido muito pesado. Conduz eletricidade." },
        new() { K = "VIDRO_F", N = "Vidro derretido", Kind = 3, C = "#ffa04a", Hidden = true, D = 2.4f, Slip = 0.15f, Spread = 1, Air = 0.0006f, Temp = 1800f, Lo = (1000f, "VIDRO") },
        new() { K = "METAL_F", N = "Metal derretido", Kind = 3, C = "#ff7a2a", Hidden = true, D = 7f, Slip = 0.4f, Spread = 2, Cond = 0.8f, Cap = 0.5f, Air = 0.0006f, Temp = 1650f, Elec = 0.95f, Lo = (1400f, "METAL") },
        new() { K = "VAPOR", N = "Vapor", Kind = 4, C = "#eef3fa", Cat = 2, D = 0.0006f, Slip = 0.75f, Cond = 0.08f, Air = 0.0006f, Temp = 105f, Alpha = 0.55f, Vary = 0.15f, Lo = (70f, "AGUA"), Desc = "Água a ferver. Sobe, arrefece nas superfícies frias e volta a chover. Faz girar o gerador." },
        new() { K = "FUMO", N = "Fumo", Kind = 4, C = "#3e3f46", Cat = 2, D = 0.001f, Slip = 0.6f, Alpha = 0.65f, Life = (60, 180), Desc = "Sobe e desaparece." },
        new() { K = "METANO", N = "Metano", Kind = 4, C = "#b8e08f", Cat = 2, D = 0.0007f, Slip = 0.55f, Flam = 0.9f, BurnT = 580f, Flame = 1300f, Trig = 1, Alpha = 0.3f, Burn = (3, 10), Desc = "Gás natural. Muito inflamável: uma faísca chega." },
        new() { K = "HIDROG", N = "Hidrogénio", Kind = 4, C = "#e6d9ff", Cat = 2, D = 0.0001f, Slip = 0.9f, Flam = 0.95f, BurnT = 500f, Flame = 1500f, Trig = 1, Alpha = 0.3f, Burn = (2, 8), Desc = "O gás mais leve. Arde de repente ao mínimo fogo ou faísca." },
        new() { K = "AZOTO", N = "Azoto", Kind = 4, C = "#dbe6f0", Cat = 2, D = 0.0012f, Slip = 0.4f, Temp = -190f, Alpha = 0.25f, Life = (80, 200), Desc = "Gás geladíssimo que sai do azoto líquido." },
        new() { K = "PEDRA", N = "Pedra", Kind = 1, C = "#7f828a", Cat = 3, Cap = 0.8f, Hi = (1200f, "LAVA"), Desc = "Fica no sítio. Derrete em lava a 1200 °C." },
        new() { K = "MADEIRA", N = "Madeira", Kind = 1, C = "#7b4a24", Cat = 3, Cond = 0.12f, Cap = 1.7f, Flam = 0.04f, BurnT = 300f, Burn = (150, 250), Desc = "Arde devagar e deixa cinza. Acende a 300 °C." },
        new() { K = "METAL", N = "Metal", Kind = 1, C = "#9aa3ad", Cat = 3, Cond = 0.9f, Cap = 0.45f, Elec = 1f, Act = 1, Vary = 0.08f, Hi = (1538f, "METAL_F"), Desc = "Ferro. Conduz calor e eletricidade, enferruja na água. Fica em brasa e derrete a 1538 °C." },
        new() { K = "VIDRO", N = "Vidro", Kind = 1, C = "#a9dbe9", Cat = 3, Cond = 0.2f, Cap = 0.8f, Alpha = 0.45f, Vary = 0.04f, Hi = (1400f, "VIDRO_F"), Desc = "Transparente. Resiste ao ácido e não conduz corrente." },
        new() { K = "GELO", N = "Gelo", Kind = 1, C = "#a8dcf5", Cat = 3, Cond = 0.5f, Cap = 2f, Temp = -10f, Vary = 0.08f, Hi = (0.5f, "AGUA"), Desc = "Água sólida. Derrete acima de 0 °C." },
        new() { K = "BETAO", N = "Betão", Kind = 1, C = "#9a9890", Cat = 3, Cond = 0.25f, Absorb = 0.08f, Hi = (1500f, "LAVA"), Desc = "Forte e isolante. Bom para paredes de reator." },
        new() { K = "PLANTA", N = "Planta", Kind = 1, C = "#3fa34d", Cat = 7, Cond = 0.2f, Cap = 3f, Flam = 0.15f, BurnT = 200f, Act = 1, Burn = (40, 80), Desc = "Bebe água e cresce. Arde." },
        new() { K = "PAREDE", N = "Parede", Kind = 1, C = "#33363f", Cat = 3, Cond = 0f, Indes = 1, Vary = 0.04f, Desc = "Indestrutível. Não deixa passar calor nem explosões." },
        new() { K = "FIO", N = "Fio de cobre", Kind = 1, C = "#c8773b", Cat = 4, Cond = 0.95f, Cap = 0.4f, Elec = 1f, Vary = 0.08f, Hi = (1085f, "METAL_F"), Desc = "Leva a corrente da bateria ou do gerador até às coisas." },
        new() { K = "BATERIA", N = "Bateria", Kind = 1, C = "#2f63c9", Cat = 4, Act = 1, Vary = 0.04f, Desc = "Fonte de corrente. Encosta-lhe fios." },
        new() { K = "RESIST", N = "Resistência", Kind = 1, C = "#9b6f5c", Cat = 4, Cond = 0.6f, Cap = 0.5f, Elec = 1f, Hi = (1400f, "METAL_F"), Desc = "Fio que aquece com corrente até ficar em brasa. Ferve água." },
        new() { K = "LAMPADA", N = "Lâmpada", Kind = 1, C = "#5d5a44", Cat = 4, Cond = 0.3f, Elec = 1f, Vary = 0.04f, Desc = "Acende quando lhe passa corrente (e deixa-a passar)." },
        new() { K = "GERADOR", N = "Gerador a vapor", Kind = 1, C = "#56606c", Cat = 4, Cond = 0.5f, Elec = 1f, Act = 1, Vary = 0.04f, Desc = "Turbina: quando o vapor lhe toca, gera corrente para os fios encostados." },
        new() { K = "AQUECEDOR", N = "Aquecedor", Kind = 1, C = "#c2452b", Cat = 4, Cond = 0.8f, Act = 1, Fixed = 600f, Desc = "Sempre a 600 °C." },
        new() { K = "ARREF", N = "Arrefecedor", Kind = 1, C = "#2b8ec2", Cat = 4, Cond = 0.8f, Act = 1, Fixed = -150f, Desc = "Sempre a -150 °C." },
        new() { K = "FOGO", N = "Fogo", Kind = 5, C = "#ff8a1f", Cat = 5, Cond = 0.5f, Temp = 800f, Life = (20, 50), Desc = "Queima o que é inflamável e aquece tudo à volta. A água apaga-o." },
        new() { K = "POLVORA", N = "Pólvora", Kind = 2, C = "#3e3e42", Cat = 5, D = 1.7f, Slip = 0.5f, Flam = 0.7f, BurnT = 200f, Trig = 1, Boom = 3, Desc = "Pó que explode em cadeia com fogo, calor ou faísca." },
        new() { K = "NITRO", N = "Nitroglicerina", Kind = 3, C = "#e9e2a8", Cat = 5, D = 1.6f, Slip = 1f, Spread = 4, Cap = 1.2f, Flam = 0.9f, BurnT = 150f, Trig = 1, Boom = 8, Desc = "Líquido muito instável: explode a 150 °C, com fogo ou com faísca." },
        new() { K = "C4", N = "C4", Kind = 1, C = "#dcd3ad", Cat = 5, Cond = 0.2f, BurnT = 400f, Trig = 1, Boom = 14, Vary = 0.06f, Desc = "Explosivo plástico fortíssimo. Detona com corrente (liga-lhe um fio) ou com outra explosão." },
        new() { K = "TERMITE", N = "Termite", Kind = 2, C = "#8a4b3a", Cat = 5, D = 2f, Slip = 0.4f, Flam = 0.03f, BurnT = 900f, Flame = 2500f, Trig = 1, Burn = (80, 140), Desc = "Arde a 2500 °C e derrete metal. Acende com faísca ou muito calor." },
        new() { K = "URANIO", N = "Urânio", Kind = 1, C = "#5c8c3c", Cat = 6, Cond = 0.4f, Cap = 0.25f, Absorb = 0.004f, Fiss = 1f, Act = 1, Life = (250, 250), Hi = (1130f, "CORIO"), Desc = "Combustível. Um neutrão parte o átomo (fissão): dá calor e mais neutrões. Dentro de água a reação acelera." },
        new() { K = "GRAFITE", N = "Grafite", Kind = 1, C = "#2b2b30", Cat = 6, Cond = 0.7f, Flam = 0.01f, BurnT = 750f, Absorb = 0.0005f, Mod = 2, Burn = (200, 255), Desc = "Moderador: abranda os neutrões sem os comer (mais fissões)." },
        new() { K = "BORO", N = "Barra de controlo", Kind = 1, C = "#c9b13a", Cat = 6, Cond = 0.4f, Absorb = 0.9f, Desc = "Boro: come neutrões. Mete entre o urânio para travar o reator." },
        new() { K = "CHUMBO", N = "Chumbo", Kind = 1, C = "#5b6070", Cat = 6, Cond = 0.35f, Cap = 0.3f, Absorb = 0.3f, Desc = "Blindagem contra radiação." },
        new() { K = "RESIDUO", N = "Resíduo nuclear", Kind = 2, C = "#8aa83a", Cat = 6, D = 3f, Slip = 0.3f, Fiss = 0.1f, Act = 1, Desc = "Urânio gasto. Continua quente e radioativo." },
        new() { K = "CORIO", N = "Cório", Kind = 3, C = "#d9a03a", Cat = 6, D = 8f, Slip = 0.15f, Spread = 1, Cond = 0.4f, Cap = 0.5f, Air = 0.0006f, Temp = 2600f, Fiss = 0.5f, Act = 1, Lo = (1000f, "RESIDUO"), Desc = "Núcleo derretido de um reator (fusão). Quentíssimo e radioativo." },
        new() { K = "FERRUGEM", N = "Ferrugem", Kind = 2, C = "#8b4a2b", Cat = 0, D = 2.5f, Slip = 0.3f, Desc = "Pó castanho em que o metal se desfaz quando passa muito tempo na água." },
        new() { K = "LAMA", N = "Lama", Kind = 3, C = "#5a3d22", Cat = 1, D = 1.6f, Slip = 0.12f, Spread = 1, Cond = 0.4f, Cap = 2.5f, Hi = (100f, "TERRA"), Desc = "Terra encharcada: escorre devagar. Com calor seca e volta a ser terra." },
        new() { K = "MEL", N = "Mel", Kind = 3, C = "#e0a526", Cat = 1, D = 1.4f, Slip = 0.04f, Spread = 1, Cap = 2f, Desc = "Muito viscoso, escorre devagarinho." },
        new() { K = "GASOLINA", N = "Gasolina", Kind = 3, C = "#e7d77a", Cat = 1, D = 0.72f, Slip = 1f, Spread = 5, Cap = 2f, Flam = 0.8f, BurnT = 250f, Flame = 1100f, Trig = 1, Alpha = 0.75f, Burn = (8, 20), Desc = "Flutua por cima da água e arde de repente. Basta uma faísca." },
        new() { K = "CLORO", N = "Cloro", Kind = 4, C = "#c8e85a", Cat = 2, D = 0.0032f, Slip = 0.3f, Alpha = 0.45f, Desc = "Gás tóxico e pesado: desce e junta-se em baixo. Mata pessoas e plantas." },
        new() { K = "CO2", N = "Dióxido de carbono", Kind = 4, C = "#cfd6dc", Cat = 2, D = 0.0019f, Slip = 0.3f, Alpha = 0.22f, Life = (250, 450), Desc = "Gás pesado que apaga o fogo. Sai do gelo seco." },
        new() { K = "GELO_SECO", N = "Gelo seco", Kind = 1, C = "#e9eef2", Cat = 3, Cond = 0.3f, Cap = 3f, Air = 0.0002f, Temp = -78f, Act = 1, Vary = 0.06f, Hi = (-70f, "CO2"), Desc = "CO2 sólido a -78 °C. Vai largando gás que apaga o fogo." },
        new() { K = "SODIO", N = "Sódio", Kind = 1, C = "#cfd2c8", Cat = 5, Cond = 0.5f, Act = 1, Vary = 0.06f, Desc = "Metal que rebenta quando toca em água." },
        new() { K = "PESSOA_B", N = "Pessoa", Kind = 1, C = "#3d6fd1", Cat = 7, Desc = "Anda sozinha, sobe degraus e cai. Afoga-se, e morre com calor, frio, ácido, cloro, choques ou radiação." },
        new() { K = "SEMENTE", N = "Semente", Kind = 2, C = "#a5852f", Cat = 7, D = 1.1f, Slip = 0.5f, Flam = 0.1f, BurnT = 200f, Act = 1, Desc = "Cai e, em cima de terra ou lama, germina numa planta." },
        new() { K = "SANGUE", N = "Sangue", Kind = 3, C = "#8a1016", Hidden = true, D = 1.06f, Slip = 0.8f, Spread = 3, Cap = 3.6f, Hi = (100f, "VAPOR"), Lo = (-2f, "GELO") },
        new() { K = "NUVEM", N = "Nuvem", Kind = 1, C = "#dfe5ee", Cat = 8, Cond = 0.05f, Air = 0.0002f, Act = 1, Alpha = 0.55f, Vary = 0.1f, Desc = "Flutua e faz chover (ou nevar, se a arrefeceres abaixo de 0 °C)." },
        new() { K = "TOCHA", N = "Tocha", Kind = 1, C = "#6b3f1e", Cat = 8, Cond = 0.1f, Act = 1, Desc = "Arde para sempre." },
        new() { K = "TORNEIRA", N = "Torneira", Kind = 1, C = "#7d8fa6", Cat = 8, Cond = 0.5f, Act = 1, Desc = "Deita água sem parar." },
        new() { K = "RALO", N = "Ralo", Kind = 1, C = "#15161c", Cat = 8, Cond = 0f, Act = 1, Indes = 1, Desc = "Engole tudo o que se mexe e lhe toca." },
        new() { K = "VIRUS", N = "Vírus", Kind = 1, C = "#b03cff", Cat = 7, Cond = 0.2f, Act = 1, Flam = 0.3f, Vary = 0.35f, Hi = (120f, "FUMO"), Desc = "Infeta tudo em que toca (e as pessoas) e transforma em vírus. Morre com fogo ou acima de 120 °C. A Cura devolve o material original." },
        new() { K = "CURA", N = "Cura", Kind = 3, C = "#7ff0e0", Cat = 7, D = 1f, Slip = 1f, Spread = 5, Cap = 4f, Act = 1, Vary = 0.08f, Desc = "Líquido que desfaz o vírus e devolve o que ele comeu. Também cura pessoas infetadas." },
        new() { K = "CLONADOR", N = "Clonador", Kind = 1, C = "#d4c24a", Cat = 8, Cond = 0f, Act = 1, Indes = 1, Vary = 0.04f, Desc = "Copia o primeiro material que lhe tocar e não pára de o fabricar." },
        new() { K = "BURACO", N = "Buraco negro", Kind = 1, C = "#000000", Cat = 8, Cond = 0f, Act = 1, Indes = 1, Vary = 0f, Desc = "Puxa e engole tudo à volta, até pedra e pessoas. Só a parede lhe resiste." },
        new() { K = "FOGUETE", N = "Fogo de artifício", Kind = 1, C = "#e84a6f", Cat = 5, Cond = 0.2f, Flam = 0.5f, BurnT = 150f, Trig = 1, Vary = 0.1f, Desc = "Acende-o com fogo, faísca ou calor: sobe a disparar e rebenta em cores." },
        new() { K = "FOGUETE_V", N = "Foguete", Kind = 1, C = "#ffd27a", Hidden = true, Cond = 0f, Act = 1, Life = (25, 45), Vary = 0f },
        new() { K = "FAGULHA", N = "Fagulha", Kind = 1, C = "#ffffff", Hidden = true, Cond = 0f, Act = 1, Life = (25, 55), Absorb = 0f, Vary = 0f },
        new() { K = "BOMBA_A", N = "Bomba atómica", Kind = 1, C = "#4d5a2a", Cat = 6, Cond = 0.2f, BurnT = 500f, Trig = 1, Vary = 0.06f, Desc = "Rebenta com corrente, fogo ou outra explosão: explosão enorme, calor e radiação." },
        new() { K = "ANTIMAT", N = "Antimatéria", Kind = 2, C = "#ff3df2", Cat = 8, D = 1f, Slip = 0.7f, Cond = 0f, Act = 1, Vary = 0.3f, Desc = "Aniquila tudo o que toca e desaparece num clarão." },
        new() { K = "OXIGENIO", N = "Oxigénio", Kind = 4, C = "#a8d8ff", Cat = 2, D = 0.0011f, Slip = 0.3f, Flam = 0.8f, Flame = 1400f, Alpha = 0.2f, Burn = (2, 6), Desc = "Não arde sozinho, mas o fogo dentro dele fica muito mais quente. Sai da água com corrente." },
        new() { K = "HELIO", N = "Hélio", Kind = 4, C = "#fff4c2", Cat = 2, D = 0.00002f, Slip = 1f, Alpha = 0.2f, Desc = "Levíssimo e inerte: dispara para cima e não arde." },
        new() { K = "PROPANO", N = "Propano", Kind = 4, C = "#d9c6f0", Cat = 2, D = 0.0019f, Slip = 0.3f, Flam = 0.9f, BurnT = 470f, Flame = 1400f, Trig = 1, Alpha = 0.3f, Burn = (3, 9), Desc = "Gás de botija, mais pesado que o ar: junta-se no chão. Explode com uma faísca." },
        new() { K = "NEON", N = "Néon", Kind = 4, C = "#ff5a8a", Cat = 4, D = 0.0009f, Slip = 0.5f, Elec = 1f, Alpha = 0.3f, Desc = "Gás que brilha cor-de-rosa quando lhe passa corrente. Fecha-o num tubo de vidro." },
        new() { K = "PLASMA", N = "Plasma", Kind = 4, C = "#e0b0ff", Cat = 8, D = 0.0003f, Slip = 0.8f, Cond = 0.5f, Temp = 5000f, Elec = 1f, Act = 1, Alpha = 0.9f, Life = (8, 20), Desc = "Gás a 5000 °C. Incendeia e derrete o que toca e conduz corrente." },
        new() { K = "CERA", N = "Cera", Kind = 1, C = "#efe3c0", Cat = 3, Cond = 0.15f, Cap = 2f, Flam = 0.02f, BurnT = 250f, Burn = (100, 200), Vary = 0.08f, Hi = (60f, "CERA_L"), Desc = "Derrete a 60 °C e volta a solidificar quando arrefece. Arde devagar." },
        new() { K = "CERA_L", N = "Cera derretida", Kind = 3, C = "#f5d98a", Hidden = true, D = 0.9f, Slip = 0.5f, Spread = 2, Cond = 0.15f, Cap = 2f, Temp = 70f, Flam = 0.1f, BurnT = 250f, Burn = (30, 60), Lo = (55f, "CERA") },
        new() { K = "OURO", N = "Ouro", Kind = 1, C = "#e8c040", Cat = 3, Cond = 0.95f, Cap = 0.3f, Elec = 1f, Vary = 0.12f, Hi = (1064f, "OURO_F"), Desc = "Conduz corrente e calor, não enferruja e o ácido não lhe pega. Derrete a 1064 °C." },
        new() { K = "OURO_F", N = "Ouro derretido", Kind = 3, C = "#ffb040", Hidden = true, D = 19f, Slip = 0.5f, Spread = 2, Cond = 0.9f, Cap = 0.3f, Air = 0.0006f, Temp = 1100f, Elec = 1f, Lo = (1000f, "OURO") },
        new() { K = "TIJOLO", N = "Tijolo", Kind = 1, C = "#a5502e", Cat = 3, Cond = 0.12f, Absorb = 0.06f, Vary = 0.15f, Hi = (1700f, "LAVA"), Desc = "Isola o calor e aguenta até 1700 °C. Bom para fornos." },
        new() { K = "CARVAO", N = "Carvão", Kind = 2, C = "#2a2624", Cat = 0, D = 1.2f, Slip = 0.3f, Cond = 0.2f, Flam = 0.02f, BurnT = 350f, Flame = 1100f, Burn = (200, 255), Desc = "Arde muito tempo e bem quente. Às vezes sobra da madeira queimada." },
        new() { K = "SERRADURA", N = "Serradura", Kind = 2, C = "#c8a26a", Cat = 0, D = 0.4f, Slip = 0.4f, Cond = 0.1f, Flam = 0.25f, BurnT = 250f, Burn = (10, 30), Desc = "Pó de madeira leve. Flutua e arde num instante." },
        new() { K = "ALCOOL", N = "Álcool", Kind = 3, C = "#d8f0ff", Cat = 1, D = 0.79f, Slip = 1f, Spread = 6, Cap = 2.4f, Flam = 0.6f, BurnT = 360f, Flame = 900f, Alpha = 0.5f, Burn = (6, 14), Hi = (78f, "METANO"), Desc = "Arde com chama limpa e ferve a 78 °C em vapor inflamável." },
        new() { K = "PEIXE", N = "Peixe", Kind = 2, C = "#f08a3a", Cat = 7, D = 1f, Slip = 0f, Act = 1, Flam = 0.1f, BurnT = 200f, Vary = 0.15f, Hi = (60f, "SANGUE"), Desc = "Nada na água. Fora de água sufoca e morre." },
        new() { K = "MINHOCA", N = "Minhoca", Kind = 2, C = "#d77a8a", Cat = 7, D = 1.3f, Slip = 0.2f, Act = 1, Vary = 0.1f, Hi = (60f, "SANGUE"), Desc = "Escava túneis na terra e na lama." },
        new() { K = "PASSARO", N = "Pássaro", Kind = 1, C = "#5a7fd0", Cat = 7, Act = 1, Flam = 0.3f, BurnT = 200f, Vary = 0.2f, Hi = (80f, "CINZA"), Desc = "Voa sozinho e foge do chão." },
        new() { K = "DINAMITE", N = "Dinamite", Kind = 1, C = "#c8302a", Cat = 5, Cond = 0.2f, Flam = 0.3f, BurnT = 200f, Trig = 1, Vary = 0.06f, Desc = "Acende-se com fogo ou faísca, o rastilho arde uns segundos e depois rebenta." },
        new() { K = "DINAMITE_A", N = "Dinamite acesa", Kind = 1, C = "#ff5a40", Hidden = true, Cond = 0.2f, Act = 1, Life = (50, 90), Vary = 0.06f },
        new() { K = "MINA", N = "Mina", Kind = 1, C = "#4a5040", Cat = 5, Cond = 0.2f, BurnT = 300f, Trig = 1, Boom = 9, Act = 1, Vary = 0f, Desc = "Rebenta quando alguém a pisa ou lhe cai coisa em cima." },
        new() { K = "BOMBA_GELO", N = "Bomba de gelo", Kind = 1, C = "#7fd8ff", Cat = 5, Cond = 0.2f, BurnT = 500f, Trig = 1, Vary = 0.06f, Desc = "Em vez de calor, liberta um frio de -250 °C que congela tudo à volta." },
        new() { K = "PLUTONIO", N = "Plutónio", Kind = 1, C = "#8a5cc0", Cat = 6, Cond = 0.4f, Cap = 0.25f, Absorb = 0.004f, Fiss = 2.5f, Act = 1, Life = (250, 250), Hi = (640f, "CORIO"), Desc = "Mais instável que o urânio: aquece sozinho e entra em cadeia com pouco. Funde a 640 °C." },
        new() { K = "INTERRUPTOR", N = "Interruptor", Kind = 1, C = "#6a3030", Cat = 4, Cond = 0.5f, Elec = 1f, Vary = 0f, Desc = "Clica nele com a Faísca para ligar (verde) ou desligar (vermelho)." },
        new() { K = "TERMOSTATO", N = "Termóstato", Kind = 1, C = "#4a6a7a", Cat = 4, Cond = 0.9f, Cap = 0.5f, Elec = 1f, Vary = 0.04f, Desc = "Deixa passar corrente só abaixo de 50 °C. Encosta-o ao que queres manter morno." },
        new() { K = "SOLAR", N = "Painel solar", Kind = 1, C = "#1e3a6a", Cat = 4, Cond = 0.3f, Act = 1, Vary = 0.04f, Desc = "Dá corrente enquanto tiver céu aberto por cima." },
        new() { K = "CAMPAINHA", N = "Campainha", Kind = 1, C = "#8a7a30", Cat = 4, Cond = 0.4f, Elec = 1f, Vary = 0.04f, Desc = "Toca quando lhe passa corrente." },
    };
}
