public enum CommandType
{
    Move,
    TurnLeft,
    TurnRight
}

public class Command
{
    public CommandType Type { get; set; }

    public Command(CommandType type)
    {
        Type = type;
    }
}
