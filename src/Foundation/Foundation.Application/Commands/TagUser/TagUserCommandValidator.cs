using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.TagUser;

internal sealed partial class TagUserCommandValidator : AbstractValidator<TagUserCommand>
{
    private readonly ILogger _logger;

    public TagUserCommandValidator(ILogger<TagUserCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.UserName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleFor(_ => _.Tags)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty();

        RuleForEach(_ => _.Tags)
            .ChildRules(tag => tag.RuleFor(_ => _.Key)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .WithMessage("Tag key must not be empty."));
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<TagUserCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "TagUserCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
