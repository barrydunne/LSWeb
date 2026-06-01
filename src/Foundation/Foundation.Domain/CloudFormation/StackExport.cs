namespace Foundation.Domain.CloudFormation;

/// <summary>
/// A value a CloudFormation stack publishes for other stacks to import via <c>Fn::ImportValue</c>.
/// </summary>
/// <param name="Name">The export name, unique within the account and region.</param>
/// <param name="Value">The exported value.</param>
/// <param name="ExportingStackId">The Amazon Resource Name of the stack that publishes the export.</param>
public sealed record StackExport(
    string Name,
    string Value,
    string ExportingStackId);
