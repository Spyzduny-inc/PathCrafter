using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] public Button PlayButton { get; set; }
	[Export] public Button EditorButton { get; set; }
	[Export] public Button SettingsButton { get; set; }
	[Export] public Button QuitButton { get; set; }

	public override void _Ready()
	{
		GC.Collect();
		Audio.Instance?.PlayMusic("menu");

		PlayButton ??= GetNodeOrNull<Button>("Margin/HBoxMain/MenuPanel/MenuButtons/PlayButton") ?? FindChild("PlayButton", true, false) as Button;
		EditorButton ??= GetNodeOrNull<Button>("Margin/HBoxMain/MenuPanel/MenuButtons/EditorButton") ?? FindChild("EditorButton", true, false) as Button;
		SettingsButton ??= GetNodeOrNull<Button>("Margin/HBoxMain/MenuPanel/MenuButtons/SettingsButton") ?? FindChild("SettingsButton", true, false) as Button;
		QuitButton ??= GetNodeOrNull<Button>("Margin/HBoxMain/MenuPanel/MenuButtons/QuitButton") ?? FindChild("QuitButton", true, false) as Button;

		if (PlayButton != null) PlayButton.Pressed += OnPlayPressed;
		if (EditorButton != null) EditorButton.Pressed += OnEditorPressed;
		if (SettingsButton != null) SettingsButton.Pressed += OnSettingsPressed;
		if (QuitButton != null) QuitButton.Pressed += OnQuitPressed;
	}

	private void OnPlayPressed()
	{
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, Paths.LevelSelectMenuScene);
	}

	private void OnEditorPressed()
	{
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, Paths.LevelEditorScene);
	}

	private void OnSettingsPressed()
	{
		Audio.Instance?.PlaySfx("click");
		var settingsScene = GD.Load<PackedScene>(Paths.SettingsMenuScene);
		if (settingsScene != null)
		{
			var settingsInstance = settingsScene.Instantiate<CanvasLayer>();
			AddChild(settingsInstance);
		}
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
