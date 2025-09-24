using OrdrMate.Models;

namespace OrdrMate.Events;

public class TableEvents
{
    public static event Action<Table>? TableCreated;

    public static void OnTableCreated(Table table)
    {
        TableCreated?.Invoke(table);
    }
}