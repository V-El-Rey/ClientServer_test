using System.Collections.Concurrent;
using ClientServer_test;

public class LoginServer : IServer
{
    public Database? database { get; set; }
    public ConcurrentQueue<ServerRequest>? requestQueue { get; set; }
    public ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>> responseDictionary { get; set; }
    public void InitializeServer(Database dataBase)
    {
        database = dataBase;
        requestQueue = new ConcurrentQueue<ServerRequest>();
        responseDictionary = new ConcurrentDictionary<string,TaskCompletionSource<ServerResponse>>();
        Task.Run(() => ProcessRequestsAsync());
    }

    public Task<ServerResponse> HandleRequestAsync(ServerRequest request)
    {
        var result = false;
        var message = "Login failed";
        var registredUsers = database.GetRegistredUsers();
        var passData = database.GetPassData();
        return Task.Run(() =>
        {
            switch (request.Action)
            {
                case "login":
                {
                    var r = request as LoginRequest;
                    if(registredUsers.TryGetValue(r.Username, out var value) && value)
                    {
                        if(passData[r.Username] == r.Password)
                        {
                            result = true;
                            message = "Login sucsessful";
                        }
                    }
                    var ret = new LoginResponse { Success = result, Message = message, Username = r.Username };
                    return ret as ServerResponse;
                }
                case "signin":
                {
                    var r = request as SignInRequest;
                    message = "Signed in!";
                    registredUsers.TryAdd(r.Username, true);
                    passData.TryAdd(r.Username, r.Password);
                    database.SaveData();
                    var ret = new SignInResponse { Success = true, Message = message };
                    return ret;
                }
                default:
                    return new LoginResponse { Success = false, Message = "message" };
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
}
