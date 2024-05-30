using System.Numerics;

public class PlayerData
{
    public string Username;
    public Vector2 Position;
    public int Health;
    public int Status;

    public void SetPosition(int x, int y) => Position = new Vector2(x, y);
    public void SetHealth(int health) => Health = health;
    public void SetStatus(int status) => Status = status;
}