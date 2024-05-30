public class ClientSyncResponse : ServerResponse, IResponse
{
    public List<PlayerData> playersDatas { get; set; }
}