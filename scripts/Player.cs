using Godot;
using System.Threading.Tasks;

public partial class Player : CharacterBody3D
{
    private int _currentDirection = 0; // 0 = вперед (-Z), 1 = праворуч (+X), 2 = назад (+Z), 3 = ліворуч (-X)
    
    private readonly Vector3[] _directions = new Vector3[]
    {
        new Vector3(0, 0, -1),
        new Vector3(1, 0,  0),
        new Vector3(0, 0,  1),
        new Vector3(-1, 0, 0)
    };

    public async Task<bool> MoveForward()
    {
        Vector3 forwardDir = _directions[_currentDirection];
        Vector3 targetPosition = GlobalPosition + forwardDir;

        // Плавний крок за 0.3 секунди
        var tween = CreateTween();
        tween.TweenProperty(this, "position", targetPosition, 0.3f);
        
        await ToSignal(tween, Tween.SignalName.Finished);
        return true;
    }

    public async Task TurnRight()
    {
        _currentDirection = (_currentDirection + 1) % 4;
        await RotateSmoothly(-Mathf.DegToRad(90));
    }

    public async Task TurnLeft()
    {
        _currentDirection = (_currentDirection + 3) % 4;
        await RotateSmoothly(Mathf.DegToRad(90));
    }

    private async Task RotateSmoothly(float targetAngleDelta)
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "rotation:y", Rotation.Y + targetAngleDelta, 0.2f);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}