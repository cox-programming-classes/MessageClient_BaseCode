namespace MessageClient_BaseCode;

public record LoginRecord(string handle, string passcode);
public record AuthorizationResponse(string clientId, string token, DateTime expiration);
public record Message(byte[] messageData, IEnumerable<string> recipients);