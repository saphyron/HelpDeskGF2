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

        group.MapPost("/", async (int userId, TicketCore core) =>
        {
            var ticket = await core.CreateTicket(userId);
            return Results.Ok(ticket);
        });

        group.MapGet("/tickets", async (TicketCore core) =>
        {
            return Results.Ok(await core.GetTodayTickets());
        });

        group.MapGet("/today/count", async (TicketCore core) =>
        {
            return Results.Ok(await core.GetNumberTicketsForTheDay());
        });
        
        
        
        group.MapPost("/tickets",
            async (TicketCore core, HttpContext ctx) =>
            {
                var role = SecurityFunctions.GetUserRole(ctx);

                if (role == "guest")
                    return Results.Forbid();

                var userId = SecurityFunctions.GetUserId(ctx);
                if (userId == null)
                    return Results.Unauthorized();

                var ticket = await core.CreateTicket(userId.Value);
                return Results.Ok(ticket);
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