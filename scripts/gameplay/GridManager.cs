using Godot;
using System.Collections.Generic;

public partial class GridManager : Node
{
    private readonly Dictionary<Vector2I, TileType> _grid = new();

    public void RegisterTile(Vector2I position, TileType type)
    {
        _grid[position] = type;
    }

    public TileType GetTileAt(Vector2I position)
    {
        return _grid.TryGetValue(position, out var type) ? type : TileType.Empty;
    }

    public bool IsWalkable(Vector2I position)
    {
        var tile = GetTileAt(position);
        return tile == TileType.Empty || tile == TileType.Finish || tile == TileType.PlayerSpawn;
    }
}
