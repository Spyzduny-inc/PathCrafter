using Godot;
using System.Collections.Generic;

public partial class Global : Node
{
    // Шлях до обраного поточного рівня
    public static string SelectedLevelPath { get; set; } = "";
    public static bool ShowCameraHelp { get; set; } = true; // За замовчуванням увімкнено

    // Система прогресу рівнів (на старті доступний лише 0-й рівень)
    public static int UnlockedLevelIndex { get; set; } = 0;

    public override void _Ready()
    {
        // Примусово глушимо систему доступності Linux для цього процесу, 
        // щоб accesskit_consumer не панікував при видаленні UI-вузлів
        System.Environment.SetEnvironmentVariable("AT_SPI_BUS_ADDRESS", "");
    }

    // Метод для розблокування наступного рівня (викликається при перемозі)
    public static void UnlockNextLevel(int completedLevelIndex)
    {
        if (completedLevelIndex >= UnlockedLevelIndex)
        {
            UnlockedLevelIndex = completedLevelIndex + 1;
            GD.Print($"[Global] Рівень пройдено! Розблоковано наступний індекс: {UnlockedLevelIndex}");
        }
    }
}