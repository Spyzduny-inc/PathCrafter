using Godot;
using System;

public partial class LevelSelectMenu : Control
{
    [Export] public LevelSelectConfig Config { get; set; }
    [Export] public VBoxContainer ChaptersContainer { get; set; }
    [Export] public Button BackButton { get; set; }

    private const string CustomsDir = "res://scenes/levels/customs/";

    public override void _Ready()
    {
        // ФІКС КРАШІВ: Чистимо пам'ять при вході в меню вибору
        GC.Collect();

        if (BackButton != null) BackButton.Pressed += OnBackButtonPressed;
        GenerateMenu();
    }

    private void GenerateMenu()
    {
        if (ChaptersContainer == null) return;

        if (Config != null && Config.Chapters != null)
        {
            foreach (var chapter in Config.Chapters)
            {
                CreateChapterUI(chapter.ChapterName, chapter.Levels);
            }
        }

        AutoLoadCustomLevels();
    }

    private void CreateChapterUI(string title, Godot.Collections.Array<LevelData> levels)
    {
        var chapterLabel = new Label();
        chapterLabel.Text = title;
        chapterLabel.AddThemeFontSizeOverride("font_size", 28);
        chapterLabel.HorizontalAlignment = HorizontalAlignment.Center;
        ChaptersContainer.AddChild(chapterLabel);

        var grid = new GridContainer();
        grid.Columns = 5; 
        grid.AddThemeConstantOverride("h_separation", 20);
        grid.AddThemeConstantOverride("v_separation", 20);
        grid.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        ChaptersContainer.AddChild(grid);

        foreach (var level in levels)
        {
            var btn = new Button();
            btn.Text = level.LevelName;
            btn.CustomMinimumSize = new Vector2(150, 80);
            string scenePath = level.LevelScenePath;
            btn.Pressed += () => LoadLevel(scenePath);
            grid.AddChild(btn);
        }

        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 40);
        ChaptersContainer.AddChild(spacer);
    }

    private void AutoLoadCustomLevels()
    {
        using var dir = DirAccess.Open(CustomsDir);
        if (dir == null) return;

        var chapterLabel = new Label();
        chapterLabel.Text = "Створені рівні";
        chapterLabel.AddThemeFontSizeOverride("font_size", 28);
        chapterLabel.HorizontalAlignment = HorizontalAlignment.Center;
        ChaptersContainer.AddChild(chapterLabel);

        var grid = new GridContainer();
        grid.Columns = 5; 
        grid.AddThemeConstantOverride("h_separation", 20);
        grid.AddThemeConstantOverride("v_separation", 20);
        grid.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        ChaptersContainer.AddChild(grid);

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
            {
                string scenePath = CustomsDir + fileName;
                string levelName = fileName.Replace(".tscn", "");

                var btn = new Button();
                btn.Text = levelName;
                btn.CustomMinimumSize = new Vector2(150, 80);
                btn.Pressed += () => LoadLevel(scenePath);
                grid.AddChild(btn);
            }
            fileName = dir.GetNext();
        }
        
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 40);
        ChaptersContainer.AddChild(spacer);
    }

    private void LoadLevel(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Global.SelectedLevelPath = path;
            // Знімаємо фокус, щоб інтерфейс не крашнувся при видаленні
            GetViewport().GuiReleaseFocus();
            GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/utilities/Game.tscn");
        }
    }

    private void OnBackButtonPressed()
    {
        GetViewport().GuiReleaseFocus();
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/ui/MainMenu.tscn");
    }
}