namespace MessageClient_BaseCode;

public record LoginRequestRecord(string handle, string passcode);
public record AuthorizationResponse(string clientId, string token, DateTime expiration);
public record Message(string sender, IEnumerable<string> recipients, byte[] message);
public sealed record GetMessageRecord(string id, string sender, IEnumerable<string> recipients, byte[] message);
public sealed record CreateClientRecord(string handle, string name, string passcode);
public sealed record ClientRecord(string id, string handle);