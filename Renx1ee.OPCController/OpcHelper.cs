using System.Net;
using Opc.Ua;
using Opc.Ua.Client;

namespace Renx1ee.OPCControl;
// TODO IOpcHelper
public class OpcHelper 
{
    private readonly string _endpointUrl;
    private readonly ApplicationConfiguration _configuration;
    private static uint _sessionTimeout = 60000;

    public OpcHelper(
        string endpointUrl,
        ApplicationConfiguration configuration)
    {
        this._endpointUrl = endpointUrl;
        this._configuration = configuration;
    }
    
    public async Task<string> GetValueAsync(
        string nodeId,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting...");
        Console.WriteLine($"Current URL: {_endpointUrl}");
        await _configuration.Validate(ApplicationType.Client);
        
        await TryToConnect(_endpointUrl);

        try
        {
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(
                application: _configuration,
                discoveryUrl: _endpointUrl,
                useSecurity: false);
            
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint);
            
            var session = await Session.Create(
                configuration: _configuration,
                endpoint: endpoint,
                updateBeforeConnect: false,
                sessionName: Guid.NewGuid().ToString(),
                sessionTimeout: _sessionTimeout, 
                identity: new UserIdentity(),
                preferredLocales: null,
                ct: cancellationToken
            );
            
            if(await SessionValidation(session) == false)
                return string.Empty;
            
            try
            {
                Console.WriteLine($"Node Id: '{nodeId}'");
                var result = await session.ReadValueAsync(nodeId);
                Console.WriteLine($"Node result: {result.Value}");
                return result.Value.ToString();
            }
            finally
            {
                session?.CloseAsync(cancellationToken);
            }
        }
        catch (ServiceResultException e) when (e.StatusCode == StatusCodes.BadSecureChannelClosed)
        {
            Console.WriteLine("Exception: ", e);
            return string.Empty;
        }
    }

    public async Task SendValueAsync(
        string nodeId, 
        string value, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting...");
        Console.WriteLine($"Current URL: {_endpointUrl}");
        try
        {
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(
                application: _configuration,
                discoveryUrl: _endpointUrl,
                useSecurity: false);
            
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint);
            
            var session = await Session.Create(
                configuration: _configuration,
                endpoint: endpoint,
                updateBeforeConnect: false,
                sessionName: "", // TODO:
                sessionTimeout: 60000, // TODO:
                identity: new UserIdentity(),
                preferredLocales: null,
                ct: cancellationToken
            );
            
            if(await SessionValidation(session) == false)
                return;
            
            try
            {
                Console.WriteLine($"Send on node Id: '{nodeId}'");
                
                var nodesToWrite = new WriteValueCollection()
                {
                    new WriteValue()
                    {
                        NodeId = new NodeId(nodeId),
                        Value = new DataValue(value)
                    }
                };
                
                var result = session.WriteAsync(
                    requestHeader: null,
                    nodesToWrite: nodesToWrite,
                    ct: cancellationToken
                );
            }
            finally
            {
                session?.CloseAsync(cancellationToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task TryToConnect(string url)
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

    private Task<bool> SessionValidation(Session? session)
    {
        if (session == null)
        {
            Console.WriteLine("session is null");
            return Task.FromResult(false);
        }
        if (!session.Connected)
        {
            Console.WriteLine("session.Connected == false");
            return Task.FromResult(false);
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
            return Task.FromResult(false);
        }
        Console.WriteLine("Session validation is success!");

        return Task.FromResult(true);
    }
}