using ClientServer_test;

public class LogInOrSignInState : IState
{
    private Client _client;
    public LogInOrSignInState(Client client)
    {
        _client = client;
    }
    public async Task Handle(StateMachine stateMachine)
    {
        Console.WriteLine("[1] Log in || [2] Sign in]");
        string res = Console.ReadLine();
        switch (res)
        {
            case "1":
            {
                _client.Login();
            } 
                break;
            case "2":
            {
                _client.SignIn();
            }
                break;
            default:
                Console.WriteLine("Invalid selection. Please try again.");
                await Handle(stateMachine);
                return;
        }
    }
}