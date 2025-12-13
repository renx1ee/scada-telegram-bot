using Opc.Ua;

namespace Renx1ee.OPCControl.Configurations;

public static class Config
{
    public static ApplicationConfiguration DefaultClient { get; } = CreateDefaultConfiguration(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pki"));
    public static string? ApplicationName { get; private set; } = "Renx1ee OPC Client";
    public static string? ApplicationUri { get; private set; } = $"urn:{Environment.MachineName}:TestClient";

    public static void Initialize(string applicationName, string applicationUri)
    {
        if (DefaultClient != null) 
            throw new InvalidOperationException("Конфигурация уже инициализирована");
        
        ApplicationName = ApplicationName ?? throw new ArgumentNullException(nameof(ApplicationName));
        ApplicationUri = ApplicationUri ?? throw new ArgumentNullException(nameof(ApplicationUri));
    }

    private static ApplicationConfiguration CreateDefaultConfiguration(string certBasePath)
    {
        var config = new ApplicationConfiguration
        {
            ApplicationName = ApplicationName,
            ApplicationType = ApplicationType.Client,
            ApplicationUri = ApplicationUri,

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
        
        config.Validate(ApplicationType.Client);
        
        return config;
    }
}
