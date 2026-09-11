using Godot;

public partial class CameraController : Camera3D
{
    [Export] public float KeyboardPanSpeed { get; set; } = 15.0f;
    [Export] public float MousePanSpeed { get; set; } = 0.05f; // Чутливість перетягування мишкою
    [Export] public float ZoomSpeed { get; set; } = 2.0f;
    [Export] public float MinZoom { get; set; } = 3.0f;
    [Export] public float MaxZoom { get; set; } = 30.0f;

    private bool isPanningWithMouse = false;

    public override void _Process(double delta)
    {
        // 1. Рух з клавіатури (WASD або стрілочки)
        Vector3 panDirection = Vector3.Zero;

        if (Input.IsPhysicalKeyPressed(Key.W) || Input.IsPhysicalKeyPressed(Key.Up)) panDirection.Z -= 1;
        if (Input.IsPhysicalKeyPressed(Key.S) || Input.IsPhysicalKeyPressed(Key.Down)) panDirection.Z += 1;
        if (Input.IsPhysicalKeyPressed(Key.A) || Input.IsPhysicalKeyPressed(Key.Left)) panDirection.X -= 1;
        if (Input.IsPhysicalKeyPressed(Key.D) || Input.IsPhysicalKeyPressed(Key.Right)) panDirection.X += 1;

        if (panDirection != Vector3.Zero)
        {
            panDirection = panDirection.Normalized();
            // Рухаємо тільки по площині XZ (паралельно землі)
            GlobalPosition += new Vector3(panDirection.X, 0, panDirection.Z) * KeyboardPanSpeed * (float)delta;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // 2. Обробка кліків миші (Зум та затискання кнопки для панорамування)
        if (@event is InputEventMouseButton mouseBtnEvent)
        {
            // Зум на прокрутку коліщатка
            if (mouseBtnEvent.ButtonIndex == MouseButton.WheelUp && mouseBtnEvent.Pressed)
            {
                Zoom(-ZoomSpeed);
            }
            else if (mouseBtnEvent.ButtonIndex == MouseButton.WheelDown && mouseBtnEvent.Pressed)
            {
                Zoom(ZoomSpeed);
            }
            
            // Вмикаємо/вимикаємо режим перетягування (на затиснуте коліщатко або праву кнопку)
            if (mouseBtnEvent.ButtonIndex == MouseButton.Middle || mouseBtnEvent.ButtonIndex == MouseButton.Right)
            {
                isPanningWithMouse = mouseBtnEvent.Pressed;
            }
        }

        // 3. Рух мишею при затиснутій кнопці
        if (@event is InputEventMouseMotion mouseMotionEvent && isPanningWithMouse)
        {
            // Конвертуємо 2D рух миші по екрану у 3D рух камери по площині XZ.
            // Знаки мінус потрібні, щоб екран "тягнувся" за курсором.
            Vector3 dragMotion = new Vector3(-mouseMotionEvent.Relative.X, 0, -mouseMotionEvent.Relative.Y) * MousePanSpeed;
            GlobalPosition += dragMotion;
        }
    }

    private void Zoom(float amount)
    {
        // Рухаємо камеру по її власному вектору Z (вперед/назад під кутом -45)
        Vector3 newPos = Position + Transform.Basis.Z * amount;

        // Блокуємо вихід за межі мінімального та максимального наближення
        if (newPos.Y >= MinZoom && newPos.Y <= MaxZoom)
        {
            Position = newPos;
        }
    }
}