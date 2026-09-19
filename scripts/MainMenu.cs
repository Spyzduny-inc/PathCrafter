using Godot;
using System; // Додано для доступу до GC (Garbage Collector)

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        // ФІКС КРАШІВ: Примусово вичищаємо з пам'яті зомбі-об'єкти від попередніх сцен
        GC.Collect();

        GetNode<Button>("Margin/MenuButtons/PlayButton").Pressed += OnPlayPressed;
        GetNode<Button>("Margin/MenuButtons/EditorButton").Pressed += OnEditorPressed;
        GetNode<Button>("Margin/MenuButtons/SettingsButton").Pressed += OnSettingsPressed;
        GetNode<Button>("Margin/MenuButtons/QuitButton").Pressed += OnQuitPressed;
    }

    private void OnPlayPressed()
    {
        // Вбудований безпечний перехід Godot
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/ui/LevelSelectMenu.tscn");
    }

    private void OnEditorPressed()
    {
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/levels/LevelEditor.tscn");
    }

    private void OnSettingsPressed()
    {
        GD.Print("Відкрито налаштування");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}