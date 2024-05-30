using System.Collections.Concurrent;

namespace ClientServer_test
{
    public interface IServer
    {
        Database database { get; set; }
        ConcurrentQueue<ServerRequest> requestQueue { get; set; }
        ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>> responseDictionary { get; set; }

        void InitializeServer(Database dataBase);
        Task<ServerResponse> HandleRequestAsync(ServerRequest request);
        Task<ServerResponse> SendRequestAsync(ServerRequest request);
        Task ProcessRequestsAsync();
    }
}
