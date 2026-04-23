using HelpDesk.Core;
using HelpDesk.Data;
using HelpDesk.Domain;
using HelpDesk.Security;

namespace HelpDesk.Endpoints;

public static class TicketEndpoints
{
    public static RouteGroupBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tickets");

        /*group.MapPost("/", async (int userId, TicketCore core) =>
        {
            var ticket = await core.CreateTicket(userId);
            return Results.Ok(ticket);
        });*/

        group.MapGet("/tickets", async (TicketCore core) =>
        {
            return Results.Ok(await core.GetTodayTickets());
        });

        group.MapGet("/today/count", async (TicketCore core) =>
        {
            return Results.Ok(await core.GetNumberTicketsForTheDay());
        });
        
        group.MapGet("/list", async (TicketCore core) =>
        {
            return Results.Ok(await core.GetTicketList());
        });

        group.MapGet("/archived", async (DateOnly? date, TicketCore Core, HttpContext ctx) =>
        {
            if (!SecurityFunctions.RequireAdmin(ctx))
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            var tickets = await Core.GetArchivedTickets(date);
            return Results.Ok(tickets);
        });
        
        group.MapPost("/",
            async (TicketCore core, CreateTicketRequest req) =>
            {
                if (req.Role == "guest")
                    return Results.BadRequest(new { Message = "Guests cannot create tickets" });
                
                if (req.UserId == 0)
                    return Results.BadRequest(new { Message = "Guests cannot create tickets" });


                try
                {
                    var ticket = await core.CreateTicket(req.UserId);
                    return Results.Ok(ticket);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { Message = ex.Message });
                }
            });


        group.MapPost("/tickets/admin/open-next",
            async (TicketCore core, HttpContext ctx) =>
            {
                if (!SecurityFunctions.RequireAdmin(ctx))
                    return Results.Forbid();

                await core.OpenNextTicket();
                return Results.Ok();
            });
        
            group.MapPost("/tickets/admin/close-day",
                async (TicketCore core, HttpContext ctx) =>
                {
                    if (!SecurityFunctions.RequireAdmin(ctx))
                        return Results.Forbid();

                    await core.CloseDay();
                    return Results.Ok();
                });




        return group;
    }
}