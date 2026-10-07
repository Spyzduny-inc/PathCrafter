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
			Audio.Instance.StopRide();
			Audio.Instance.PlaySfx("crash");
			return MoveResult.HitWall;
		}

		if (tileAtTarget == TileType.Pit)
		{
			Audio.Instance.StopRide();
			Audio.Instance.PlaySfx("fall");
			var fallTween = CreateTween();
			fallTween.TweenProperty(this, "position", targetPosition + new Vector3(0, -2.0f, 0), 0.4f);
			await ToSignal(fallTween, Tween.SignalName.Finished);
			return MoveResult.FellInPit;
		}

		if (GridManager.Instance == null || GridManager.Instance.IsWalkable(targetGridPos))
		{
			var tween = CreateTween();
			tween.TweenProperty(this, "position", targetPosition, 0.3f);
			await ToSignal(tween, Tween.SignalName.Finished);
			return MoveResult.Success;
		}

		Audio.Instance.StopRide();
		Audio.Instance.PlaySfx("crash");
		return MoveResult.HitWall;
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
