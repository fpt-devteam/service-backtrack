using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

using QuestionEntity = Backtrack.Core.Domain.Entities.Question;

namespace Backtrack.Core.Application.Usecases.QnA.CreateQuestions;

public sealed class CreateQuestionsHandler(
    IQnARepository qnARepository,
    IPostRepository postRepository) : IRequestHandler<CreateQuestionsCommand, IReadOnlyList<QuestionResult>>
{
    public async Task<IReadOnlyList<QuestionResult>> Handle(CreateQuestionsCommand command, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(command.PostId)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.PostType != PostType.Found)
            throw new ForbiddenException(QnAErrors.OnlyFoundPostsAllowed);

        var now = DateTimeOffset.UtcNow;
        var questions = command.QuestionTexts.Select(text => new QuestionEntity
        {
            Id           = Guid.NewGuid(),
            PostId       = command.PostId,
            AskerId      = command.AskerId,
            QuestionText = text,
            CreatedAt    = now,
        }).ToList();

        await qnARepository.CreateBatchAsync(questions, cancellationToken);
        await qnARepository.SaveChangesAsync();

        return questions.Select(q => q.ToQuestionResult()).ToList();
    }
}
