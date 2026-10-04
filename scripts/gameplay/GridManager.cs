using Godot;
using System.Collections.Generic;

public partial class GridManager : Node
{
    public static GridManager Instance { get; private set; }

    private readonly Dictionary<Vector2I, TileType> _grid = new();

    public override void _Ready()
    {
        Instance = this;
    }

    public void Clear()
    {
        _grid.Clear();
    }

    public void RegisterTile(Vector2I position, TileType type)
    {
        _grid[position] = type;
    }

    public void UnregisterTile(Vector2I position)
    {
        _grid.Remove(position);
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

    public void ScanLevel(Node levelRoot)
    {
        _grid.Clear();
        if (levelRoot == null) return;

        ScanNodeRecursive(levelRoot);
    }

    private void ScanNodeRecursive(Node node)
    {
        if (node is Tile tile)
        {
            RegisterTile(tile.CurrentGridPosition, tile.Type);
        }

        foreach (Node child in node.GetChildren())
        {
            ScanNodeRecursive(child);
        }
    }
}
