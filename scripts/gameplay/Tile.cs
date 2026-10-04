using Godot;

public partial class Tile : Node3D
{
    [Export] public TileType Type { get; set; } = TileType.Empty;
    [Export] public Vector2I GridPosition { get; set; }

    public Vector2I CurrentGridPosition =>
        GridPosition != Vector2I.Zero
            ? GridPosition
            : new Vector2I(Mathf.RoundToInt(GlobalPosition.X), Mathf.RoundToInt(GlobalPosition.Z));

    public override void _Ready()
    {
        if (GridPosition == Vector2I.Zero)
        {
            GridPosition = CurrentGridPosition;
        }

        GridManager.Instance?.RegisterTile(CurrentGridPosition, Type);

        if (Type == TileType.PlayerSpawn)
        {
            Node currentScene = GetTree()?.CurrentScene;
            bool isEditor = currentScene != null && currentScene.Name.ToString().Contains("LevelEditor");
            if (!isEditor)
            {
                Visible = false;
            }
        }

        if (Type == TileType.Wall)
        {
            Rotation = new Vector3(Rotation.X, (float)GD.RandRange(0, Mathf.Tau), Rotation.Z);
        }
    }

    public override void _ExitTree()
    {
        GridManager.Instance?.UnregisterTile(CurrentGridPosition);
    }
}
