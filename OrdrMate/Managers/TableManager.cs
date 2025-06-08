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

    public void AddBranchQueue(Branch branch)
    {
        if (!_branchQueues.ContainsKey(branch.Id))
        {
            _branchQueues[branch.Id] = new TableQueueManager(branch);

            var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();
            var reservations = tableRepo.GetTableReservationsByBranchId(branch.Id).Result;

            foreach (var reservation in reservations)
            {
                if (reservation.ReservationStatus == "Queued" || reservation.ReservationStatus == "Seated")
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
                await BindNextReservation(reservation.BranchId, tableNumber);
            }

            var branchSocket = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<BranchSocketHandler>();

            var json = new
            {
                Type = "TableReserved",
                Reservation = new TableReservationResponseDto
                {
                    TableNumber = createdReservation.TableNumber,
                    CustomerName = createdReservation.Customer?.Username ?? "Unknown",
                    ReservationDate = createdReservation.ReservationTime,
                    ReservationStatus = createdReservation.ReservationStatus,
                    OrderId = createdReservation.OrderId,
                }
            };

            await branchSocket.SendToBranch(reservation.BranchId, JsonSerializer.Serialize(json));

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

            await BindNextReservation(branchId, tableNumber);
        }
        else
        {
            Console.WriteLine($"No reservation queue found for branch {branchId} and table {tableNumber}.");
        }
    }

    public async Task BindNextReservation(string branchId, int tableNumber) {

        if (!_branchQueues.TryGetValue(branchId, out var queueManager))
        {
            Console.WriteLine($"No reservation queue found for branch {branchId}.");
            return;
        }

        var tableRepo = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITableRepo>();

        var peekReservation = queueManager.PeekReservation(tableNumber);

        if (peekReservation == null)
        {
            Console.WriteLine($"No reservations found in queue for table {tableNumber} in branch {branchId}.");
            return;
        }

        var order = await tableRepo.GetTableOrderByReservationId(peekReservation.ReservationId);
        if (order == null || order.OrderItems == null)
        {
            throw new Exception($"No order found for reservation {peekReservation.ReservationId} in branch {peekReservation.BranchId}.");
        }

        await tableRepo.UpdateTableReservationStatus(peekReservation.ReservationId, "Seated");
        OrderEvents.OnOrderPlaced(peekReservation.BranchId, [.. order.OrderItems]);
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
            return queueManager.GetOrderPosition(reservation.TableNumber, reservation.OrderId) + 1;
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

}