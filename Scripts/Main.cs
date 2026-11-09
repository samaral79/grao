using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using static Grao.Mat;

namespace Grao;

public partial class Main : Control
{
    // ---------- estado ----------
    Sim sim, view;
    Sons sons;
    Image cellImg, tempImg;
    ImageTexture cellTex, tempTex;
    byte[] cellBuf, tempBuf;
    ShaderMaterial mat;
    TextureRect world;
    Overlay overlay;
    Control center;
    Label hud, hintText, desc, tipLabel, brushLabel, benchRes, stressInfo, cpuInfo;
    PanelContainer hintPanel, tip, perfPanel;
    ColorRect tipSwatch;
    Button pauseBtn, heatBtn, benchBtn, stressBtn, soundBtn;
    OptionButton presetOpt, sizeOpt;
    HSlider brushSlider;
    SpinBox threadsSpin;
    ProgressBar benchBar;
    HFlowContainer matBox;

    int selMat = AREIA; string selTool;
    bool replace, paused, heatView, down;
    float speed = 1, acc;
    int brush = 4, curTab;
    MouseButton btn;
    Vector2I? hover, last;
    Vector2 mouseLocal;
    double lastSpawn, hudT, tipT, simAcc; int frames;

    static readonly string[] PresetKeys = { "", "reator", "circuito", "eletrico", "vulcao", "explosivos", "pessoas", "virus", "artificio", "natureza", "demo" };
    static readonly int[] Widths = { 320, 480, 640, 800 };
    static readonly (string tool, string n, string c, string desc)[] Tools =
    {
        ("erase", "Borracha", "#1a1c24", "Apaga. (O botão direito apaga sempre.)"),
        ("heat", "Aquecer", "#e0502a", "Aquece o que está debaixo do pincel."),
        ("cool", "Arrefecer", "#3a9be0", "Arrefece o que está debaixo do pincel."),
        ("spark", "Faísca", "#bfe0ff", "Dá um choque: liga circuitos, acende explosivos e coisas inflamáveis. Clica num interruptor para o ligar ou desligar."),
        ("neutron", "Neutrões", "#66ffbf", "Dispara neutrões. Num bloco de urânio começam reações em cadeia."),
        ("raio", "Raio", "#fff6a0", "Cai um raio na coluna onde clicas: 3000 °C, eletrifica e incendeia."),
    };

    // ---------- arranque ----------
    public override async void _Ready()
    {
        GetWindow().Title = "Grão";
        mat = new ShaderMaterial { Shader = GD.Load<Shader>("res://world.gdshader") };
        var pal = new Vector3[NM]; var al = new float[NM]; var va = new float[NM];
        for (int i = 0; i < NM; i++) { pal[i] = new Vector3(Pal[i * 3], Pal[i * 3 + 1], Pal[i * 3 + 2]); al[i] = Alpha[i]; va[i] = Vary[i]; }
        mat.SetShaderParameter("pal", pal);
        mat.SetShaderParameter("alpha_", al);
        mat.SetShaderParameter("vary", va);
        mat.SetShaderParameter("fire_id", FOGO);
        mat.SetShaderParameter("lamp_id", LAMPADA);
        mat.SetShaderParameter("virus_id", VIRUS);
        mat.SetShaderParameter("spark_id", FAGULHA);
        mat.SetShaderParameter("switch_id", INTERRUPTOR);
        mat.SetShaderParameter("neon_id", NEON);
        mat.SetShaderParameter("plasma_id", PLASMA);
        mat.SetShaderParameter("lit_id", DINAMITE_A);
        sons = new Sons(); AddChild(sons);
        BuildUi();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        NewSim();
        Hint(sim.Preset("reator"));
        // modo de teste: godot -- --shot=ficheiro.png [--preset=nome] [--frames=N] [--steps=N]
        foreach (var a in OS.GetCmdlineUserArgs())
        {
            if (a.StartsWith("--shot=")) shotPath = a[7..];
            else if (a.StartsWith("--preset=")) Hint(sim.Preset(a[9..]));
            else if (a.StartsWith("--frames=")) shotFrames = int.Parse(a[9..]);
            else if (a.StartsWith("--steps=")) { int n = int.Parse(a[8..]); for (int k = 0; k < n; k++) sim.Step(); }
            else if (a.StartsWith("--dumpsfx=")) sons.Dump(a[10..]);
            else if (a.StartsWith("--paint=") || a.StartsWith("--tool="))
            {
                // --paint=MATERIAL:fx:fy:r  ou  --tool=nome:fx:fy:r  (fx, fy entre 0 e 1)
                var q = a[(a.IndexOf('=') + 1)..].Split(':');
                int x = (int)(float.Parse(q[1], System.Globalization.CultureInfo.InvariantCulture) * sim.W);
                int y = (int)(float.Parse(q[2], System.Globalization.CultureInfo.InvariantCulture) * sim.H), r = int.Parse(q[3]);
                if (a.StartsWith("--paint=")) sim.Paint(x, y, r, Id[q[0]], true); else sim.Tool(x, y, r, q[0]);
            }
            else if (a.StartsWith("--switches=")) sim.SetSwitches((byte)(a[11..] == "on" ? 255 : 0));
        }
    }
    string shotPath; int shotFrames = 120, frameNo;

