using System.Net;
using Opc.Ua;
using Opc.Ua.Client;

namespace SkadaTelegramBot_.Helpers;

public class OpcHelper : IOpcHelper
{
    private readonly ILogger<OpcHelper> _logger;
    private readonly string _endpointUrl;
    private readonly ApplicationConfiguration _configuration;
    private readonly uint _sessionTimeout = 60000;

    public OpcHelper(
        ILogger<OpcHelper> logger,
        string endpointUrl,
        ApplicationConfiguration configuration)
    {
        this._logger = logger;
        this._endpointUrl = endpointUrl;
        this._configuration = configuration;
    }
    
    public async Task<string> GetValueAsync(
        string nodeId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Current URL: {_endpointUrl}");

        /*if (await TryToConnect(_endpointUrl))
        {
            return string.Empty;
        }*/

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
                _logger.LogInformation($"Node Id: '{nodeId}'");
                var result = await session.ReadValueAsync(nodeId);
                _logger.LogInformation($"Node result: {result.Value}");
                return result.Value.ToString();
            }
            finally
            {
                session?.CloseAsync(cancellationToken);
            }
        }
        catch (ServiceResultException e) when (e.StatusCode == Opc.Ua.StatusCodes.BadSecureChannelClosed)
        {
            _logger.LogError("Exception: " + e);
            return string.Empty;
        }
    }

    public async Task SendValueAsync(
        string nodeId, 
        string value, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Current URL: {_endpointUrl}");
        
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
                sessionTimeout: 60000, // TODO:
                identity: new UserIdentity(),
                preferredLocales: null,
                ct: cancellationToken
            );
            
            if(await SessionValidation(session) == false)
                return;
            
            try
            {
                _logger.LogInformation($"Send on node Id: '{nodeId}'");
                
                var nodesToWrite = new WriteValueCollection()
                {
                    new WriteValue()
                    {
                        NodeId = NodeId.Parse(nodeId),
                        AttributeId = Attributes.Value,
                        Value = new DataValue()
                        {
                            Value = new Variant((string)value),
                            StatusCode = Opc.Ua.StatusCodes.Good,
                            SourceTimestamp = DateTime.UtcNow
                        }
                    }
                };
                
                var response = await session.WriteAsync(
                    requestHeader: null,
                    nodesToWrite: nodesToWrite,
                    ct: cancellationToken
                );
                _logger.LogInformation("Server response: " + response.Results[0].ToString());
            }
            finally
            {
                session?.CloseAsync(cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error: " + e.Message);
        }
    }
    
    private async Task<bool> TryToConnect(string url)
    {
        var uri = new Uri(url);

        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogError($"Url is null: {url}");
            return false;
        }
        
        _logger.LogInformation($"Attempt to connect to: {uri.Host}");
        
        try
        {
            var ip = await Dns.GetHostAddressesAsync(uri.Host);
            _logger.LogInformation($"DNS resolved in: {ip}");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("DNS wasn't resolved: " + e.Message);
            return false;
        }
    }

    private Task<bool> SessionValidation(Session? session)
    {
        if (session == null)
        {
            _logger.LogInformation("session is null!");
            return Task.FromResult(false);
        }
        if (!session.Connected)
        {
            _logger.LogInformation("session.Connected == false");
            return Task.FromResult(false);
        }
        _logger.LogInformation($"Session ID: {session.SessionId}");
        try
        {
            // This variable has any server
            var serverStatus = session.ReadValue("i=2258");
            _logger.LogInformation("Status: " + serverStatus);
        }
        catch (Exception e)
        {
            _logger.LogError("Session is not working");
            return Task.FromResult(false);
        }
        _logger.LogInformation("Session validation is success!");
        return Task.FromResult(true);
    }
}