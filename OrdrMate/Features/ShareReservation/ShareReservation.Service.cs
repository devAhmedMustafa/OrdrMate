namespace OrdrMate.Features.ShareReservation;

public class ShareReservationService
{

    public ShareReservationService()
    {
    }

    public async Task<string> GenerateShareableLinkAsync(string reservationId)
    {
        await Task.Delay(100);
        return $"https://ordrmate.com/share/{reservationId}";
    }
}