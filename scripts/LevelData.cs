using Godot;

[GlobalClass]
public partial class LevelData : Resource
{
    [Export] public string LevelName { get; set; } = "Новий рівень";
    [Export(PropertyHint.File, "*.tscn")] public string LevelScenePath { get; set; }
}