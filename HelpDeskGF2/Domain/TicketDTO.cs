namespace HelpDesk.Domain;

public class TicketDto
{
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = TicketStatus.Waiting;
    public int PositionInQueue { get; set; }
}
public static class TicketStatus
{
    public const string Waiting = "Waiting";
    public const string Open = "Open";
    public const string Archived = "Archived";
}

public record CreateTicketRequest(int UserId, string Role);


public class TicketListItemDto
{
    public int TicketId { get; set; }
    public string Username { get; set; } = "";
    public string Status { get; set; } = "";
    public int PositionInQueue { get; set; }
    public DateTime CreatedAt { get; set; }
}
