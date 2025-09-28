using OrdrMate.Models;
using OrdrMate.Utils.Exceptions;

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

    public void AddTable(int tableNumber, int seats)
    {
        if (_tableQueues.ContainsKey(tableNumber))
        {
            Console.WriteLine($"Table {tableNumber} already exists in the reservation system.");
            throw new InvalidOperationException($"Table {tableNumber} already exists in the reservation system.");
        }

        _tableQueues[tableNumber] = new ReservationQueue(seats, tableNumber);
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

    public int GetReservationPosition(int tableNumber, string reservationId)
    {
        if (_tableQueues.TryGetValue(tableNumber, out var queue))
        {
            return queue.GetReservationPosition(reservationId);
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

    public void MoveTableReservation(int fromTableNumber, int toTableNumber, string reservationId)
    {
        if (_tableQueues.TryGetValue(fromTableNumber, out var fromQueue) &&
            _tableQueues.TryGetValue(toTableNumber, out var toQueue))
        {

            Console.WriteLine($"Attempting to move reservation {reservationId} from table {fromTableNumber} to table {toTableNumber}.");

            if (toQueue.Queue.Count > 0)
            {
                throw new BadRequestException("Cannot move reservation to a table that is currently occupied.");
            }

            var reservation = fromQueue.RemoveReservationById(reservationId);
            if (reservation != null)
            {
                toQueue.EnqueueReservation(reservation);
            }
            else
            {
                Console.WriteLine($"Reservation with ID {reservationId} not found in table {fromTableNumber}'s queue.");
                throw new InvalidOperationException($"Reservation with ID {reservationId} not found in table {fromTableNumber}'s queue.");
            }
        }
        else
        {
            Console.WriteLine($"One or both tables ({fromTableNumber}, {toTableNumber}) do not exist in the reservation system.");
            throw new InvalidOperationException($"One or both tables ({fromTableNumber}, {toTableNumber}) do not exist in the reservation system.");
        }
    }
}