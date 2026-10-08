using Godot;
using System.Threading.Tasks;

public partial class Player : CharacterBody3D
{
    public enum MoveResult { Success, HitWall, FellInPit }

    // --- Окремі змінні для ВСІХ ЧОТИРЬОХ КОЛІС ---
    [Export] public Node3D FrontLeftWheel { get; set; }
    [Export] public Node3D BackLeftWheel { get; set; }
    [Export] public Node3D FrontRightWheel { get; set; }
    [Export] public Node3D BackRightWheel { get; set; }

    private int _currentDirection = 0; // 0 = вперед (-Z), 1 = праворуч (+X), 2 = назад (+Z), 3 = ліворуч (-X)

    private readonly Vector3[] _directions = new Vector3[]
    {
        new Vector3(0, 0, -1),
        new Vector3(1, 0,  0),
        new Vector3(0, 0,  1),
        new Vector3(-1, 0, 0)
    };

    private Vector3 _startPosition;
    private float _startRotationY;
    private int _startDirection = 0;

    public Node3D LevelRoot { get; set; }

    public void SaveStartPosition()
    {
        _startPosition = GlobalPosition;
        _startRotationY = Rotation.Y;

        float degrees = Mathf.PosMod(Mathf.RadToDeg(Rotation.Y), 360f);
        if (degrees >= 45f && degrees < 135f) _currentDirection = 3;       // 90 deg = left (-X)
        else if (degrees >= 135f && degrees < 225f) _currentDirection = 2;  // 180 deg = back (+Z)
        else if (degrees >= 225f && degrees < 315f) _currentDirection = 1;  // 270 deg = right (+X)
        else _currentDirection = 0;                                        // 0 deg = forward (-Z)

        _startDirection = _currentDirection;
    }

    public void ResetToStart()
    {
        GlobalPosition = _startPosition;

        Vector3 rot = Rotation;
        rot.Y = _startRotationY;
        Rotation = rot;

        _currentDirection = _startDirection;
    }

    public async Task<MoveResult> MoveForward()
    {
        Vector3 forwardDir = _directions[_currentDirection];
        Vector3 targetPosition = GlobalPosition + forwardDir;
        Vector2I targetGridPos = new Vector2I(Mathf.RoundToInt(targetPosition.X), Mathf.RoundToInt(targetPosition.Z));

        TileType tileAtTarget = GridManager.Instance != null
            ? GridManager.Instance.GetTileAt(targetGridPos)
            : TileType.Empty;

        if (tileAtTarget == TileType.Wall)
        {
            return MoveResult.HitWall;
        }

        if (tileAtTarget == TileType.Pit)
        {
            var fallTween = CreateTween();
            fallTween.TweenProperty(this, "position", targetPosition + new Vector3(0, -2.0f, 0), 0.4f);
            await ToSignal(fallTween, Tween.SignalName.Finished);
            return MoveResult.FellInPit;
        }

        if (GridManager.Instance == null || GridManager.Instance.IsWalkable(targetGridPos))
        {
            var tween = CreateTween();

            tween.SetParallel(true);
            tween.TweenProperty(this, "position", targetPosition, 0.3f);

            // Анімуємо всі чотири колеса (рух вперед)
            float rotAngle = Mathf.Pi;
            if (FrontLeftWheel != null) tween.TweenProperty(FrontLeftWheel, "rotation:x", FrontLeftWheel.Rotation.X - rotAngle, 0.3f);
            if (BackLeftWheel != null) tween.TweenProperty(BackLeftWheel, "rotation:x", BackLeftWheel.Rotation.X - rotAngle, 0.3f);
            if (FrontRightWheel != null) tween.TweenProperty(FrontRightWheel, "rotation:x", FrontRightWheel.Rotation.X - rotAngle, 0.3f);
            if (BackRightWheel != null) tween.TweenProperty(BackRightWheel, "rotation:x", BackRightWheel.Rotation.X - rotAngle, 0.3f);

            tween.SetParallel(false);

            await ToSignal(tween, Tween.SignalName.Finished);
            return MoveResult.Success;
        }

        return MoveResult.HitWall;
    }

    public async Task TurnRight()
    {
        _currentDirection = (_currentDirection + 1) % 4;
        // Праворуч: ліві колеса крутяться вперед (-), праві назад (+)
        await RotateSmoothly(-Mathf.DegToRad(90), -Mathf.Pi, Mathf.Pi);
    }

    public async Task TurnLeft()
    {
        _currentDirection = (_currentDirection + 3) % 4;
        // Ліворуч: ліві колеса крутяться назад (+), праві вперед (-)
        await RotateSmoothly(Mathf.DegToRad(90), Mathf.Pi, -Mathf.Pi);
    }

    private async Task RotateSmoothly(float targetAngleDelta, float leftWheelSpin, float rightWheelSpin)
    {
        const float duration = 0.2f;

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(this, "rotation:y", Rotation.Y + targetAngleDelta, duration);

        // Колеса лівого та правого борту крутяться в різні боки
        if (FrontLeftWheel != null) tween.TweenProperty(FrontLeftWheel, "rotation:x", FrontLeftWheel.Rotation.X + leftWheelSpin, duration);
        if (BackLeftWheel != null) tween.TweenProperty(BackLeftWheel, "rotation:x", BackLeftWheel.Rotation.X + leftWheelSpin, duration);
        if (FrontRightWheel != null) tween.TweenProperty(FrontRightWheel, "rotation:x", FrontRightWheel.Rotation.X + rightWheelSpin, duration);
        if (BackRightWheel != null) tween.TweenProperty(BackRightWheel, "rotation:x", BackRightWheel.Rotation.X + rightWheelSpin, duration);

        await ToSignal(tween, Tween.SignalName.Finished);
    }
}