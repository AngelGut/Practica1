using System.Security.Cryptography;
using System.Text;

namespace Practica1.Servicios;

internal static class AuthService
{
    // Base de datos simple en memoria (usuario → hash)
    private static readonly Dictionary<string, string> _usuarios = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = CalcularHash("admin123"), // usuario inicial
        ["docente"] = CalcularHash("profesor2024")
    };

    // Calcula hash SHA256 de la contraseña
    private static string CalcularHash(string texto)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
        return Convert.ToHexString(bytes);
    }

    // Verifica credenciales
    public static bool ValidarLogin(string usuario, string password)
    {
        if (!_usuarios.TryGetValue(usuario, out var hashGuardado))
            return false;

        var hashIngresado = CalcularHash(password);
        return hashGuardado.Equals(hashIngresado, StringComparison.OrdinalIgnoreCase);
    }
}
