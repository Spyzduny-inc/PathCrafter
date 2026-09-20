using Godot;

public partial class CameraHelpPanel : PanelContainer
{
    private Label _helpLabel;
    private Button _toggleButton;

    public override void _Ready()
    {
        // Знаходимо дочірні ноди по імені (або через шляхи)
        _toggleButton = GetNode<Button>("MarginContainer/VBoxContainer/ToggleHelpBtn");
        _helpLabel = GetNode<Label>("MarginContainer/VBoxContainer/Label");

        // Відновлюємо збережений стан із глобальної пам'яті
        ApplyState(Global.ShowCameraHelp);

        // Підписуємося на клік
        if (_toggleButton != null)
        {
            _toggleButton.FocusMode = Control.FocusModeEnum.None;
            _toggleButton.Pressed += OnTogglePressed;
        }
    }

    private void OnTogglePressed()
    {
        bool newState = !_helpLabel.Visible;
        Global.ShowCameraHelp = newState; // Зберігаємо у Global
        ApplyState(newState);
    }

    private void ApplyState(bool isOpen)
    {
        if (_helpLabel != null) _helpLabel.Visible = isOpen;
        if (_toggleButton != null) 
        {
            _toggleButton.Text = isOpen ? "▲ Сховати підказки" : "▼ Підказки камери";
        }
    }
}