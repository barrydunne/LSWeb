namespace Foundation.Domain.Lambda;

/// <summary>
/// The fields required to create a new Lambda function from the console.
/// </summary>
/// <param name="FunctionName">The unique name of the function to create.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>.</param>
/// <param name="Handler">The entry point the runtime invokes.</param>
/// <param name="Role">The execution role ARN the function assumes.</param>
/// <param name="Description">The function description; empty when none is set.</param>
/// <param name="MemorySize">The memory to allocate to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
/// <param name="ZipFileBase64">The deployment package as a base64-encoded ZIP archive.</param>
public sealed record LambdaFunctionCreateSpec(
    string FunctionName,
    string Runtime,
    string Handler,
    string Role,
    string Description,
    int MemorySize,
    int Timeout,
    string ZipFileBase64);
