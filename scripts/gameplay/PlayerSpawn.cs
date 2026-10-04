using Godot;

public partial class PlayerSpawn : Area3D
{
	public override void _Ready()
	{
		// Отримуємо поточну головну сцену
		Node currentScene = GetTree().CurrentScene;
		
		// Перевіряємо, чи ми зараз знаходимося в редакторі
		// (Припускаємо, що твоя сцена редактора називається LevelEditor)
		bool isEditor = currentScene != null && currentScene.Name.ToString().Contains("LevelEditor");

		// Якщо це вже ЗАПУЩЕНА ГРА (або тестовий рівень), а не редактор
		if (!isEditor)
		{
			// 1. Робимо об'єкт невидимим
			Visible = false;
			
			// 2. Вимикаємо колізію, щоб гравець не чіплявся за невидимий стовп
			// Використовуємо SetDeferred, це найбезпечніший спосіб вимкнути колізію в _Ready
			var col = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
			if (col != null)
			{
				col.SetDeferred("disabled", true);
			}
		}
	}
}
