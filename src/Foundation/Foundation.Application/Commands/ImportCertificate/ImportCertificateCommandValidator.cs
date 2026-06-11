using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ImportCertificate;

internal sealed partial class ImportCertificateCommandValidator : AbstractValidator<ImportCertificateCommand>
{
    private const string CertificateMarker = "BEGIN CERTIFICATE";
    private const string PrivateKeyMarker = "PRIVATE KEY";

    private readonly ILogger _logger;

    public ImportCertificateCommandValidator(ILogger<ImportCertificateCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.Certificate)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(certificate => certificate.Contains(CertificateMarker, StringComparison.Ordinal))
                .WithMessage("The certificate must be PEM-encoded and contain a 'BEGIN CERTIFICATE' block.");

        RuleFor(_ => _.PrivateKey)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(privateKey => privateKey.Contains(PrivateKeyMarker, StringComparison.Ordinal))
                .WithMessage("The private key must be PEM-encoded and contain a 'PRIVATE KEY' block.");

        RuleFor(_ => _.CertificateChain)
            .Must(chain => chain!.Contains(CertificateMarker, StringComparison.Ordinal))
                .WithMessage("The certificate chain must be PEM-encoded and contain a 'BEGIN CERTIFICATE' block.")
            .When(_ => !string.IsNullOrEmpty(_.CertificateChain), ApplyConditionTo.CurrentValidator);
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<ImportCertificateCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "ImportCertificateCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
