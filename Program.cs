namespace ClientServer_test
{
    internal class Program
    {
        private static Dictionary<string, IServer> _gameServers;
        private static IServer? _loginServer;
        private static Database? _database;

        private static Client? _client;

        static async Task Main(string[] args)
        {
            _database = new Database("P:\\clientservdata\\" + FileDataPath.Database);
            var cts = new CancellationTokenSource();
            _loginServer = new LoginServer();
            _loginServer.InitializeServer(_database);
            _gameServers = new Dictionary<string, IServer>
            {
                { "Europe", new GameServer("Europe") },
                { "Asia", new GameServer("Asia") },
                { "North America", new GameServer("North America") }
            };

            Random random = new Random();
            foreach(var kvp in _gameServers)
            {
                kvp.Value.InitializeServer(_database);
                 if(kvp.Value is GameServer server)
                 {
                     server.ConnectTestPlayers(random.Next(0, 4));
                 }
            }

            _client = new Client(_loginServer, _gameServers);
            _client.Start();

            await Task.Delay(-1);
            cts.Cancel();
        }
    }
}
