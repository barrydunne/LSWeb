using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RequestCertificate;

internal sealed partial class RequestCertificateCommandValidator : AbstractValidator<RequestCertificateCommand>
{
    private static readonly string[] _allowedValidationMethods = ["DNS", "EMAIL"];

    private readonly ILogger _logger;

    public RequestCertificateCommandValidator(ILogger<RequestCertificateCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.DomainName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.ValidationMethod)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(method => _allowedValidationMethods.Contains(method, StringComparer.Ordinal))
                .WithMessage("The validation method must be either 'DNS' or 'EMAIL'.");

        RuleForEach(_ => _.SubjectAlternativeNames)
            .NotEmpty()
                .WithMessage("Subject alternative names must not be empty.");
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<RequestCertificateCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "RequestCertificateCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
