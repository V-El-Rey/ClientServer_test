public class GameplayResponse : ServerResponse, IResponse
{
    public bool Sucsess { get; set; }
    public PlayerData PlayerData { get; set; }
    public PlayerData PlayerNear { get; set; }
}