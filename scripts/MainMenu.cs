using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		// Знаходимо кнопки і підписуємося на подію Pressed
		GetNode<Button>("Margin/MenuButtons/PlayButton").Pressed += OnPlayPressed;
		GetNode<Button>("Margin/MenuButtons/EditorButton").Pressed += OnEditorPressed;
		GetNode<Button>("Margin/MenuButtons/SettingsButton").Pressed += OnSettingsPressed;
		GetNode<Button>("Margin/MenuButtons/QuitButton").Pressed += OnQuitPressed;
	}

	private void OnPlayPressed()
	{
		// Завантажуємо тестовий рівень
		GetTree().ChangeSceneToFile("res://scenes/levels/TestLevel.tscn");
	}

	private void OnEditorPressed()
	{
		// Завантажуємо сцену редактора рівнів
		GetTree().ChangeSceneToFile("res://scenes/levels/LevelEditor.tscn");
	}

	private void OnSettingsPressed()
	{
		GD.Print("Відкрито налаштування");
	}

	private void OnQuitPressed()
	{
		// Закриваємо гру
		GetTree().Quit();
	}
}
