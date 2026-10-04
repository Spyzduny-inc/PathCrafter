using Godot;
using System.Collections.Generic;

public partial class LevelEditor : Node3D
{
    [Export] public PackedScene RockScene { get; set; }
    [Export] public PackedScene PitScene { get; set; }
    [Export] public PackedScene FinishScene { get; set; }
    [Export] public PackedScene PlayerSpawnScene { get; set; }
    [Export] public PackedScene SkyScene { get; set; }

    [Export] public Button RockButton { get; set; }
    [Export] public Button PitButton { get; set; }
    [Export] public Button FinishButton { get; set; }
    [Export] public Button PlayerSpawnButton { get; set; }
    [Export] public Button GenerateButton { get; set; }
    [Export] public Button UndoButton { get; set; }
    [Export] public Button SaveButton { get; set; } 
    [Export] public Button BackToMenuButton { get; set; }

    [Export] public SpinBox WidthSpinBox { get; set; }
    [Export] public SpinBox HeightSpinBox { get; set; }
    [Export] public Node GridGeneratorNode { get; set; }

    public enum SelectedItem { None, Rock, Pit, Finish, PlayerSpawn }
    public SelectedItem CurrentSelection = SelectedItem.None;

    private const float GridSize = 1.0f;
    private List<Node3D> spawnedObjectsHistory = new List<Node3D>();

    private enum ActionType { Place, Remove }
    private class EditorAction
    {
        public ActionType Type;
        public Node3D TargetObj;
    }
    private List<EditorAction> actionHistory = new List<EditorAction>();

    private ConfirmationDialog saveDialog;
    private AcceptDialog errorDialog; 
    private LineEdit levelNameInput;
    private CheckBox autoNumberCheckbox;
    private const string SaveDir = "res://scenes/levels/customs/";

    public override void _Ready()
    {
        if (WidthSpinBox != null) WidthSpinBox.Value = 15;
        if (HeightSpinBox != null) HeightSpinBox.Value = 15;

        if (RockButton != null) RockButton.FocusMode = Control.FocusModeEnum.None;
        if (PitButton != null) PitButton.FocusMode = Control.FocusModeEnum.None;
        if (FinishButton != null) FinishButton.FocusMode = Control.FocusModeEnum.None;
        if (PlayerSpawnButton != null) PlayerSpawnButton.FocusMode = Control.FocusModeEnum.None;
        if (GenerateButton != null) GenerateButton.FocusMode = Control.FocusModeEnum.None;
        if (UndoButton != null) UndoButton.FocusMode = Control.FocusModeEnum.None;
        if (SaveButton != null) SaveButton.FocusMode = Control.FocusModeEnum.None;

        if (RockButton != null) RockButton.Pressed += () => SetSelection(SelectedItem.Rock, "Камінь");
        if (PitButton != null) PitButton.Pressed += () => SetSelection(SelectedItem.Pit, "Яма");
        if (FinishButton != null) FinishButton.Pressed += () => SetSelection(SelectedItem.Finish, "Фініш");
        if (PlayerSpawnButton != null) PlayerSpawnButton.Pressed += () => SetSelection(SelectedItem.PlayerSpawn, "Точка спавну гравця");
        
        if (GenerateButton != null) GenerateButton.Pressed += OnGenerateLevelPressed;
        if (UndoButton != null) UndoButton.Pressed += PerformUndo;
        if (SaveButton != null) SaveButton.Pressed += ShowSaveDialog; 

        if (BackToMenuButton != null) 
        {
          BackToMenuButton.FocusMode = Control.FocusModeEnum.None;
          BackToMenuButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
        }   

        EnsureSkyOnCurrentScene();
        SetupDialogs();
    }

