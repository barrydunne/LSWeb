namespace Foundation.Domain.Lambda;

/// <summary>
/// The HTTP function URL configuration of a Lambda function, used for direct HTTP invocation.
/// </summary>
/// <param name="FunctionUrl">The HTTPS endpoint that invokes the function.</param>
/// <param name="AuthType">The authentication mode, either <c>NONE</c> (public) or <c>AWS_IAM</c> (signed requests).</param>
/// <param name="CreationTime">The timestamp the URL configuration was created; empty when not reported.</param>
/// <param name="LastModifiedTime">The timestamp the URL configuration was last updated; empty when not reported.</param>
public sealed record LambdaFunctionUrl(
    string FunctionUrl,
    string AuthType,
    string CreationTime,
    string LastModifiedTime);
