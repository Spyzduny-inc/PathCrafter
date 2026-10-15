using Godot;
using System;

public partial class SettingsMenu : CanvasLayer
{
    [Signal]
    public delegate void SettingsClosedEventHandler();

    [Export] public HSlider MasterSlider { get; set; }
    [Export] public Label MasterValueLabel { get; set; }

    [Export] public HSlider MusicSlider { get; set; }
    [Export] public Label MusicValueLabel { get; set; }

    [Export] public HSlider SfxSlider { get; set; }
    [Export] public Label SfxValueLabel { get; set; }

    [Export] public HSlider EngineSlider { get; set; }
    [Export] public Label EngineValueLabel { get; set; }

    [Export] public CheckBox CameraHelpCheckBox { get; set; }

    [Export] public Button TestSfxButton { get; set; }
    [Export] public Button TestEngineButton { get; set; }
    [Export] public Button ResetDefaultsButton { get; set; }
    [Export] public Button CloseButton { get; set; }
    [Export] public Button CloseIconButton { get; set; }

    private bool _isUpdatingUi = false;

    public override void _Ready()
    {
        // Прив'язка вузлів, якщо не призначені через інспектор
        MasterSlider ??= FindChild("MasterSlider", true, false) as HSlider;
        MasterValueLabel ??= FindChild("MasterValueLabel", true, false) as Label;

        MusicSlider ??= FindChild("MusicSlider", true, false) as HSlider;
        MusicValueLabel ??= FindChild("MusicValueLabel", true, false) as Label;

        SfxSlider ??= FindChild("SfxSlider", true, false) as HSlider;
        SfxValueLabel ??= FindChild("SfxValueLabel", true, false) as Label;

        EngineSlider ??= FindChild("EngineSlider", true, false) as HSlider;
        EngineValueLabel ??= FindChild("EngineValueLabel", true, false) as Label;

        CameraHelpCheckBox ??= FindChild("CameraHelpCheckBox", true, false) as CheckBox;

        TestSfxButton ??= FindChild("TestSfxButton", true, false) as Button;
        TestEngineButton ??= FindChild("TestEngineButton", true, false) as Button;
        ResetDefaultsButton ??= FindChild("ResetDefaultsButton", true, false) as Button;
        CloseButton ??= FindChild("CloseButton", true, false) as Button;
        CloseIconButton ??= FindChild("CloseIconButton", true, false) as Button;

        // Підключення подій повзунків
        if (MasterSlider != null)
        {
            MasterSlider.FocusMode = Control.FocusModeEnum.None;
            MasterSlider.ValueChanged += OnMasterSliderChanged;
        }

        if (MusicSlider != null)
        {
            MusicSlider.FocusMode = Control.FocusModeEnum.None;
            MusicSlider.ValueChanged += OnMusicSliderChanged;
        }

        if (SfxSlider != null)
        {
            SfxSlider.FocusMode = Control.FocusModeEnum.None;
            SfxSlider.ValueChanged += OnSfxSliderChanged;
            SfxSlider.DragEnded += (valueChanged) => { if (valueChanged) Audio.Instance?.PlaySfx("click"); };
        }

        if (EngineSlider != null)
        {
            EngineSlider.FocusMode = Control.FocusModeEnum.None;
            EngineSlider.ValueChanged += OnEngineSliderChanged;
            EngineSlider.DragEnded += (valueChanged) => { if (valueChanged) Audio.Instance?.PlayEngineTest(); };
        }

        // Підключення чекбокса
        if (CameraHelpCheckBox != null)
        {
            CameraHelpCheckBox.FocusMode = Control.FocusModeEnum.None;
            CameraHelpCheckBox.Toggled += OnCameraHelpToggled;
        }

        // Кнопки тестування звуків
        if (TestSfxButton != null)
        {
            TestSfxButton.FocusMode = Control.FocusModeEnum.None;
            TestSfxButton.Pressed += () => Audio.Instance?.PlaySfx("win");
        }

        if (TestEngineButton != null)
        {
            TestEngineButton.FocusMode = Control.FocusModeEnum.None;
            TestEngineButton.Pressed += () => Audio.Instance?.PlayEngineTest();
        }

        // Кнопки скидання та закриття
        if (ResetDefaultsButton != null)
        {
            ResetDefaultsButton.FocusMode = Control.FocusModeEnum.None;
            ResetDefaultsButton.Pressed += ResetToDefaults;
        }

        if (CloseButton != null)
        {
            CloseButton.FocusMode = Control.FocusModeEnum.None;
            CloseButton.Pressed += Close;
        }

        if (CloseIconButton != null)
        {
            CloseIconButton.FocusMode = Control.FocusModeEnum.None;
            CloseIconButton.Pressed += Close;
        }

        // Ініціалізація значень UI з глобального стану
        LoadValuesToUi();
    }

