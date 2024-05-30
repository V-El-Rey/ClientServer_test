using System.Collections.Concurrent;
using ClientServer_test;

public class GameServer : IServer
{
    public string serverName;
    private Database _database;
    public Database database { get => _database; set => _database = value; }
    public ConcurrentQueue<ServerRequest> requestQueue { get; set; }
    public ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>> responseDictionary { get; set; }
    public Dictionary<string, Client> _connectedClients;

    public GameServer(string name)
    {
        serverName = name;
        _connectedClients = new Dictionary<string, Client>();
    }
    public void InitializeServer(Database dataBase)
    {
        database = dataBase;
        requestQueue = new ConcurrentQueue<ServerRequest>();
        responseDictionary = new ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>>();
        Task.Run(() => ProcessRequestsAsync());
    }

    public void ConnectPlayer(Client client)
    {
        _connectedClients.Add(client.Username, client);
    }

    public async Task<ClientSyncResponse> HandleClientsSyncronizationAsync()
    {
        var playersData = _database.GetServerPlayerData(serverName);

        // Создаем и возвращаем ответ
        var response = new ClientSyncResponse
        {
            playersDatas = playersData
        };
        return await Task.FromResult(response);
    }

    public Task<ServerResponse> HandleRequestAsync(ServerRequest request)
    {

        return Task.Run(async () =>
        {
            switch (request.Action)
            {
                case "datarequest":
                    {
                        var r = request as PlayerDataRequest;
                        var playerData = _database.GetServerPlayerData(r.ServerName);
                        var pd = playerData.FirstOrDefault(p => p.Username == r.Username);
                        if (pd != null)
                        {
                            var response = new PlayerDataResponse { PlayerData = pd, Message = "Load sucsessful" };
                            return response as ServerResponse;
                        }
                        else
                        {
                            var newPd = new PlayerData
                            {
                                Position = new System.Numerics.Vector2(0, 0),
                                Health = 100,
                                Status = 0,
                                Username = r.Username
                            };
                            _database.AddPlayerData(r.ServerName, newPd);
                            return new PlayerDataResponse
                            {
                                PlayerData = newPd,
                                Message = "Cannot load, created new"
                            };
                        }
                    }
                case "clientSync":
                    {
                        return await HandleClientsSyncronizationAsync();
                    }
                case "UP":
                case "LEFT":
                case "DOWN":
                case "RIGHT":
                    {
                        var r = request as GameplayRequest;
                        var players = _database.GetServerPlayerData(serverName);
                        foreach (var player in players)
                        {

                            if (player.Username != r.Username)
                            {
                                bool isNear = Math.Abs(r.NewPosition.X - player.Position.X) <= 1 && Math.Abs(r.NewPosition.Y - player.Position.Y) <= 1;

                                if (isNear)
                                {
                                    _connectedClients[player.Username].AddPlayersNear(_database.GetPlayerData(serverName, r.Username));
                                    _connectedClients[r.Username].AddPlayersNear(player);
                                }
                                else
                                {
                                    _connectedClients[player.Username].RemovePlayerNear(_database.GetPlayerData(serverName, r.Username));
                                    _connectedClients[r.Username].RemovePlayerNear(player);
                                }

                                if (r.NewPosition == player.Position)
                                {
                                    return new GameplayResponse
                                    {
                                        Sucsess = false,
                                        Message = $"Cannot go, player {player.Username} is there",
                                        PlayerNear = player
                                    };
                                }
                            }
                        }
                        return new GameplayResponse { Sucsess = true, PlayerData = new PlayerData { Position = r.NewPosition, Status = 0, Health = r.NewHealth } };
                    }
                case "REST":
                    {
                        var r = request as GameplayRequest;
                        return new GameplayResponse { Sucsess = true, PlayerData = new PlayerData { Position = r.NewPosition, Status = r.NewStatus, Health = 100 } };
                    }
                case "FIGHT":
                    {
                        var r = request as GameplayRequest;
                        var players = _database.GetServerPlayerData(serverName);
                        if (_connectedClients[r.Username].GetPlayersNear().Count > 0)
                        {
                            var nearPlayers = _connectedClients[r.Username].GetPlayersNear();
                            var message = "Choose player to fight: ";
                            for(int i = 0; i< nearPlayers.Count; i++)
                            {
                                message += $"[{i}] -- {nearPlayers[i].Username}";
                            }
                            return new PlayerNearResponse { Message = message };
                        }
                        else
                        {
                            return new GameplayResponse { Sucsess = false, Message = "No one nearby!" };
                        }
                    }
                case "Startfight":
                    {
                        var r = request as StartFightRequest;
                        var players = _connectedClients[r.Username].GetPlayersNear();
                        var playerToFight = players[r.PlayerSelection];
                        var clientToFight = _connectedClients[playerToFight.Username];
                        var result = await StartFightAsync(_connectedClients[r.Username], clientToFight);
                        return new GameplayResponse { Sucsess = result };
                    }

                default: return new EmptyResponse();
            }
        });
    }

