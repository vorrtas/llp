using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace llp;

public class Server<T> : IDisposable
{
    private Socket? listener;
    private IPEndPoint endpoint;
    private Action<T> _action;

    public Server(ReadOnlySpan<char> ip, int port, Action<T> action)
    {
        listener = null;
        if (!IPAddress.TryParse(ip, out IPAddress? address))
        {
            throw new Exception(Constants.ERR_INVALID_IP);
        }

        endpoint = new(address, port);
        _action = action;
    }

    private async Task ProcessRequest(Socket listener)
    {
        var handler = await listener.AcceptAsync();
        var buffer = new byte[1_024];
        var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
        var response = Encoding.UTF8.GetString(buffer, 0, received);
        if (response.IndexOf(Constants.EOM) > -1)
        {
            using (var stream = response.Replace(Constants.EOM, string.Empty).ToStream())
            {
                T? val = await JsonSerializer.DeserializeAsync<T>(stream);
                if (val is null) { return; }
                _action(val);
                await handler.SendAsync(Constants.ACKBytes, 0);
            }
        }
    }

    public async Task Bind(CancellationToken token = default)
    {
        try
        {
            using (Socket listener = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                listener.Bind(endpoint);
                listener.Listen(100);

                while (!token.IsCancellationRequested)
                {
                    await ProcessRequest(listener);
                }
            }
        }
        catch
        {
            throw new Exception(Constants.ERR_BAD_INIT);
        }
        finally
        {
            listener?.Dispose();
        }
    }

    public void Dispose()
    {
        listener?.Dispose();
    }
}
