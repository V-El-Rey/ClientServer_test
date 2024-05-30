
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClientServer_test
{
    public class Database
    {
        [JsonInclude]
        public Dictionary<string, bool> registredUsers { get; private set; }
        [JsonInclude]
        public Dictionary<string, string> passData { get; private set; }
        public Dictionary<string, List<PlayerData>> playersData { get; private set; }

        private string filePath;

        [JsonConstructor]
        public Database()
        {
            registredUsers = new Dictionary<string, bool>();
            passData = new Dictionary<string,string>();
        }

        public Database(string filePath)
        {
            this.filePath = filePath;
            playersData ??= new Dictionary<string, List<PlayerData>>();
            playersData.Add("Europe", new List<PlayerData>());
            playersData.Add("Asia", new List<PlayerData>());
            playersData.Add("North America", new List<PlayerData>());
            //users = new Dictionary<string, User>();
            if (File.Exists(filePath))
            {
                Load();
            }
            else
            {
                registredUsers ??= new Dictionary<string,bool>();
                passData ??= new Dictionary<string,string>();
                Save();
            }
        }

        public Dictionary<string, string> GetPassData() => passData;
        public Dictionary<string, bool> GetRegistredUsers() => registredUsers;
        public List<PlayerData> GetServerPlayerData(string serverName) => playersData[serverName];
        public void SetServerPlayersData(string serverName, List<PlayerData> playerDatas) =>  playersData[serverName] = playerDatas;
        public void AddPlayerData(string serverName, PlayerData playerData) => playersData[serverName].Add(playerData);
        public PlayerData GetPlayerData(string serverName, string username)
        {
            return playersData[serverName].Find(x => x.Username == username);
        }

        public void SaveData() => Save();

        private void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
                string jsonString = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }   
        }

        private void Load()
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var database = JsonSerializer.Deserialize<Database>(jsonString, new JsonSerializerOptions { IncludeFields = true });
                registredUsers = database.registredUsers;
                passData = database.passData;
                Console.WriteLine("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                registredUsers = new Dictionary<string, bool>();
                passData = new Dictionary<string, string>();
            }
        }
    }
}
