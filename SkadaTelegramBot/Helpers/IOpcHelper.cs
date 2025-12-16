namespace SkadaTelegramBot_.Helpers;
/// <summary>
/// The class for receiving and sending values from the server via OPC protocol.
/// </summary>
public interface IOpcHelper
{
    /// <summary>
    /// Receive value from the server.
    /// </summary>
    /// <param name="nodeId">Unique identity key from a variable on the server.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>String if value is found; overwise returns <see cref="string.Empty"/>.</returns>
    Task<string> GetValueAsync(string nodeId, CancellationToken cancellationToken);
    /// <summary>
    /// Send value on the server.
    /// </summary>
    /// <param name="nodeId">Unique identity key from a variable on the server.</param>
    /// <param name="value">The value to be sent to the variable on the server.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns></returns>
    Task SendValueAsync(string nodeId, string value, CancellationToken cancellationToken);
}