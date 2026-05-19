namespace Foundation.Domain.Lambda;

/// <summary>
/// The full configuration of a single Lambda function shown on its detail view.
/// </summary>
/// <param name="FunctionName">The unique name of the function.</param>
/// <param name="FunctionArn">The Amazon Resource Name identifying the function.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>; empty when not reported.</param>
/// <param name="Handler">The entry point the runtime invokes; empty when not reported.</param>
/// <param name="Description">The function description; empty when none is set.</param>
/// <param name="LastModified">The timestamp the function configuration was last updated, as reported by AWS.</param>
/// <param name="MemorySize">The memory allocated to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
/// <param name="Role">The execution role ARN assumed by the function; empty when not reported.</param>
public sealed record LambdaFunctionDetail(
    string FunctionName,
    string FunctionArn,
    string Runtime,
    string Handler,
    string Description,
    string LastModified,
    int MemorySize,
    int Timeout,
    string Role);
