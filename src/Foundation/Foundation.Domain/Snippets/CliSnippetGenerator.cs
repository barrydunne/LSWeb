using System.Text;

namespace Foundation.Domain.Snippets;

/// <summary>
/// Generates AWS CLI snippets that reproduce an operation against a configured endpoint.
/// Sensitive parameter values are never embedded; placeholders are emitted in their place.
/// </summary>
public static class CliSnippetGenerator
{
    /// <summary>
    /// Generate a runnable AWS CLI snippet for the supplied operation and connection context.
    /// </summary>
    /// <param name="operation">The operation to reproduce.</param>
    /// <param name="context">The endpoint, region and optional profile to target.</param>
    /// <returns>The generated <see cref="CliSnippet"/>.</returns>
    public static CliSnippet Generate(CliOperation operation, CliConnectionContext context)
    {
        var builder = new StringBuilder("aws ")
            .Append(operation.Service)
            .Append(' ')
            .Append(operation.Operation);

        foreach (var parameter in operation.Parameters)
        {
            builder
                .Append(" --")
                .Append(parameter.Name)
                .Append(' ')
                .Append(RenderValue(parameter));
        }

        builder
            .Append(" --endpoint-url ")
            .Append(context.Endpoint)
            .Append(" --region ")
            .Append(context.Region);

        if (!string.IsNullOrWhiteSpace(context.Profile))
        {
            builder
                .Append(" --profile ")
                .Append(context.Profile);
        }

        return new CliSnippet(builder.ToString());
    }

    private static string RenderValue(CliParameter parameter)
    {
        if (parameter.IsSensitive || string.IsNullOrWhiteSpace(parameter.Value))
        {
            return $"<{parameter.Name}>";
        }

        return parameter.Value.Contains(' ', StringComparison.Ordinal)
            ? $"\"{parameter.Value}\""
            : parameter.Value;
    }
}
