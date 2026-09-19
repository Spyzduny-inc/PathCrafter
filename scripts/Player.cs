using Godot;
using System.Threading.Tasks;

public partial class Player : CharacterBody3D
{
    public enum MoveResult { Success, HitWall, FellInPit }

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
    
    // Сюди Game.cs передає корінь рівня
    public Node3D LevelRoot { get; set; }

    public void SaveStartPosition()
    {
        _startPosition = GlobalPosition;
        _startRotationY = Rotation.Y;
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

        bool hitRock = false;
        bool hitPit = false;
        bool hasFloor = false;

        // Запускаємо глибинне рекурсивне сканування рівня
        if (LevelRoot != null)
        {
            Vector2 targetPos2D = new Vector2(targetPosition.X, targetPosition.Z);
            ScanCellForModules(LevelRoot, targetPos2D, ref hasFloor, ref hitRock, ref hitPit);
        }

        // Логіка результатів кроку
        if (hitRock)
        {
            return MoveResult.HitWall;
        }
        
        if (hitPit || !hasFloor)
        {
            var fallTween = CreateTween();
            fallTween.TweenProperty(this, "position", targetPosition + new Vector3(0, -2.0f, 0), 0.4f);
            await ToSignal(fallTween, Tween.SignalName.Finished);
            return MoveResult.FellInPit;
        }

        // Плавний успішний крок
        var tween = CreateTween();
        tween.TweenProperty(this, "position", targetPosition, 0.3f);
        await ToSignal(tween, Tween.SignalName.Finished);
        
        return MoveResult.Success;
    }

    // Той самий рекурсивний метод, якого не вистачало. Він шукає всюди.
    private void ScanCellForModules(Node currentNode, Vector2 targetPos2D, ref bool hasFloor, ref bool hitRock, ref bool hitPit)
    {
        if (currentNode is Node3D block)
        {
            // Об'єднуємо шлях до префабу і поточне ім'я (щоб точно зловити Floor.tscn, Rock.tscn тощо)
            string identity = (block.SceneFilePath + " " + block.Name).ToLower();
            
            // Якщо це взагалі модуль з наших ассетів
            if (identity.Contains("floor") || identity.Contains("rock") || identity.Contains("pit") || identity.Contains("finish") || identity.Contains("spawn"))
            {
                // Перевіряємо, чи лежить він на цільовій координаті (з похибкою 0.2 для надійності)
                Vector2 blockPos2D = new Vector2(block.GlobalPosition.X, block.GlobalPosition.Z);
                if (blockPos2D.DistanceTo(targetPos2D) < 0.2f)
                {
                    if (identity.Contains("rock")) hitRock = true;
                    if (identity.Contains("pit")) hitPit = true;
                    if (identity.Contains("floor") || identity.Contains("finish") || identity.Contains("spawn")) hasFloor = true;
                }
            }
        }
        
        // Рекурсія: ліземо всередину кожного знайденого вузла
        foreach (Node child in currentNode.GetChildren())
        {
            ScanCellForModules(child, targetPos2D, ref hasFloor, ref hitRock, ref hitPit);
        }
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