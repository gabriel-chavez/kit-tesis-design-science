using System.Text;
namespace ArnesE7e;
/// <summary>
/// Controlador de la doble de evaluacion y de los escenarios.
///
/// NOTA SOBRE LA EJECUCION: este archivo arranca y detiene el doble
/// externo (o lo reutiliza si ya esta levantado) para que cada corrida
/// sea reproducible.
/// </summary>
public static class Doble
{
    public const string BaseUrl = "http://localhost:5098";
    public static void Reiniciar()
    {
        using var c = new HttpClient();
        try
        {
            c.PostAsync($"{BaseUrl}/admin/reiniciar", null).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"El doble de evaluacion no responde en {BaseUrl}. Inicialo con: " +
                $"dotnet run --project busa-eval-stub --urls \"{BaseUrl}\". Detalle: {ex.Message}");
        }
    }
    public static void ConfigurarFallo(string modo, string id, int veces) =>
        Post($"{BaseUrl}/admin/fallo?modo={modo}&id={Uri.EscapeDataString(id)}&veces={veces}");
    public static void AvanzarReloj(int minutos) =>
        Post($"{BaseUrl}/admin/reloj/avanzar?minutos={minutos}");
    public static void Pagar(string id) => Post($"{BaseUrl}/admin/pagar?id={Uri.EscapeDataString(id)}");
    public static void Vencer(string id) => Post($"{BaseUrl}/admin/vencer?id={Uri.EscapeDataString(id)}");
    public static void DejarPendiente(string id) => Post($"{BaseUrl}/admin/pendiente?id={Uri.EscapeDataString(id)}");
    /// <summary>Estado crudo que reporta el externo (truth externa).</summary>
    public static string EstadoExterno(string id)
    {
        var v = Volcado(id);
        foreach (var l in v.Split('\n'))
        {
            var t = l.Trim();
            if (!t.StartsWith("estado=")) continue;
            return t.Substring("estado=".Length).Split(' ')[0];
        }
        return "DESCONOCIDO";
    }

    public static string Volcado(string id)
    {
        using var c = new HttpClient();
        return c.GetStringAsync($"{BaseUrl}/admin/volcado/{Uri.EscapeDataString(id)}").GetAwaiter().GetResult();
    }
    private static void Post(string url)
    {
        using var c = new HttpClient();
        var r = c.PostAsync(url, null).GetAwaiter().GetResult();
        if (!r.IsSuccessStatusCode)
            throw new InvalidOperationException($"Fallo {r.StatusCode} en {url}: {r.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
    }
}
/// <summary>Utilidades de presentacion de evidencia.</summary>
