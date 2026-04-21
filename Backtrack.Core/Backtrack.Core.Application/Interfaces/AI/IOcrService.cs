using Backtrack.Core.Application.Usecases.Posts;

namespace Backtrack.Core.Application.Interfaces.AI;

public interface IOcrService
{
    /// <summary>Extracts structured fields from a card/document image.</summary>
    Task<CardDetailDto> ExtractCardTextAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken = default);
}
