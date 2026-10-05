using Godot;

public partial class LevelCompleteMenu : CanvasLayer
{
    [Export] public Button NextLevelButton { get; set; }
    [Export] public Button MenuButton { get; set; }
    [Export] public Label TitleLabel { get; set; }

    public override void _Ready()
    {
        Visible = false; // На старті рівня меню приховане

        if (NextLevelButton != null)
        {
            NextLevelButton.FocusMode = Control.FocusModeEnum.None;
            NextLevelButton.Pressed += OnNextLevelPressed;
        }

        if (MenuButton != null)
        {
            MenuButton.FocusMode = Control.FocusModeEnum.None;
            MenuButton.Pressed += OnMenuPressed;
        }
    }

    public void ShowVictory()
    {
        Visible = true;
        GetViewport().GuiReleaseFocus();

        // Приховуємо праву панель з кодом, щоб відкрити 3D-вид бази при перемозі
        var game = GetParent() as Game ?? GetTree().CurrentScene as Game;
        if (game != null)
        {
            if (game.RightEngineerPanel != null) game.RightEngineerPanel.Visible = false;
            if (game.ToggleTerminalButton != null) game.ToggleTerminalButton.Text = "[ < ] ВІДКРИТИ КОД";
        }
    }

    private void OnNextLevelPressed()
    {
        GetViewport().GuiReleaseFocus();
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/ui/LevelSelectMenu.tscn");
    }

    private void OnMenuPressed()
    {
        GetViewport().GuiReleaseFocus();
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/ui/MainMenu.tscn");
    }
}