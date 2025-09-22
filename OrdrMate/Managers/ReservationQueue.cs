using OrdrMate.Models;

namespace OrdrMate.Managers;

public class ReservationQueue
{
    private readonly Queue<TableReservation> _queue;
    public Queue<TableReservation> Queue => _queue;
    private int _seats;
    private int _tableNumber;
    public int TableNumber => _tableNumber;
    public int Seats => _seats;
    public ReservationQueue(int seats, int tableNumber)
    {
        _queue = new Queue<TableReservation>();
        _seats = seats;
        _tableNumber = tableNumber;
    }

    public void EnqueueReservation(TableReservation reservation)
    {
        _queue.Enqueue(reservation);
    }

    public TableReservation? DequeueReservation()
    {
        if (_queue.Count == 0)
            return null;

        return _queue.Dequeue();
    }

    public int Count => _queue.Count;
    public bool IsEmpty => _queue.Count == 0;
    public TableReservation Peek()
    {
        if (_queue.Count == 0)
            throw new InvalidOperationException("Queue is empty.");

        return _queue.Peek();
    }

    public int GetOrderPosition(string orderId)
    {
        int position = 0;
        foreach (var reservation in _queue)
        {
            if (reservation.OrderId == orderId)
            {
                return position;
            }
            position++;
        }
        return -1;
    }

    public TableReservation? RemoveReservationById(string reservationId)
    {
        var tempQueue = new Queue<TableReservation>();
        TableReservation? removedReservation = null;

        while (_queue.Count > 0)
        {
            var reservation = _queue.Dequeue();
            if (reservation.ReservationId == reservationId)
            {
                removedReservation = reservation;
                continue;
            }
            tempQueue.Enqueue(reservation);
        }

        while (tempQueue.Count > 0)
        {
            _queue.Enqueue(tempQueue.Dequeue());
        }

        return removedReservation;
    }
}