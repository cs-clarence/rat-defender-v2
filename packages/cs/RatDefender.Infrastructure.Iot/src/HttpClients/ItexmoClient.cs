using System.Net.Http.Json;

namespace RatDefender.Infrastructure.Iot.HttpClients;

public record Sms
{
    public required string ApiCode { get; init; }
    public string? SenderId { get; init; }
    public string? ClientId { get; init; }
    public required string Email { get; init; }
    public required string Message { get; init; }
    public required string[] Recipients { get; init; }
    public required string Password { get; init; }
}
public class ItexmoClient(HttpClient client)
{
    
    public async Task SendSmsAsync(Sms sms, CancellationToken cancellationToken = default)
    {
        await client.PostAsJsonAsync("/api/broadcast", sms, cancellationToken);
    }
}