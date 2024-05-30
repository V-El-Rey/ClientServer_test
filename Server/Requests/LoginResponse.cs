public class LoginResponse : ServerResponse, IResponse
{
    public string Username { get; set; }
    public bool Success { get; set; }
}