    static StyleBoxFlat Box(string bg, int pad = 6, string border = null)
    {
        var s = new StyleBoxFlat { BgColor = new Color(bg) };
        s.ContentMarginLeft = s.ContentMarginRight = pad + 2; s.ContentMarginTop = s.ContentMarginBottom = pad;
        if (border != null) { s.BorderColor = new Color(border); s.SetBorderWidthAll(1); s.SetCornerRadiusAll(7); }
        return s;
    }
    static T NoFocus<T>(T c) where T : Control { c.FocusMode = FocusModeEnum.None; return c; }
    Button Btn(Container parent, string text, Action onClick)
    {
        var b = NoFocus(new Button { Text = text });
        b.Pressed += () => onClick();
        parent.AddChild(b); return b;
    }
    static ImageTexture Swatch(string hex)
    {
        var img = Image.CreateEmpty(14, 14, false, Image.Format.Rgba8);
        img.Fill(new Color(hex));
        return ImageTexture.CreateFromImage(img);
    }

    void BuildUi()
    {
        var root = new VBoxContainer(); root.SetAnchorsPreset(LayoutPreset.FullRect);
        root.AddThemeConstantOverride("separation", 0);
        AddChild(root);

        // barra de cima
        var topP = new PanelContainer(); topP.AddThemeStyleboxOverride("panel", Box("#101219"));
        root.AddChild(topP);
        var top = new HBoxContainer(); top.AddThemeConstantOverride("separation", 8); topP.AddChild(top);
        var logo = new Label { Text = "Grão" }; logo.AddThemeFontSizeOverride("font_size", 20);
        logo.AddThemeColorOverride("font_color", new Color("#ffb547")); top.AddChild(logo);
        pauseBtn = Btn(top, "Pausa", TogglePause);
        var speedOpt = NoFocus(new OptionButton());
        foreach (var s in new[] { "½×", "1×", "2×", "4×" }) speedOpt.AddItem(s);
        speedOpt.Select(1); speedOpt.ItemSelected += i => speed = new[] { .5f, 1, 2, 4 }[i]; top.AddChild(speedOpt);
        heatBtn = Btn(top, "Vista de calor", ToggleHeat); heatBtn.ToggleMode = true;
        top.AddChild(new VSeparator());
        top.AddChild(new Label { Text = "Pincel" });
        brushSlider = NoFocus(new HSlider { MinValue = 0, MaxValue = 30, Value = brush, CustomMinimumSize = new Vector2(120, 0) });
        brushSlider.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        brushSlider.ValueChanged += v => SetBrush((int)v); top.AddChild(brushSlider);
        brushLabel = new Label { Text = brush.ToString(), CustomMinimumSize = new Vector2(22, 0) }; top.AddChild(brushLabel);
        var rep = NoFocus(new CheckBox { Text = "Substituir" }); rep.Toggled += on => replace = on; top.AddChild(rep);
        top.AddChild(new VSeparator());
        presetOpt = NoFocus(new OptionButton());
        foreach (var s in new[] { "Exemplos…", "Reator nuclear", "Circuito e C4", "Elétrico", "Vulcão", "Explosivos", "Pessoas", "Vírus", "Fogo de artifício", "Natureza", "Mistura" }) presetOpt.AddItem(s);
        presetOpt.ItemSelected += i => { if (i > 0) { Hint(sim.Preset(PresetKeys[i])); presetOpt.Select(0); } };
        top.AddChild(presetOpt);
        Btn(top, "Limpar", () => { sim.Clear(); Hint(""); });
        top.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        sizeOpt = NoFocus(new OptionButton());
        foreach (var s in new[] { "Pequeno", "Médio", "Grande", "Enorme" }) sizeOpt.AddItem(s);
        sizeOpt.TooltipText = "Tamanho do mundo (ajusta-se à forma da janela; volta a escolher depois de mudar o tamanho da janela)";
        sizeOpt.Select(0); sizeOpt.ItemSelected += _ => { NewSim(); Hint(""); }; top.AddChild(sizeOpt);
        soundBtn = Btn(top, "Som: ligado", ToggleSound);
        Btn(top, "Teste CPU", () => perfPanel.Visible = !perfPanel.Visible);

        // centro (mundo)
        center = new Control { SizeFlagsVertical = SizeFlags.ExpandFill, ClipContents = true };
        root.AddChild(center);
        world = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = TextureFilterEnum.Nearest, Material = mat, MouseFilter = MouseFilterEnum.Ignore,
        };
        center.AddChild(world);
        overlay = new Overlay { M = this, MouseFilter = MouseFilterEnum.Stop, MouseDefaultCursorShape = CursorShape.Cross };
        overlay.MouseExited += () => { if (!down) hover = null; tip.Visible = false; };
        center.AddChild(overlay);
        center.Resized += LayoutWorld;