    private void LoadValuesToUi()
    {
        _isUpdatingUi = true;

        if (MasterSlider != null)
        {
            int val = (int)Math.Round(Mathf.Clamp(Global.MasterVolume, 0f, 1f) * 100);
            MasterSlider.Value = val;
            if (MasterValueLabel != null) MasterValueLabel.Text = $"{val}%";
        }

        if (MusicSlider != null)
        {
            int val = (int)Math.Round(Mathf.Clamp(Global.MusicVolume, 0f, 1f) * 100);
            MusicSlider.Value = val;
            if (MusicValueLabel != null) MusicValueLabel.Text = $"{val}%";
        }

        if (SfxSlider != null)
        {
            int val = (int)Math.Round(Mathf.Clamp(Global.SfxVolume, 0f, 1f) * 100);
            SfxSlider.Value = val;
            if (SfxValueLabel != null) SfxValueLabel.Text = $"{val}%";
        }

        if (EngineSlider != null)
        {
            int val = (int)Math.Round(Mathf.Clamp(Global.EngineVolume, 0f, 1f) * 100);
            EngineSlider.Value = val;
            if (EngineValueLabel != null) EngineValueLabel.Text = $"{val}%";
        }

        if (CameraHelpCheckBox != null)
        {
            CameraHelpCheckBox.ButtonPressed = Global.ShowCameraHelp;
        }

        _isUpdatingUi = false;
    }

    private void OnMasterSliderChanged(double value)
    {
        if (_isUpdatingUi) return;
        Global.MasterVolume = (float)(value / 100.0);
        if (MasterValueLabel != null) MasterValueLabel.Text = $"{(int)value}%";
        Audio.Instance?.ApplySettings();
    }

    private void OnMusicSliderChanged(double value)
    {
        if (_isUpdatingUi) return;
        Global.MusicVolume = (float)(value / 100.0);
        if (MusicValueLabel != null) MusicValueLabel.Text = $"{(int)value}%";
        Audio.Instance?.ApplySettings();
    }

    private void OnSfxSliderChanged(double value)
    {
        if (_isUpdatingUi) return;
        Global.SfxVolume = (float)(value / 100.0);
        if (SfxValueLabel != null) SfxValueLabel.Text = $"{(int)value}%";
        Audio.Instance?.ApplySettings();
    }

    private void OnEngineSliderChanged(double value)
    {
        if (_isUpdatingUi) return;
        Global.EngineVolume = (float)(value / 100.0);
        if (EngineValueLabel != null) EngineValueLabel.Text = $"{(int)value}%";
        Audio.Instance?.ApplySettings();
    }

    private void OnCameraHelpToggled(bool buttonPressed)
    {
        if (_isUpdatingUi) return;
        Global.ShowCameraHelp = buttonPressed;
    }

    public void ResetToDefaults()
    {
        Audio.Instance?.PlaySfx("click");

        Global.MasterVolume = 1.0f;
        Global.MusicVolume = 0.8f;
        Global.SfxVolume = 0.8f;
        Global.EngineVolume = 0.8f;
        Global.ShowCameraHelp = true;

        LoadValuesToUi();
        Audio.Instance?.ApplySettings();
        Global.SaveToDisk();
    }

    public void Close()
    {
        Global.SaveToDisk();
        EmitSignal(SignalName.SettingsClosed);

        // Плавна анімація зникнення
        var panel = FindChild("PanelContainer", true, false) as Control;
        var backdrop = FindChild("DimBackdrop", true, false) as Control;

        if (panel != null)
        {
            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(panel, "modulate:a", 0.0f, 0.15f);
            if (backdrop != null) tween.TweenProperty(backdrop, "modulate:a", 0.0f, 0.15f);
            tween.Chain().TweenCallback(Callable.From(QueueFree));
        }
        else
        {
            QueueFree();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.Escape)
            {
                GetViewport().SetInputAsHandled();
                Close();
            }
        }
    }
}
