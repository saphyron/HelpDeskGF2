using Dapper;
using HelpDesk.Data;
using HelpDesk.Domain;
using HelpDesk.Security;
using Microsoft.AspNetCore.Http;

namespace HelpDesk.Core;

public class UserCore
{
    private readonly ISqlConnectionFactory _factory;

    public UserCore(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    /* ---------- AUTH ---------- */

    public async Task<LoginResponse?> Login(
        LoginRequest body,
        HttpContext context)
    {
        using var conn = _factory.Create();
        conn.Open();

        var user = await conn.QuerySingleOrDefaultAsync<UserAuthRow>(
            """
            select UserId, Username, Password, Role, Name, Hold, StudieRetning
            from dbo.Users
            where Username = @Username
            """,
            new { body.Username });

        if (user is null)
            return null;

        bool validPassword;

        // TJEK KUN DATABASEVÆRDIEN
        if (SecurityFunctions.LooksLikeSha256Hash(user.Password))
        {
            // Allerede hashed → normal verificering
            validPassword = SecurityFunctions.VerifyPassword(
                user.Username,
                body.Password,
                user.Password
            );
        }
        else
        {
            // Legacy plain‑text password
            validPassword = string.Equals(
                user.Password,
                body.Password,
                StringComparison.Ordinal
            );

            // Lazy migration
            if (validPassword)
            {
                var hashed = SecurityFunctions.HashPassword(
                    user.Username,
                    body.Password
                );

                await conn.ExecuteAsync(
                    "update dbo.Users set Password = @Password where UserId = @Id",
                    new { Password = hashed, Id = user.Id }
                );
            }
        }

        if (!validPassword)
            return null;

        /*SecurityFunctions.CreateSession(
            context,
            user.Id,
            user.Username,
            user.Role
        );*/

        return new LoginResponse
        {
            User = new UserSummary
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                Name = user.Name,
                Hold = user.Hold,
                StudieRetning = user.StudieRetning
            }
        };
    }

    /* ---------- USERS ---------- */

    public async Task<IEnumerable<AppUser>> GetAll()
    {
        using var conn = _factory.Create();
        conn.Open();

        return await conn.QueryAsync<AppUser>(
            """
            select UserId, Username, Password, Role, Name, Hold, StudieRetning
            from dbo.Users
            order by UserId
            """
        );
    }

    public async Task<AppUser?> GetById(int id)
    {
        using var conn = _factory.Create();
        conn.Open();

        return await conn.QuerySingleOrDefaultAsync<AppUser>(
            """
            select UserId, Username, Password, Role, Name, Hold, StudieRetning
            from dbo.Users
            where UserId = @UserId
            """,
            new { UserId = id });
    }

    public async Task<AppUser?> Create(CreateUserRequest req)
    {
        using var conn = _factory.Create();
        conn.Open();

        try
        {
            var hashedPassword =
                SecurityFunctions.HashPassword(req.Username, req.Password);

            var sql = """
                insert into dbo.Users (Username, Password)
                values (@Username, @Password);
                select cast(scope_identity() as int);
            """;

            var newId = await conn.ExecuteScalarAsync<int>(sql, new
            {
                req.Username,
                Password = hashedPassword
            });

            return new AppUser
            {
                UserId = newId,
                Username = req.Username,
                Role = "user"
            };
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
            when (ex.Number is 2627 or 2601)
        {
            return null;
        }
    }

    public async Task<bool> UpdateAdmin(int id, UpdateUserRequestAdmin req)
    {
        using var conn = _factory.Create();
        conn.Open();

        var exists = await conn.ExecuteScalarAsync<int>(
            "select count(1) from dbo.Users where UserId = @ID",
            new { ID = id });

        if (exists == 0)
            return false;

        string? hashedPassword = null;

        if (!string.IsNullOrWhiteSpace(req.Password) &&
            !string.IsNullOrWhiteSpace(req.Username))
        {
            hashedPassword =
                SecurityFunctions.HashPassword(req.Username, req.Password);
        }

        var sql = """
            update dbo.Users
            set Username = coalesce(@Username, Username),
                Password = coalesce(@Password, Password),
                Role = coalesce(@Role, Role),
                Name = coalesce(@Name, Name),
                Hold = coalesce(@Hold, Hold),
                StudieRetning = coalesce(@StudieRetning, StudieRetning)
            where UserId = @UserId;
        """;

        await conn.ExecuteAsync(sql, new
        {
            UserId = id,
            req.Username,
            Password = hashedPassword,
            req.Role,
            req.Name,
            req.Hold,
            req.StudieRetning
        });

        return true;
    }

    public async Task<bool> Delete(int id)
    {
        using var conn = _factory.Create();
        conn.Open();

        var affected = await conn.ExecuteAsync(
            "delete from dbo.Users where UserId = @ID",
            new { ID = id });

        return affected == 1;
    }

    public async Task<LoginResponse> LoginAsGuest(HttpContext context)
{
    using var conn = _factory.Create();
    conn.Open();

    var guest = await conn.QuerySingleAsync<UserAuthRow>(
        """
        select UserId, Username, Role, Name, Hold, StudieRetning
        from dbo.Users
        where UserId = 0
        """
    );

    // ✅ Opret session som om brugeren var logget ind
    SecurityFunctions.CreateSession(
        context,
        guest.Id,
        guest.Username,
        guest.Role
    );

    return new LoginResponse
    {
        User = new UserSummary
        {
            UserId = guest.Id,
            Username = guest.Username,
            Role = guest.Role,
            Name = guest.Name,
            Hold = guest.Hold,
            StudieRetning = guest.StudieRetning
        }
    };
}
}
