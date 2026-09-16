using Godot;
using System.Collections.Generic;

public partial class LevelEditor : Node3D
{
    [Export] public PackedScene RockScene { get; set; }
    [Export] public PackedScene PitScene { get; set; }
    [Export] public PackedScene FinishScene { get; set; }
    [Export] public PackedScene SkyScene { get; set; }

    [Export] public Button RockButton { get; set; }
    [Export] public Button PitButton { get; set; }
    [Export] public Button FinishButton { get; set; }
    [Export] public Button GenerateButton { get; set; }
    [Export] public Button UndoButton { get; set; }

    [Export] public SpinBox WidthSpinBox { get; set; }
    [Export] public SpinBox HeightSpinBox { get; set; }
    [Export] public Node GridGeneratorNode { get; set; }

    public enum SelectedItem { None, Rock, Pit, Finish }
    public SelectedItem CurrentSelection = SelectedItem.None;

    private const float GridSize = 1.0f;
    private List<Node3D> spawnedObjectsHistory = new List<Node3D>();

    // --- НОВА СИСТЕМА ІСТОРІЇ ---
    private enum ActionType { Place, Remove }
    private class EditorAction
    {
        public ActionType Type;
        public Node3D TargetObj;
    }
    private List<EditorAction> actionHistory = new List<EditorAction>();
    // ----------------------------

    public override void _Ready()
    {
        // Дефолтні значення 15x15 зі старту
        if (WidthSpinBox != null) WidthSpinBox.Value = 15;
        if (HeightSpinBox != null) HeightSpinBox.Value = 15;

        // ВИМИКАЄМО ФОКУС ДЛЯ КНОПОК, щоб стрілочки не бігали по UI
        if (RockButton != null) RockButton.FocusMode = Control.FocusModeEnum.None;
        if (PitButton != null) PitButton.FocusMode = Control.FocusModeEnum.None;
        if (FinishButton != null) FinishButton.FocusMode = Control.FocusModeEnum.None;
        if (GenerateButton != null) GenerateButton.FocusMode = Control.FocusModeEnum.None;
        if (UndoButton != null) UndoButton.FocusMode = Control.FocusModeEnum.None;

        // Підключення сигналів кнопок
        if (RockButton != null) RockButton.Pressed += () => SetSelection(SelectedItem.Rock, "Камінь");
        if (PitButton != null) PitButton.Pressed += () => SetSelection(SelectedItem.Pit, "Яма");
        if (FinishButton != null) FinishButton.Pressed += () => SetSelection(SelectedItem.Finish, "Фініш");
        
        if (GenerateButton != null) GenerateButton.Pressed += OnGenerateLevelPressed;
        if (UndoButton != null) UndoButton.Pressed += PerformUndo;

        EnsureSkyOnCurrentScene();
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
        if (SkyScene == null) SkyScene = GD.Load<PackedScene>("res://scenes/ui/sky.tscn");

        if (SkyScene != null)
        {
            var skyInstance = SkyScene.Instantiate();
            skyInstance.Name = "SkyEnvironment";
            AddChild(skyInstance);

            if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
            {
                if (skyInstance is Node3D node3D && GetTree().EditedSceneRoot is Node3D)
                {
                    node3D.Owner = GetTree().EditedSceneRoot;
                }
            }
        }
    }

    private void SetSelection(SelectedItem item, string name)
    {
        CurrentSelection = item;
        GD.Print($"[LevelEditor] Обрано об'єкт: {name}");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (CurrentSelection != SelectedItem.None) TryPlaceObject(mouseEvent.Position);
                else GD.Print("Оберіть об'єкт для розміщення на панелі!");
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
            float surfaceY = hitPosition.Y; 

            Vector3 spawnPos = new Vector3(gridX, surfaceY, gridZ);

            if (IsCellOccupied(spawnPos))
            {
                GD.Print($"[LevelEditor] Клітинка X:{gridX}, Z:{gridZ} вже зайнята!");
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
                // Замість QueueFree ховаємо об'єкт і записуємо дію
                spawnedObjectsHistory.Remove(targetObj);
                RemoveChild(targetObj);
                actionHistory.Add(new EditorAction { Type = ActionType.Remove, TargetObj = targetObj });
                
                GD.Print("[LevelEditor] Об'єкт видалено зі сцени!");
            }
        }
    }

    public void PerformUndo()
    {
        if (actionHistory.Count > 0)
        {
            var lastAction = actionHistory[actionHistory.Count - 1];
            actionHistory.RemoveAt(actionHistory.Count - 1);

            if (lastAction.Type == ActionType.Place)
            {
                if (GodotObject.IsInstanceValid(lastAction.TargetObj))
                {
                    spawnedObjectsHistory.Remove(lastAction.TargetObj);
                    lastAction.TargetObj.QueueFree(); // Видаляємо те, що щойно поставили
                    GD.Print("[LevelEditor] Undo: скасовано постановку об'єкта.");
                }
            }
            else if (lastAction.Type == ActionType.Remove)
            {
                if (GodotObject.IsInstanceValid(lastAction.TargetObj))
                {
                    AddChild(lastAction.TargetObj); // Повертаємо те, що щойно видалили
                    spawnedObjectsHistory.Add(lastAction.TargetObj);
                    GD.Print("[LevelEditor] Undo: відновлено видалений об'єкт.");
                }
            }
        }
        else
        {
            GD.Print("[LevelEditor] Історія порожня, нема чого скасовувати!");
        }
    }

    private bool IsCellOccupied(Vector3 position)
    {
        foreach (var obj in spawnedObjectsHistory)
        {
            if (obj != null && GodotObject.IsInstanceValid(obj))
            {
                if (Mathf.Abs(obj.Position.X - position.X) < 0.1f && Mathf.Abs(obj.Position.Z - position.Z) < 0.1f)
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
            case SelectedItem.Rock:
                if (RockScene != null) newObj = RockScene.Instantiate<Node3D>();
                break;
            case SelectedItem.Pit:
                if (PitScene != null) newObj = PitScene.Instantiate<Node3D>();
                break;
            case SelectedItem.Finish:
                if (FinishScene != null) newObj = FinishScene.Instantiate<Node3D>();
                break;
        }

        if (newObj != null)
        {
            newObj.Position = position;
            AddChild(newObj);
            
            if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
            {
                newObj.Owner = GetTree().EditedSceneRoot;
            }

            spawnedObjectsHistory.Add(newObj);
            actionHistory.Add(new EditorAction { Type = ActionType.Place, TargetObj = newObj });
            
            GD.Print($"[LevelEditor] Успішний спавн {CurrentSelection} на {position}");
        }
    }
}