    private void SetupDialogs()
    {
        saveDialog = new ConfirmationDialog();
        saveDialog.Title = "Збереження рівня";
        saveDialog.Size = new Vector2I(350, 150);

        var vbox = new VBoxContainer();
        saveDialog.AddChild(vbox);

        autoNumberCheckbox = new CheckBox();
        autoNumberCheckbox.Text = "Автоматична нумерація (Level_X)";
        autoNumberCheckbox.ButtonPressed = true;
        vbox.AddChild(autoNumberCheckbox);

        levelNameInput = new LineEdit();
        levelNameInput.PlaceholderText = "Введіть власну назву...";
        levelNameInput.Editable = false; 
        vbox.AddChild(levelNameInput);

        autoNumberCheckbox.Toggled += (bool toggledOn) => 
        {
            levelNameInput.Editable = !toggledOn;
        };

        saveDialog.Confirmed += OnSaveConfirmed;
        AddChild(saveDialog);

        errorDialog = new AcceptDialog();
        errorDialog.Title = "Помилка збереження!";
        AddChild(errorDialog);
    }

    private void ShowSaveDialog()
    {
        if (!IsPlayerSpawnPlaced())
        {
            errorDialog.DialogText = "Неможливо зберегти рівень!\nСпочатку встановіть 'Точку спавну гравця' на карті.";
            errorDialog.PopupCentered();
            return;
        }

        saveDialog.PopupCentered();
    }

