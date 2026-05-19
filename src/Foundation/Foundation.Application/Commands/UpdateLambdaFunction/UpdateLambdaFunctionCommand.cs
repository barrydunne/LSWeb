using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateLambdaFunction;

/// <summary>
/// Update the configuration of an existing Lambda function and, optionally, replace its code.
/// </summary>
/// <param name="FunctionName">The name of the function to update.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>.</param>
/// <param name="Handler">The entry point the runtime invokes.</param>
/// <param name="Role">The execution role ARN the function assumes.</param>
/// <param name="Description">The function description.</param>
/// <param name="MemorySize">The memory to allocate to the function in megabytes.</param>
/// <param name="Timeout">The function execution timeout in seconds.</param>
/// <param name="ZipFileBase64">An optional replacement deployment package as a base64-encoded ZIP archive; when null or empty the code is left unchanged.</param>
public record UpdateLambdaFunctionCommand(
    string FunctionName,
    string Runtime,
    string Handler,
    string Role,
    string Description,
    int MemorySize,
    int Timeout,
    string? ZipFileBase64) : ICommand;
