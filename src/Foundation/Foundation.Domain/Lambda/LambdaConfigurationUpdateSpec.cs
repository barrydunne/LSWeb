namespace Foundation.Domain.Lambda;

/// <summary>
/// The configuration fields that can be updated on an existing Lambda function.
/// </summary>
/// <param name="FunctionName">The name of the function to update.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>.</param>
/// <param name="Handler">The entry point the runtime invokes.</param>
/// <param name="Role">The execution role ARN the function assumes.</param>
/// <param name="Description">The function description; empty when none is set.</param>
/// <param name="MemorySize">The memory to allocate to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
public sealed record LambdaConfigurationUpdateSpec(
    string FunctionName,
    string Runtime,
    string Handler,
    string Role,
    string Description,
    int MemorySize,
    int Timeout);
