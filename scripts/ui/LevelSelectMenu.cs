using Godot;
using System;
using System.Collections.Generic;

public partial class LevelSelectMenu : Control
{
	[Export] public LevelSelectConfig Config { get; set; }
	[Export] public VBoxContainer ChaptersContainer { get; set; }
	[Export] public Button BackButton { get; set; }

	private const string CustomsDir = Paths.CustomsDirectory;

	public override void _Ready()
	{
		GC.Collect();
		Audio.Instance.PlayMusic("select");

		if (BackButton != null) BackButton.Pressed += OnBackButtonPressed;
		GenerateMenu();
	}

	private void GenerateMenu()
	{
		if (ChaptersContainer == null) return;

		var registeredLevelPaths = new HashSet<string>();

		if (Config != null && Config.Chapters != null)
		{
			foreach (var chapter in Config.Chapters)
			{
				if (chapter.Levels != null)
				{
					foreach (var level in chapter.Levels)
					{
						if (!string.IsNullOrEmpty(level.LevelScenePath))
						{
							registeredLevelPaths.Add(level.LevelScenePath);
						}
					}
				}
				CreateChapterUI(chapter.ChapterName, chapter.Levels);
			}
		}

		AutoLoadCustomLevels(registeredLevelPaths);
	}

	private StyleBoxFlat CreateNormalStyle()
	{
		var box = new StyleBoxFlat();
		box.BgColor = new Color(0.118f, 0.118f, 0.18f, 0.85f);
		box.CornerRadiusTopLeft = 4;
		box.CornerRadiusTopRight = 4;
		box.CornerRadiusBottomRight = 4;
		box.CornerRadiusBottomLeft = 4;
		box.ContentMarginLeft = 12;
		box.ContentMarginTop = 8;
		box.ContentMarginRight = 12;
		box.ContentMarginBottom = 8;
		return box;
	}

	private StyleBoxFlat CreateHoverStyle()
	{
		var box = new StyleBoxFlat();
		box.BgColor = new Color(0.165f, 0.165f, 0.24f, 0.95f);
		box.BorderWidthLeft = 4;
		box.BorderColor = new Color(0.902f, 0.361f, 0f, 1f);
		box.CornerRadiusTopLeft = 2;
		box.CornerRadiusTopRight = 4;
		box.CornerRadiusBottomRight = 4;
		box.CornerRadiusBottomLeft = 2;
		box.ContentMarginLeft = 12;
		box.ContentMarginTop = 8;
		box.ContentMarginRight = 12;
		box.ContentMarginBottom = 8;
		return box;
	}

	private void CreateChapterUI(string title, Godot.Collections.Array<LevelData> levels)
	{
		var chapterLabel = new Label();
		chapterLabel.Text = title.ToUpper();
		chapterLabel.AddThemeFontSizeOverride("font_size", 18);
		chapterLabel.AddThemeColorOverride("font_color", new Color(0.902f, 0.361f, 0f, 1f));
		chapterLabel.HorizontalAlignment = HorizontalAlignment.Left;
		ChaptersContainer.AddChild(chapterLabel);

		var grid = new GridContainer();
		grid.Columns = 3; 
		grid.AddThemeConstantOverride("h_separation", 12);
		grid.AddThemeConstantOverride("v_separation", 12);
		grid.SizeFlagsHorizontal = SizeFlags.Fill;
		ChaptersContainer.AddChild(grid);

		foreach (var level in levels)
		{
			var btn = new Button();
			btn.Text = level.LevelName;
			btn.CustomMinimumSize = new Vector2(130, 60);
			btn.AddThemeStyleboxOverride("normal", CreateNormalStyle());
			btn.AddThemeStyleboxOverride("hover", CreateHoverStyle());
			btn.AddThemeStyleboxOverride("focus", CreateHoverStyle());
			btn.AddThemeColorOverride("font_color", new Color(0.796f, 0.835f, 0.882f, 1f));
			btn.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f, 1f));

			string scenePath = level.LevelScenePath;
			btn.Pressed += () => LoadLevel(scenePath);
			grid.AddChild(btn);
		}

		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(0, 20);
		ChaptersContainer.AddChild(spacer);
	}

	private void AutoLoadCustomLevels(HashSet<string> registeredLevelPaths)
	{
		using var dir = DirAccess.Open(CustomsDir);
		if (dir == null) return;

		var validFiles = new List<(string Name, string Path)>();

		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (fileName != "")
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
			{
				string scenePath = CustomsDir + fileName;
				
				if (!registeredLevelPaths.Contains(scenePath))
				{
					string levelName = fileName.Replace(".tscn", "");
					validFiles.Add((levelName, scenePath));
				}
			}
			fileName = dir.GetNext();
		}

		if (validFiles.Count == 0) return;

		var chapterLabel = new Label();
		chapterLabel.Text = "КОРИСТУВАЦЬКІ РІВНІ";
		chapterLabel.AddThemeFontSizeOverride("font_size", 18);
		chapterLabel.AddThemeColorOverride("font_color", new Color(0.902f, 0.361f, 0f, 1f));
		chapterLabel.HorizontalAlignment = HorizontalAlignment.Left;
		ChaptersContainer.AddChild(chapterLabel);

		var grid = new GridContainer();
		grid.Columns = 3; 
		grid.AddThemeConstantOverride("h_separation", 12);
		grid.AddThemeConstantOverride("v_separation", 12);
		grid.SizeFlagsHorizontal = SizeFlags.Fill;
		ChaptersContainer.AddChild(grid);

		foreach (var file in validFiles)
		{
			var btn = new Button();
			btn.Text = file.Name;
			btn.CustomMinimumSize = new Vector2(130, 60);
			btn.AddThemeStyleboxOverride("normal", CreateNormalStyle());
			btn.AddThemeStyleboxOverride("hover", CreateHoverStyle());
			btn.AddThemeStyleboxOverride("focus", CreateHoverStyle());
			btn.AddThemeColorOverride("font_color", new Color(0.796f, 0.835f, 0.882f, 1f));
			btn.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f, 1f));

			string scenePath = file.Path;
			btn.Pressed += () => LoadLevel(scenePath);
			grid.AddChild(btn);
		}
		
		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(0, 20);
		ChaptersContainer.AddChild(spacer);
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
		GetViewport().GuiReleaseFocus();
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, Paths.MainMenuScene);
	}
}
