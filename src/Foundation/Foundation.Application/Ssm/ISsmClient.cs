using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Ssm;

namespace Foundation.Application.Ssm;

/// <summary>
/// Abstracts the SSM Parameter Store operations the application needs so the handlers stay free of
/// any direct AWS SDK dependency. The implementation flows every call through the resilient AWS
/// gateway and translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface ISsmClient
{
    /// <summary>
    /// List the parameters that live under the supplied path, optionally descending into the full
    /// hierarchy beneath it.
    /// </summary>
    /// <param name="path">The hierarchy path to browse, such as <c>/</c> or <c>/app/config</c>.</param>
    /// <param name="recursive">Whether to include parameters in nested paths beneath the path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parameters under the path, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<Parameter>>> GetParametersByPathAsync(
        string path, bool recursive, CancellationToken cancellationToken);

    /// <summary>
    /// Create or overwrite a parameter with the supplied specification.
    /// </summary>
    /// <param name="specification">The parameter to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> CreateParameterAsync(
        ParameterSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a parameter by name.
    /// </summary>
    /// <param name="name">The fully-qualified name of the parameter to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> DeleteParameterAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Read the current value of a parameter, decrypting SecureString values so the handler can
    /// decide whether to mask or reveal them.
    /// </summary>
    /// <param name="name">The fully-qualified name of the parameter to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parameter value, or an error when the backend cannot be reached.</returns>
    Task<Result<ParameterValue>> GetParameterValueAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Read the change history of a parameter, decrypting SecureString values so the handler can
    /// decide whether to mask or reveal them.
    /// </summary>
    /// <param name="name">The fully-qualified name of the parameter to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parameter history, or an error when the backend cannot be reached.</returns>
    Task<Result<ParameterHistoryList>> GetParameterHistoryAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Store a new value against an existing parameter, creating a new version.
    /// </summary>
    /// <param name="specification">The parameter name and value to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> PutParameterValueAsync(
        ParameterValueSpecification specification, CancellationToken cancellationToken);
}
