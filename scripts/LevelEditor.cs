using Godot;
using System.Collections.Generic;

public partial class LevelEditor : Control
{
	private SpinBox widthSpinBox;
	private SpinBox heightSpinBox;
	private Button generateButton;
	
	private Button wallButton;
	private Button finishButton;

	// Посилання на генератор сітки
	private GridGenerator gridGenerator;

	private Dictionary<Vector2I, int> levelData = new Dictionary<Vector2I, int>();
	private int currentSelectedTile = 2; // 2 - Стіна, 3 - Фініш

	public override void _Ready()
	{
		// Прив'язка UI елементів з правильними шляхами
		widthSpinBox = GetNode<SpinBox>("UI/TopBar/WidthSpinBox");
		heightSpinBox = GetNode<SpinBox>("UI/TopBar/HeightSpinBox");
		generateButton = GetNode<Button>("UI/TopBar/GenerateButton");

		wallButton = GetNode<Button>("UI/RightPanel/WallButton");
		finishButton = GetNode<Button>("UI/RightPanel/FinishButton");

		// Шукаємо GridGenerator у 3D-просторі сцени
		gridGenerator = GetNode<GridGenerator>("SubViewportContainer/SubViewport/GridGenerator");

		generateButton.Pressed += OnGenerateButtonPressed;
		wallButton.Pressed += () => currentSelectedTile = 2;
		finishButton.Pressed += () => currentSelectedTile = 3;
	}

	private void OnGenerateButtonPressed()
	{
		int mapWidth = (int)widthSpinBox.Value;
		int mapHeight = (int)heightSpinBox.Value;

		GD.Print($"Генеруємо сітку: {mapWidth}x{mapHeight}");
		
		levelData.Clear();

		if (gridGenerator != null)
		{
			gridGenerator.GridWidth = mapWidth;
			gridGenerator.GridDepth = mapHeight;
			
			// Викликаємо генерацію підлоги
			gridGenerator.GenerateGrid();
		}
		else
		{
			GD.PrintErr("Помилка: GridGenerator не знайдено за вказаним шляхом!");
		}
	}
}
