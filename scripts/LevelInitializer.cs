using Godot;

public partial class LevelInitializer : Node3D
{
    [Export] public PackedScene SkyScene { get; set; }

    public override void _Ready()
    {
        // Якщо на рівні ще немає неба, додаємо його
        if (!HasNode("SkyEnvironment"))
        {
            if (SkyScene == null)
            {
                SkyScene = GD.Load<PackedScene>("res://scenes/ui/sky.tscn");
            }

            if (SkyScene != null)
            {
                var skyInstance = SkyScene.Instantiate();
                skyInstance.Name = "SkyEnvironment";
                AddChild(skyInstance);
                GD.Print("[LevelInitializer] Небо успішно завантажено на рівень!");
            }
        }
    }
}