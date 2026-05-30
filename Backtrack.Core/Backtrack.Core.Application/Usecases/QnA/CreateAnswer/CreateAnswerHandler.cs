using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

using AnswerEntity = Backtrack.Core.Domain.Entities.Answer;

namespace Backtrack.Core.Application.Usecases.QnA.CreateAnswer;

/// <summary>
/// Handler for adding an answer to an existing question.
/// Any authenticated user can answer; the answerer identity is recorded.
/// </summary>
public sealed class CreateAnswerHandler(
    IQnARepository qnARepository,
    IAnswerRepository answerRepository,
    IUserRepository userRepository) : IRequestHandler<CreateAnswerCommand, AnswerResult>
{
    public async Task<AnswerResult> Handle(CreateAnswerCommand command, CancellationToken cancellationToken)
    {
        _ = await qnARepository.GetByIdAsync(command.QuestionId)
            ?? throw new NotFoundException(QnAErrors.NotFound);

        _ = await userRepository.GetByIdAsync(command.AnswererId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var answer = new AnswerEntity
        {
            Id         = Guid.NewGuid(),
            QuestionId = command.QuestionId,
            AnswererId = command.AnswererId,
            Type       = command.Type,
            AnswerText = command.AnswerText,
            ImageUrls  = command.ImageUrls?.ToList(),
        };

        await answerRepository.CreateAsync(answer);
        await answerRepository.SaveChangesAsync();

        return answer.ToAnswerResult();
    }
}
