using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LevelSelectConfig : Resource
{
    [Export] public Array<ChapterData> Chapters { get; set; } = new Array<ChapterData>();
}