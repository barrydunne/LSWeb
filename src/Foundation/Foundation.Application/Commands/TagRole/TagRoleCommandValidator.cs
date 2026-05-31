using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.TagRole;

internal sealed partial class TagRoleCommandValidator : AbstractValidator<TagRoleCommand>
{
    private readonly ILogger _logger;

    public TagRoleCommandValidator(ILogger<TagRoleCommandValidator> logger)
    {
        _logger = logger;

        RuleFor(_ => _.RoleName)
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
        ValidationContext<TagRoleCommand> context, CancellationToken cancellation = default)
    {
        var result = await base.ValidateAsync(context, cancellation);
        if (!result.IsValid)
            LogValidationFailure(result.ToString());

        return result;
    }

    [LoggerMessage(LogLevel.Warning, "TagRoleCommand validation failure: {Error}")]
    private partial void LogValidationFailure(string error);
}
