using ClientServer_test;

public class LoginState : IState
{
    private Client _client;
    public LoginState(Client client)
    {
        _client = client;
    }
    public async Task Handle(StateMachine stateMachine)
    {
        // Логика логина пользователя.
        Console.WriteLine("Log into game:");
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var pass = Console.ReadLine();
        await SendLoginRequest(username, pass);
    }

    public async Task SendLoginRequest(string username, string password)
        {
            var loginRequest = new LoginRequest() { Username = username, Password = password, Action = "login" };
            var response = await _client.SendLoginRequest(loginRequest);
            _client.Username = response.Username;
            if (response.Success)
            {
                _client.ChangeState("SelectServer");
            }
            else
            {
                _client.ChangeState("LogInOrSignIn");
            }
            await Task.CompletedTask;
        }
}