    public async Task ProcessRequestsAsync()
    {
        while (true)
        {
            if (requestQueue.TryDequeue(out var request))
            {
                var response = await HandleRequestAsync(request);
                if (responseDictionary.TryRemove(request.Username, out var tcs))
                {
                    tcs.SetResult(response);
                }
            }
            await Task.Delay(200); // Обработка запросов раз в 0.2 секунды
        }
    }

    public Task<ServerResponse> SendRequestAsync(ServerRequest request)
    {
        var tcs = new TaskCompletionSource<ServerResponse>();
        responseDictionary.TryAdd(request.Username, tcs);
        requestQueue.Enqueue(request);
        return tcs.Task;
    }

    public void ConnectTestPlayers(int playersCount)
    {
        _connectedClients = new Dictionary<string, Client>();
        var debugConnectedPlayersDatas = new List<PlayerData>();
        for (int i = 0; i < playersCount; i++)
        {
            var debugClient = new Client(this)
            {
                Username = $"player_{i}"
            };
            Random random = new Random();
            var pd = new PlayerData
            {
                Position = new System.Numerics.Vector2(random.Next(-3, 3), random.Next(-3, 3)),
                Health = random.Next(50, 101),
                Status = random.Next(0),
                Username = debugClient.Username
            };
            debugClient.SetPlayerData(pd);
            debugClient.ChangeState("idle_test");
            _connectedClients.Add(debugClient.Username, debugClient);
            debugConnectedPlayersDatas.Add(pd);
        }
        _database.SetServerPlayersData(serverName, debugConnectedPlayersDatas);
        _database.SaveData();
    }

    private async Task<bool> StartFightAsync(Client playerOne, Client playerTwo)
{
    Random random = new Random();
    var playerOneData = playerOne.GetPlayerData();
    var playerTwoData = playerTwo.GetPlayerData();
    while (playerOneData.Health > 0 && playerTwoData.Health > 0)
    {
        int damageToPlayerOne = random.Next(5, 16); // случайный урон от 5 до 15
        int damageToPlayerTwo = random.Next(5, 16);

        playerOneData.Health -= damageToPlayerOne;
        playerTwoData.Health -= damageToPlayerTwo;

        // Отправка обновленных данных игрокам
        playerOne.RecieveResponse(new GameplayResponse { PlayerData = playerOneData, Message = $"{playerOneData.Username} : You took damage - {damageToPlayerOne}" });
        playerTwo.RecieveResponse(new GameplayResponse { PlayerData = playerTwoData, Message = $"{playerTwoData.Username} : You took damage - {damageToPlayerTwo}" });

        // Ждем одну секунду
        await Task.Delay(1000);
    }

    // Отправка финального состояния игрокам
    if (playerOneData.Health <= 0)
    {
        playerOne.RecieveResponse(new FightSucsessResponse { PlayerData = playerOneData, Message = $"{playerOneData.Username} : You have been defeated" });
        playerTwo.RecieveResponse(new FightFailedResponse { PlayerData = playerTwoData, Message = $"{playerTwoData.Username} : You won the fight" });
    }
    else if (playerTwoData.Health <= 0)
    {
        playerOne.RecieveResponse(new FightSucsessResponse { PlayerData = playerOneData, Message = $"{playerOneData.Username} : You won the fight" });
        playerTwo.RecieveResponse(new FightFailedResponse { PlayerData = playerTwoData, Message = $"{playerTwoData.Username} : You have been defeated" });
    }
    return playerOneData.Health > 0;
}
}