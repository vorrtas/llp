using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace llp;

public class Client
{
    private Socket? client;
    private IPEndPoint endpoint;
    public Client(string ip, int port)
    {
        client = null;
        if (!IPAddress.TryParse(ip, out IPAddress? address))
        {
            throw new Exception(Constants.ERR_INVALID_IP);
        }

        endpoint = new(address, port);
    }

    public async Task SendAsync(object? message, CancellationToken token = default)
    {
        using (Socket client = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
            await client.ConnectAsync(endpoint);
            while (!token.IsCancellationRequested)
            {
                var encodedMessage = JsonSerializer.Serialize(message);
                var messageBytes = Encoding.UTF8.GetBytes(encodedMessage + Constants.EOM);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                var buffer = new byte[1_024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response != Constants.ACK)
                {
                    throw new Exception(Constants.ERR_BAD_RESPONSE);
                }
                break;
            }
            client.Shutdown(SocketShutdown.Both);
        }
    }
}

