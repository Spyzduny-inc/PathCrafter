using Godot;

public partial class RandomRotation : Node3D 
{
    public override void _Ready()
    {
        // Повний рандом від 0 до 360 градусів (у радіанах Mathf.Tau - це 2 * Pi)
        Rotation = new Vector3(Rotation.X, (float)GD.RandRange(0, Mathf.Tau), Rotation.Z);
    }
}