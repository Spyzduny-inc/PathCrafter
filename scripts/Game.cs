using Godot;

public partial class Game : Node3D
{
    [Export] public PackedScene PlayerScene { get; set; }
    [Export] public Button ExitButton { get; set; }
    [Export] public CodeEdit CodeInput { get; set; } // Оновлено на CodeEdit
    [Export] public Button RunButton { get; set; }
    [Export] public Button ResetButton { get; set; } // Нова кнопка для скидання
    [Export] public Label ConsoleOutput { get; set; }

    private Player _spawnedRover;
    private Node3D _finishPoint;
    private bool _isRunning = false;

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(Global.SelectedLevelPath))
        {
            GD.PrintErr("[Game] Шлях до рівня порожній!");
            return;
        }

        var levelScene = GD.Load<PackedScene>(Global.SelectedLevelPath);
        if (levelScene != null)
        {
            var levelInstance = levelScene.Instantiate<Node3D>();
            AddChild(levelInstance);
            GD.Print($"[Game] Рівень завантажено: {Global.SelectedLevelPath}");

            SpawnPlayer(levelInstance);
            
            // Шукаємо модуль фінішу за його назвою (FinishPoint)
            _finishPoint = FindFinishPoint(levelInstance);
            if (_finishPoint != null)
            {
                GD.Print($"[Game] Фініш знайдено: {_finishPoint.Name}");
            }
            else
            {
                GD.PrintErr("[Game] Увага! Блок фінішу не знайдено на рівні!");
            }
        }

        if (ExitButton != null)
        {
            ExitButton.FocusMode = Control.FocusModeEnum.None;
            ExitButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
        }

        if (RunButton != null)
        {
            RunButton.FocusMode = Control.FocusModeEnum.None;
            RunButton.Pressed += OnRunButtonPressed;
        }
        
        if (ResetButton != null) 
        {
            ResetButton.FocusMode = Control.FocusModeEnum.None;
            ResetButton.Pressed += OnResetButtonPressed;
            ResetButton.Disabled = true; // Вимикаємо до першого запуску
        }
    }

    private void SpawnPlayer(Node3D levelInstance)
    {
        if (PlayerScene == null) return;

        Node3D spawnPoint = FindNodeWithMeta(levelInstance, "is_player_spawn");
        
        if (spawnPoint != null)
        {
            var playerInstance = PlayerScene.Instantiate<Node3D>();
            playerInstance.Position = spawnPoint.GlobalPosition;
            AddChild(playerInstance);

            _spawnedRover = playerInstance as Player;
            _spawnedRover.SaveStartPosition(); // Запам'ятовуємо старт
            GD.Print("[Game] Марсохід успішно заспавнено!");
        }
    }

    private Node3D FindNodeWithMeta(Node node, string metaName)
    {
        if (node is Node3D node3d && node3d.HasMeta(metaName) && node3d.GetMeta(metaName).AsBool())
            return node3d;

        foreach (Node child in node.GetChildren())
        {
            var result = FindNodeWithMeta(child, metaName);
            if (result != null) return result;
        }
        return null;
    }

    // Рекурсивний пошук вузла фінішу за назвою
    private Node3D FindFinishPoint(Node node)
    {
        if (node is Node3D node3d && node.Name.ToString().Contains("FinishPoint"))
        {
            return node3d;
        }

        foreach (Node child in node.GetChildren())
        {
            var result = FindFinishPoint(child);
            if (result != null) return result;
        }

        return null;
    }

    private async void OnRunButtonPressed()
    {
        if (_isRunning || CodeInput == null || _spawnedRover == null) return;

        _isRunning = true;
        RunButton.Disabled = true;
        if (ResetButton != null) ResetButton.Disabled = true;
        CodeInput.Editable = false; // Блокуємо редагування коду
        
        if (ConsoleOutput != null) ConsoleOutput.Text = "Запуск програми...\n";

        string[] lines = CodeInput.Text.Split('\n');
        bool crashed = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim().ToLower();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            if (line == "move()" || line == "forward")
            {
                bool success = await _spawnedRover.MoveForward();
                if (!success) // Якщо колись додамо перевірку на зіткнення зі стінами
                {
                    if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ КРИТИЧНА ПОМИЛКА ]: Аварія! Врізався у стіну!";
                    crashed = true;
                    break;
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
                if (ConsoleOutput != null) ConsoleOutput.Text += $"\n[ СИНТАКСИЧНА ПОМИЛКА ]: Невідома команда '{line}'!";
                crashed = true;
                break;
            }

            // Пауза між командами для візуалу
            await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
        }

        // Перевірка фінішу після завершення всіх рядків коду
        if (!crashed)
        {
            // Перевіряємо дистанцію до фінішу (менше 0.5 означає, що ми стоїмо прямо на ньому)
            if (_finishPoint != null && _spawnedRover.GlobalPosition.DistanceTo(_finishPoint.GlobalPosition) < 0.5f)
            {
                if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ УСПІХ ]: Рівень пройдено! Ти бог C#!";
                // TODO: Додати логіку переходу на наступний рівень
            }
            else
            {
                if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ ПРОВАЛ ]: Код завершився, але ти не на фініші. Бах і капець!";
            }
        }

        _isRunning = false;
        // Вмикаємо кнопку скидання, щоб гравець міг спробувати ще раз
        if (ResetButton != null) ResetButton.Disabled = false; 
    }

    private void OnResetButtonPressed()
    {
        if (_spawnedRover != null)
        {
            _spawnedRover.ResetToStart();
            if (ConsoleOutput != null) ConsoleOutput.Text = "Рівень скинуто. Пиши код заново.";
            
            // Повертаємо інтерфейс у початковий стан
            if (CodeInput != null) CodeInput.Editable = true;
            if (RunButton != null) RunButton.Disabled = false;
            if (ResetButton != null) ResetButton.Disabled = true;
        }
    }
}