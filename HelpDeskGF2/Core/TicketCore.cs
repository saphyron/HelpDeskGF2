using HelpDesk.Domain;
using HelpDesk.Data;
using Dapper;

namespace HelpDesk.Core;

public class TicketCore
{
    private readonly ISqlConnectionFactory _factory;

    public TicketCore(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<TicketDto> CreateTicket(int userId)
    {
        using var conn = _factory.Create();
        conn.Open();

        // position = antal ventende tickets før denne
        var position = await conn.ExecuteScalarAsync<int>(
            @"select count(1)
              from dbo.Tickets
              where CreatedAt >= cast(getdate() as date)
                and Status = @Status",
            new { Status = TicketStatus.Waiting });

        var sql = @"
            insert into dbo.Tickets (UserId, Status)
            output inserted.TicketId
            values (@UserId, @Status);";

        var id = await conn.ExecuteScalarAsync<int>(sql, new
        {
            UserId = userId,
            Status = TicketStatus.Waiting
        });

        return new TicketDto
        {
            TicketId = id,
            UserId = userId,
            CreatedAt = DateTime.Now,
            Status = TicketStatus.Waiting,
            PositionInQueue = position
        };
    }

    public async Task<IEnumerable<TicketDto>> GetTodayTickets()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<TicketDto>(
            @"select TicketId, UserId, CreatedAt, Status
              from dbo.Tickets
              where CreatedAt >= cast(getdate() as date)
              order by Status, CreatedAt");
    }

    public async Task<int> GetNumberTicketsForTheDay()
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            @"select count(1)
              from dbo.Tickets
              where CreatedAt >= cast(getdate() as date)");
    }

    public async Task OpenNextTicket()
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(
            @"update top (1) dbo.Tickets
              set Status = @Open
              where Status = @Waiting
              order by CreatedAt",
            new { Open = TicketStatus.Open, Waiting = TicketStatus.Waiting });
    }

    public async Task CloseDay()
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(
            @"update dbo.Tickets
              set Status = @Archived
              where Status != @Archived",
            new { Archived = TicketStatus.Archived });
    }
}