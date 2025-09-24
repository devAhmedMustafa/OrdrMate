using System.Diagnostics;
using System.Text.Json;
using OrdrMate.DTOs.Table;
using OrdrMate.Events;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Sockets;

namespace OrdrMate.Managers;

public class TableManager
{

    private readonly static Dictionary<string, TableQueueManager> _branchQueues = [];
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBranchRepo _branchRepo;
    private readonly AiService _aiService;
    private static bool _initialized = false;
    public TableManager(
        IBranchRepo branchRepo,
        IServiceScopeFactory scopeFactory,
        AiService aiService
    )
    {
        _scopeFactory = scopeFactory;
        _branchRepo = branchRepo;
        _aiService = aiService;

        if (_initialized) return;

        Init();

        BranchEvents.BranchCreated += OnBranchCreated;
        TableEvents.TableCreated += OnTableCreated;

        _initialized = true;
    }

    private void Init()
    {
        var restaurants = _branchRepo.GetAllBranches().Result;
        foreach (var restaurant in restaurants)
        {
            AddBranchQueue(restaurant);
        }
    }

    public void OnBranchCreated(Branch branch)
    {
        if (!_branchQueues.ContainsKey(branch.Id))
        {
            AddBranchQueue(branch);
        }
    }

    public void OnTableCreated(Table table)
    {
        if (_branchQueues.TryGetValue(table.BranchId, out var queueManager))
        {
            queueManager.AddTable(table.TableNumber, table.Seats);
        }
        else
        {
            Console.WriteLine($"No queue manager found for branch {table.BranchId} when adding table {table.TableNumber}.");
        }
    }

    public void AddBranchQueue(Branch branch)
    {
        if (!_branchQueues.ContainsKey(branch.Id))
        {
            _branchQueues[branch.Id] = new TableQueueManager(branch);

            var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();
            var reservations = tableRepo.GetTableReservationsByBranchId(branch.Id).Result;

            foreach (var reservation in reservations)
            {
                if (reservation.ReservationStatus != "Left" && reservation.ReservationStatus != "Cancelled")
                {
                    _branchQueues[branch.Id].ReserveTable(reservation.TableNumber, reservation);
                    Console.WriteLine($"Added reservation {reservation.ReservationId} to queue for branch {branch.Id}, table {reservation.TableNumber}.");
                }
                else
                {
                    Console.WriteLine($"Skipping reservation {reservation.ReservationId} with status {reservation.ReservationStatus}.");
                }
            }
        }
    }

    public async Task<int> ReserveTable(int tableNumber, TableReservation reservation)
    {
        if (_branchQueues.TryGetValue(reservation.BranchId, out var queueManager))
        {
            queueManager.ReserveTable(tableNumber, reservation);
            var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();
            var createdReservation = await tableRepo.CreateTableReservation(reservation);

            if (createdReservation == null)
            {
                throw new Exception("Failed to create table reservation.");
            }

            if (queueManager.GetTableReservationsCount(tableNumber) == 1)
            {
                await tableRepo.UpdateTableReservationStatus(createdReservation.ReservationId, "Waiting");
            }

            var branchSocket = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<BranchSocketHandler>();

            var json = new
            {
                Type = "TableReserved",
                Reservation = new TableReservationResponseDto
                {
                    ReservationId = createdReservation.ReservationId,
                    TableNumber = createdReservation.TableNumber,
                    CustomerName = createdReservation.Customer?.Username ?? "Unknown",
                    ReservationDate = createdReservation.ReservationTime,
                    ReservationStatus = createdReservation.ReservationStatus,
                }
            };

            await branchSocket.SendTo(reservation.BranchId, JsonSerializer.Serialize(json));

            return tableNumber;
        }
        else
        {
            throw new Exception($"No queue manager found for branch {reservation.BranchId}");
        }
    }

    public async Task DequeueReservation(string branchId, int tableNumber)
    {
        if (_branchQueues.TryGetValue(branchId, out var queueManager))
        {
            var reservation = queueManager.DequeueReservation(tableNumber);
            if (reservation == null)
            {
                Console.WriteLine($"No reservation found for table {tableNumber} in branch {branchId}.");
                return;
            }

            var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();

            await tableRepo.UpdateTableReservationStatus(reservation.ReservationId, "Left");
            var peekReservation = queueManager.PeekReservation(tableNumber);

            if (peekReservation == null)
            {
                Console.WriteLine($"No more reservations in queue for table {tableNumber} in branch {branchId}.");
                return;
            }

            await tableRepo.UpdateTableReservationStatus(peekReservation.ReservationId, "Waiting");
            // await BindNextReservation(branchId, tableNumber);
        }
        else
        {
            Console.WriteLine($"No reservation queue found for branch {branchId} and table {tableNumber}.");
        }
    }

