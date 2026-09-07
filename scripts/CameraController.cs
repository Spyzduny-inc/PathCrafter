using Godot;

public partial class CameraController : Camera3D
{
    private Node3D Target;

    // Відстань від гравця (можеш міняти в інспекторі)
    [Export]
    public Vector3 Offset { get; set; } = new Vector3(0, 4, 12);

    [Export]
    public float FollowSpeed { get; set; } = 5.0f;

    public override void _Ready()
    {
        Target = GetParent<Node3D>();
        TopLevel = true; 
        
        // Миттєво ставимо камеру на стартову позицію над гравцем
        if (Target != null) 
        {
            Position = Target.Position + Offset;
        }
    }

    public override void _Process(double delta)
    {
        if (Target != null)
        {
            // Плавно летимо за гравцем із заданим відступом
            Vector3 targetPosition = Target.Position + Offset;
            Position = Position.Lerp(targetPosition, FollowSpeed * (float)delta);
        }
    }
}