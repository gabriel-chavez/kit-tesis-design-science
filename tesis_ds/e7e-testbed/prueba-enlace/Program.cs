using System;
using System.ServiceModel;
using NegociosPagoSimple.wsUniQrService;

namespace PruebaEnlaceReal
{
    /// <summary>
    /// PRUEBA DE ENLACE CON LOS CLIENTES WCF REALES.
    ///
    /// Usa las MISMAS clases generadas que la produccion:
    ///   - wsUniQrService.QRServiceClient  -> CPagoSimple.cs:1096 (BUSA)
    ///   - wsFirmaDigital.IwsFirmaDigitalClient -> CPagoSimple.cs:1119 (firma)
    ///
    /// Sin una sola linea propia en la capa de transporte. Si estos clientes
    /// enlazan contra los dobles, el CPagoSimple real tambien.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            var urlBusa = args.Length > 0 ? args[0] : "http://localhost:5098/UNIQRService/UNIQRService.svc";
            var id = args.Length > 1 ? args[1] : "ENLACE-1";
            var veces = args.Length > 2 ? int.Parse(args[2]) : 1;

            Console.WriteLine("=================================================================");
            Console.WriteLine("PRUEBA DE ENLACE CON EL CLIENTE WCF REAL DE BUSA");
            Console.WriteLine("BUSA  : " + urlBusa);
            Console.WriteLine("=================================================================");
            Console.WriteLine();

            var ok = ProbarBusa(urlBusa, id, veces);

            Console.WriteLine();
            Console.WriteLine(ok ? "RESULTADO GLOBAL: el cliente real de BUSA enlaza" : "RESULTADO GLOBAL: FALLO");
            return ok ? 0 : 1;
        }

        private static bool ProbarBusa(string url, string id, int veces)
        {
            Console.WriteLine();
            Console.WriteLine("--- 2. wsUniQrService.QRServiceClient.BunApi (" + veces + " invocaciones) ---");
            var binding = Binding();
            int ok = 0;

            for (int i = 1; i <= veces; i++)
            {
                try
                {
                    var json = "{\"IdOperacionLogica\":\"" + id + "\",\"Servicio\":\"UNIVIDA_QR_001\"}";
                    using (var cliente = new QRServiceClient(binding, new EndpointAddress(url)))
                    {
                        var r = cliente.BunApi(json);
                        Console.WriteLine("  invocacion #" + i + ": OK  " + Recortar(r, 120));
                        ok++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  invocacion #" + i + ": EXCEPCION " + ex.GetType().Name);
                    Console.WriteLine("    " + ex.Message.Split('\n')[0]);
                }
            }

            Console.WriteLine("  exitosas: " + ok + " / " + veces);
            return ok == veces;
        }

        private static BasicHttpBinding Binding() => new BasicHttpBinding(BasicHttpSecurityMode.None)
        {
            OpenTimeout = TimeSpan.FromSeconds(5),
            CloseTimeout = TimeSpan.FromSeconds(5),
            SendTimeout = TimeSpan.FromSeconds(15),
            ReceiveTimeout = TimeSpan.FromSeconds(15),
            MaxReceivedMessageSize = 4 * 1024 * 1024,
            MaxBufferSize = 4 * 1024 * 1024
        };

        private static string Recortar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "(vacia)";
            s = s.Replace("\r", "").Replace("\n", " ");
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }
    }
}