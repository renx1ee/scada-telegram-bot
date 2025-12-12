using Opc.Ua;

namespace Renx1ee.OPCControl.Configurations;

public static class Config
{
    public static ApplicationConfiguration Default { get; } = CreateDefaultConfiguration();
    public static string? ApplicationName { get; private set; } = "Renx1ee OPC Client";
    public static string? ApplicationUri { get; private set; } = $"urn:Renx1ee:{Environment.MachineName}:Client";

    public static void Initialize(string applicationName, string applicationUri)
    {
        if (Default != null) 
            throw new InvalidOperationException("Конфигурация уже инициализирована");
        
        ApplicationName = ApplicationName ?? throw new ArgumentNullException(nameof(ApplicationName));
        ApplicationUri = ApplicationUri ?? throw new ArgumentNullException(nameof(ApplicationUri));
    }

    private static ApplicationConfiguration CreateDefaultConfiguration()
    {
        var config = new ApplicationConfiguration()
        {
            ApplicationName = ApplicationName,
            ApplicationType = ApplicationType.Client,
            ApplicationUri = ApplicationUri,
            SecurityConfiguration = new SecurityConfiguration() { },
            TransportConfigurations = new TransportConfigurationCollection(),
            ClientConfiguration = new ClientConfiguration()
        };
        return config;
    }
}