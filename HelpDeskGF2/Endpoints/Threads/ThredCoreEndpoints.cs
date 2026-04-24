using HelpDesk.Core;
using HelpDesk.Domain;
using HelpDesk.Security;


namespace HelpDesk.Endpoints;

public static class ThreadCreation
{
    public static RouteGroupBuilder MapThreadCreationEndpoint(this IEndpointRouteBuilder g)
    {
        var group = g.MapGroup("/thread");

        group.MapGet("/threads", async (ThreadCore core, int? userId, string? role) =>
        {
            return role switch
            {
                "admin" => Results.Ok(await core.GetAllThreads()),
                "user" => Results.Ok(await core.GetVisibleByUserThreads(userId, false)),
                _ => Results.Ok(await core.GetAnonymousThreads())
            };
        });
            

        group.MapGet("/threads/{id}", async (int id, ThreadCore core) =>
        {
            var thread = await core.GetThreadById(id);
            return thread is null
                ? Results.NotFound()
                : Results.Ok(thread);
        }).AllowAnonymous();

        group.MapPost("/threads",
            async (CreateThreadDto body, ThreadCore core) =>
            {
                
                if (body.CreatedByUserId == 0 || body.CreatedByUserId == null)
                {
                    body.CreatedByUserId = null;
                    body.AnonymousName = "Gæst";
                }
                else if(body.CreatedByUserId <= 0)
                {
                    return Results.BadRequest("Invalid CreatedByUserId");
                }

                var ok = await core.CreateThread(body);
                return ok ? Results.Ok() : Results.Problem();
            });


        group.MapPut("/threads/{id}", async (
            int id,
            UpdateThreadDto body,
            ThreadCore core) =>
        {
            var ok = await core.UpdateThread(id, body);
            return ok ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/threads/{id}/responses", async (
            int id,
            AddThreadResponseDto body,
            ThreadCore core) =>
        {
            if (string.IsNullOrWhiteSpace(body.ResponseBody))
                return TypedResults.BadRequest("ResponseBody required");

            var ok = await core.AddResponse(id, body);
            return ok ? Results.Ok() : Results.NotFound();
        });

        group.MapPut("/threads/{threadId}/responses/{responseId}", async (
            int threadId,
            int responseId,
            string responseBody,
            ThreadCore core) =>
        {
            var ok = await core.UpdateResponse(threadId, responseId, responseBody);
            return ok ? Results.Ok() : Results.NotFound();
        });

        return group;
    }
}