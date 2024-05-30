using ClientServer_test;

public class GameState : IState
{
    private Client _client;
    public GameState(Client client)
    {
        _client = client;
    }
    public async Task Handle(StateMachine stateMachine)
    {
        Console.WriteLine("Choose action: [W]: walk up || [A]: walk left || [S]: walk down || [D]: walk right \n" 
                            + "[R]: Rest || [F]: Fight");
        var action = Console.ReadLine();
        var serverRequestAction = "";
        var position = _client.GetPlayerData().Position;
        var status = _client.GetPlayerData().Status;
        var health = _client.GetPlayerData().Health;
        switch (action)
        {
            case "w":
            case "W":
                serverRequestAction = "UP";
                position = new System.Numerics.Vector2(position.X, position.Y + 1);
                break;
            case "a":
            case "A":
                serverRequestAction = "LEFT";
                position = new System.Numerics.Vector2(position.X - 1, position.Y);
                break;
            case "s":
            case "S":
                serverRequestAction = "DOWN";
                position = new System.Numerics.Vector2(position.X, position.Y - 1);
                break;
            case "d":
            case "D":
                serverRequestAction = "RIGHT";
                position = new System.Numerics.Vector2(position.X + 1, position.Y);
                break;
            case "r":
            case "R":
                serverRequestAction = "REST";
                status = 1;
                break;
            case "f":
            case "F":
                serverRequestAction = "FIGHT";
                status = 2;
                break;
            default: await Handle(stateMachine);
                break;    
        }

        var response = await _client.SendGameServerRequest(new GameplayRequest 
            { Action = serverRequestAction, 
            Username = _client.Username, 
            NewPosition = position,
            NewStatus = status,
            NewHealth = health});
        var clientsOnServer = await _client.SyncronizeClients();

        if(response is GameplayResponse gameplayResponse && gameplayResponse.Sucsess)
        {
            _client.SetPosition((int)gameplayResponse.PlayerData.Position.X, (int)gameplayResponse.PlayerData.Position.Y);
            _client.SetStatus(gameplayResponse.PlayerData.Status);
            _client.SetHealth(gameplayResponse.PlayerData.Health);
        }
        else
        {
            if(response is PlayerNearResponse fightSucsessResponse)
            {
                Console.WriteLine(fightSucsessResponse.Message);
                var selection = int.TryParse(Console.ReadLine(), out var res);
                await _client.StartFightRequest(new StartFightRequest {PlayerSelection = res, Action = "Startfight", Username = _client.Username} );
            }
            else
            {
                Console.WriteLine(response.Message);
            }
        }

        foreach(var c in clientsOnServer.playersDatas)
        {
            var st = "";
            switch(c.Status)
            {
                case 0: st = "Idle";
                    break;
                case 1: st = "Resting";
                    break;
                case 2: st = "Fighting"; 
                    break;
                default: st = "";
                    break;
            }
            Console.WriteLine($"Player {c.Username} is on {c.Position}, is now {st} with {c.Health} HP");
        }

        await Handle(stateMachine);
    }
}