namespace Foundation.Domain.CloudFormation;

/// <summary>
/// An output a CloudFormation stack exposes.
/// </summary>
/// <param name="OutputKey">The output name.</param>
/// <param name="OutputValue">The output value.</param>
/// <param name="Description">The output description, where one was provided.</param>
/// <param name="ExportName">The export name the output is published under, where one was provided.</param>
public sealed record StackOutput(
    string OutputKey,
    string OutputValue,
    string? Description,
    string? ExportName);
