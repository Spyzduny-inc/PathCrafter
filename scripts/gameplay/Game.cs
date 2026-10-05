using Godot;
using System;

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
    [Export] public Control RightEngineerPanel { get; set; }
    [Export] public Button ToggleTerminalButton { get; set; }

    private CanvasLayer _levelCompleteMenuNode;

    private Player _spawnedRover;
    private Node3D _finishPoint;
    private bool _isRunning = false;

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(Global.SelectedLevelPath))
        {
            Global.SelectedLevelPath = "res://scenes/levels/level_1.tscn";
        }

        var levelScene = GD.Load<PackedScene>(Global.SelectedLevelPath);
        if (levelScene != null)
        {
            var levelInstance = levelScene.Instantiate<Node3D>();
            AddChild(levelInstance);

            GridManager.Instance?.ScanLevel(levelInstance);

            SpawnPlayer(levelInstance);
            _finishPoint = FindFinishPoint(levelInstance);
        }

        if (ExitButton != null)
        {
            ExitButton.FocusMode = Control.FocusModeEnum.None;
            ExitButton.Pressed += OnExitButtonPressed;
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

        if (ToggleTerminalButton != null && RightEngineerPanel != null)
        {
            ToggleTerminalButton.FocusMode = Control.FocusModeEnum.None;
            ToggleTerminalButton.Pressed += () =>
            {
                RightEngineerPanel.Visible = !RightEngineerPanel.Visible;
                ToggleTerminalButton.Text = RightEngineerPanel.Visible ? "[ > ] СХОВАТИ КОД" : "[ < ] ВІДКРИТИ КОД";
            };
        }

        // Автоматично завантажуємо і додаємо меню перемоги на рівень із файлу сцени
        var menuScene = GD.Load<PackedScene>("res://scenes/ui/LevelCompleteMenu.tscn");
        if (menuScene != null)
        {
            _levelCompleteMenuNode = menuScene.Instantiate<CanvasLayer>();
            AddChild(_levelCompleteMenuNode);
        }

        if (CodeInput != null)
        {
            SetupCodeEditor();
        }
    }

    private void SetupCodeEditor()
    {
        if (CodeInput == null) return;

        // Встановлюємо білий колір тексту за замовчуванням для невідомих слів/функцій (наприклад moe())
        CodeInput.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
        CodeInput.AddThemeColorOverride("member_variable_color", new Color(1, 1, 1, 1));
        CodeInput.AddThemeColorOverride("function_color", new Color(1, 1, 1, 1));
        CodeInput.AddThemeColorOverride("caret_color", new Color(1, 1, 1, 1));
        CodeInput.AddThemeColorOverride("font_selected_color", new Color(1, 1, 1, 1));
        CodeInput.AddThemeColorOverride("selection_color", new Color(0.26f, 0.45f, 0.76f, 0.6f));
        CodeInput.AddThemeColorOverride("line_number_color", new Color(0.45f, 0.5f, 0.58f, 1));

        // Налаштування автодужок {} () [] <> ""
        CodeInput.AutoBraceCompletionEnabled = true;
        CodeInput.AutoBraceCompletionHighlightMatching = true;
        CodeInput.AutoBraceCompletionPairs = new Godot.Collections.Dictionary
        {
            { "{", "}" },
            { "(", ")" },
            { "[", "]" },
            { "<", ">" },
            { "\"", "\"" }
        };

        // Тема підсвічування One Dark C++
        var highlighter = new CodeHighlighter();
        Color includeColor = Color.FromHtml("#98c379");  // Зелений для #include
        Color keywordColor = Color.FromHtml("#c678dd");  // Пурпуровий для int, void, return, for, if
        Color functionColor = Color.FromHtml("#61afef"); // Блакитний для методів move, turn_left, main
        Color roverColor = Color.FromHtml("#e06c75");    // Червоний/Кораловий для об'єкта rover
        Color numberColor = Color.FromHtml("#d19a66");   // Оранжевий для чисел
        Color commentColor = Color.FromHtml("#5c6370");  // Сірий для коментарів

        highlighter.SymbolColor = new Color(0.85f, 0.85f, 0.85f);
        highlighter.MemberVariableColor = new Color(1f, 1f, 1f, 1f); // ГАРАНТОВАНО БІЛИЙ колір після крапки
        highlighter.FunctionColor = new Color(1f, 1f, 1f, 1f);       // ГАРАНТОВАНО БІЛИЙ колір для невідомих функцій перед ()

        highlighter.AddKeywordColor("#include", includeColor);
        highlighter.AddKeywordColor("int", keywordColor);
        highlighter.AddKeywordColor("void", keywordColor);
        highlighter.AddKeywordColor("return", keywordColor);
        highlighter.AddKeywordColor("for", keywordColor);
        highlighter.AddKeywordColor("if", keywordColor);
        highlighter.AddKeywordColor("while", keywordColor);
        highlighter.AddKeywordColor("main", functionColor);
        highlighter.AddKeywordColor("rover", roverColor);

        // Методи після крапки
        highlighter.AddMemberKeywordColor("move", functionColor);
        highlighter.AddMemberKeywordColor("turn_right", functionColor);
        highlighter.AddMemberKeywordColor("turn_left", functionColor);

        highlighter.AddKeywordColor("move", functionColor);
        highlighter.AddKeywordColor("turn_right", functionColor);
        highlighter.AddKeywordColor("turn_left", functionColor);

        for (int i = 0; i <= 9; i++)
        {
            highlighter.AddKeywordColor(i.ToString(), numberColor);
        }

        highlighter.AddColorRegion("//", "", commentColor, true);
        highlighter.AddColorRegion("/*", "*/", commentColor, false);

        CodeInput.SyntaxHighlighter = highlighter;
        CodeInput.PlaceholderText = Global.DefaultCodeTemplate;

        // Автозавершення коду (IntelliSense)
        CodeInput.CodeCompletionEnabled = true;
        CodeInput.CodeCompletionPrefixes = new Godot.Collections.Array<string> { ".", "<", "#", "r", "ro", "rov", "rover", "m", "t", "f" };
        CodeInput.Connect(CodeEdit.SignalName.CodeCompletionRequested, Callable.From(OnRequestCodeCompletion));

        // Перехоплення клавіш Tab та Enter для автопідтвердження обраної підказки
        CodeInput.GuiInput += (InputEvent @event) =>
        {
            if (@event is InputEventKey k && k.Pressed && !k.Echo)
            {
                if (k.Keycode == Key.Tab || k.Keycode == Key.Enter)
                {
                    CodeInput.ConfirmCodeCompletion();
                }
            }
        };

        // Завантажуємо збережений код або базову структуру C++
        CodeInput.Text = Global.GetCodeForLevel(Global.SelectedLevelPath);

        // Автоматичне збереження та автоматичний виклик автодоповнення при наборі 'r', 'ro', 'rov', 'rover.', '.'
        CodeInput.TextChanged += () =>
        {
            Global.SaveCodeForLevel(Global.SelectedLevelPath, CodeInput.Text);

            int line = CodeInput.GetCaretLine();
            int col = CodeInput.GetCaretColumn();
            string fullLine = CodeInput.GetLine(line);
            if (col <= fullLine.Length)
            {
                string lineBefore = fullLine.Substring(0, col);
                if (lineBefore.EndsWith(".") || System.Text.RegularExpressions.Regex.IsMatch(lineBefore, @"\b(r|ro|rov|rove|rover|m|mov|move|t|tur|turn|f|for|i|inc|incl|include)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    CodeInput.RequestCodeCompletion();
                }
            }
        };
    }

    private void OnRequestCodeCompletion()
    {
        if (CodeInput == null) return;

        int line = CodeInput.GetCaretLine();
        int col = CodeInput.GetCaretColumn();
        string fullLineText = CodeInput.GetLine(line);
        string lineBeforeCaret = col <= fullLineText.Length ? fullLineText.Substring(0, col) : fullLineText;
        string trimmedBefore = lineBeforeCaret.Trim();

        Color functionColor = Color.FromHtml("#61afef");
        Color roverColor = Color.FromHtml("#e06c75");
        Color includeColor = Color.FromHtml("#98c379");
        Color keywordColor = Color.FromHtml("#c678dd");

        // КОНТЕКСТ 1: Якщо ми пишемо після крапки об'єкта rover. (наприклад "rover." або "rover.m")
        if (System.Text.RegularExpressions.Regex.IsMatch(lineBeforeCaret, @"\brover\s*\.\s*\w*$"))
        {
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Member, "move()", "move();", functionColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Member, "turn_left()", "turn_left();", functionColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Member, "turn_right()", "turn_right();", functionColor);
        }
        // КОНТЕКСТ 2: На початку файлу чи для вводу #include та main()
        else if (trimmedBefore.StartsWith("#") || trimmedBefore.StartsWith("<") || line <= 1)
        {
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Keyword, "#include <moving>", "#include <moving>\n", includeColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, "int main()", "int main() {\n    rover.move();\n    return 0;\n}", keywordColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, "void main()", "void main() {\n    rover.move();\n}", keywordColor);
        }
        // КОНТЕКСТ 3: Усередині тіла функції main()
        else
        {
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Keyword, "rover", "rover.", roverColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, "rover.move()", "rover.move();", functionColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, "rover.turn_left()", "rover.turn_left();", functionColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, "rover.turn_right()", "rover.turn_right();", functionColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Keyword, "for (int i = 0; i < N; i++)", "for (int i = 0; i < 3; i++) {\n    \n}", keywordColor);
            CodeInput.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Keyword, "return 0;", "return 0;", keywordColor);
        }

        CodeInput.UpdateCodeCompletionOptions(true);
    }

    private void SpawnPlayer(Node3D levelInstance)
    {
        if (PlayerScene == null) return;
        
        Node3D spawnPoint = FindSpawnPoint(levelInstance);
        
        if (spawnPoint != null)
        {
            var playerInstance = PlayerScene.Instantiate<Node3D>();
            playerInstance.Position = spawnPoint.GlobalPosition;
            playerInstance.Rotation = spawnPoint.Rotation;
            AddChild(playerInstance);

            _spawnedRover = playerInstance as Player;
            _spawnedRover.LevelRoot = levelInstance; 
            _spawnedRover.SaveStartPosition(); 

            CallDeferred(nameof(CenterCameraOnPlayer), playerInstance.GlobalPosition);
        }
    }

    private void CenterCameraOnPlayer(Vector3 targetPos)
    {
        var camera = GetNodeOrNull<Camera3D>("CameraController/Camera3D") 
                     ?? GetViewport().FindChild("Camera3D", true, false) as Camera3D;
        if (camera != null)
        {
            var cameraParent = camera.GetParent() as Node3D;
            if (cameraParent != null && cameraParent.Name.ToString().Contains("Controller"))
            {
                cameraParent.GlobalPosition = new Vector3(targetPos.X, cameraParent.GlobalPosition.Y, targetPos.Z);
            }
            else
            {
                camera.GlobalPosition = new Vector3(targetPos.X, camera.GlobalPosition.Y + 5.0f, targetPos.Z + 5.0f);
                camera.LookAt(targetPos);
            }
        }
    }

    private Node3D FindSpawnPoint(Node node)
    {
        if (node is Tile tile && tile.Type == TileType.PlayerSpawn) return tile;
        if (node is Node3D node3d && node3d.HasMeta("is_player_spawn") && node3d.GetMeta("is_player_spawn").AsBool()) return node3d;
        foreach (Node child in node.GetChildren())
        {
            var result = FindSpawnPoint(child);
            if (result != null) return result;
        }
        return null;
    }

    private Node3D FindFinishPoint(Node node)
    {
        if (node is Tile tile && tile.Type == TileType.Finish) return tile;
        if (node is Node3D node3d && node3d.HasMeta("is_finish") && node3d.GetMeta("is_finish").AsBool()) return node3d;
        foreach (Node child in node.GetChildren())
        {
            var result = FindFinishPoint(child);
            if (result != null) return result;
        }
        return null;
    }

    private void OnExitButtonPressed()
    {
        _isRunning = false; 
        GetViewport().GuiReleaseFocus();
        QueueFree();
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/ui/MainMenu.tscn");
    }

    private async void OnRunButtonPressed()
    {
        if (_isRunning || CodeInput == null || _spawnedRover == null || !IsInstanceValid(_spawnedRover)) return;

        GetViewport().GuiReleaseFocus();

        _isRunning = true;
        RunButton.Disabled = true;
        if (ResetButton != null) ResetButton.Disabled = true;
        CodeInput.Editable = false; 
        
        if (HelpPanel != null) HelpPanel.Visible = false;
        if (ConsoleOutput != null) ConsoleOutput.Text = "Запуск програми...\n";

        bool crashed = false;

        try
        {
            var commands = CodeParser.Parse(CodeInput.Text);

            foreach (var cmd in commands)
            {
                if (!_isRunning || !IsInsideTree() || !IsInstanceValid(this) || !IsInstanceValid(_spawnedRover)) return;

                switch (cmd.Type)
                {
                    case CommandType.Move:
                        Player.MoveResult result = await _spawnedRover.MoveForward();

                        if (!_isRunning || !IsInsideTree() || !IsInstanceValid(this)) return;

                        if (result == Player.MoveResult.HitWall)
                        {
                            if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ КРИТИЧНА ПОМИЛКА ]: Аварія! Марсохід в'єбався в стіну!";
                            crashed = true;
                        }
                        else if (result == Player.MoveResult.FellInPit)
                        {
                            if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ КРИТИЧНА ПОМИЛКА ]: Аварія! Марсохід впав у канаву!";
                            crashed = true;
                        }
                        break;

                    case CommandType.TurnRight:
                        await _spawnedRover.TurnRight();
                        break;

                    case CommandType.TurnLeft:
                        await _spawnedRover.TurnLeft();
                        break;
                }

                if (crashed) break;

                await ToSignal(GetTree().CreateTimer(0.2, false), SceneTreeTimer.SignalName.Timeout);
            }

            if (!_isRunning || !IsInsideTree() || !IsInstanceValid(this)) return;

            if (!crashed)
            {
                if (_finishPoint != null && IsInstanceValid(_finishPoint) && _spawnedRover.GlobalPosition.DistanceTo(_finishPoint.GlobalPosition) < 0.5f)
                {
                    if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ УСПІХ ]: Місію виконано! Команда C++ успішно опрацьована.";
                    
                    // Розблоковуємо наступний рівень та викликаємо вікно перемоги
                    Global.UnlockNextLevel(0);
                    
                    if (RightEngineerPanel != null) RightEngineerPanel.Visible = false;
                    if (ToggleTerminalButton != null) ToggleTerminalButton.Text = "[ < ] ВІДКРИТИ КОД";

                    if (_levelCompleteMenuNode != null)
                    {
                        _levelCompleteMenuNode.Call("ShowVictory");
                    }
                }
                else
                {
                    if (ConsoleOutput != null) ConsoleOutput.Text += "\n[ ПРОВАЛ ]: Код завершився, але ти не на фініші. Бах і капець!";
                }
            }
        }
        catch (FormatException fex)
        {
            if (ConsoleOutput != null) ConsoleOutput.Text += $"\n[ СИНТАКСИЧНА ПОМИЛКА ]: {fex.Message}!";
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Game] Перехоплено помилку: {ex.Message}");
        }
        finally
        {
            if (IsInstanceValid(this))
            {
                _isRunning = false;
                if (ResetButton != null) ResetButton.Disabled = false; 
            }
        }
    }

    private void OnResetButtonPressed()
    {
        if (_spawnedRover != null && IsInstanceValid(_spawnedRover))
        {
            _spawnedRover.ResetToStart();
            if (ConsoleOutput != null) ConsoleOutput.Text = "Рівень скинуто. Пиши код заново.";
            
            if (CodeInput != null) CodeInput.Editable = true;
            if (RunButton != null) RunButton.Disabled = false;
            if (ResetButton != null) ResetButton.Disabled = true;
        }
    }
}