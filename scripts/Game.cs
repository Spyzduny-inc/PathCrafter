using Godot;

public partial class Game : Node3D
{
    [Export] public PackedScene PlayerScene { get; set; }
    [Export] public Button ExitButton { get; set; }
    [Export] public CodeEdit CodeInput { get; set; } 
    [Export] public Button RunButton { get; set; }
    [Export] public Button ResetButton { get; set; } 
    [Export] public Label ConsoleOutput { get; set; }
    
    [Export] public Button HelpButton { get; set; }
    [Export] public Control HelpPanel { get; set; }

    private Player _spawnedRover;
    private Node3D _finishPoint;
    private bool _isRunning = false;

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(Global.SelectedLevelPath)) return;

        var levelScene = GD.Load<PackedScene>(Global.SelectedLevelPath);
        if (levelScene != null)
        {
            var levelInstance = levelScene.Instantiate<Node3D>();
            AddChild(levelInstance);

            SpawnPlayer(levelInstance);
            _finishPoint = FindFinishPoint(levelInstance);
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
            ResetButton.Disabled = true; 
        }

        if (HelpButton != null && HelpPanel != null)
        {
            HelpButton.FocusMode = Control.FocusModeEnum.None;
            HelpButton.Pressed += () => HelpPanel.Visible = !HelpPanel.Visible;
        }

        if (CodeInput != null)
        {
            CodeInput.AutoBraceCompletionEnabled = true;
            CodeInput.AutoBraceCompletionHighlightMatching = true;

            var highlighter = new CodeHighlighter();
            
            Color functionColor = new Color(0.3f, 0.7f, 1.0f); // Блакитний
            Color commentColor = new Color(0.4f, 0.6f, 0.4f);  // Зелений

            // Фарбуємо дужки () в той самий синій колір, що і команди
            highlighter.SymbolColor = functionColor;

            highlighter.AddKeywordColor("move", functionColor);
            highlighter.AddKeywordColor("forward", functionColor);
            highlighter.AddKeywordColor("turn_right", functionColor);
            highlighter.AddKeywordColor("right", functionColor);
            highlighter.AddKeywordColor("turn_left", functionColor);
            highlighter.AddKeywordColor("left", functionColor);

            highlighter.AddColorRegion("#", "", commentColor, true);

            CodeInput.SyntaxHighlighter = highlighter;
        }
    }

    private void SpawnPlayer(Node3D levelInstance)
    {
        if (PlayerScene == null) return;
        
        Node3D spawnPoint = FindNodeWithMeta(levelInstance, "is_player_spawn");
        
        if (spawnPoint == null)
        {
            spawnPoint = FindSpawnPoint(levelInstance);
        }
        
        if (spawnPoint != null)
        {
            var playerInstance = PlayerScene.Instantiate<Node3D>();
            playerInstance.Position = spawnPoint.GlobalPosition;
            AddChild(playerInstance);

            _spawnedRover = playerInstance as Player;
            _spawnedRover.LevelRoot = levelInstance; 
            _spawnedRover.SaveStartPosition(); 
        }
    }

    private Node3D FindNodeWithMeta(Node node, string metaName)
    {
        if (node is Node3D node3d && node3d.HasMeta(metaName) && node3d.GetMeta(metaName).AsBool()) return node3d;
        foreach (Node child in node.GetChildren())
        {
            var result = FindNodeWithMeta(child, metaName);
            if (result != null) return result;
        }
        return null;
    }

    private Node3D FindSpawnPoint(Node node)
    {
        string identity = (node.SceneFilePath + " " + node.Name).ToLower();
        if (node is Node3D node3d && identity.Contains("spawn")) return node3d;
        
        foreach (Node child in node.GetChildren())
        {
            var result = FindSpawnPoint(child);
            if (result != null) return result;
        }
        return null;
    }

    private Node3D FindFinishPoint(Node node)
    {
        string identity = (node.SceneFilePath + " " + node.Name).ToLower();
        if (node is Node3D node3d && identity.Contains("finish")) return node3d;
        
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
        CodeInput.Editable = false; 
        
        if (HelpPanel != null) HelpPanel.Visible = false;
        
        if (ConsoleOutput != null) ConsoleOutput.Text = "Запуск програми...\n";

        string[] lines = CodeInput.Text.Split('\n');
        bool crashed = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim().ToLower();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            if (line == "move()" || line == "forward")
            {
                Player.MoveResult result = await _spawnedRover.MoveForward();
                
                if (result == Player.MoveResult.HitWall)
                {
                    if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ КРИТИЧНА ПОМИЛКА ]: Аварія! Марсохід в'єбався в стіну!";
                    crashed = true;
                    break;
                }
                else if (result == Player.MoveResult.FellInPit)
                {
                    if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ КРИТИЧНА ПОМИЛКА ]: Аварія! Марсохід впав у канаву!";
                    crashed = true;
                    break;
                }
            }
            else if (line == "turn_right()" || line == "right") await _spawnedRover.TurnRight();
            else if (line == "turn_left()" || line == "left") await _spawnedRover.TurnLeft();
            else
            {
                if (ConsoleOutput != null) ConsoleOutput.Text += $"\n[ СИНТАКСИЧНА ПОМИЛКА ]: Невідома команда '{line}'!";
                crashed = true;
                break;
            }

            await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
        }

        if (!crashed)
        {
            if (_finishPoint != null && _spawnedRover.GlobalPosition.DistanceTo(_finishPoint.GlobalPosition) < 0.5f)
            {
                if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ УСПІХ ]: Рівень пройдено! Ти бог C#!";
            }
            else
            {
                if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ ПРОВАЛ ]: Код завершився, але ти не на фініші. Бах і капець!";
            }
        }

        _isRunning = false;
        if (ResetButton != null) ResetButton.Disabled = false; 
    }

    private void OnResetButtonPressed()
    {
        if (_spawnedRover != null)
        {
            _spawnedRover.ResetToStart();
            if (ConsoleOutput != null) ConsoleOutput.Text = "Рівень скинуто. Пиши код заново.";
            
            if (CodeInput != null) CodeInput.Editable = true;
            if (RunButton != null) RunButton.Disabled = false;
            if (ResetButton != null) ResetButton.Disabled = true;
        }
    }
}