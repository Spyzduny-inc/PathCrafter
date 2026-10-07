using Godot;
using System.Collections.Generic;

public partial class Audio : Node
{
	public static Audio Instance { get; private set; }

	private readonly AudioStreamPlayer _music = new();
	private readonly AudioStreamPlayer _ride = new();
	private readonly Dictionary<string, AudioStream> _sfx = new();
	private string _currentMusic = "";

	private const string SoundsPath = "res://assets/sounds/";

	public override void _Ready()
	{
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;

		foreach (var name in new[] { "click", "stop", "crash", "fall" })
			_sfx[name] = GD.Load<AudioStream>($"{SoundsPath}{name}.mp3");

		_music.VolumeDb = -8;
		AddChild(_music);

		var ride = GD.Load<AudioStreamMP3>($"{SoundsPath}ride.mp3");
		ride.Loop = true;
		_ride.Stream = ride;
		_ride.VolumeDb = 8;
		AddChild(_ride);

		// клік на всіх кнопках в усіх сценах
		GetTree().NodeAdded += node =>
		{
			if (node is BaseButton b) b.Pressed += () => PlaySfx("click");
		};
	}

	// "menu", "select" або "on_play"
	public void PlayMusic(string name)
	{
		if (_currentMusic == name && _music.Playing) return;
		_currentMusic = name;

		var stream = GD.Load<AudioStreamMP3>($"{SoundsPath}{name}.mp3");
		stream.Loop = true;
		_music.Stream = stream;
		_music.Play();
	}

	public void PlaySfx(string name)
	{
		if (!_sfx.TryGetValue(name, out var stream)) return;
		var p = new AudioStreamPlayer { Stream = stream };
		AddChild(p);
		p.Finished += p.QueueFree;
		p.Play();
	}

	public void StartRide() { if (!_ride.Playing) _ride.Play(); }
	public void StopRide()  { _ride.Stop(); }
}