    private void OnSaveConfirmed()
    {
        if (!DirAccess.DirExistsAbsolute(SaveDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(SaveDir);
        }

        string fileName = "";

        if (autoNumberCheckbox.ButtonPressed)
        {
            fileName = GetNextAutoLevelName();
        }
        else
        {
            fileName = levelNameInput.Text.StripEdges();
            if (string.IsNullOrEmpty(fileName)) fileName = "UnnamedLevel";
        }

        if (!fileName.EndsWith(".tscn")) fileName += ".tscn";

        string fullPath = SaveDir + fileName;
        ExecuteSave(fullPath);
    }

    private string GetNextAutoLevelName()
    {
        int maxNumber = 0;
        using var dir = DirAccess.Open(SaveDir);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.StartsWith("Level_") && fileName.EndsWith(".tscn"))
                {
                    string numPart = fileName.Replace("Level_", "").Replace(".tscn", "");
                    if (int.TryParse(numPart, out int num))
                    {
                        if (num > maxNumber) maxNumber = num;
                    }
                }
                fileName = dir.GetNext();
            }
        }
        return $"Level_{maxNumber + 1}";
    }

    private void ExecuteSave(string savePath)
    {
        GD.Print($"[LevelEditor] Пакуємо сцену для збереження у {savePath}...");
        
        Node3D levelRoot = new Node3D();
        levelRoot.Name = "Level";
        AddChild(levelRoot); 

        Node originalOwner = Engine.IsEditorHint() && GetTree().EditedSceneRoot != null 
            ? GetTree().EditedSceneRoot 
            : GetTree().CurrentScene;

        var sky = GetNodeOrNull("SkyEnvironment");

        if (GridGeneratorNode != null)
        {
            GridGeneratorNode.GetParent().RemoveChild(GridGeneratorNode);
            levelRoot.AddChild(GridGeneratorNode);
            GridGeneratorNode.Owner = levelRoot;
            foreach (Node child in GridGeneratorNode.GetChildren()) child.Owner = levelRoot;
        }

        if (sky != null)
        {
            sky.GetParent().RemoveChild(sky);
            levelRoot.AddChild(sky);
            sky.Owner = levelRoot;
        }

        foreach (var obj in spawnedObjectsHistory)
        {
            if (GodotObject.IsInstanceValid(obj))
            {
                obj.GetParent().RemoveChild(obj);
                levelRoot.AddChild(obj);
                obj.Owner = levelRoot;
            }
        }

        var packedScene = new PackedScene();
        var result = packedScene.Pack(levelRoot);
        
        if (result == Error.Ok)
        {
            var saveResult = ResourceSaver.Save(packedScene, savePath);
            if (saveResult == Error.Ok)
                GD.Print($"[LevelEditor] УСПІХ! Рівень збережено: {savePath}");
            else
                GD.PrintErr($"[LevelEditor] Помилка запису файлу: {saveResult}");
        }
        else
        {
            GD.PrintErr($"[LevelEditor] Помилка пакування сцени: {result}");
        }

        if (GridGeneratorNode != null)
        {
            levelRoot.RemoveChild(GridGeneratorNode);
            this.AddChild(GridGeneratorNode);
            GridGeneratorNode.Owner = originalOwner;
            foreach (Node child in GridGeneratorNode.GetChildren()) child.Owner = originalOwner;
        }

        if (sky != null)
        {
            levelRoot.RemoveChild(sky);
            this.AddChild(sky);
            sky.Owner = originalOwner;
        }

        foreach (var obj in spawnedObjectsHistory)
        {
            if (GodotObject.IsInstanceValid(obj))
            {
                levelRoot.RemoveChild(obj);
                this.AddChild(obj);
                obj.Owner = originalOwner;
            }
        }

        levelRoot.QueueFree();
    }

    private void OnGenerateLevelPressed()
    {
        if (GridGeneratorNode != null)
        {
            int w = WidthSpinBox != null ? (int)WidthSpinBox.Value : 15;
            int h = HeightSpinBox != null ? (int)HeightSpinBox.Value : 15;
            GridGeneratorNode.Set("GridWidth", w);
            GridGeneratorNode.Set("GridDepth", h);

            if (GridGeneratorNode.HasMethod("GenerateGrid")) GridGeneratorNode.Call("GenerateGrid");
            else if (GridGeneratorNode.HasMethod("Generate")) GridGeneratorNode.Call("Generate");
        }
        EnsureSkyOnCurrentScene();
    }

    private void EnsureSkyOnCurrentScene()
    {
        if (HasNode("SkyEnvironment")) return;
        if (SkyScene == null) SkyScene = GD.Load<PackedScene>("res://scenes/environment/sky.tscn");
        if (SkyScene != null)
        {
            var skyInstance = SkyScene.Instantiate();
            skyInstance.Name = "SkyEnvironment";
            AddChild(skyInstance);
            if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null) skyInstance.Owner = GetTree().EditedSceneRoot;
            else if (GetTree().CurrentScene != null) skyInstance.Owner = GetTree().CurrentScene;
        }
    }

    private void SetSelection(SelectedItem item, string name)
    {
        CurrentSelection = item;
        GD.Print($"[LevelEditor] Обрано об'єкт: {name}");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            if ((mouseMotion.ButtonMask & MouseButtonMask.Left) != 0) 
            {
                if (CurrentSelection != SelectedItem.None) TryPlaceObject(mouseMotion.Position);
            }
            else if ((mouseMotion.ButtonMask & MouseButtonMask.Right) != 0) 
            {
                TryRemoveObject(mouseMotion.Position);
            }
        }

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (CurrentSelection != SelectedItem.None) TryPlaceObject(mouseEvent.Position);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                TryRemoveObject(mouseEvent.Position);
            }
        }
    }

    private void TryPlaceObject(Vector2 mousePos)
    {
        var camera = GetNodeOrNull<Camera3D>("SubViewportContainer/SubViewport/Camera3D") ?? GetTree().Root.FindChild("Camera3D", true, false) as Camera3D;
        if (camera == null) return;
        var from = camera.ProjectRayOrigin(mousePos);
        var to = from + camera.ProjectRayNormal(mousePos) * 1000f;
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollisionMask = 1; 
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            Vector3 hitPosition = (Vector3)result["position"];
            float gridX = Mathf.Round(hitPosition.X / GridSize) * GridSize;
            float gridZ = Mathf.Round(hitPosition.Z / GridSize) * GridSize;
            
            Vector3 spawnPos = new Vector3(gridX, hitPosition.Y, gridZ);
            
            if (IsCellOccupied(spawnPos))
            {
                return; 
            }

            if (CurrentSelection == SelectedItem.PlayerSpawn && IsPlayerSpawnPlaced())
            {
                return; 
            }

            if (CurrentSelection == SelectedItem.Finish && IsFinishPlaced())
            {
                return;
            }

            SpawnPrefab(spawnPos);
        }
    }

    private void TryRemoveObject(Vector2 mousePos)
    {
        var camera = GetNodeOrNull<Camera3D>("SubViewportContainer/SubViewport/Camera3D") ?? GetTree().Root.FindChild("Camera3D", true, false) as Camera3D;
        if (camera == null) return;
        var from = camera.ProjectRayOrigin(mousePos);
        var to = from + camera.ProjectRayNormal(mousePos) * 1000f;
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollisionMask = 2; 
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            var hitCollider = result["collider"].As<Node>();
            Node3D targetObj = null;
            Node current = hitCollider;
            while (current != null && current != this)
            {
                if (current is Node3D node3D && spawnedObjectsHistory.Contains(node3D))
                {
                    targetObj = node3D;
                    break;
                }
                current = current.GetParent();
            }
            if (targetObj != null)
            {
                spawnedObjectsHistory.Remove(targetObj);
                RemoveChild(targetObj);
                actionHistory.Add(new EditorAction { Type = ActionType.Remove, TargetObj = targetObj });
            }
        }
    }

    public void PerformUndo()
    {
        if (actionHistory.Count > 0)
        {
            var lastAction = actionHistory[actionHistory.Count - 1];
            actionHistory.RemoveAt(actionHistory.Count - 1);
            if (lastAction.Type == ActionType.Place && GodotObject.IsInstanceValid(lastAction.TargetObj))
            {
                spawnedObjectsHistory.Remove(lastAction.TargetObj);
                lastAction.TargetObj.QueueFree();
            }
            else if (lastAction.Type == ActionType.Remove && GodotObject.IsInstanceValid(lastAction.TargetObj))
            {
                AddChild(lastAction.TargetObj);
                spawnedObjectsHistory.Add(lastAction.TargetObj);
            }
        }
    }

    private bool IsCellOccupied(Vector3 position)
    {
        foreach (var obj in spawnedObjectsHistory)
        {
            if (obj != null && GodotObject.IsInstanceValid(obj))
            {
                if (Mathf.Abs(obj.Position.X - position.X) < 0.1f && Mathf.Abs(obj.Position.Z - position.Z) < 0.1f) return true;
            }
        }
        return false;
    }

    private bool IsPlayerSpawnPlaced()
    {
        foreach (var obj in spawnedObjectsHistory)
        {
            if (obj != null && GodotObject.IsInstanceValid(obj))
            {
                if (obj.HasMeta("is_player_spawn") && obj.GetMeta("is_player_spawn").AsBool())
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsFinishPlaced()
    {
        foreach (var obj in spawnedObjectsHistory)
        {
            if (obj != null && GodotObject.IsInstanceValid(obj))
            {
                if (obj.HasMeta("is_finish") && obj.GetMeta("is_finish").AsBool())
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void SpawnPrefab(Vector3 position)
    {
        Node3D newObj = null;
        switch (CurrentSelection)
        {
            case SelectedItem.Rock: if (RockScene != null) newObj = RockScene.Instantiate<Node3D>(); break;
            case SelectedItem.Pit: if (PitScene != null) newObj = PitScene.Instantiate<Node3D>(); break;
            case SelectedItem.Finish: 
                if (FinishScene != null) 
                {
                    newObj = FinishScene.Instantiate<Node3D>();
                    newObj.SetMeta("is_finish", true); 
                }
                break;
            case SelectedItem.PlayerSpawn: 
                if (PlayerSpawnScene != null) 
                {
                    newObj = PlayerSpawnScene.Instantiate<Node3D>();
                    newObj.SetMeta("is_player_spawn", true); 
                }
                break;
        }
        
        if (newObj != null)
        {
            position.Y = (CurrentSelection == SelectedItem.Pit) ? 0.51f : 1.0f;
            
            newObj.Position = position;
            AddChild(newObj);
            
            if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null) newObj.Owner = GetTree().EditedSceneRoot;
            else if (GetTree().CurrentScene != null) newObj.Owner = GetTree().CurrentScene;
            
            spawnedObjectsHistory.Add(newObj);
            actionHistory.Add(new EditorAction { Type = ActionType.Place, TargetObj = newObj });
        }
    }
}