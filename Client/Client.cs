using System.Diagnostics;

namespace ClientServer_test
{
    public class Client
    {
        public string Username { get; set; }
        private PlayerData _playerData;
        private LoginServer? _loginServer;
        private Dictionary<string, IServer> _gameServers;
        private GameServer _currentGameServer;
        private StateMachine _stateMachine;
        private List<PlayerData> _playersNear;

        public Client(IServer gameServer)
        {
            _currentGameServer = (GameServer)gameServer;
            _playerData = new PlayerData();
            _playersNear = new List<PlayerData>();
            InitializeStateMachine();
        }
        public Client(IServer loginServer, Dictionary<string, IServer> gameServers)
        {
            if (loginServer != null)
            {
                _loginServer = loginServer as LoginServer;
            }
            _gameServers = gameServers;
            _playersNear = new List<PlayerData>();
            InitializeStateMachine();
        }

        public void InitializeStateMachine()
        {
            _stateMachine = new StateMachine();
            _stateMachine.InitializeState(new LoginState(this), "Login");
            _stateMachine.InitializeState(new SignInState(this), "Signin");
            _stateMachine.InitializeState(new LogInOrSignInState(this), "LoginOrSignIn");
            _stateMachine.InitializeState(new SelectServerState(this), "SelectServer");
            _stateMachine.InitializeState(new GameState(this), "Game");
            _stateMachine.InitializeState(new IdleTestState(), "idle_test");
        }

        public void Start()
        {
            _stateMachine.TransitionTo("LoginOrSignIn");
        }

        public void Login()
        {
            //server.AddUser(username);
            _stateMachine.TransitionTo("Login");
        }

        public async Task<LoginResponse?> SendLoginRequest(LoginRequest loginRequest)
        {
            return await _loginServer.SendRequestAsync(loginRequest) as LoginResponse;
        }

        public async Task<SignInResponse?> SendSignInRequest(SignInRequest signInRequest)
        {
            return await _loginServer.SendRequestAsync(signInRequest) as SignInResponse;
        }

        public async Task<ServerResponse> SendGameServerRequest(ServerRequest serverRequest)
        {
            return await _currentGameServer.SendRequestAsync(serverRequest);
        }

        public async Task<PlayerDataResponse?> SendPlayerDataRequest(PlayerDataRequest playerDataRequest)
        {
            return await _currentGameServer.SendRequestAsync(playerDataRequest) as PlayerDataResponse;
        }

        public async Task<ClientSyncResponse> SyncronizeClients()
        {
            return await _currentGameServer.HandleClientsSyncronizationAsync();
        }

        public async Task<ServerResponse> StartFightRequest(StartFightRequest startFightrequest)
        {
            return await _currentGameServer.SendRequestAsync(startFightrequest);
        }

        public async void SetCurrentGameServer(string serverName)
        {
            if (_gameServers.ContainsKey(serverName))
            {
                _currentGameServer = (GameServer)_gameServers[serverName];
                _currentGameServer.ConnectPlayer(this);
                var request = new PlayerDataRequest { ServerName = serverName, Username = Username, Action = "datarequest" };
                var response = await SendPlayerDataRequest(request);
                _playerData = response.PlayerData;
                Console.WriteLine($"Connected to {serverName}");
            }
            else
            {
                Console.WriteLine("Invalid server name.");
            }
        }

        public void ChangeState(string state)
        {
            _stateMachine.TransitionTo(state);
        }

        public void SignIn()
        {
            _stateMachine.TransitionTo("Signin");
        }

        public void Connect()
        {
            _stateMachine.TransitionTo("Game");
            _playerData.SetPosition(0, 0);
        }

        public void SetPosition(int x, int y) => _playerData.SetPosition(x, y);
        public void SetHealth(int health) => _playerData.SetHealth(health);
        public void SetStatus(int status) => _playerData.SetStatus(status);
        public void SetPlayerData(PlayerData playerData) => _playerData = playerData;
        public void AddPlayersNear(PlayerData player)
        {
            if (!_playersNear.Contains(player))
            {
                _playersNear.Add(player);
            }
        }

        public void RemovePlayerNear(PlayerData player)
        {
            if (_playersNear.Contains(player))
            {
                _playersNear.Remove(player);
            }
        }

        public List<PlayerData> GetPlayersNear() => _playersNear;
        public PlayerData GetPlayerData() => _playerData;

        public void RecieveResponse(ServerResponse response)
        {
            switch (response)
            {
                case GameplayResponse gameplayResponse:
                    HandleGameplayResponse(gameplayResponse);
                    break;
                case FightSucsessResponse fightSucsessResponse:
                    HandleSucsessFightResponse(fightSucsessResponse);
                    break;
                case FightFailedResponse fightFailedResponse:
                    HandleFailedFightResponse(fightFailedResponse);
                    break;
                default:
                    Console.WriteLine("Unknown response type received");
                    break;
            }
        }

        private void HandleFailedFightResponse(FightFailedResponse fightFailedResponse) => Console.WriteLine(fightFailedResponse.Message);
        private void HandleSucsessFightResponse(FightSucsessResponse fightSucsessResponse) => Console.WriteLine(fightSucsessResponse.Message);
        private void HandleGameplayResponse(GameplayResponse gameplayResponse) => Console.WriteLine(gameplayResponse.Message);
    }
}
