using System.Diagnostics;
using OrdrMate.Models;

namespace OrdrMate.Managers;

public class TableQueueManager
{
    private readonly Dictionary<int, ReservationQueue> _tableQueues;
    public TableQueueManager(Branch branch)
    {
        _tableQueues = [];

        if (branch.Tables == null)
        {
            Console.WriteLine("No tables found for the branch.");
            return;
        }

        foreach (var table in branch.Tables)
        {
            Console.WriteLine($"Adding table {table.TableNumber} with {table.Seats} seats to the queue manager.");
            _tableQueues[table.TableNumber] = new ReservationQueue(table.Seats, table.TableNumber);
        }
    }

    public int ReserveLessReservedTable(int seats, TableReservation reservation)
    {

        int minReserved = int.MaxValue;
        int bestTable = -1;

        foreach (var kvp in _tableQueues)
        {
            if (kvp.Value.Seats >= seats)
            {
                if (kvp.Value.Count < minReserved)
                {
                    minReserved = kvp.Value.Count;
                    bestTable = kvp.Key;
                }
            }
        }

        _tableQueues[bestTable].EnqueueReservation(reservation);

        return bestTable;
    }

    public void ReserveTable(int tableNumber, TableReservation reservation)
    {
        if (_tableQueues.TryGetValue(tableNumber, out var queue))
        {
            queue.EnqueueReservation(reservation);
        }
        else
        {
            Console.WriteLine($"No reservation queue found for table {tableNumber}.");
            throw new InvalidOperationException($"Table {tableNumber} does not exist in the reservation system.");
        }
    }

    public TableReservation? DequeueReservation(int tableNumber)
    {
        if (_tableQueues.TryGetValue(tableNumber, out var queue))
        {
            return queue.DequeueReservation();
        }
        else
        {
            Console.WriteLine($"No reservation queue found for table {tableNumber}.");
            return null;
        }
    }

    public int GetTableReservationsCount(int tableNumber)
    {
        if (_tableQueues.TryGetValue(tableNumber, out var queue))
        {
            return queue.Count;
        }
        else
        {
            Console.WriteLine($"No reservation queue found for table {tableNumber}.");
            return 0;
        }
    }

    public int GetOrderPosition(int tableNumber, string orderId)
    {
        if (_tableQueues.TryGetValue(tableNumber, out var queue))
        {
            return queue.GetOrderPosition(orderId);
        }
        else
        {
            Console.WriteLine($"No reservation queue found for table {tableNumber}.");
            return 0;
        }
    }

    public int GetMinimumWaitingTime(int seats)
    {
        int minWaitingTime = int.MaxValue;

        foreach (var queue in _tableQueues.Values)
        {
            if (queue.Seats >= seats && queue.Count < minWaitingTime)
            {
                minWaitingTime = queue.Count;
            }
        }

        return minWaitingTime == int.MaxValue ? 0 : minWaitingTime;
    }

    public TableReservation? PeekReservation(int tableNumber)
    {
        if (_tableQueues.TryGetValue(tableNumber, out var queue))
        {
            return queue.Peek();
        }
        else
        {
            Console.WriteLine($"No reservation queue found for table {tableNumber}.");
            return null;
        }
    }

    public Queue<TableReservation> GetQueue(int tableNumber)
    {
        if (_tableQueues.TryGetValue(tableNumber, out var queue))
        {
            return queue.Queue;
        }
        else
        {
            Console.WriteLine($"No reservation queue found for table {tableNumber}.");
            return new Queue<TableReservation>();
        }
    }

    public List<ReservationQueue> GetAllTablesWithSeats(int seats)
    {
        var tablesWithSeats = new List<ReservationQueue>();

        foreach (var kvp in _tableQueues)
        {
            if (kvp.Value.Seats >= seats)
            {
                tablesWithSeats.Add(kvp.Value);
            }
        }

        return tablesWithSeats;
    }

}