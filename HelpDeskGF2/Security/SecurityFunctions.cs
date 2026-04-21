using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace HelpDesk.Security;

public static class SecurityFunctions
{
    /* =========================
     *  PASSWORD HASHING (SHA256)
     * ========================= */

    // 🔐 Application-wide pepper
    // (i produktion bør denne ligge i appsettings / env-var)
    private const string Pepper = "GF2_HELPDESK_SECRET";

    /// <summary>
    /// Bruger til lazy migration:
    /// - SHA256 base64 er altid 44 tegn og slutter typisk med '='
    /// - Bruges kun som FORMAT-check (ikke sikkerhedsgaranti)
    /// </summary>
    
    public static bool LooksLikeSha256Hash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // SHA256 base64 = 44 chars
        if (value.Length != 44)
            return false;

        // Valid Base64
        Span<byte> buffer = stackalloc byte[32];
        return Convert.TryFromBase64String(value, buffer, out _);
    }


    /// <summary>
    /// Hasher password deterministisk vha. SHA256(username:password:pepper)
    /// </summary>
    public static string HashPassword(string username, string password)
    {
        var input = $"{username}:{password}:{Pepper}";
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verificerer et indtastet password mod et gemt hash
    /// </summary>
    public static bool VerifyPassword(
        string username,
        string password,
        string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        var computed = HashPassword(username, password);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computed),
            Convert.FromBase64String(storedHash));
    }

    /* =========================
     *  SESSION HANDLING
     * ========================= */

    private const string SessionUserId = "UserId";
    private const string SessionUsername = "Username";
    private const string SessionRole = "Role";

    public static void CreateSession(
        HttpContext context,
        int userId,
        string username,
        string? role)
    {
        context.Session.SetInt32(SessionUserId, userId);
        context.Session.SetString(SessionUsername, username);

        if (!string.IsNullOrWhiteSpace(role))
            context.Session.SetString(SessionRole, role);
    }

    public static void ClearSession(HttpContext context)
    {
        context.Session.Clear();
    }

    public static bool IsLoggedIn(HttpContext context)
    {
        return context.Session.GetInt32(SessionUserId) != null;
    }

    public static int? GetUserId(HttpContext context)
    {
        return context.Session.GetInt32(SessionUserId);
    }

    public static string? GetUserRole(HttpContext context)
    {
        return context.Session.GetString(SessionRole);
    }

    public static bool IsAdmin(HttpContext context)
    {
        return GetUserRole(context) == "admin";
    }

    /* =========================
     *  GUARDS (CONVENIENCE)
     * ========================= */

    public static bool RequireLogin(HttpContext context)
    {
        return IsLoggedIn(context);
    }

    public static bool RequireAdmin(HttpContext context)
    {
        return IsAdmin(context);
    }
}