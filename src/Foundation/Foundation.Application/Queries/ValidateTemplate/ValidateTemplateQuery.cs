using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudFormation;

namespace Foundation.Application.Queries.ValidateTemplate;

/// <summary>
/// Validate a CloudFormation template supplied either inline or by S3 URL before creating a stack from it.
/// </summary>
/// <param name="TemplateBody">The inline template body to validate, or <see langword="null"/> when validating by URL.</param>
/// <param name="TemplateUrl">The S3 URL of the template to validate, or <see langword="null"/> when validating an inline body.</param>
public record ValidateTemplateQuery(string? TemplateBody, string? TemplateUrl)
    : IQuery<ValidateTemplateQueryResult>;

/// <summary>
/// The outcome of validating a CloudFormation template.
/// </summary>
/// <param name="Validation">The validation result describing the template.</param>
public record ValidateTemplateQueryResult(TemplateValidationResult Validation);
