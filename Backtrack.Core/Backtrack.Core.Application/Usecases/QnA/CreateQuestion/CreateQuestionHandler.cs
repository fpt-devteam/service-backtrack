using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

using QnAEntity = Backtrack.Core.Domain.Entities.QnA;

namespace Backtrack.Core.Application.Usecases.QnA.CreateQuestion;

/// <summary>
/// Handler for creating a new question.
/// </summary>
public sealed class CreateQuestionHandler(
    IQnARepository qnARepository,
    IUserRepository userRepository,
    IPostRepository postRepository) : IRequestHandler<CreateQuestionCommand, QnAResult>
{
    public async Task<QnAResult> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        _ = await postRepository.GetByIdAsync(command.PostId)
            ?? throw new NotFoundException(PostErrors.NotFound);

        _ = await userRepository.GetByIdAsync(command.AskerId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var qna = new QnAEntity
        {
            Id           = Guid.NewGuid(),
            PostId       = command.PostId,
            AskerId      = command.AskerId,
            QuestionText = command.QuestionText,
            CreatedAt    = DateTimeOffset.UtcNow,
        };

        await qnARepository.CreateAsync(qna);
        await qnARepository.SaveChangesAsync();

        return qna.ToQnAResult();
    }
}
