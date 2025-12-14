using FluentValidation;

namespace Backtrack.Core.Contract.Users.Requests;

public sealed record CreateUserRequest
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
}

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("DisplayName must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));
    }
}
