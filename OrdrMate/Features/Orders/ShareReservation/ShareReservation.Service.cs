using OrdrMate.Utils.Exceptions;
using OrdrMate.Features.Orders.ShareReservation.Middlewares;
using OrdrMate.Repositories;

namespace OrdrMate.Features.Orders.ShareReservation;

public class ShareReservationService
{
    private readonly TableReservationJwtMiddleware _jwtService;
    private readonly ITableRepo _tableRepo;
    private static readonly Dictionary<string, string> _tokenCache = [];
    public ShareReservationService(TableReservationJwtMiddleware jwtService, ITableRepo tableRepo)
    {
        _jwtService = jwtService;
        _tableRepo = tableRepo;
    }

    public string GenerateShareableLink(string reservationId)
    {
        try
        {

            var reservation = _tableRepo.GetTableReservationById(reservationId).Result;
            if (reservation == null)
            {
                throw new NotFoundException("Reservation not found.");
            }

            if (reservation.ReservationStatus == "Left")
            {
                throw new BadRequestException("Cannot share a reservation that has already left.");
            }

            if (_tokenCache.ContainsKey(reservationId))
            {
                var existingToken = _tokenCache[reservationId];
                if (_jwtService.ValidateJWT(existingToken))
                {
                    return $"share_reservation?token={existingToken}";
                }
            }

            var token = _jwtService.GenerateJWT(reservationId);
            var shareableLink = $"share_reservation?token={token}";
            _tokenCache[reservationId] = token;

            return shareableLink;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"An error occurred while generating the shareable link. {ex.Message}");
        }
    }

    public bool AccessSharedReservation(string token)
    {
        try
        {
            if (!_jwtService.ValidateJWT(token))
            {
                throw new UnauthorizedAccessException("Invalid or expired token.");
            }

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"An error occurred while accessing the shared reservation. {ex.Message}");
        }
    }
}