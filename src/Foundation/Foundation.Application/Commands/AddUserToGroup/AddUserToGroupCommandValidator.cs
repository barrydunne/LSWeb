using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.AddUserToGroup;

internal sealed partial class AddUserToGroupCommandValidator : AbstractValidator<AddUserToGroupCommand>
{
    private readonly ILogger _logger;

    public AddUserToGroupCommandValidator(ILogger<AddUserToGroupCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.GroupName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<AddUserToGroupCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "AddUserToGroupCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
