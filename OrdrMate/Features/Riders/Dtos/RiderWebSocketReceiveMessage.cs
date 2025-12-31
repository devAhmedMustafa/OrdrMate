namespace OrdrMate.Features.Riders.Dtos;

public record RiderWebSocketReceiveMessage(
    string Type,
    string Payload
);