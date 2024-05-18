namespace SuperAdmin.Service;

public static class UniqueIdentifierGenerator
{
    private static readonly Random random = new();
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string GenerateRandomCharacters(int length)
    {
        char[] ticketId = new char[length];
        for (int i = 0; i < length; i++)
            ticketId[i] = AllowedCharacters[random.Next(AllowedCharacters.Length)];
        return new string(ticketId);
    }

    public static string GenerateUniqueTicketIdWithTimestamp(int length)
    {
        string timestampPart = DateTime.Now.ToString("MMddHHmmss");
        string randomPart = GenerateRandomCharacters(length - timestampPart.Length);
        return timestampPart + randomPart;
    }
}
