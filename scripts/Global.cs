using Godot;

public partial class Global : Node
{
    // Тут ми будемо зберігати шлях до обраного рівня
    public static string SelectedLevelPath { get; set; } = "";
}