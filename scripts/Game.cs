using Godot;

public partial class Game : Node3D
{
    // Сюди в Інспекторі перетягнемо сцену гравця
    [Export] public PackedScene PlayerScene { get; set; }
    
    public override void _Ready()
    {
        if (string.IsNullOrEmpty(Global.SelectedLevelPath))
        {
            GD.PrintErr("[Game] Шлях до рівня порожній!");
            return;
        }

        // 1. Завантажуємо рівень як дочірній об'єкт
        var levelScene = GD.Load<PackedScene>(Global.SelectedLevelPath);
        if (levelScene != null)
        {
            var levelInstance = levelScene.Instantiate<Node3D>();
            AddChild(levelInstance);
            GD.Print($"[Game] Рівень завантажено: {Global.SelectedLevelPath}");

            // 2. Шукаємо точку спавну і ставимо гравця
            SpawnPlayer(levelInstance);
        }
    }

    private void SpawnPlayer(Node3D levelInstance)
    {
        if (PlayerScene == null) 
        {
            GD.PrintErr("[Game] Сцена гравця не призначена в Інспекторі!");
            return;
        }

        // Шукаємо об'єкт з міткою "is_player_spawn", яку ми ставили в редакторі
        Node3D spawnPoint = FindSpawnPoint(levelInstance);
        
        if (spawnPoint != null)
        {
            var player = PlayerScene.Instantiate<Node3D>();
            // Ставимо гравця рівно на координати невидимої капсули
            player.Position = spawnPoint.GlobalPosition;
            AddChild(player);
            GD.Print("[Game] Гравця успішно заспавнено!");
        }
        else
        {
            GD.PrintErr("[Game] Точку спавну не знайдено на рівні!");
        }
    }

    // Рекурсивний пошук точки спавну серед усіх блоків рівня
    private Node3D FindSpawnPoint(Node node)
    {
        if (node is Node3D node3d && node3d.HasMeta("is_player_spawn") && node3d.GetMeta("is_player_spawn").AsBool())
        {
            return node3d;
        }

        foreach (Node child in node.GetChildren())
        {
            var result = FindSpawnPoint(child);
            if (result != null) return result;
        }

        return null;
    }
}