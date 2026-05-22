using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.AnswerQuestion;

/// <summary>
/// Handler for adding or updating the answer on a question.
/// Any authenticated user can answer; the answerer identity is recorded.
/// </summary>
public sealed class AnswerQuestionHandler(
    IQnARepository qnARepository,
    IUserRepository userRepository) : IRequestHandler<AnswerQuestionCommand, QnAResult>
{
    public async Task<QnAResult> Handle(AnswerQuestionCommand command, CancellationToken cancellationToken)
    {
        var answerer = await userRepository.GetByIdAsync(command.AnswererId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var qna = await qnARepository.GetByIdAsync(command.QnAId, isTrack: true)
            ?? throw new NotFoundException(QnAErrors.NotFound);

        qna.AnswererId  = command.AnswererId;
        qna.AnswerText  = command.AnswerText;
        qna.AnsweredAt  = DateTimeOffset.UtcNow;
        qna.UpdatedAt   = DateTimeOffset.UtcNow;

        await qnARepository.SaveChangesAsync();

        return qna.ToQnAResult();
    }
}
