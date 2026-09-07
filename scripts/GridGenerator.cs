using Godot;

public partial class GridGenerator : Node3D
{
    [Export]
    public PackedScene FloorScene { get; set; }

    [Export]
    public int GridWidth { get; set; } = 10;

    [Export]
    public int GridDepth { get; set; } = 10;

    public override void _Ready()
    {
        if (FloorScene == null)
        {
            GD.PrintErr("Помилка: FloorScene пуста в інспекторі!");
            return;
        }

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        int startX = -GridWidth / 2;
        int endX = GridWidth / 2;
        int startZ = -GridDepth / 2;
        int endZ = GridDepth / 2;

        for (int x = startX; x < endX; x++)
        {
            for (int z = startZ; z < endZ; z++)
            {
                Node3D floorBlock = FloorScene.Instantiate<Node3D>();
                
                // Ставимо кубики ідеально впритул один до одного
                floorBlock.Position = new Vector3(x, 0, z);
                AddChild(floorBlock);
            }
        }
    }
}