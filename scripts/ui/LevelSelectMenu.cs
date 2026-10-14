using Godot;
using System;
using System.Collections.Generic;

public partial class LevelSelectMenu : Control
{
    [Export] public Texture2D Chapter1Bg { get; set; }
    [Export] public Texture2D Chapter2Bg { get; set; }
    [Export] public LevelSelectConfig Config { get; set; }

    [Export] public TextureRect ChapterBackground { get; set; }
    [Export] public Label ChapterTitleLabel { get; set; }
    [Export] public Label ChapterSubtitleLabel { get; set; }
    [Export] public Label ChapterProgressLabel { get; set; }
    [Export] public Button BackButton { get; set; }
    [Export] public Button PrevChapterButton { get; set; }
    [Export] public Button NextChapterButton { get; set; }

    [Export] public Control LevelNodesContainer { get; set; }
    [Export] public Node2D RouteLinesContainer { get; set; }

    [Export] public Button BtnLevel1 { get; set; }
    [Export] public Button BtnLevel2 { get; set; }
    [Export] public Button BtnLevel3 { get; set; }
    [Export] public Button BtnLevel4 { get; set; }
    [Export] public Button BtnLevel5 { get; set; }
    [Export] public Button BtnLevel6 { get; set; }
    [Export] public Button BtnLevel7 { get; set; }
    [Export] public Button BtnLevel8 { get; set; }

    public int CurrentChapterIndex { get; private set; } = 0;

    private readonly string[] _chapterTitles = new string[]
    {
        "ГЛАВА 01 // ПЕРШІ КРОКИ",
        "ГЛАВА 02 // СЕКТОР B"
    };

    private readonly string[] _chapterSubtitles = new string[]
    {
        "TACTICAL CIRCUIT MATRIX // СЕКТОРИ 01 — 08",
        "DEEP MARS EXPEDITION // СЕКТОРИ 09 — 16"
    };

    // Точні координати кнопок рівнів (розмір 70х70) для Глави 1
    private readonly Vector2[] _chapter1Positions = new Vector2[]
    {
        new Vector2(130, 185), // Рівень 1
        new Vector2(320, 185), // Рівень 2
        new Vector2(250, 325), // Рівень 3
        new Vector2(420, 370), // Рівень 4
        new Vector2(580, 305), // Рівень 5
        new Vector2(690, 205), // Рівень 6
        new Vector2(840, 215), // Рівень 7
        new Vector2(860, 335)  // Рівень 8
    };

    // Точні координати кнопок рівнів (розмір 70х70) для Глави 2
    private readonly Vector2[] _chapter2Positions = new Vector2[]
    {
        new Vector2(120, 430), // Рівень 1
        new Vector2(215, 320), // Рівень 2
        new Vector2(320, 380), // Рівень 3
        new Vector2(465, 465), // Рівень 4
        new Vector2(620, 440), // Рівень 5
        new Vector2(705, 330), // Рівень 6
        new Vector2(825, 435), // Рівень 7
        new Vector2(920, 290)  // Рівень 8
    };

    // Товсті білі маршрутні лінії для Глави 1 (ортогональні траєкторії)
    private readonly Vector2[][] _chapter1Routes = new Vector2[][]
    {
        // 1 -> 2
        new Vector2[] { new Vector2(200, 220), new Vector2(320, 220) },
        // 2 -> 3
        new Vector2[] { new Vector2(355, 255), new Vector2(355, 290), new Vector2(285, 290), new Vector2(285, 325) },
        // 3 -> 4
        new Vector2[] { new Vector2(320, 360), new Vector2(455, 360), new Vector2(455, 370) },
        // 4 -> 5
        new Vector2[] { new Vector2(490, 405), new Vector2(540, 405), new Vector2(540, 340), new Vector2(580, 340) },
        // 5 -> 6
        new Vector2[] { new Vector2(615, 305), new Vector2(615, 240), new Vector2(725, 240), new Vector2(725, 275) },
        // 6 -> 7
        new Vector2[] { new Vector2(760, 240), new Vector2(840, 240) },
        // 7 -> 8
        new Vector2[] { new Vector2(910, 250), new Vector2(970, 250), new Vector2(970, 370), new Vector2(930, 370) }
    };