        hintPanel = new PanelContainer { Position = new Vector2(10, 8), Visible = false };
        hintPanel.AddThemeStyleboxOverride("panel", Box("#1d1708ee", 7, "#5a4516"));
        var hb = new HBoxContainer(); hintPanel.AddChild(hb);
        hintText = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(520, 0) };
        hb.AddChild(hintText);
        var hx = NoFocus(new Button { Text = "✕", Flat = true }); hx.Pressed += () => Hint(""); hb.AddChild(hx);
        center.AddChild(hintPanel);

        hud = new Label(); hud.AddThemeFontSizeOverride("font_size", 12);
        hud.AddThemeColorOverride("font_color", new Color("#c9cdd8"));
        hud.SetAnchorsPreset(LayoutPreset.TopRight); hud.HorizontalAlignment = HorizontalAlignment.Right;
        hud.Position = new Vector2(-260, 8); hud.Size = new Vector2(250, 40); hud.GrowHorizontal = GrowDirection.Begin;
        center.AddChild(hud);

        tip = new PanelContainer { Visible = false, MouseFilter = MouseFilterEnum.Ignore };
        tip.AddThemeStyleboxOverride("panel", Box("#0d0f15f2", 5, "#333a4d"));
        var th = new HBoxContainer(); tip.AddChild(th);
        tipSwatch = new ColorRect { CustomMinimumSize = new Vector2(12, 12), SizeFlagsVertical = SizeFlags.ShrinkCenter }; th.AddChild(tipSwatch);
        tipLabel = new Label(); th.AddChild(tipLabel);
        center.AddChild(tip);

        BuildPerfPanel();

        // barra de baixo
        var botP = new PanelContainer(); botP.AddThemeStyleboxOverride("panel", Box("#101219", 6));
        root.AddChild(botP);
        var bot = new VBoxContainer(); botP.AddChild(bot);
        var tabs = NoFocus(new TabBar());
        foreach (var c in Cats) tabs.AddTab(c);
        tabs.AddTab("Ferramentas");
        tabs.TabChanged += t => { curTab = (int)t; BuildMats(); };
        bot.AddChild(tabs);
        matBox = new HFlowContainer(); matBox.AddThemeConstantOverride("h_separation", 5); matBox.AddThemeConstantOverride("v_separation", 5);
        bot.AddChild(matBox);
        desc = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        desc.AddThemeColorOverride("font_color", new Color("#8b91a5"));
        bot.AddChild(desc);
        BuildMats();
        ShowDesc(CurrentName(), CurrentDesc());
    }

    void BuildPerfPanel()
    {
        perfPanel = new PanelContainer { Visible = false, CustomMinimumSize = new Vector2(320, 0) };
        perfPanel.AddThemeStyleboxOverride("panel", Box("#101219", 10, "#252937"));
        perfPanel.SetAnchorsPreset(LayoutPreset.TopRight); perfPanel.Position = new Vector2(-332, 52); perfPanel.GrowHorizontal = GrowDirection.Begin;
        var v = new VBoxContainer(); perfPanel.AddChild(v);
        v.AddChild(new Label { Text = "Teste de CPU" });
        cpuInfo = new Label
        {
            Text = $"CPU: {OS.GetProcessorName()}\nThreads: {OS.GetProcessorCount()}\nGPU: {RenderingServer.GetVideoAdapterName()}",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        cpuInfo.AddThemeColorOverride("font_color", new Color("#8b91a5")); cpuInfo.AddThemeFontSizeOverride("font_size", 12);
        v.AddChild(cpuInfo);
        var bi = new Label { Text = "Benchmark: 1 thread, 640×360, 10 s com chuva contínua.", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        bi.AddThemeFontSizeOverride("font_size", 12); v.AddChild(bi);
        benchBtn = Btn(v, "Benchmark 1 thread", StartBench);
        benchBar = new ProgressBar { MaxValue = 1, ShowPercentage = false, CustomMinimumSize = new Vector2(0, 8) }; v.AddChild(benchBar);
        benchRes = new Label(); benchRes.AddThemeFontSizeOverride("font_size", 12); v.AddChild(benchRes);
        var row = new HBoxContainer(); v.AddChild(row);
        row.AddChild(new Label { Text = "Threads" });
        threadsSpin = new SpinBox { MinValue = 1, MaxValue = Math.Max(32, OS.GetProcessorCount() * 2), Value = OS.GetProcessorCount() };
        row.AddChild(threadsSpin);
        stressBtn = Btn(v, "Stress multi-core", ToggleStress);
        stressInfo = new Label(); stressInfo.AddThemeFontSizeOverride("font_size", 12); v.AddChild(stressInfo);
        center.AddChild(perfPanel);
    }

    // ---------- paleta ----------
    string CurrentName() => selTool != null ? Array.Find(Tools, t => t.tool == selTool).n : Defs[selMat].N;
    string CurrentDesc() => selTool != null ? Array.Find(Tools, t => t.tool == selTool).desc : Defs[selMat].Desc;
    void ShowDesc(string n, string d) => desc.Text = $"{n} — {d}";

    void BuildMats()
    {
        foreach (var c in matBox.GetChildren()) c.QueueFree();
        var group = new ButtonGroup { AllowUnpress = true };
        void Add(string n, string c, string d, bool on, Action pick)
        {
            var b = NoFocus(new Button { Text = n, Icon = Swatch(c), ToggleMode = true, ButtonGroup = group, ButtonPressed = on });
            b.Pressed += () => { pick(); ShowDesc(n, d); };
            b.MouseEntered += () => ShowDesc(n, d);
            b.MouseExited += () => ShowDesc(CurrentName(), CurrentDesc());
            matBox.AddChild(b);
        }
        if (curTab == Cats.Length)
            foreach (var t in Tools) { var tt = t; Add(t.n, t.c, t.desc, selTool == t.tool, () => selTool = tt.tool); }
        else
            for (int i = 0; i < Defs.Length; i++)
            {
                var d = Defs[i]; if (d.Cat != curTab || d.Hidden) continue;
                int id = i; Add(d.N, d.C, d.Desc, selTool == null && selMat == i, () => { selMat = id; selTool = null; });
            }
    }

    // ---------- mundo ----------
    void NewSim()
    {
        // a altura acompanha a forma da área do mundo, para não sobrar espaço vazio
        int w = Widths[sizeOpt.Selected];
        var area = center.Size.X > 50 && center.Size.Y > 50 ? center.Size - new Vector2(12, 12) : new Vector2(16, 9);
        int h = Math.Clamp((int)MathF.Round(w * area.Y / area.X), w / 4, w);
        sim = new Sim(w, h, (int)(GD.Randi() & 0x7fffffff));
        if (bench == null) SetActive(sim);
    }
    void SetActive(Sim s)
    {
        view = s;
        cellBuf = new byte[s.N * 4]; tempBuf = new byte[s.N * 4];
        cellImg = Image.CreateFromData(s.W, s.H, false, Image.Format.Rgba8, cellBuf);
        tempImg = Image.CreateFromData(s.W, s.H, false, Image.Format.Rf, tempBuf);
        cellTex = ImageTexture.CreateFromImage(cellImg); tempTex = ImageTexture.CreateFromImage(tempImg);
        world.Texture = cellTex;
        mat.SetShaderParameter("cells", cellTex);
        mat.SetShaderParameter("temp", tempTex);
        mat.SetShaderParameter("size", new Vector2I(s.W, s.H));
        LayoutWorld();
    }
    void LayoutWorld()
    {
        if (view == null || center == null) return;
        var sz = center.Size - new Vector2(12, 12);
        float sc = MathF.Max(.1f, MathF.Min(sz.X / view.W, sz.Y / view.H));
        var size = new Vector2(MathF.Floor(view.W * sc), MathF.Floor(view.H * sc));
        var pos = ((center.Size - size) / 2).Floor();
        world.Position = pos; world.Size = size;
        overlay.Position = pos; overlay.Size = size;
    }
    void Upload()
    {
        var s = view; var T = s.T; var Sh = s.Sh; var E = s.E; var b = cellBuf;
        for (int i = 0, k = 0; i < s.N; i++, k += 4) { b[k] = T[i]; b[k + 1] = Sh[i]; b[k + 2] = E[i]; b[k + 3] = 255; }
        Buffer.BlockCopy(s.Tm, 0, tempBuf, 0, s.N * 4);
        cellImg.SetData(s.W, s.H, false, Image.Format.Rgba8, cellBuf); cellTex.Update(cellImg);
        tempImg.SetData(s.W, s.H, false, Image.Format.Rf, tempBuf); tempTex.Update(tempImg);
    }

    // ---------- desenho por cima ----------
    static Color Hx(string h) => new(h);
    static readonly Color[] SKIN = { Hx("#f1c9a0"), Hx("#e0ac7e"), Hx("#c68b5e"), Hx("#8d5a3b"), Hx("#5c3a24") };
    static readonly Color[] HAIR = { Hx("#2a1d14"), Hx("#4a2f1b"), Hx("#7a5230"), Hx("#d9b26a"), Hx("#b0442a"), Hx("#1a1a1a"), Hx("#8c8c8c") };
    static readonly Color[] SHIRT = { Hx("#d1453b"), Hx("#3d6fd1"), Hx("#3fa34d"), Hx("#e3b23c"), Hx("#8a4fc7"), Hx("#e8e8e8"), Hx("#2b2b2b"), Hx("#e07a2f"), Hx("#2bb3b3") };
    static readonly Color[] PANTS = { Hx("#2d3a5c"), Hx("#3a3a3a"), Hx("#5a4632"), Hx("#1f2a44"), Hx("#6b6b6b") };

    public void DrawOverlay(Overlay o)
    {
        var s = view; if (s == null) return;
        float k = o.Size.X / s.W;
        void Rc(float x0, float y0, float x1, float y1, Color c) => o.DrawRect(new Rect2(x0 * k, y0 * k, (x1 - x0) * k, (y1 - y0) * k), c);
        float flick = Time.GetTicksMsec() * .02f;
        for (int p = 0; p < s.PeopleCount; p++)
        {
            int cx = (int)MathF.Round(s.pX[p]), fy = (int)MathF.Floor(s.pY[p]), d = s.pDir[p];
            var L = s.pLook;
            Color skin = SKIN[L[p * 4] % SKIN.Length], hair = HAIR[L[p * 4 + 1] % HAIR.Length];
            Color shirt = SHIRT[L[p * 4 + 2] % SHIRT.Length], pants = PANTS[L[p * 4 + 3] % PANTS.Length];
            if (s.VirusOf(p) > 0)
            {
                var vc = new Color(.69f, .24f, 1f);
                skin = skin.Lerp(vc, .45f); shirt = shirt.Lerp(vc, .3f);
            }
            if (s.pBurn[p] > 0)
            {
                float f = .5f + .5f * MathF.Sin(flick + p); var fire = new Color(1, .45f + .3f * f, .1f);
                skin = skin.Lerp(fire, .6f); hair = hair.Lerp(fire, .6f); shirt = shirt.Lerp(fire, .6f); pants = pants.Lerp(fire, .6f);
            }
            int x0 = cx - 1, y0 = fy - 6;
            Rc(x0, y0, x0 + 3, y0 + 1, hair);
            Rc(x0, y0 + 1, x0 + 3, y0 + 2, skin);
            Rc(cx + d, y0 + 1, cx + d + 1, y0 + 2, new Color(.08f, .08f, .1f));
            Rc(x0, y0 + 2, x0 + 3, y0 + 5, shirt);
            if (((int)MathF.Floor(s.pWalk[p] * 1.2f) & 1) == 0) { Rc(x0, y0 + 5, x0 + 1, y0 + 7, pants); Rc(x0 + 2, y0 + 5, x0 + 3, y0 + 7, pants); }
            else { Rc(x0, y0 + 5, x0 + 3, y0 + 6, pants); Rc(cx, y0 + 6, cx + 1, y0 + 7, pants); }
        }
        int n = s.NeutronCount; float ns = MathF.Max(2, k * .9f);
        var nc = new Color(.35f, 1f, .7f, .75f);
        for (int i = 0; i < n; i++) o.DrawRect(new Rect2(s.nX[i] * k - ns / 2, s.nY[i] * k - ns / 2, ns, ns), nc);
        if (hover is { } h && bench == null)
            o.DrawArc(new Vector2((h.X + .5f) * k, (h.Y + .5f) * k), (brush + .5f) * k, 0, Mathf.Tau, 48, new Color(1, 1, 1, .55f), 1.5f);
    }

    // ---------- rato e teclado ----------
    Vector2I ToGrid(Vector2 p) => new((int)MathF.Floor(p.X / overlay.Size.X * view.W), (int)MathF.Floor(p.Y / overlay.Size.Y * view.H));

    public void OnWorldInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp && mb.Pressed) { SetBrush(brush + 1); return; }
            if (mb.ButtonIndex == MouseButton.WheelDown && mb.Pressed) { SetBrush(brush - 1); return; }
            if (mb.ButtonIndex != MouseButton.Left && mb.ButtonIndex != MouseButton.Right) return;
            if (mb.Pressed)
            {
                down = true; btn = mb.ButtonIndex; hover = ToGrid(mb.Position); last = hover; mouseLocal = mb.Position;
                if (btn == MouseButton.Left && selTool == "raio") sim.Tool(hover.Value.X, hover.Value.Y, brush, "raio");
                if (btn == MouseButton.Left && selTool == "spark") { sim.ToggleSwitch(hover.Value.X, hover.Value.Y, brush); sons.Spark(); }
            }
            else { down = false; last = null; }
        }
        else if (e is InputEventMouseMotion mm) { hover = ToGrid(mm.Position); mouseLocal = mm.Position; UpdateTip(); }
    }
    public override void _UnhandledInput(InputEvent e)
    {
        if (e is not InputEventKey { Pressed: true, Echo: false } k) return;
        if (k.Keycode == Key.Space) TogglePause();
        else if (k.Keycode == Key.H) ToggleHeat();
        else if (k.Keycode == Key.M) ToggleSound();
        else if (k.Keycode == Key.Bracketleft) SetBrush(brush - 1);
        else if (k.Keycode == Key.Bracketright) SetBrush(brush + 1);
        else return;
        GetViewport().SetInputAsHandled();
    }
    void ApplyAt(int x, int y)
    {
        if (btn == MouseButton.Right) sim.Tool(x, y, brush, "erase");
        else if (selTool != null) sim.Tool(x, y, brush, selTool);
        else sim.Paint(x, y, brush, selMat, replace);
    }
    void DoPaint()
    {
        if (btn != MouseButton.Right && selTool == "raio") return;
        var h = hover.Value;
        if (btn != MouseButton.Right && selTool == null && selMat == PESSOA_B)
        {
            double now = Time.GetTicksMsec();
            if (now - lastSpawn > 140) { sim.SpawnPerson(h.X, h.Y); lastSpawn = now; }
            return;
        }
        var l = last ?? h;
        int n = Math.Max(1, (int)MathF.Ceiling(MathF.Sqrt((h.X - l.X) * (h.X - l.X) + (h.Y - l.Y) * (h.Y - l.Y)) / MathF.Max(1, brush * .5f)));
        for (int k = 1; k <= n; k++) ApplyAt((int)MathF.Round(l.X + (h.X - l.X) * k / (float)n), (int)MathF.Round(l.Y + (h.Y - l.Y) * k / (float)n));
        last = h;
    }
    void SetBrush(int v)
    {
        brush = Math.Clamp(v, 0, 30);
        if ((int)brushSlider.Value != brush) brushSlider.SetValueNoSignal(brush);
        brushLabel.Text = brush.ToString();
    }
    void TogglePause() { paused = !paused; pauseBtn.Text = paused ? "Continuar" : "Pausa"; }
    void ToggleHeat() { heatView = !heatView; heatBtn.SetPressedNoSignal(heatView); mat.SetShaderParameter("view_mode", heatView ? 1 : 0); }
    void ToggleSound() { sons.ToggleMute(); soundBtn.Text = sons.Muted ? "Som: desligado" : "Som: ligado"; }
    void Hint(string t) { hintText.Text = t; hintPanel.Visible = t != ""; }

    void UpdateTip()
    {
        if (hover is not { } h || bench != null || h.X < 0 || h.Y < 0 || h.X >= sim.W || h.Y >= sim.H) { tip.Visible = false; return; }
        int i = h.Y * sim.W + h.X, t = sim.T[i], pp = sim.PersonAt(h.X, h.Y);
        string name = "Ar", extra = "20 °C"; string color = "#0b0d12";
        if (pp >= 0)
        {
            name = "Pessoa"; color = "#3d6fd1";
            extra = $"vida {Math.Max(0, (int)MathF.Round(sim.pHP[pp] / 2))}% · {sim.pTemp[pp]:0} °C";
            if (sim.pAir[pp] < 199) extra += $" · fôlego {(int)MathF.Round(sim.pAir[pp] / 2)}%";
            if (sim.pBurn[pp] > 0) extra += " · a arder!";
            if (sim.VirusOf(pp) > 0) extra += " · infetada!";
        }
        else if (t != 0)
        {
            var d = Defs[t]; name = d.N; color = d.C; extra = $"{sim.Tm[i]:0} °C";
            int e = sim.E[i];
            if ((e & 3) != 0 || (t == LAMPADA && (e >> 2) != 0)) extra += " · ⚡ com corrente";
            if (t == FOGO && sim.Sh[i] != 0 && sim.Sh[i] < Defs.Length) extra += $" · a queimar {Defs[sim.Sh[i]].N.ToLower()}";
            if ((t == VIRUS || t == CLONADOR) && sim.Sh[i] != 0 && sim.Sh[i] < Defs.Length)
                extra += t == VIRUS ? $" · comeu {Defs[sim.Sh[i]].N.ToLower()}" : $" · a copiar {Defs[sim.Sh[i]].N.ToLower()}";
            if (t == URANIO || t == PLUTONIO) extra += $" · combustível {(int)MathF.Round(sim.L[i] / 2.5f)}%";
            if (t == INTERRUPTOR) extra += sim.Sh[i] >= 128 ? " · ligado" : " · desligado";
            if (t == TERMOSTATO) extra += sim.Tm[i] < 50 ? " · deixa passar" : " · cortado (quente)";
            if (t == SOLAR) extra += sim.L[i] != 0 ? " · ao sol" : " · à sombra";
            if (t == PEIXE && sim.L[i] > 0) extra += $" · fôlego {Math.Max(0, 100 - sim.L[i] * 100 / 220)}%";
            if (t == DINAMITE_A) extra += " · a arder o rastilho!";
        }
        tipSwatch.Color = new Color(color);
        tipLabel.Text = $"{name}   {extra}";
        tip.Visible = true;
        tip.ResetSize();
        var pos = overlay.Position + mouseLocal + new Vector2(16, 16);
        if (pos.X + tip.Size.X > center.Size.X - 4) pos.X -= tip.Size.X + 28;
        if (pos.Y + tip.Size.Y > center.Size.Y - 4) pos.Y -= tip.Size.Y + 28;
        tip.Position = pos;
    }

    // ---------- ciclo ----------
    public override void _Process(double delta)
    {
        if (sim == null) return;
        frames++;
        if (bench == null)
        {
            if (down && hover != null) DoPaint();
            if (!paused)
            {
                ulong t0 = Time.GetTicksUsec();
                acc += speed * (float)delta * 60f; int n = 0;
                while (acc >= 1 && n < 6) { sim.Step(); acc--; n++; }
                if (acc > 1) acc = 1;
                simAcc += (Time.GetTicksUsec() - t0) / 1000.0;
                sons.SetPaused(false);
                sons.Tick(sim, n);
            }
            else { sons.SetPaused(true); sons.Tick(sim, 0); }
        }
        else PollBench();
        Upload();
        mat.SetShaderParameter("time_s", Time.GetTicksMsec() / 1000f);
        overlay.QueueRedraw();
        if (shotPath != null && ++frameNo == shotFrames)
        {
            var hp = sim.PeopleCount > 0 ? sim.pHP[0] : -1;
            GD.Print($"TESTE corrente={sim.Powered} fps={Engine.GetFramesPerSecond()} pessoas={sim.PeopleCount} neutroes={sim.NeutronCount} ocupadas={sim.Count()}");
            RenderingServer.FramePostDraw += () => { GetViewport().GetTexture().GetImage().SavePng(shotPath); GetTree().Quit(); };
        }
        double now = Time.GetTicksMsec();
        if (now - tipT > 100) { UpdateTip(); tipT = now; }
        if (now - hudT > 500)
        {
            hud.Text = $"FPS {Engine.GetFramesPerSecond():0} · física {simAcc / Math.Max(1, frames):0.0} ms\n" +
                       $"ativo {view.AwakeFraction() * 100:0}% · neutrões {view.NeutronCount} · pessoas {view.PeopleCount}" +
                       (bench != null ? "\nBENCHMARK…" : paused ? "\nPAUSA" : "");
            UpdateStress(now - hudT);
            hudT = now; frames = 0; simAcc = 0;
        }
    }

    // ---------- benchmark (thread própria) ----------
    sealed class Bench { public Sim S; public volatile bool Done; public long Steps; public double SimMs; public double Start; }
    Bench bench;
    readonly List<string> history = new();
    static readonly int[] BenchRain = { AREIA, AGUA, OLEO, AREIA };

    void StartBench()
    {
        if (bench != null) return;
        var b = new Bench { S = new Sim(640, 360, 777), Start = Time.GetTicksMsec() };
        b.S.FillBench();
        bench = b; benchBtn.Disabled = true; benchBtn.Text = "A correr…";
        SetActive(b.S); tip.Visible = false;
        new Thread(() =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 10000) { b.S.Rain(40, BenchRain); b.S.Drain(); b.S.Step(); b.Steps++; }
            b.SimMs = sw.Elapsed.TotalMilliseconds; b.Done = true;
        }) { IsBackground = true, Name = "benchmark" }.Start();
    }
    void PollBench()
    {
        var b = bench;
        benchBar.Value = Math.Min(1, (Time.GetTicksMsec() - b.Start) / 10000.0);
        if (!b.Done) return;
        double mc = b.Steps * 640.0 * 360 / (b.SimMs / 1000) / 1e6, sps = b.Steps / (b.SimMs / 1000);
        history.Insert(0, $"{DateTime.Now:HH:mm:ss}  {mc:0.0} Mcél/s  {sps:0} passos/s");
        benchRes.Text = string.Join("\n", history.GetRange(0, Math.Min(5, history.Count)));
        bench = null; benchBtn.Disabled = false; benchBtn.Text = "Benchmark 1 thread"; benchBar.Value = 1;
        SetActive(sim);
    }

    // ---------- stress multi-core ----------
    volatile bool stressRun;
    long[] stressCells = Array.Empty<long>(), stressPrev = Array.Empty<long>();
    readonly List<Thread> stressThreads = new();

    void ToggleStress()
    {
        if (stressRun)
        {
            stressRun = false;
            foreach (var t in stressThreads) t.Join(500);
            stressThreads.Clear(); stressBtn.Text = "Stress multi-core"; stressInfo.Text = "";
            return;
        }
        int n = (int)threadsSpin.Value;
        stressCells = new long[n]; stressPrev = new long[n]; stressRun = true;
        for (int k = 0; k < n; k++)
        {
            int id = k;
            var th = new Thread(() =>
            {
                var s = new Sim(640, 360, 1000 + id); s.FillBench();
                long cells = (long)s.W * s.H;
                while (stressRun) { s.Rain(40, BenchRain); s.Drain(); s.Step(); Interlocked.Add(ref stressCells[id], cells); }
            }) { IsBackground = true, Name = "stress" + k };
            stressThreads.Add(th); th.Start();
        }
        stressBtn.Text = "Parar stress";
    }
    void UpdateStress(double ms)
    {
        if (!stressRun || ms <= 0) return;
        double tot = 0; var sb = new System.Text.StringBuilder();
        for (int k = 0; k < stressCells.Length; k++)
        {
            long c = Interlocked.Read(ref stressCells[k]); double r = (c - stressPrev[k]) / ms / 1000; stressPrev[k] = c; tot += r;
            sb.Append($"#{k}: {r:0.0}  ");
            if (k % 4 == 3) sb.Append('\n');
        }
        stressInfo.Text = $"Total: {tot:0.0} Mcél/s em {stressCells.Length} threads\n{sb}";
    }

    public override void _ExitTree() { stressRun = false; }
}
