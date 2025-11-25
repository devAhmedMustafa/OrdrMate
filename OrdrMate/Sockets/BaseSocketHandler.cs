using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace OrdrMate.Sockets;

public abstract class BaseSocketHandler
{
    private static readonly ConcurrentDictionary<string, List<WebSocket>> _sockets = new();

    public async Task AddSocketAsync(string key, WebSocket socket)
    {
        if (!_sockets.TryGetValue(key, out List<WebSocket>? value))
        {
            value = [];
            _sockets[key] = value;
        }

        value.Add(socket);
        await Listen(socket);
    }

    private async Task Listen(WebSocket socket)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                MessageListener(message);
            }
            else
                if (result.MessageType == WebSocketMessageType.Close)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
        }
    }

    protected abstract void MessageListener(string message);

    public async Task SendTo(string key, string message)
    {
        if (!_sockets.TryGetValue(key, out var sockets)) return;

        var tasks = sockets.Where(s => s.State == WebSocketState.Open)
            .Select(async socket =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            });

        await Task.WhenAll(tasks);
    }
}