using Backtrack.Core.Application.Configurations;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace Backtrack.Core.Application.Usecases.Users.GetMe;

public sealed class GetMeHandler : IRequestHandler<GetMeQuery, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly int _freeTierLimit;

    public GetMeHandler(
        IUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository,
        IOptions<PostSettings> postSettings)
    {
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _freeTierLimit = postSettings.Value.FreeTierPostLimit;
    }

    public async Task<UserResult> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var hasSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken) != null;

        return new UserResult
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            ShowEmail = user.ShowEmail,
            ShowPhone = user.ShowPhone,
            PostActionCount = user.PostActionCount,
            PostActionLimit = hasSubscription ? null : _freeTierLimit,
            GlobalRole = user.GlobalRole,
            Status = user.Status,
        };
    }
}
