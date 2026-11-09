using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BusaEvalStub;

/// <summary>
/// Adaptador SOAP del doble.
///
/// POR QUE EXISTE: el cliente REAL de produccion (CPagoSimple.cs:1096)
/// es un cliente WCF que habla SOAP con el contrato
/// wsUniQrService.IQRService, operacion BunApi(string) -> string.
///
/// El doble se aloja en ASP.NET Core en lugar de en un ServiceHost de WCF
/// porque ServiceHost usa HttpListener con el prefijo comodin http://+:PUERTO,
/// y esa reserva de URL exige elevacion, que no hay en esta maquina.
///
/// Esto NO reduce la fidelidad del transporte: el cliente real sigue
/// enviando SOAP sobre HTTP y sigue recibiendo SOAP sobre HTTP. Los fallos
/// (timeout, conexion abortada) se siguen produciendo en el nivel de socket.
/// Lo que no se reproduce es el stack de servidor WCF del BUSA, que es un
/// detalle del servidor y no del protocolo que se evalua.
///
/// CONTRATO, reproducido exactamente del Reference.cs generado:
///   Namespace   = http://tempuri.org/IQRService
///   Operacion   = BunApi
///   Parametro   = Data   (string)
///   Resultado   = BunApiResult (string)
/// </summary>
public static class SoapBusa
{
    // El cliente generado declara ServiceContract(ConfigurationName=...) sin Namespace,
    // por lo que WCF usa el namespace POR DEFECTO http://tempuri.org/ para el
    // elemento de respuesta, aunque el Action sea http://tempuri.org/IQRService/BunApi.
    // Reproducir el namespace equivocado hace que OperationFormatter rechace la respuesta.
    private const string NsRespuesta = "http://tempuri.org/";


    /// <summary>Extrae el contenido de &lt;Data&gt; del sobre de peticion.</summary>
    public static bool EsPeticionBunApi(string cuerpo, out string data)
    {
        data = null;
        if (string.IsNullOrWhiteSpace(cuerpo)) return false;
        if (!cuerpo.Contains("BunApi")) return false;

        // Regex tolerante: noDepends de que el cliente use prefijos ns0/soapenv.
        var m = Regex.Match(cuerpo,
            @"<(?<p>[A-Za-z0-9_]+:)?Data\b[^>]*>(?<v>.*?)</(?<p>[A-Za-z0-9_]+:)?Data>",
            RegexOptions.Singleline);
        if (m.Success)
        {
            data = System.Net.WebUtility.HtmlDecode(m.Groups["v"].Value.Trim());
            return true;
        }
        return false;
    }

    /// <summary>Construye el sobre de respuesta que el cliente WCF espera.</summary>
    public static string EnvolturaRespuesta(string resultado)
    {
        var valor = resultado ?? string.Empty;
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
               "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
               "<soap:Body>" +
               "<BunApiResponse xmlns=\"http://tempuri.org/\">" +
               "<BunApiResult>" + Escape(valor) + "</BunApiResult>" +
               "</BunApiResponse>" +
               "</soap:Body>" +
               "</soap:Envelope>";
    }

    /// <summary>Construye un sobre de fallo (Fault), que el cliente ve como excepcion.</summary>
    public static string EnvolturaFault(string mensaje)
    {
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
               "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
               "<soap:Body>" +
               "<soap:Fault>" +
               "<faultcode>soap:Server</faultcode>" +
               "<faultstring>" + Escape(mensaje) + "</faultstring>" +
               "</soap:Fault>" +
               "</soap:Body>" +
               "</soap:Envelope>";
    }

    private static string Escape(string s) =>
        (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}