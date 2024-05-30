public class SignInRequest : ServerRequest
{
    public string Password { get; set; }
    public string ConfirmedPassword { get; set; }
}