    public async Task<IEnumerable<Order>?> BindNextReservation(string branchId, int tableNumber)
    {
        try
        {


            if (!_branchQueues.TryGetValue(branchId, out var queueManager))
            {
                Console.WriteLine($"No reservation queue found for branch {branchId}.");
                return null;
            }

            var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();

            var peekReservation = queueManager.PeekReservation(tableNumber);

            if (peekReservation == null)
            {
                Console.WriteLine($"No reservations found in queue for table {tableNumber} in branch {branchId}.");
                return null;
            }

            var orders = await tableRepo.GetTableOrdersByReservationId(peekReservation.ReservationId);
            if (orders == null)
            {
                throw new Exception($"No order found for reservation {peekReservation.ReservationId} in branch {peekReservation.BranchId}.");
            }

            await tableRepo.UpdateTableReservationStatus(peekReservation.ReservationId, "Seated");
            Console.WriteLine($"Reservation {peekReservation.ReservationId} for table {tableNumber} in branch {branchId} is now seated.");

            foreach (var order in orders)
            {
                OrderEvents.OnOrderPlaced(peekReservation.BranchId, [.. order.OrderItems!]);
            }

            return orders;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in BindNextReservation: {ex.Message}");
            throw;
        }
    }

    public TableReservation? GetCurrentReservation(string branchId, int tableNumber)
    {
        if (_branchQueues.TryGetValue(branchId, out var queueManager))
        {
            return queueManager.PeekReservation(tableNumber);
        }
        else
        {
            Debug.WriteLine($"No reservation queue found for branch {branchId}.");
            return null;
        }
    }

    public async Task<int> GetOrderPosition(string reservationId)
    {
        var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();
        var reservation = await tableRepo.GetTableReservationByOrderId(reservationId);
        if (reservation == null)
        {
            Debug.WriteLine($"No reservation found for order {reservationId}.");
            return -1;
        }

        if (_branchQueues.TryGetValue(reservation.BranchId, out var queueManager))
        {
            return queueManager.GetReservationPosition(reservation.TableNumber, reservation.ReservationId) + 1;
        }
        else
        {
            Debug.WriteLine($"No reservation queue found for branch {reservation.BranchId}.");
            return -1;
        }
    }

    public async Task<TableWaitingDto> GetMinimumWaitingTime(string branchId, int seats)
    {

        if (_branchQueues.TryGetValue(branchId, out var queueManager))
        {
            var tables = queueManager.GetAllTablesWithSeats(seats);

            var minTime = decimal.MaxValue;
            var minSeats = int.MaxValue;
            ReservationQueue bestTable = null!;

            foreach (var table in tables)
            {
                if (table.Seats < seats) continue;

                var queue = table.Queue;
                var totalTime = 0.0m;

                foreach (var reservation in queue)
                {
                    var estimatedTime = await _aiService.PredictStayDuration(reservation);
                    totalTime += estimatedTime;
                }

                if (totalTime < minTime)
                {
                    if (totalTime == minTime && table.Seats < minSeats)
                    {
                        // Prefer tables with fewer seats if the time is the same
                        continue;
                    }

                    minTime = totalTime;
                    bestTable = table;
                }
            }

            if (bestTable == null)
            {
                Debug.WriteLine($"No suitable table found for branch {branchId} with seats {seats}.");
                return new TableWaitingDto
                {
                    TableNumber = -1,
                    WaitingCount = 0,
                    WaitingTime = 0.0m
                };
            }

            return new TableWaitingDto
            {
                TableNumber = bestTable.TableNumber,
                WaitingCount = bestTable.Count,
                WaitingTime = minTime,
            };

        }
        else
        {
            Debug.WriteLine($"No reservation queue found for branch {branchId}.");
            throw new Exception($"No reservation queue found for branch {branchId}.");
        }
    }

    public int GetReservationCount(string branchId, int tableNumber)
    {
        if (_branchQueues.TryGetValue(branchId, out var queueManager))
        {
            return queueManager.GetTableReservationsCount(tableNumber);
        }
        else
        {
            Debug.WriteLine($"No reservation queue found for branch {branchId}.");
            throw new Exception($"No reservation queue found for branch {branchId}.");
        }
    }

    public async Task MoveTableReservation(string branchId, int fromTableNumber, int toTableNumber, string reservationId)
    {
        try
        {
            if (_branchQueues.TryGetValue(branchId, out var queueManager))
            {
                var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();
                queueManager.MoveTableReservation(fromTableNumber, toTableNumber, reservationId);
                await tableRepo.UpdateTableReservationTableNumber(reservationId, toTableNumber);

                var peekT1 = queueManager.PeekReservation(fromTableNumber);
                if (peekT1 != null)
                {
                    await tableRepo.UpdateTableReservationStatus(peekT1.ReservationId, "Waiting");
                }
            }
            else
            {
                Debug.WriteLine($"No reservation queue found for branch {branchId}.");
                throw new Exception($"No reservation queue found for branch {branchId}.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error moving reservation {reservationId} from table {fromTableNumber} to table {toTableNumber} in branch {branchId}: {ex.Message}");
            throw;
        }
    }
}