using Godot;

public partial class LevelEditor : Node3D
{
    [Export] public PackedScene RockScene { get; set; }
    [Export] public PackedScene PitScene { get; set; }
    [Export] public PackedScene FinishScene { get; set; }

    [Export] public Button RockButton { get; set; }
    [Export] public Button PitButton { get; set; }
    [Export] public Button FinishButton { get; set; }

    public enum SelectedItem { None, Rock, Pit, Finish }
    public SelectedItem CurrentSelection = SelectedItem.None;

    private const float GridSize = 1.0f;

    public override void _Ready()
    {
        if (RockButton != null)
            RockButton.Pressed += () => SetSelection(SelectedItem.Rock, "Камінь");

        if (PitButton != null)
            PitButton.Pressed += () => SetSelection(SelectedItem.Pit, "Яма");

        if (FinishButton != null)
            FinishButton.Pressed += () => SetSelection(SelectedItem.Finish, "Фініш");
    }

    private void SetSelection(SelectedItem item, string name)
    {
        CurrentSelection = item;
        GD.Print($"[LevelEditor] Обрано об'єкт: {name}");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (CurrentSelection != SelectedItem.None)
            {
                TryPlaceObject(mouseEvent.Position);
            }
            else
            {
                GD.Print("Оберіть об'єкт для розміщення на панелі!");
            }
        }
    }

    private void TryPlaceObject(Vector2 mousePos)
    {
        var camera = GetNodeOrNull<Camera3D>("SubViewportContainer/SubViewport/Camera3D") 
                     ?? GetTree().Root.FindChild("Camera3D", true, false) as Camera3D;

        if (camera == null)
        {
            GD.PrintErr("[LevelEditor] Помилка: Камера 3D не знайдена!");
            return;
        }

        var from = camera.ProjectRayOrigin(mousePos);
        var to = from + camera.ProjectRayNormal(mousePos) * 1000f;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            Vector3 hitPosition = (Vector3)result["position"];
            
            // Чітке вирівнювання по сітці 1x1 метр
            float gridX = Mathf.Round(hitPosition.X / GridSize) * GridSize;
            float gridZ = Mathf.Round(hitPosition.Z / GridSize) * GridSize;
            
            // Беремо реальну висоту поверхні тайла підлоги, куди влучив промінь
            float surfaceY = hitPosition.Y; 

            Vector3 spawnPos = new Vector3(gridX, surfaceY, gridZ);

            // Перевіряємо, чи клітинка вже зайнята іншим об'єктом
            if (IsCellOccupied(spawnPos))
            {
                GD.Print($"[LevelEditor] Клітинка {spawnPos} вже зайнята!");
                return;
            }

            SpawnPrefab(spawnPos);
        }
        else
        {
            GD.Print("[LevelEditor] Клік у порожнечу! Ставити можна тільки на підлогу.");
        }
    }

    private bool IsCellOccupied(Vector3 position)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        // Шукаємо об'єкти виключно НАД поверхнею тайла, щоб не чіпати саму підлогу
        var query = PhysicsRayQueryParameters3D.Create(position + Vector3.Up * 0.8f, position + Vector3.Up * 0.1f);
        var result = spaceState.IntersectRay(query);
        
        return result.Count > 0;
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

            GD.Print($"[LevelEditor] УСПІШНИЙ СПАВН {CurrentSelection} на координатах: {position}");
        }
        else
        {
            GD.PrintErr($"[LevelEditor] Помилка: PackedScene для {CurrentSelection} не призначена в інспекторі!");
        }
    }
}