    // Товсті білі маршрутні лінії для Глави 2
    private readonly Vector2[][] _chapter2Routes = new Vector2[][]
    {
        // 1 -> 2
        new Vector2[] { new Vector2(155, 430), new Vector2(155, 355), new Vector2(215, 355) },
        // 2 -> 3
        new Vector2[] { new Vector2(285, 355), new Vector2(355, 355), new Vector2(355, 380) },
        // 3 -> 4
        new Vector2[] { new Vector2(355, 450), new Vector2(355, 500), new Vector2(465, 500) },
        // 4 -> 5
        new Vector2[] { new Vector2(535, 500), new Vector2(580, 500), new Vector2(580, 475), new Vector2(620, 475) },
        // 5 -> 6
        new Vector2[] { new Vector2(655, 440), new Vector2(655, 365), new Vector2(740, 365), new Vector2(740, 400) },
        // 6 -> 7
        new Vector2[] { new Vector2(775, 365), new Vector2(860, 365), new Vector2(860, 435) },
        // 7 -> 8
        new Vector2[] { new Vector2(895, 470), new Vector2(955, 470), new Vector2(955, 360) }
    };

    private Button[] _levelButtons;
    private readonly List<Line2D> _routeLineNodes = new List<Line2D>();

    public override void _Ready()
    {
        GC.Collect();
        Audio.Instance?.PlayMusic("select");

        ChapterBackground ??= GetNodeOrNull<TextureRect>("ChapterBackground");
        ChapterTitleLabel ??= GetNodeOrNull<Label>("TopBar/Margin/HBox/CenterPanel/TitleVBox/ChapterTitleLabel") ?? FindChild("ChapterTitleLabel", true, false) as Label;
        ChapterSubtitleLabel ??= GetNodeOrNull<Label>("TopBar/Margin/HBox/CenterPanel/TitleVBox/ChapterSubtitleLabel") ?? FindChild("ChapterSubtitleLabel", true, false) as Label;
        ChapterProgressLabel ??= GetNodeOrNull<Label>("TopBar/Margin/HBox/ProgressPanel/ProgressLabel") ?? FindChild("ProgressLabel", true, false) as Label;
        BackButton ??= GetNodeOrNull<Button>("TopBar/Margin/HBox/BackButton") ?? FindChild("BackButton", true, false) as Button;
        PrevChapterButton ??= GetNodeOrNull<Button>("TopBar/Margin/HBox/CenterPanel/TitleVBox/NavHBox/PrevChapterButton") ?? FindChild("PrevChapterButton", true, false) as Button;
        NextChapterButton ??= GetNodeOrNull<Button>("TopBar/Margin/HBox/CenterPanel/TitleVBox/NavHBox/NextChapterButton") ?? FindChild("NextChapterButton", true, false) as Button;
        LevelNodesContainer ??= GetNodeOrNull<Control>("LevelNodes");
        RouteLinesContainer ??= GetNodeOrNull<Node2D>("RouteLines");

        BtnLevel1 ??= FindLevelButton("Btn_Level_1");
        BtnLevel2 ??= FindLevelButton("Btn_Level_2");
        BtnLevel3 ??= FindLevelButton("Btn_Level_3");
        BtnLevel4 ??= FindLevelButton("Btn_Level_4");
        BtnLevel5 ??= FindLevelButton("Btn_Level_5");
        BtnLevel6 ??= FindLevelButton("Btn_Level_6");
        BtnLevel7 ??= FindLevelButton("Btn_Level_7");
        BtnLevel8 ??= FindLevelButton("Btn_Level_8");

        _levelButtons = new Button[]
        {
            BtnLevel1, BtnLevel2, BtnLevel3, BtnLevel4,
            BtnLevel5, BtnLevel6, BtnLevel7, BtnLevel8
        };

        Chapter1Bg ??= GD.Load<Texture2D>("res://assets/ui/Chapter1_Bg.jpg");
        Chapter2Bg ??= GD.Load<Texture2D>("res://assets/ui/Chapter2_Bg.jpg");

        if (BackButton != null)
        {
            BackButton.FocusMode = FocusModeEnum.None;
            BackButton.Pressed += OnBackButtonPressed;
        }

        if (PrevChapterButton != null)
        {
            PrevChapterButton.FocusMode = FocusModeEnum.None;
            PrevChapterButton.Pressed += PrevChapter;
        }

        if (NextChapterButton != null)
        {
            NextChapterButton.FocusMode = FocusModeEnum.None;
            NextChapterButton.Pressed += NextChapter;
        }

        for (int i = 0; i < _levelButtons.Length; i++)
        {
            int index = i;
            var btn = _levelButtons[i];
            if (btn != null)
            {
                btn.FocusMode = FocusModeEnum.None;
                btn.Pressed += () => OnLevelButtonPressed(index);
            }
        }

        SetupRouteLines();
        SetChapter(0);
    }

