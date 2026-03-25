using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using HelpDesk.Data;
using HelpDesk.Domain;

namespace HelpDesk.Endpoints;

public static class UserCoreLogin
{
    public static RouteGroupBuilder MapLoginEndpoint(this IEndpointRouteBuilder g)
    {
        var group = g.MapGroup("/user").AllowAnonymous();
        group.MapPost("/login",
               async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult, BadRequest<string>>> (
                   LoginRequest body,
                   ISqlConnectionFactory factory,
                   IConfiguration config) =>
               {
                   using var conn = factory.Create();
                   conn.Open();

                   var user = await conn.QuerySingleOrDefaultAsync<UserAuthRow>(
                       @"select UserId, Username, Password, Role, Name
                          from dbo.Users
                          where Username = @Username;",
                       new { body.Username });
                   if (user is null || user.PasswordClear != body.Password)
                       return TypedResults.Unauthorized();
                   var response = new LoginResponse
                   {
                       User = new UserSummary
                       {
                           UserId = user.Id,
                           Username = user.Username,
                           Role = user.Role,
                           Name = user.Name
                       }
                   };
                   return TypedResults.Ok(response);
               }).AllowAnonymous();

        group.MapGet("/",
            async (ISqlConnectionFactory factory) =>
            {
                using var conn = factory.Create();
                conn.Open();
                var sql = @"select UserId, Username, Role, Name 
                        from dbo.Users
                        order by UserId";
                var users = await conn.QueryAsync<AppUser>(sql);
                return Results.Ok(users);
            }).AllowAnonymous();

        group.MapGet("/{id}",
            async (int id, ISqlConnectionFactory factory) =>
            {
                using var conn = factory.Create();
                conn.Open();
                var sql = @"select UserId, Username, Role, Name 
                        from dbo.Users
                        where UserId = @UserId";
                var user = await conn.QuerySingleOrDefaultAsync<AppUser>
                    (sql, new { UserId = id });
                if (user is null) return Results.NotFound();
                return Results.Ok(user);
            }).AllowAnonymous();

        group.MapPost("/",
            async Task<Results<Created<AppUser>, BadRequest<string>, Conflict<string>>>
                (CreateUserRequest req, ISqlConnectionFactory factory) =>
            {
                using var conn = factory.Create();
                conn.Open();
                try
                {
                    var sql = @"insert into dbo.Users (Username, Password) 
                                    values (@Username, @PasswordClear);
                                    select cast(scope_identity() as int);";
                    var newId = await conn.ExecuteScalarAsync<int>(sql, req);
                    var created = new AppUser { UserId = newId, Username = req.Username };
                    return TypedResults.Created($"/user/{newId}", created);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
                {
                    return TypedResults.Conflict("Username already exists.");
                }
            }).AllowAnonymous();

        group.MapPut("/{id}",
            async Task<Results<NoContent, NotFound, BadRequest<string>, Conflict<string>>>
                (int id, UpdateUserRequest req, ISqlConnectionFactory factory) =>
            {
                using var conn = factory.Create();
                conn.Open();
                var exists = await conn.ExecuteScalarAsync<int>(
                    "select count(1) from dbo.Users where UserId = @ID", new { id });
                if (exists == 0) return TypedResults.NotFound();

                var sql = @"update dbo.Users
                                set Password = @PasswordClear
                                , Role = @Role
                                , Name = @Name
                                where UserId = @UserId;";
                await conn.ExecuteAsync(sql, new
                {
                    UserId = id,
                    PasswordClear = req.PasswordClear,
                    Role = req.Role,
                    Name = req.Name
                });
                return TypedResults.NoContent();
            }).AllowAnonymous();

        group.MapPut("/admin/{id}",
            async Task<Results<NoContent, NotFound, BadRequest<string>, Conflict<string>>>
                (int id, UpdateUserRequestAdmin req, ISqlConnectionFactory factory) =>
            {
                using var conn = factory.Create();
                conn.Open();
                var exists = await conn.ExecuteScalarAsync<int>(
                    "select count(1) from dbo.Users where UserId = @ID", new { id });
                if (exists == 0) return TypedResults.NotFound();

                var sql = @"update dbo.Users
                                set Username = @Username
                                , Password = @PasswordClear
                                , Role = @Role
                                , Name = @Name
                                where UserId = @UserId;";
                await conn.ExecuteAsync(sql, new
                {
                    UserId = id,
                    Username = req.Username,
                    PasswordClear = req.PasswordClear,
                    Role = req.Role,
                    Name = req.Name
                });
                return TypedResults.NoContent();
            }).AllowAnonymous();

        group.MapDelete("/{id}",
            async Task<Results<NoContent, NotFound>>
                (int id, ISqlConnectionFactory factory) =>
            {
                using var conn = factory.Create();
                conn.Open();
                var affected = await conn.ExecuteAsync(
                    "delete from dbo.Users where UserId = @ID", new { id });
                if (affected == 0) return TypedResults.NotFound();
                return TypedResults.NoContent();
            }).AllowAnonymous();

        return group;
    }
}