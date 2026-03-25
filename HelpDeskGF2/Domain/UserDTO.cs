namespace HelpDesk.Domain;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";       // plain i første omgang (paritet) -> skift til hash senere
    public string Role { get; set; } = "User";         // "admin" | "user" | ...
    public string? Name { get; set; }             // visningsnavn
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserSummary User { get; set; } = new();
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class UserSummary
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Name { get; set; }
}
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string PasswordClear { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? PasswordClear { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
}

public class UserAuthRow
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordClear { get; set; } = "";
    public string? Role { get; set; }
    public string? Name { get; set; }
}