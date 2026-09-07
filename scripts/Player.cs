using Godot;

public partial class Player : CharacterBody3D
{
	[Export]
	public float Speed { get; set; } = 5.0f;

	[Export]
	public float JumpVelocity { get; set; } = 4.5f;

	// Отримуємо гравітацію з налаштувань проекту
	private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// 1. Додаємо гравітацію
		if (!IsOnFloor())
		{
			velocity.Y -= gravity * (float)delta;
		}

		// 2. Обробка стрибка
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// 3. Отримуємо напрямок руху по осях (2.5D фіксація по X)
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		if (direction != Vector3.Zero)
		{
   			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
