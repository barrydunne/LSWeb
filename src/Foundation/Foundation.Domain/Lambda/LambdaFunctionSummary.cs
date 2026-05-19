namespace Foundation.Domain.Lambda;

/// <summary>
/// A concise view of a Lambda function as it appears in a function list.
/// </summary>
/// <param name="FunctionName">The unique name of the function.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>; empty when not reported.</param>
/// <param name="Description">The function description; empty when none is set.</param>
/// <param name="LastModified">The timestamp the function configuration was last updated, as reported by AWS.</param>
/// <param name="MemorySize">The memory allocated to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
public sealed record LambdaFunctionSummary(
    string FunctionName,
    string Runtime,
    string Description,
    string LastModified,
    int MemorySize,
    int Timeout);
