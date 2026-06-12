using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Cognito;

/// <summary>
/// Provides access to Amazon Cognito user pools on the configured backend.
/// </summary>
public interface ICognitoClient
{
    /// <summary>
    /// Lists the user pools available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The user pools, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<UserPoolSummary>>> ListUserPoolsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single user pool.
    /// </summary>
    /// <param name="id">The unique identifier of the user pool.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The user pool detail, or an error if the pool could not be read.</returns>
    Task<Result<UserPoolDetail>> GetUserPoolAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new user pool from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the user pool to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The identifier of the created user pool, or an error if it could not be created.</returns>
    Task<Result<string>> CreateUserPoolAsync(UserPoolSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a user pool by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="id">The unique identifier of the user pool to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the user pool could not be deleted.</returns>
    Task<Result> DeleteUserPoolAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the app clients configured within a user pool.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool whose app clients should be listed.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The app clients, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<UserPoolClientSummary>>> ListUserPoolClientsAsync(string userPoolId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single app client, including its client secret when present.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The unique identifier of the app client.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The app client detail, or an error if the client could not be read.</returns>
    Task<Result<UserPoolClientDetail>> GetUserPoolClientAsync(string userPoolId, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new app client within a user pool from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the app client to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The created app client detail, or an error if it could not be created.</returns>
    Task<Result<UserPoolClientDetail>> CreateUserPoolClientAsync(UserPoolClientSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the configuration of an existing app client.
    /// </summary>
    /// <param name="specification">The desired configuration of the app client to update.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the app client could not be updated.</returns>
    Task<Result> UpdateUserPoolClientAsync(UserPoolClientSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an app client from a user pool by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The unique identifier of the app client to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the app client could not be deleted.</returns>
    Task<Result> DeleteUserPoolClientAsync(string userPoolId, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Regenerates the client secret of an app client by recreating it with the same configuration,
    /// since Amazon Cognito does not support rotating a client secret in place. The recreated client
    /// has a new identifier and a freshly generated secret.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The unique identifier of the app client whose secret should be regenerated.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The recreated app client detail with its new secret, or an error if it could not be regenerated.</returns>
    Task<Result<UserPoolClientDetail>> RegenerateUserPoolClientSecretAsync(string userPoolId, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the users within a user pool.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool whose users should be listed.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The users, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<CognitoUserSummary>>> ListUsersAsync(string userPoolId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single user, including its attributes.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The user detail, or an error if the user could not be read.</returns>
    Task<Result<CognitoUserDetail>> GetUserAsync(string userPoolId, string username, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new user within a user pool from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the user to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The created user detail, or an error if it could not be created.</returns>
    Task<Result<CognitoUserDetail>> CreateUserAsync(CognitoUserSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a user from a user pool by its username. This action cannot be undone.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the user could not be deleted.</returns>
    Task<Result> DeleteUserAsync(string userPoolId, string username, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the password of a user, optionally marking it as permanent so no reset is required.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user.</param>
    /// <param name="password">The new password to assign.</param>
    /// <param name="permanent">Whether the password is permanent rather than a temporary one.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the password could not be set.</returns>
    Task<Result> SetUserPasswordAsync(string userPoolId, string username, string password, bool permanent, CancellationToken cancellationToken);

    /// <summary>
    /// Enables or disables a user account, controlling whether it is able to sign in.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the user belongs to.</param>
    /// <param name="username">The unique username of the user.</param>
    /// <param name="enabled">Whether the account should be enabled.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the account state could not be changed.</returns>
    Task<Result> SetUserEnabledAsync(string userPoolId, string username, bool enabled, CancellationToken cancellationToken);

    /// <summary>
    /// Requests bearer tokens for an app client using the admin user-password authentication flow,
    /// then decodes the claims of the issued identity token.
    /// </summary>
    /// <param name="userPoolId">The identifier of the user pool the app client belongs to.</param>
    /// <param name="clientId">The identifier of the app client to authenticate against.</param>
    /// <param name="username">The username to authenticate.</param>
    /// <param name="password">The password to authenticate with.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The issued tokens and decoded claims, or an error if authentication failed.</returns>
    Task<Result<TokenResult>> RequestTokenAsync(string userPoolId, string clientId, string username, string password, CancellationToken cancellationToken);
}
