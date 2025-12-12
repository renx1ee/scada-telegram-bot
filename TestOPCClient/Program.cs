using System.Net;
using Opc.Ua;
using Opc.Ua.Client;
using TestOPCClient.Common;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting...");
        Console.WriteLine($"Current URL: {Constants.ENDPOINT_URL}");
        
        // 1. Create a config.
        var config = new ApplicationConfiguration()
        {
            ApplicationName = Constants.APPLICATION_NAME,
            ApplicationType = ApplicationType.Client,
            ApplicationUri = Constants.APPLICATION_URI,
            SecurityConfiguration = new SecurityConfiguration()
            {
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            ClientConfiguration = new ClientConfiguration()
        };
        
        // 2. Create folders if not exists.
        
        // 3. Validation.
        await config.Validate(ApplicationType.Client);
        
        // 4. Try to connect the host.
        await TryToConnect(Constants.ENDPOINT_URL);

        try
        {
            // 5. Search and select the certain Endpoint on the MasterSCADA server.
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(
                application: config,
                discoveryUrl: Constants.ENDPOINT_URL,
                useSecurity: false);
            // 6. Wrap the selected point in an object that accept Session.Create.
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint);
            // 7. Create a session.
            var session = await Session.Create(
                config,
                endpoint: endpoint,
                updateBeforeConnect: false,
                sessionName: Constants.SESSION_NAME,
                sessionTimeout: Constants.SESSION_TIMEOUT,
                identity: new UserIdentity(),
                preferredLocales: null
            );

            if (session == null)
            {
                Console.WriteLine("session is null");
                return;
            }
            
            if (!session.Connected)
            {
                Console.WriteLine("session.Connected == false");
                return;
            }

            Console.WriteLine($"Session ID: {session.SessionId}");

            try
            {
                // This variable has any server
                var serverStatus = session.ReadValue("i=2258");
                Console.WriteLine("Status: " + serverStatus);
                /*Console.WriteLine($"Time of server: {((ServerStatusDataType)serverStatus.Value).StartTime}");
                Console.WriteLine($"Server statue: {((ServerStatusDataType)serverStatus.Value).State}");*/
            }
            catch (Exception e)
            {
                Console.WriteLine("Session is not working");
                return;
            }
            // 8. Try to read value
            try
            {
                Console.WriteLine("Tag name: 'ns=1;i=39153'");
                var nodeId = new NodeId("ns=1;i=39153");
            }
            finally
            {
                session?.Close();
            }
        }
        catch (ServiceResultException e) when (e.StatusCode == StatusCodes.BadSecureChannelClosed)
        {
            Console.WriteLine("Exception: ", e);
            return;
        }
        
        Console.WriteLine("Press any key to...");
    }

    private static async Task TryToConnect(string url)
    {
        var uri = new Uri(url);
        Console.WriteLine($"Attempt to connect to: {uri.Host}");
        try
        {
            var ip = await Dns.GetHostAddressesAsync(uri.Host);
            Console.WriteLine($"DNS resolved in: {ip}");
        }
        catch (Exception e)
        {
            Console.WriteLine("DNS wasn't resolved: " + e.Message);
        }
    }
}