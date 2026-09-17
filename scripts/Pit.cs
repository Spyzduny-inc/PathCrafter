using Godot;

public partial class Pit : Area3D
{
    public override void _Ready()
    {
        // Підписуємося на вбудований сигнал Ґодота "хтось увійшов у зону"
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        // Перевіряємо, чи той, хто впав у яму — це наш гравець
        if (body.Name == "Player")
        {
            GD.Print("Гравець впав у яму! Перезапуск...");
            GetTree().ReloadCurrentScene();
        }
    }
}