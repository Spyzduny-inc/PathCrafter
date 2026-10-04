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
        // Знімаємо фокус з усього іншого, щоб гравець міг клацати кнопки мишею
        GetViewport().GuiReleaseFocus();
    }

    private void OnNextLevelPressed()
    {
        // Тут підтягнемо логіку завантаження наступного рівня зі списку
        GD.Print("[LevelCompleteMenu] Клік: Наступний рівень");
        // Наприклад, перехід назад у вибір рівнів або наступний файл
        GetTree().ChangeSceneToFile("res://scenes/ui/LevelSelectMenu.tscn");
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
    }
}