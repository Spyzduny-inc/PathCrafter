using Godot;
using System.Collections.Generic;

public partial class Audio : Node
{
	public static Audio Instance { get; private set; }

	private AudioStreamPlayer _musicA = new();
	private AudioStreamPlayer _musicB = new();
	private AudioStreamPlayer _activePlayer;
	private AudioStreamPlayer _standbyPlayer;
	private Tween _musicTween;

	private readonly AudioStreamPlayer _ride = new();
	private readonly Dictionary<string, AudioStream> _sfx = new();
	private string _currentMusic = "";

	private const string SoundsPath = "res://assets/sounds/";
	private const float DefaultMusicVolumeDb = -10.0f;
	private const float FadeSilenceDb = -50.0f;

	// Збалансована гучність музичних треків (select гучніший за оригіналом, тому пом'якшуємо його)
	private readonly Dictionary<string, float> _musicVolume = new()
	{
		{ "menu",     -9.0f },
		{ "select",  -14.5f },
		{ "on_play", -10.0f },
	};

	// Гучність кожного ефекту в дБ: 0 = оригінал, менше число = тихіше
	private readonly Dictionary<string, float> _sfxVolume = new()
	{
		{ "click", -15f },
		{ "stop",  -12f },
		{ "crash",   0f },
		{ "fall",    0f },
		{ "win",     0f },
	};

	public override void _Ready()
	{
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;

		foreach (var name in _sfxVolume.Keys)
			_sfx[name] = GD.Load<AudioStream>($"{SoundsPath}{name}.mp3");

		_musicA.VolumeDb = FadeSilenceDb;
		_musicB.VolumeDb = FadeSilenceDb;
		AddChild(_musicA);
		AddChild(_musicB);

		_activePlayer = _musicA;
		_standbyPlayer = _musicB;

		var ride = GD.Load<AudioStreamMP3>($"{SoundsPath}ride.mp3");
		ride.Loop = true;
		_ride.Stream = ride;
		_ride.VolumeDb = 6;
		AddChild(_ride);

		ApplySettings();

		// клік на всіх кнопках в усіх сценах
		GetTree().NodeAdded += node =>
		{
			if (node is BaseButton b) b.Pressed += () => PlaySfx("click");
		};
	}

	public void ApplySettings()
	{
		// 1. Загальна гучність (Master Bus)
		int masterBus = AudioServer.GetBusIndex("Master");
		if (masterBus >= 0)
		{
			if (Global.MasterVolume <= 0.001f)
			{
				AudioServer.SetBusMute(masterBus, true);
			}
			else
			{
				AudioServer.SetBusMute(masterBus, false);
				AudioServer.SetBusVolumeDb(masterBus, Mathf.LinearToDb(Global.MasterVolume));
			}
		}

		// 2. Гучність поточної фонової музики
		if (_activePlayer != null && _activePlayer.Playing && !string.IsNullOrEmpty(_currentMusic))
		{
			float baseVol = _musicVolume.GetValueOrDefault(_currentMusic, DefaultMusicVolumeDb);
			_activePlayer.VolumeDb = baseVol + GetMusicOffsetDb();
		}

		// 3. Гучність звуку двигуна марсохода
		float engineOffset = Global.EngineVolume <= 0.001f ? FadeSilenceDb : Mathf.LinearToDb(Global.EngineVolume);
		_ride.VolumeDb = 6.0f + engineOffset;
	}

	private float GetMusicOffsetDb()
	{
		return Global.MusicVolume <= 0.001f ? FadeSilenceDb : Mathf.LinearToDb(Global.MusicVolume);
	}

	// "menu", "select" або "on_play" з ніжним, м'яким crossfade переходом
	public void PlayMusic(string name, float fadeDuration = 1.3f)
	{
		if (_currentMusic == name && _activePlayer != null && _activePlayer.Playing) return;
		_currentMusic = name;

		string soundFile = $"{SoundsPath}{name}.mp3";
		if (!ResourceLoader.Exists(soundFile)) return;

		var stream = GD.Load<AudioStreamMP3>(soundFile);
		if (stream == null) return;
		stream.Loop = true;

		float targetVolume = _musicVolume.GetValueOrDefault(name, DefaultMusicVolumeDb) + GetMusicOffsetDb();

		// Якщо зараз ніяка музика не грає — плавно запускаємо активний плеєр
		if (_activePlayer == null || !_activePlayer.Playing)
		{
			_activePlayer ??= _musicA;
			_activePlayer.Stream = stream;
			_activePlayer.VolumeDb = FadeSilenceDb;
			_activePlayer.Play();

			_musicTween?.Kill();
			_musicTween = CreateTween();
			_musicTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_musicTween.TweenProperty(_activePlayer, "volume_db", targetVolume, fadeDuration);
			return;
		}

		// Ніжний та логічний кросфейд (Crossfade):
		// Старий трек плавно затихає, а новий м'яко випливає з тиші
		var oldPlayer = _activePlayer;
		var newPlayer = (_activePlayer == _musicA) ? _musicB : _musicA;

		newPlayer.Stream = stream;
		newPlayer.VolumeDb = FadeSilenceDb;
		newPlayer.Play();

		_musicTween?.Kill();
		_musicTween = CreateTween();
		_musicTween.SetParallel(true);

		// Стара музика делікатно згасає
		_musicTween.TweenProperty(oldPlayer, "volume_db", FadeSilenceDb, fadeDuration * 0.7f)
			.SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Sine);

		// Нова музика м'яко і плавно наростає за кривою Cubic
		_musicTween.TweenProperty(newPlayer, "volume_db", targetVolume, fadeDuration)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);

		_musicTween.Chain().TweenCallback(Callable.From(() =>
		{
			oldPlayer.Stop();
		}));

		_activePlayer = newPlayer;
		_standbyPlayer = oldPlayer;
	}

	public void PlaySfx(string name)
	{
		if (Global.SfxVolume <= 0.001f) return;
		if (!_sfx.TryGetValue(name, out var stream)) return;
		
		float sfxOffset = Mathf.LinearToDb(Global.SfxVolume);
		var p = new AudioStreamPlayer
		{
			Stream = stream,
			VolumeDb = _sfxVolume.GetValueOrDefault(name, 0f) + sfxOffset
		};
		AddChild(p);
		p.Finished += p.QueueFree;
		p.Play();
	}

	public void StartRide()
	{
		float engineOffset = Global.EngineVolume <= 0.001f ? FadeSilenceDb : Mathf.LinearToDb(Global.EngineVolume);
		_ride.VolumeDb = 6.0f + engineOffset;
		if (!_ride.Playing) _ride.Play();
	}

	public void StopRide()  { _ride.Stop(); }

	public void PlayEngineTest()
	{
		StartRide();
		var timer = GetTree().CreateTimer(0.8);
		timer.Timeout += StopRide;
	}
}
