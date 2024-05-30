using ClientServer_test;

public class SignInState: IState
{
    private Client _client;
    public SignInState(Client client)
    {
        _client = client;
    }
    public async Task Handle(StateMachine stateMachine)
    {
        Console.WriteLine("Create username and password to sign in:");
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var pass = Console.ReadLine();
        Console.Write("Confirm password: ");
        var passC = Console.ReadLine();
        await SendSignInRequest(username, pass, passC);
    }

    public async Task SendSignInRequest(string username, string password, string confirmPassword)
        {
            var signInRequest = new SignInRequest() { Username = username, Password = password, ConfirmedPassword = confirmPassword, Action = "signin" };
            var response = await _client.SendSignInRequest(signInRequest);
            if (response.Success)
            {
                _client.ChangeState("Login");
            }
            else
            {
                _client.ChangeState("Signin");
            }
            await Task.CompletedTask;
        }
}  
 