    private Button FindLevelButton(string name)
    {
        if (LevelNodesContainer != null)
        {
            var btn = LevelNodesContainer.GetNodeOrNull<Button>(name);
            if (btn != null) return btn;
        }
        return FindChild(name, true, false) as Button;
    }

    private void SetupRouteLines()
    {
        if (RouteLinesContainer == null) return;

        // Очищаємо попередні вузли, якщо є
        foreach (Node child in RouteLinesContainer.GetChildren())
        {
            child.QueueFree();
        }
        _routeLineNodes.Clear();

        // Створюємо 7 товстих ліній маршруту (1->2, 2->3, ..., 7->8)
        for (int i = 0; i < 7; i++)
        {
            var line = new Line2D();
            line.Name = $"RouteLine_{i + 1}_{i + 2}";
            line.Width = 5.0f;
            line.JointMode = Line2D.LineJointMode.Round;
            line.BeginCapMode = Line2D.LineCapMode.Round;
            line.EndCapMode = Line2D.LineCapMode.Round;
            line.Antialiased = true;
            line.DefaultColor = new Color(1.0f, 1.0f, 1.0f, 0.95f);

            RouteLinesContainer.AddChild(line);
            _routeLineNodes.Add(line);
        }
    }

    public void NextChapter()
    {
        Audio.Instance?.PlaySfx("click");
        int nextIndex = (CurrentChapterIndex + 1) % _chapterTitles.Length;
        SetChapter(nextIndex);
    }

    public void PrevChapter()
    {
        Audio.Instance?.PlaySfx("click");
        int prevIndex = (CurrentChapterIndex - 1 + _chapterTitles.Length) % _chapterTitles.Length;
        SetChapter(prevIndex);
    }

    public void SetChapter(int chapterIndex)
    {
        CurrentChapterIndex = Mathf.Clamp(chapterIndex, 0, _chapterTitles.Length - 1);

        // 1. Оновлюємо фон
        if (ChapterBackground != null)
        {
            ChapterBackground.Texture = (CurrentChapterIndex == 0) ? Chapter1Bg : Chapter2Bg;
        }

        // 2. Оновлюємо заголовок та підзаголовок глави
        if (ChapterTitleLabel != null)
        {
            ChapterTitleLabel.Text = _chapterTitles[CurrentChapterIndex];
        }

        if (ChapterSubtitleLabel != null)
        {
            ChapterSubtitleLabel.Text = _chapterSubtitles[CurrentChapterIndex];
        }

        // 3. Оновлюємо кнопки рівнів (позиції, блокування, стилі під наш UI)
        int completedCount = 0;
        var positions = (CurrentChapterIndex == 0) ? _chapter1Positions : _chapter2Positions;

        for (int i = 0; i < _levelButtons.Length; i++)
        {
            var btn = _levelButtons[i];
            if (btn == null) continue;

            int globalLevelIndex = (CurrentChapterIndex * 8) + i;
            int displayNum = i + 1;
            btn.TooltipText = $"Місія {globalLevelIndex + 1} // Глава {CurrentChapterIndex + 1}";

            if (i < positions.Length)
            {
                btn.Position = positions[i];
            }

            bool isUnlocked = globalLevelIndex <= Global.UnlockedLevelIndex;
            bool isCompleted = globalLevelIndex < Global.UnlockedLevelIndex;
            bool isCurrent = globalLevelIndex == Global.UnlockedLevelIndex;

            if (isCompleted) completedCount++;

            // Застосовуємо стилі Mission Control залежно від статусу
            ApplyMissionControlButtonStyle(btn, displayNum, isUnlocked, isCompleted, isCurrent);
        }

        // 4. Оновлюємо статус прогресу глави
        if (ChapterProgressLabel != null)
        {
            ChapterProgressLabel.Text = $"ПРОГРЕС: {completedCount} / 8";
        }

        // 5. Оновлюємо товсті білі лінії маршруту
        UpdateRouteLines();

        GD.Print($"[LevelSelectMenu] Активовано {CurrentChapterIndex + 1}: {_chapterTitles[CurrentChapterIndex]} (Пройдено: {completedCount}/8)");
    }

