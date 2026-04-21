using HelpDesk.Core;
using HelpDesk.Domain;
using HelpDesk.Security;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HelpDesk.Endpoints;

public static class UserCoreEndpoints
{
    public static RouteGroupBuilder MapLoginEndpoint(this IEndpointRouteBuilder g)
    {
        var group = g.MapGroup("/user").AllowAnonymous();

        group.MapPost("/login",
            async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> (
                LoginRequest body,
                UserCore core,
                HttpContext ctx) =>
            {
                var result = await core.Login(body, ctx);
                return result is null
                    ? TypedResults.Unauthorized()
                    : TypedResults.Ok(result);
            });

        group.MapGet("/",
            async (UserCore core) =>
                Results.Ok(await core.GetAll()));

        group.MapGet("/{id}",
            async (int id, UserCore core) =>
            {
                var user = await core.GetById(id);
                return user is null
                    ? Results.NotFound()
                    : Results.Ok(user);
            });

        group.MapPost("/",
            async Task<Results<Created<AppUser>, Conflict<string>>> (
                CreateUserRequest req,
                UserCore core) =>
            {
                var created = await core.Create(req);
                return created is null
                    ? TypedResults.Conflict("Username already exists.")
                    : TypedResults.Created($"/user/{created.UserId}", created);
            });

        
        group.MapPut("/admin/{id}",
            async (int id,
                UpdateUserRequestAdmin req,
                UserCore core,
                HttpContext ctx) =>
            {
                if (!SecurityFunctions.RequireAdmin(ctx))
                    return Results.Forbid();

                var ok = await core.UpdateAdmin(id, req);
                return ok ? Results.NoContent() : Results.NotFound();
            });


        group.MapDelete("/{id}",
            async Task<Results<NoContent, NotFound>> (
                int id,
                UserCore core) =>
            {
                var ok = await core.Delete(id);
                return ok
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });
        group.MapPost("/logout",
            (HttpContext ctx) =>
            {
                SecurityFunctions.ClearSession(ctx);
                return Results.Ok();
            });
        group.MapGet("/me",
            (HttpContext ctx) =>
            {
                if (!SecurityFunctions.RequireLogin(ctx))
                    return Results.Unauthorized();

                return Results.Ok(new
                {
                    UserId = ctx.Session.GetInt32("UserId"),
                    Username = ctx.Session.GetString("Username"),
                    Role = ctx.Session.GetString("Role")
                });
            });

        group.MapPost("/guest",
            async (UserCore core, HttpContext ctx) =>
            {
                var result = await core.LoginAsGuest(ctx);
                return Results.Ok(result);
            });


        return group;
    }
}
