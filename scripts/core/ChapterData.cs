using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ChapterData : Resource
{
    [Export] public string ChapterName { get; set; } = "Нова глава";
    [Export] public Array<LevelData> Levels { get; set; } = new Array<LevelData>();
}