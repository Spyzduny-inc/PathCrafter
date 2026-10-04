using Godot;

public partial class Tile : Node3D
{
    [Export] public TileType Type { get; set; } = TileType.Empty;
    [Export] public Vector2I GridPosition { get; set; }

    public override void _Ready()
    {
        var gridManager = GetViewport()?.FindChild("GridManager", recursive: true, owned: false) as GridManager
                          ?? GetTree()?.Root?.FindChild("GridManager", recursive: true, owned: false) as GridManager;

        if (gridManager != null)
        {
            gridManager.RegisterTile(GridPosition, Type);
        }
        else
        {
            GD.PrintErr($"[Tile] GridManager not found for Tile at {GridPosition}");
        }
    }
}
