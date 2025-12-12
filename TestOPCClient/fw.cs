using Opc.Ua;
using Opc.Ua.Client;

namespace TestOPCClient;

public class fw
{
     static async Task Mdain(string[] args)
    {
        Console.WriteLine("Start");

        string endpointUrl = "opc.tcp://192.168.1.10:4840";  // ← твой MasterSCADA

        // Папка, где будут лежать все сертификаты (создаётся автоматически рядом с exe)
        string certBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pki");

        var config = new ApplicationConfiguration
        {
            ApplicationName = "TestClient",
            ApplicationType = ApplicationType.Client,
            ApplicationUri = $"urn:{Environment.MachineName}:TestClient",

            SecurityConfiguration = new SecurityConfiguration
            {
                // ← Свой сертификат клиента
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(certBasePath, "own"),
                    SubjectName = "TestClient"
                },

                // ← Доверенные сертификаты серверов (сюда попадёт сертификат MasterSCADA)
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(certBasePath, "trusted")
                },

                // ← Сертификаты издателей (TrustedIssuerCertificates) — ОБЯЗАТЕЛЬНО на Linux/macOS!
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(certBasePath, "issuers")
                },

                // ← Отклонённые сертификаты
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(certBasePath, "rejected")
                },

                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024
            },

            TransportConfigurations = new TransportConfigurationCollection(),
            ClientConfiguration = new ClientConfiguration()
        };

        // Создаём папки, если их нет
        Directory.CreateDirectory(Path.Combine(certBasePath, "own", "certs"));
        Directory.CreateDirectory(Path.Combine(certBasePath, "own", "private"));
        Directory.CreateDirectory(Path.Combine(certBasePath, "trusted", "certs"));
        Directory.CreateDirectory(Path.Combine(certBasePath, "issuers", "certs"));
        Directory.CreateDirectory(Path.Combine(certBasePath, "rejected"));

        await config.Validate(ApplicationType.Client);

        var selectedEndpoint = CoreClientUtils.SelectEndpoint(config, endpointUrl, useSecurity: false);
        var endpoint = new ConfiguredEndpoint(null, selectedEndpoint);

        var session = await Session.Create(
            config,
            endpoint,
            updateBeforeConnect: false,
            sessionName: "TestSession",
            sessionTimeout: 60000,
            identity: new UserIdentity(),   // анонимно
            preferredLocales: null
        );

        Console.WriteLine("Подключено к MasterSCADA!");

        var value = session.ReadValue("ns=2;s=Температура");
        Console.WriteLine($"Температура = {value}");

        Console.WriteLine("Нажмите любую клавишу...");
        Console.ReadKey();

        await session.CloseAsync();
    }
}