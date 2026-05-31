namespace Foundation.Domain.Iam;

/// <summary>
/// A resource that consumes (references) an IAM role, such as a Lambda function that uses the role
/// as its execution role. The shape is deliberately generic so additional consumer types can be
/// reported by later tasks without changing the contract.
/// </summary>
/// <param name="ConsumerType">A human-readable description of the consumer kind, for example <c>Lambda function</c>.</param>
/// <param name="ResourceName">The name or identifier of the consuming resource, used as the reference for navigation.</param>
/// <param name="ServiceKey">The service the consumer belongs to, for example <c>lambda</c>, used to route the reference.</param>
public sealed record IamRoleConsumer(
    string ConsumerType,
    string ResourceName,
    string ServiceKey);
