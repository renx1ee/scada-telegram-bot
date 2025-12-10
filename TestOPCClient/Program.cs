using Opc.Ua;
using Opc.Ua.Client;

public class Program
{
    private static async Task Main(string[] args)
    {
        string endpointUrl = "opc.tcp://localhost:16550";
        bool useSecurity  = false;
        
        Console.WriteLine("Start");

        var config = new ApplicationConfiguration()
        {
            ApplicationName = "MasterScadaClient",
            ApplicationType = ApplicationType.Client,
            ApplicationUri = $"urn:{Environment.MachineName}:MasterScadaClient",
            SecurityConfiguration = new SecurityConfiguration
            {
                AutoAcceptUntrustedCertificates = true // Ключевая строчка
                                                       // Без неё при первом подключении ты получишь ошибку "сертификат сервера не доверенный".
                                                       // Эта строка автоматически говорит: "Да, я доверяю сертификату MasterSCADA, даже если он самоподписанный".
            },
            ClientConfiguration = new ClientConfiguration()
        };
        // Проверка, что конфигурация корректна (создаёт папки для сертификатов, проверяет права и т.д.).
        await config.Validate(ApplicationType.Client);
        // Ищем и выбираем подходящую конечную точку (Endpoint) на сервере MasterSCADA.
        var selectedEndpoint = CoreClientUtils.SelectEndpoint(config, endpointUrl, false);
        // Оборачиваем выбранный endpoint в объект, который понимает Session.Create.
        var endpoint = new ConfiguredEndpoint(null, selectedEndpoint);
        // Создаём сессию — это и есть живое подключение к MasterSCADA
        var session = await Session.Create(
            configuration: config, 
            endpoint: endpoint, 
            updateBeforeConnect: false, 
            sessionName: "", 
            sessionTimeout: 60000, 
            identity: new UserIdentity(), 
            preferredLocales: null);

        Console.WriteLine("Подключено к MasterSCADA!");
        try
        {
            // Пример чтения тега
            /*var nodeId = new NodeId("ns=2;s=Температура");
            var readValue = new ReadValueId
            {
                NodeId = nodeId,
                AttributeId = Attributes.Value
            };*/
            
            var value = session.ReadValue("ns=2;s=Температура");
            Console.WriteLine("Температура = " + value);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            Console.WriteLine("Завершение...");
            session?.Close();
        }
    }
}
