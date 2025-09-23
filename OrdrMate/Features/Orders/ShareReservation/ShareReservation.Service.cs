using OrdrMate.Utils.Exceptions;
using OrdrMate.Features.Orders.ShareReservation.Middlewares;
using OrdrMate.DTOs.Order;

namespace OrdrMate.Features.Orders.ShareReservation;

public class ShareReservationService
{
    private readonly TableReservationJwtMiddleware _jwtService;
    public ShareReservationService(TableReservationJwtMiddleware jwtService)
    {
        _jwtService = jwtService;
    }

    public string GenerateShareableLink(string reservationId)
    {
        try
        {
            var token = _jwtService.GenerateJWT(reservationId);
            var shareableLink = $"share_reservation?token={token}";
            return shareableLink;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"An error occurred while generating the shareable link. {ex.Message}");
        }
    }
}