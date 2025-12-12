/*using System;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace MasterScadaOpcUaClient
{
    class Program
    {
        static async Task Maine(string[] args)
        {
            // URL сервера MasterSCADA (замените на ваш IP/порт)
            string endpointUrl = "opc.tcp://localhost:4840";  // Пример: opc.tcp://192.168.1.10:4840

            // Создаём конфигурацию приложения (анонимная аутентификация; для сертификатов — см. ниже)
            var config = new ApplicationConfiguration
            {
                ApplicationName = "MasterSCADA Client",
                ApplicationType = ApplicationType.Client,
                ApplicationUri = new Uri("urn:example:MasterSCADA:Client"),
                ProductUri = new Uri("urn:example:MasterSCADA:Client"),
                SecurityConfiguration = new SecurityConfiguration { /* Настройки безопасности #1# },
                TransportConfigurations = new TransportConfigurationCollection(),
                ClientConfiguration = new ClientConfiguration()
            };

            await config.Validate(ApplicationType.Client);  // Валидация

            // Создаём сессию
            var endpoint = CoreClientUtils.SelectEndpoint(endpointUrl, useSecurity: false);  // useSecurity: true для HTTPS-подобного
            var session = await Session.Create(config, new ConfiguredEndpoint(null, endpoint), false, "MySession",
                60000, new UserIdentity(), null);  // Таймаут 60 сек

            try
            {
                // Пример: Чтение значения тега (NodeId из MasterSCADA)
                var nodeId = new NodeId("ns=2;s=Температура_печи_1");  // Замените на ваш тег
                var readValue = new ReadValueId { NodeId = nodeId, AttributeId = Attributes.Value };

                var readRequest = new ReadRequest
                {
                    NodesToRead = new ReadValueIdCollection { readValue },
                    TimestampsToReturn = TimestampsToReturn.Both,
                    MaxAge = 0
                };

                var readResult = await session.ReadAsync(null, 0, TimestampsToReturn.Both, new ReadValueIdCollection { readValue }, null);

                if (StatusCode.IsGood(readResult.Results[0].StatusCode))
                {
                    var value = readResult.Results[0].Value;
                    Console.WriteLine($"Значение тега '{nodeId}': {value} (тип: {value?.GetType().Name})");
                }
                else
                {
                    Console.WriteLine($"Ошибка чтения: {readResult.Results[0].StatusCode}");
                }

                // Пример записи (если нужно)
                // var writeValue = new WriteValue { NodeId = nodeId, AttributeId = Attributes.Value, Value = new Variant(25.5f) };
                // await session.WriteAsync(null, new WriteValueCollection { writeValue }, null);

                Console.WriteLine("Нажмите Enter для выхода...");
                Console.ReadLine();
            }
            finally
            {
                session?.Close();
            }
        }
    }
}*/