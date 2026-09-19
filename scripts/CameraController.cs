using Godot;

public partial class CameraController : Camera3D
{
    [Export] public float KeyboardPanSpeed { get; set; } = 7.0f;
    [Export] public float MousePanSpeed { get; set; } = 0.05f; 
    [Export] public float ZoomSpeed { get; set; } = 2.0f;
    [Export] public float MinZoom { get; set; } = 3.0f;
    [Export] public float MaxZoom { get; set; } = 30.0f;

	private bool isPanningWithMouse = false;

    public override void _Process(double delta)
    {
        // Перевіряємо, чи не пише зараз гравець код (блокуємо стрілочки)
        Control focusOwner = GetViewport().GuiGetFocusOwner();
        if (focusOwner is TextEdit || focusOwner is LineEdit) 
            return;

        // 1. Рух з клавіатури
        Vector3 panDirection = Vector3.Zero;

		if (Input.IsPhysicalKeyPressed(Key.Up)) panDirection.Z -= 1;
		if (Input.IsPhysicalKeyPressed(Key.Down)) panDirection.Z += 1;
		if (Input.IsPhysicalKeyPressed(Key.Left)) panDirection.X -= 1;
		if (Input.IsPhysicalKeyPressed(Key.Right)) panDirection.X += 1;

        if (panDirection != Vector3.Zero)
        {
            panDirection = panDirection.Normalized();
            GlobalPosition += new Vector3(panDirection.X, 0, panDirection.Z) * KeyboardPanSpeed * (float)delta;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Якщо клікнули лівою кнопкою миші по порожньому місцю (не по UI) — знімаємо фокус з коду
        if (@event is InputEventMouseButton clickEvent && clickEvent.ButtonIndex == MouseButton.Left && clickEvent.Pressed)
        {
            GetViewport().GuiReleaseFocus();
        }

        // Блокуємо зум і перетягування мишею, якщо гравець пише код
        Control focusOwner = GetViewport().GuiGetFocusOwner();
        if (focusOwner is TextEdit || focusOwner is LineEdit) 
            return;

        // 2. Обробка кліків миші (Зум та затискання кнопки для панорамування)
        if (@event is InputEventMouseButton mouseBtnEvent)
        {
            if (mouseBtnEvent.ButtonIndex == MouseButton.WheelUp && mouseBtnEvent.Pressed)
            {
                Zoom(-ZoomSpeed);
            }
            else if (mouseBtnEvent.ButtonIndex == MouseButton.WheelDown && mouseBtnEvent.Pressed)
            {
                Zoom(ZoomSpeed);
            }
            
            if (mouseBtnEvent.ButtonIndex == MouseButton.Middle)
            {
                isPanningWithMouse = mouseBtnEvent.Pressed;
            }
        }

        // 3. Рух мишею при затиснутій середній кнопці
        if (@event is InputEventMouseMotion mouseMotionEvent && isPanningWithMouse)
        {
            Vector3 dragMotion = new Vector3(-mouseMotionEvent.Relative.X, 0, -mouseMotionEvent.Relative.Y) * MousePanSpeed;
            GlobalPosition += dragMotion;
        }
    }

    private void Zoom(float amount)
    {
        Vector3 newPos = Position + Transform.Basis.Z * amount;

        if (newPos.Y >= MinZoom && newPos.Y <= MaxZoom)
        {
            Position = newPos;
        }
    }
}
