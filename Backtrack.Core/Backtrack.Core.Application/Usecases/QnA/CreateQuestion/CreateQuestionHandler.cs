using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

using QuestionEntity = Backtrack.Core.Domain.Entities.Question;

namespace Backtrack.Core.Application.Usecases.QnA.CreateQuestion;

/// <summary>
/// Handler for creating a new question on a found-item post.
/// Only posts of type Found allow questions.
/// </summary>
public sealed class CreateQuestionHandler(
    IQnARepository qnARepository,
    IUserRepository userRepository,
    IPostRepository postRepository) : IRequestHandler<CreateQuestionCommand, QuestionResult>
{
    public async Task<QuestionResult> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(command.PostId)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.PostType != PostType.Found)
            throw new ForbiddenException(QnAErrors.OnlyFoundPostsAllowed);

        _ = await userRepository.GetByIdAsync(command.AskerId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var question = new QuestionEntity
        {
            Id           = Guid.NewGuid(),
            PostId       = command.PostId,
            AskerId      = command.AskerId,
            QuestionText = command.QuestionText,
        };

        await qnARepository.CreateAsync(question);
        await qnARepository.SaveChangesAsync();

        // Return with empty answers list — newly created question has no answers yet
        return question.ToQuestionResult();
    }
}
