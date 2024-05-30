using System.Numerics;

public class GameplayRequest : ServerRequest
{
    public int NewStatus { get; set; }
    public int NewHealth { get; set; }
    public Vector2 NewPosition { get; set; }
}