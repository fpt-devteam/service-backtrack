using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrDesigns.GetMyQrDesign;

public sealed class GetMyQrDesignHandler(IQrDesignRepository qrDesignRepository)
    : IRequestHandler<GetMyQrDesignQuery, QrDesignResult>
{
    public async Task<QrDesignResult> Handle(GetMyQrDesignQuery query, CancellationToken cancellationToken)
    {
        var design = await qrDesignRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        if (design is null)
        {
            // Auto-provision default design on first access
            design = await qrDesignRepository.CreateAsync(new QrDesign
            {
                Id = Guid.NewGuid(),
                UserId = query.UserId,
                ForegroundColor = "#000000",
                BackgroundColor = "#ffffff",
                DotStyle = "square",
                CornerSquareStyle = "square",
                CornerSquareColor = "#000000",
                CornerDotStyle = "square",
                CornerDotColor = "#000000",
                ErrorCorrectionLevel = QrEcl.M,
            });
            await qrDesignRepository.SaveChangesAsync();
        }

        return MapToResult(design);
    }

    internal static QrDesignResult MapToResult(QrDesign design) => new()
    {
        Id = design.Id,
        UserId = design.UserId,
        ForegroundColor = design.ForegroundColor,
        BackgroundColor = design.BackgroundColor,
        DotStyle = design.DotStyle,
        CornerSquareStyle = design.CornerSquareStyle,
        CornerSquareColor = design.CornerSquareColor,
        CornerDotStyle = design.CornerDotStyle,
        CornerDotColor = design.CornerDotColor,
        ErrorCorrectionLevel = design.ErrorCorrectionLevel,
        Logo = design.Logo,
        Gradient = design.Gradient,
        CreatedAt = design.CreatedAt,
        UpdatedAt = design.UpdatedAt,
    };
}
