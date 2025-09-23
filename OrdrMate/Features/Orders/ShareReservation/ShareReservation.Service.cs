using OrdrMate.Utils.Exceptions;
using OrdrMate.Features.Orders.ShareReservation.Middlewares;
using System.Diagnostics;

namespace OrdrMate.Features.Orders.ShareReservation;

public class ShareReservationService
{
    private readonly TableReservationJwtMiddleware _jwtService;
    private static readonly Dictionary<string, string> _tokenCache = [];
    public ShareReservationService(TableReservationJwtMiddleware jwtService)
    {
        _jwtService = jwtService;
    }

    public string GenerateShareableLink(string reservationId)
    {
        try
        {

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