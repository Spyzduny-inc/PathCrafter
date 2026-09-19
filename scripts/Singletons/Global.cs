using Godot;

public partial class Global : Node
{
    // Тут ми будемо зберігати шлях до обраного рівня
    public static string SelectedLevelPath { get; set; } = "";
    public static bool ShowCameraHelp { get; set; } = true; // За замовчуванням увімкнено
    
    public override void _Ready()
    {
        // Примусово глушимо систему доступності Linux для цього процесу, 
        // щоб accesskit_consumer не панікував при видаленні UI-вузлів
        System.Environment.SetEnvironmentVariable("AT_SPI_BUS_ADDRESS", "");
    }
}