namespace HelpDesk.Domain;

/* ===============================
 *  DATABASE / DOMAIN MODEL
 * =============================== */

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";

    // Indeholder SHA256-hash + salt, ikke plain password
    public string Password { get; set; } = "";

    // "admin" | "user"
    public string Role { get; set; } = "user";

    // Visningsdata
    public string? Name { get; set; }
    public string? Hold { get; set; }
    public string? StudieRetning { get; set; }
}

/* ===============================
 *  AUTH
 * =============================== */

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // plain → hashes i backend
}

public class LoginResponse
{
    // ✅ Ikke brugt endnu – placeholder til evt. JWT senere
    public string Token { get; set; } = string.Empty;

    public UserSummary User { get; set; } = new();
}

/* ===============================
 *  VIEW / SESSION DATA
 * =============================== */

public class UserSummary
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Name { get; set; }
    public string? Hold { get; set; }
    public string? StudieRetning { get; set; }
}


/* ===============================
 *  CREATE / UPDATE REQUESTS
 * =============================== */

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // hashes før insert
}

public class UpdateUserRequest
{
    public int UserId { get; set; }
    public string? Password { get; set; } // optional
    public string? Name { get; set; }
    public string? Hold { get; set; }
    public string? StudieRetning { get; set; }
}

public class UpdateUserRequestAdmin
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
    public string? Hold { get; set; }
    public string? StudieRetning { get; set; }
}

/* ===============================
 *  AUTH QUERY RESULT
 * =============================== */

// Bruges kun internt ved login (DB → Core)
public class UserAuthRow
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = ""; // SHA256 hash
    public string? Role { get; set; }
    public string? Name { get; set; }
    public string? Hold { get; set; }
    public string? StudieRetning { get; set; }
}
