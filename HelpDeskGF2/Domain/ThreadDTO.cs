namespace HelpDesk.Domain;

public class ThreadDto
{
    public int ThreadId { get; set; }
    public string Title { get; set; } = "";
    public int? CreatedByUserId { get; set; }
    public string? AnonymousName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ThreadBody { get; set; } = "";
    public string Status { get; set; } = "open"; 
    // "working on" | "open" | "closed"  ...
    public List<ThreadResponseDto> Responses { get; set; } = new();
}


public class ThreadResponseDto
{
    public int ResponseId { get; set; }
    public int ThreadId { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? AnonymousName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ResponseBody { get; set; } = "";
}


public class CreateThreadDto
{
    public string Title { get; set; } = "";
    public int? CreatedByUserId { get; set; }
    public string? AnonymousName { get; set; }
    public string ThreadBody { get; set; } = "";
}

public class ThreadSummary
{
    public int ThreadId { get; set; }
    public string Title { get; set; } = "";
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "open";
}

public class UpdateThreadDto
{
    public int ThreadId { get; set; }
    public string? Title { get; set; }
    public string? ThreadBody { get; set; }
}

public class AddThreadResponseDto
{
    public int ThreadId { get; set; }
    public string ResponseBody { get; set; } = "";
    
    public int? CreatedByUserId { get; set; }
    public string? AnonymousName { get; set; }

}