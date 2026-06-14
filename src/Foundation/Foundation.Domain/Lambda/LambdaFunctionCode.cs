namespace Foundation.Domain.Lambda;

/// <summary>
/// A read-only view of a Lambda function's deployed package and entry point, shown on the function
/// detail "Code" tab so a developer can inspect what is deployed without leaving the UI.
/// </summary>
/// <param name="FunctionName">The unique name of the function.</param>
/// <param name="Runtime">The managed runtime identifier, for example <c>python3.12</c>; empty for image packages or when not reported.</param>
/// <param name="Handler">The entry point the runtime invokes, for example <c>index.handler</c>; empty when not reported.</param>
/// <param name="PackageType">The deployment package type, either <c>Zip</c> or <c>Image</c>; empty when not reported.</param>
/// <param name="CodeSize">The size of the deployed package in bytes.</param>
/// <param name="CodeSha256">The base64 SHA-256 hash of the deployed package; empty when not reported.</param>
/// <param name="RepositoryType">The repository hosting the code, for example <c>S3</c> or <c>ECR</c>; empty when not reported.</param>
/// <param name="Location">The download location for a zip package, or the resolved image URI for an image package; empty when not reported.</param>
/// <param name="ImageUri">The container image URI for an image package; empty for zip packages.</param>
public sealed record LambdaFunctionCode(
    string FunctionName,
    string Runtime,
    string Handler,
    string PackageType,
    long CodeSize,
    string CodeSha256,
    string RepositoryType,
    string Location,
    string ImageUri);