    private void UpdateRouteLines()
    {
        var routes = (CurrentChapterIndex == 0) ? _chapter1Routes : _chapter2Routes;

        for (int i = 0; i < _routeLineNodes.Count; i++)
        {
            if (i >= routes.Length) break;

            var line = _routeLineNodes[i];
            var pts = routes[i];

            line.ClearPoints();
            foreach (var pt in pts)
            {
                line.AddPoint(pt);
            }

            // Рівень призначення цієї лінії
            int targetGlobalIndex = (CurrentChapterIndex * 8) + (i + 1);
            bool isTargetUnlocked = targetGlobalIndex <= Global.UnlockedLevelIndex;

            if (isTargetUnlocked)
            {
                // Яскрава неонова біла лінія маршруту
                line.Width = 5.0f;
                line.DefaultColor = new Color(1.0f, 1.0f, 1.0f, 0.98f);
            }
            else
            {
                // Заблокована слабка лінія схеми
                line.Width = 3.0f;
                line.DefaultColor = new Color(1.0f, 1.0f, 1.0f, 0.22f);
            }
        }
    }

    private void ApplyMissionControlButtonStyle(Button btn, int displayNum, bool isUnlocked, bool isCompleted, bool isCurrent)
    {
        btn.Disabled = !isUnlocked;

        var normalBox = new StyleBoxFlat();
        normalBox.CornerRadiusTopLeft = 4;
        normalBox.CornerRadiusTopRight = 4;
        normalBox.CornerRadiusBottomRight = 4;
        normalBox.CornerRadiusBottomLeft = 4;

        var hoverBox = new StyleBoxFlat();
        hoverBox.CornerRadiusTopLeft = 4;
        hoverBox.CornerRadiusTopRight = 4;
        hoverBox.CornerRadiusBottomRight = 4;
        hoverBox.CornerRadiusBottomLeft = 4;

        if (isCompleted)
        {
            // Пройдено: темно-графітовий із зеленим акцентом
            normalBox.BgColor = new Color(0.07f, 0.12f, 0.11f, 0.95f);
            normalBox.BorderWidthLeft = 2;
            normalBox.BorderWidthTop = 2;
            normalBox.BorderWidthRight = 2;
            normalBox.BorderWidthBottom = 2;
            normalBox.BorderColor = new Color(0.063f, 0.725f, 0.506f, 0.9f); // #10b981 Emerald
            normalBox.ShadowColor = new Color(0.063f, 0.725f, 0.506f, 0.25f);
            normalBox.ShadowSize = 3;

            hoverBox.BgColor = new Color(0.1f, 0.18f, 0.15f, 1.0f);
            hoverBox.BorderWidthLeft = 2;
            hoverBox.BorderWidthTop = 2;
            hoverBox.BorderWidthRight = 2;
            hoverBox.BorderWidthBottom = 2;
            hoverBox.BorderColor = new Color(0.2f, 0.9f, 0.6f, 1.0f);
            hoverBox.ShadowColor = new Color(0.063f, 0.725f, 0.506f, 0.45f);
            hoverBox.ShadowSize = 6;

            btn.Text = $"✔ {displayNum}";
            btn.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 1f));
            btn.AddThemeColorOverride("font_hover_color", new Color(0.4f, 1f, 0.7f, 1f));
        }
        else if (isCurrent)
        {
            // Поточний (розблоковано, наступний до проходження): фірмовий Martian Orange
            normalBox.BgColor = new Color(0.09f, 0.11f, 0.16f, 0.96f);
            normalBox.BorderWidthLeft = 2;
            normalBox.BorderWidthTop = 2;
            normalBox.BorderWidthRight = 2;
            normalBox.BorderWidthBottom = 2;
            normalBox.BorderColor = new Color(0.902f, 0.361f, 0f, 1f); // #e65c00 Orange
            normalBox.ShadowColor = new Color(0.902f, 0.361f, 0f, 0.4f);
            normalBox.ShadowSize = 5;

            hoverBox.BgColor = new Color(0.14f, 0.17f, 0.24f, 1.0f);
            hoverBox.BorderWidthLeft = 3;
            hoverBox.BorderWidthTop = 3;
            hoverBox.BorderWidthRight = 3;
            hoverBox.BorderWidthBottom = 3;
            hoverBox.BorderColor = new Color(1f, 0.6f, 0.2f, 1f);
            hoverBox.ShadowColor = new Color(0.902f, 0.361f, 0f, 0.6f);
            hoverBox.ShadowSize = 8;

            btn.Text = $"▶ {displayNum}";
            btn.AddThemeColorOverride("font_color", new Color(1f, 0.75f, 0.4f, 1f));
            btn.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f, 1f));
        }
        else
        {
            // ЗАБЛОКОВАНО: темно-сірий, замок, неактивний
            normalBox.BgColor = new Color(0.05f, 0.06f, 0.08f, 0.75f);
            normalBox.BorderWidthLeft = 1;
            normalBox.BorderWidthTop = 1;
            normalBox.BorderWidthRight = 1;
            normalBox.BorderWidthBottom = 1;
            normalBox.BorderColor = new Color(0.2f, 0.25f, 0.35f, 0.4f);

            hoverBox.BgColor = normalBox.BgColor;
            hoverBox.BorderColor = normalBox.BorderColor;

            btn.Text = "🔒";
            btn.AddThemeColorOverride("font_disabled_color", new Color(0.45f, 0.5f, 0.6f, 0.65f));
        }

        btn.AddThemeStyleboxOverride("normal", normalBox);
        btn.AddThemeStyleboxOverride("hover", hoverBox);
        btn.AddThemeStyleboxOverride("focus", hoverBox);
        btn.AddThemeStyleboxOverride("disabled", normalBox);
    }

    private void OnLevelButtonPressed(int levelSlotIndex)
    {
        Audio.Instance?.PlaySfx("click");

        int globalLevelIndex = (CurrentChapterIndex * 8) + levelSlotIndex;
        if (globalLevelIndex > Global.UnlockedLevelIndex)
        {
            GD.Print($"[LevelSelectMenu] Рівень {globalLevelIndex + 1} заблоковано! Спочатку пройдіть попередній рівень.");
            return;
        }

        string levelPath = GetLevelScenePath(CurrentChapterIndex, levelSlotIndex);

        if (!string.IsNullOrEmpty(levelPath) && ResourceLoader.Exists(levelPath))
        {
            GD.Print($"[LevelSelectMenu] Завантаження місії {globalLevelIndex + 1}: {levelPath}");
            Global.CurrentLevelNumber = globalLevelIndex + 1;
            LoadLevel(levelPath);
        }
        else
        {
            GD.Print($"[LevelSelectMenu] Рівень {globalLevelIndex + 1} ще не створено ({levelPath}).");
            string fallbackPath = "res://scenes/levels/customs/Level_1.tscn";
            if (ResourceLoader.Exists(fallbackPath))
            {
                LoadLevel(fallbackPath);
            }
        }
    }

    private string GetLevelScenePath(int chapterIndex, int slotIndex)
    {
        if (Config != null && Config.Chapters != null && chapterIndex < Config.Chapters.Count)
        {
            var chapter = Config.Chapters[chapterIndex];
            if (chapter != null && chapter.Levels != null && slotIndex < chapter.Levels.Count)
            {
                var lvl = chapter.Levels[slotIndex];
                if (lvl != null && !string.IsNullOrEmpty(lvl.LevelScenePath))
                {
                    return lvl.LevelScenePath;
                }
            }
        }

        int levelNumber = (chapterIndex * 8) + (slotIndex + 1);
        return $"{Paths.CustomsDirectory}Level_{levelNumber}.tscn";
    }

    private void LoadLevel(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Global.SelectedLevelPath = path;
            GetViewport().GuiReleaseFocus();
            GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, Paths.GameScene);
        }
    }

    private void OnBackButtonPressed()
    {
        Audio.Instance?.PlaySfx("click");
        GetViewport().GuiReleaseFocus();
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, Paths.MainMenuScene);
    }
}
