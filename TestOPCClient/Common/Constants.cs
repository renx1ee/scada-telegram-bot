namespace TestOPCClient.Common;

public static class Constants
{
    public const string ENDPOINT_URL              = "opc.tcp://192.168.1.10:16550";
    public const string APPLICATION_NAME          = "TestClient";
    public const string SESSION_NAME              = "TestSessionName";
    public const int SESSION_TIMEOUT              = 60000;
    public const string SERVER_VARIABLE_1         = "ns=1;i=39153";
    public static readonly string APPLICATION_URI = $"urn:{Environment.MachineName}:TestClient";
}