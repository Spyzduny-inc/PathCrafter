using Godot;

public partial class Game : Node3D
{
    [Export] public PackedScene PlayerScene { get; set; }
    [Export] public Button ExitButton { get; set; }
    
    [Export] public TextEdit CodeInput { get; set; }
    [Export] public Button RunButton { get; set; }
    [Export] public Label ConsoleOutput { get; set; }

    private Player _spawnedRover;

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(Global.SelectedLevelPath))
        {
            GD.PrintErr("[Game] Шлях до рівня порожній!");
            return;
        }

        // 1. Завантажуємо рівень
        var levelScene = GD.Load<PackedScene>(Global.SelectedLevelPath);
        if (levelScene != null)
        {
            var levelInstance = levelScene.Instantiate<Node3D>();
            AddChild(levelInstance);
            GD.Print($"[Game] Рівень завантажено: {Global.SelectedLevelPath}");

            SpawnPlayer(levelInstance);
        }

        // 2. Кнопка виходу в меню
        if (ExitButton != null)
        {
            ExitButton.FocusMode = Control.FocusModeEnum.None;
            ExitButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
        }

        // 3. Кнопка запуску інтерпретатора
        if (RunButton != null)
        {
            RunButton.FocusMode = Control.FocusModeEnum.None;
            RunButton.Pressed += OnRunButtonPressed;
        }
    }

    private void SpawnPlayer(Node3D levelInstance)
    {
        if (PlayerScene == null) 
        {
            GD.PrintErr("[Game] Сцена марсохода не призначена в Інспекторі!");
            return;
        }

        Node3D spawnPoint = FindSpawnPoint(levelInstance);
        
        if (spawnPoint != null)
        {
            var playerInstance = PlayerScene.Instantiate<Node3D>();
            playerInstance.Position = spawnPoint.GlobalPosition;
            AddChild(playerInstance);

            _spawnedRover = playerInstance as Player;
            GD.Print("[Game] Марсохід успішно заспавнено!");
        }
        else
        {
            GD.PrintErr("[Game] Точку спавну не знайдено на рівні!");
        }
    }

    private Node3D FindSpawnPoint(Node node)
    {
        if (node is Node3D node3d && node3d.HasMeta("is_player_spawn") && node3d.GetMeta("is_player_spawn").AsBool())
        {
            return node3d;
        }

        foreach (Node child in node.GetChildren())
        {
            var result = FindSpawnPoint(child);
            if (result != null) return result;
        }

        return null;
    }

    // ІНТЕРПРЕТАТОР КОДУ З AWAIT ДЛЯ ПЛАВНОГО РУХУ
    private async void OnRunButtonPressed()
    {
        if (CodeInput == null || _spawnedRover == null)
        {
            if (ConsoleOutput != null) ConsoleOutput.Text = "[ ПОМИЛКА ]: Не знайдено поле коду або марсохід!";
            return;
        }

        string[] lines = CodeInput.Text.Split('\n');
        if (ConsoleOutput != null) ConsoleOutput.Text = "Запуск програми на Марсі...\n";

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim().ToLower();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            if (line == "move()" || line == "forward")
            {
                bool success = await _spawnedRover.MoveForward();
                if (!success)
                {
                    if (ConsoleOutput != null) 
                        ConsoleOutput.Text += "\n[ ПОМИЛКА ]: Марсохід врізався у перешкоду!\nТи не прогер, а дебіл, кодіруй заново!";
                    return;
                }
            }
            else if (line == "turn_right()" || line == "right")
            {
                await _spawnedRover.TurnRight();
            }
            else if (line == "turn_left()" || line == "left")
            {
                await _spawnedRover.TurnLeft();
            }
            else
            {
                if (ConsoleOutput != null) 
                    ConsoleOutput.Text += $"\n[ ПОМИЛКА ]: Невідома команда '{line}'!";
                return;
            }

            // Коротка пауза між виконанням рядків коду
            await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
        }

        if (ConsoleOutput != null) 
            ConsoleOutput.Text += "\n[ УСПІХ ]: Красавчик! Дійшов до цілі, пиздуй на наступний рівень!";
    }
}