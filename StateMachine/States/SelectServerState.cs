using ClientServer_test;

public class SelectServerState : IState
{
    private Client _client;
    public SelectServerState(Client client)
    {
        _client = client;
    }
    public async Task Handle(StateMachine stateMachine)
    {
        Console.WriteLine("Available servers: [1] Europe, [2] Asia, [3] North America");
        Console.Write("Select a server: ");
        string serverSelection = Console.ReadLine();

        switch (serverSelection)
        {
            case "1":
                _client.SetCurrentGameServer("Europe");
                break;
            case "2":
                _client.SetCurrentGameServer("Asia");
                break;
            case "3":
                _client.SetCurrentGameServer("North America");
                break;
            default:
                Console.WriteLine("Invalid selection. Please try again.");
                await Handle(stateMachine); 
                return;
        }

        _client.Connect();
    }
}