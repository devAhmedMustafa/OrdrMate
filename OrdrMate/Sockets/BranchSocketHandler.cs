using System.Collections.Concurrent;
using System.Net.WebSockets;
using OrdrMate.Events;

namespace OrdrMate.Sockets;

public class BranchSocketHandler
{
    private static readonly ConcurrentDictionary<string, List<WebSocket>> _branchSockets = new();

    public BranchSocketHandler()
    {
    }

    public async Task AddSocketAsync(string branchId, WebSocket socket)
    {
        if (!_branchSockets.ContainsKey(branchId))
        {
            _branchSockets[branchId] = [];
        }

        _branchSockets[branchId].Add(socket);

        BranchEvents.OnBranchSocketConnected(branchId);

        await Listen(socket);
    }

    private async Task Listen(WebSocket socket)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
        }
    }

    public async Task SendToBranch(string branchId, string message)
    {
        if (!_branchSockets.TryGetValue(branchId, out var sockets)) return;

        var tasks = sockets.Where(s => s.State == WebSocketState.Open)
            .Select(async socket =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            });

        await Task.WhenAll(tasks);
    }
}