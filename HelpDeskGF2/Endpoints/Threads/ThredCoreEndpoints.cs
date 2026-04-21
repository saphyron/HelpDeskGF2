using HelpDesk.Core;
using HelpDesk.Domain;
using HelpDesk.Security;

namespace HelpDesk.Endpoints;

public static class ThreadCreation
{
    public static RouteGroupBuilder MapThreadCreationEndpoint(this IEndpointRouteBuilder g)
    {
        var group = g.MapGroup("/thread");

        group.MapGet("/threads", async (ThreadCore core) =>
            Results.Ok(await core.GetAllThreads())
        ).AllowAnonymous();

        group.MapGet("/threads/{id}", async (int id, ThreadCore core) =>
        {
            var thread = await core.GetThreadById(id);
            return thread is null
                ? Results.NotFound()
                : Results.Ok(thread);
        }).AllowAnonymous();

        group.MapPost("/threads",
            async (CreateThreadDto body, ThreadCore core, HttpContext ctx) =>
            {
                var role = SecurityFunctions.GetUserRole(ctx);

                if (role == "guest")
                {
                    // 🔒 GUEST: tving anonym oprettelse
                    body.CreatedByUserId = null;
                    body.AnonymousName = "Gæst";
                }
                else
                {
                    var userId = SecurityFunctions.GetUserId(ctx);
                    if (userId == null)
                        return Results.Unauthorized();

                    body.CreatedByUserId = userId;
                }

                await core.CreateThread(body);
                return Results.Ok();
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