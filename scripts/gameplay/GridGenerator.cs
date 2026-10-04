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

	public void GenerateGrid()
	{
		// Очищаємо стару сітку перед створенням нової
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}

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

				// --- ДОДАНО ДЛЯ ЗБЕРЕЖЕННЯ У ФАЙЛ ---
				// Задаємо Owner, щоб Ґодот знав, що цю підлогу треба пакувати в .tscn
				if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
				{
					floorBlock.Owner = GetTree().EditedSceneRoot;
				}
				else if (GetTree().CurrentScene != null)
				{
					floorBlock.Owner = GetTree().CurrentScene;
				}
				// ------------------------------------
			}
		}
		
		GD.Print($"[GridGenerator] Успішно згенеровано сітку {GridWidth}x{GridDepth}");
	}
}
