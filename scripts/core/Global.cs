using Godot;
using System.Collections.Generic;

public partial class Global : Node
{
    // Шлях до обраного поточного рівня
    public static string SelectedLevelPath { get; set; } = "";
    public static bool ShowCameraHelp { get; set; } = true; // За замовчуванням увімкнено

    // Система прогресу рівнів (на старті доступний лише 0-й рівень)
    public static int UnlockedLevelIndex { get; set; } = 0;

    // Збережена позиція панелі документації
    public static Vector2? SavedDocsPosition { get; set; } = null;

    // Збережений код гравця для кожного рівня (LevelPath -> Code)
    public static Dictionary<string, string> SavedLevelCodes { get; set; } = new Dictionary<string, string>();

    // Базовий шаблон C++ коду за замовчуванням
    public const string DefaultCodeTemplate = @"#include <moving>

int main() {
    rover.move();
    return 0;
}";

    private const string SavePath = "user://save_game.cfg";

    public override void _Ready()
    {
        // Примусово глушимо систему доступності Linux для цього процесу, 
        // щоб accesskit_consumer не панікував при видаленні UI-вузлів
        System.Environment.SetEnvironmentVariable("AT_SPI_BUS_ADDRESS", "");

        LoadFromDisk();
    }

    public static string GetCodeForLevel(string levelPath)
    {
        if (string.IsNullOrEmpty(levelPath)) return DefaultCodeTemplate;
        if (SavedLevelCodes.TryGetValue(levelPath, out string code) && !string.IsNullOrWhiteSpace(code))
        {
            return code;
        }
        return DefaultCodeTemplate;
    }

    public static void SaveCodeForLevel(string levelPath, string code)
    {
        if (string.IsNullOrEmpty(levelPath)) return;
        SavedLevelCodes[levelPath] = code;
        SaveToDisk();
    }

    // Метод для розблокування наступного рівня (викликається при перемозі)
    public static void UnlockNextLevel(int completedLevelIndex)
    {
        if (completedLevelIndex >= UnlockedLevelIndex)
        {
            UnlockedLevelIndex = completedLevelIndex + 1;
            GD.Print($"[Global] Рівень пройдено! Розблоковано наступний індекс: {UnlockedLevelIndex}");
            SaveToDisk();
        }
    }

    public static void SaveToDisk()
    {
        var config = new ConfigFile();
        config.SetValue("Progress", "UnlockedLevelIndex", UnlockedLevelIndex);

        foreach (var kvp in SavedLevelCodes)
        {
            config.SetValue("Codes", kvp.Key, kvp.Value);
        }

        if (SavedDocsPosition.HasValue)
        {
            config.SetValue("UI", "DocsPosX", SavedDocsPosition.Value.X);
            config.SetValue("UI", "DocsPosY", SavedDocsPosition.Value.Y);
        }

        config.Save(SavePath);
    }

    public static void LoadFromDisk()
    {
        var config = new ConfigFile();
        Error err = config.Load(SavePath);
        if (err != Error.Ok) return;

        UnlockedLevelIndex = (int)config.GetValue("Progress", "UnlockedLevelIndex", 0);

        if (config.HasSection("Codes"))
        {
            foreach (string levelPath in config.GetSectionKeys("Codes"))
            {
                string code = (string)config.GetValue("Codes", levelPath, "");
                if (!string.IsNullOrEmpty(code))
                {
                    SavedLevelCodes[levelPath] = code;
                }
            }
        }

        if (config.HasSectionKey("UI", "DocsPosX") && config.HasSectionKey("UI", "DocsPosY"))
        {
            float x = (float)config.GetValue("UI", "DocsPosX", 0f);
            float y = (float)config.GetValue("UI", "DocsPosY", 0f);
            SavedDocsPosition = new Vector2(x, y);
        }
    }
}