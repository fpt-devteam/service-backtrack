using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrDesigns.UpdateMyQrDesign;

public sealed class UpdateMyQrDesignHandler(IQrDesignRepository qrDesignRepository)
    : IRequestHandler<UpdateMyQrDesignCommand, QrDesignResult>
{
    public async Task<QrDesignResult> Handle(UpdateMyQrDesignCommand command, CancellationToken cancellationToken)
    {
        var design = await qrDesignRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (design is null)
        {
            design = await qrDesignRepository.CreateAsync(new QrDesign
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                ForegroundColor = command.ForegroundColor,
                BackgroundColor = command.BackgroundColor,
                DotStyle = command.DotStyle,
                CornerSquareStyle = command.CornerSquareStyle,
                CornerSquareColor = command.CornerSquareColor,
                CornerDotStyle = command.CornerDotStyle,
                CornerDotColor = command.CornerDotColor,
                ErrorCorrectionLevel = command.ErrorCorrectionLevel,
                Logo = command.Logo,
                Gradient = command.Gradient,
            });
        }
        else
        {
            design.ForegroundColor = command.ForegroundColor;
            design.BackgroundColor = command.BackgroundColor;
            design.DotStyle = command.DotStyle;
            design.CornerSquareStyle = command.CornerSquareStyle;
            design.CornerSquareColor = command.CornerSquareColor;
            design.CornerDotStyle = command.CornerDotStyle;
            design.CornerDotColor = command.CornerDotColor;
            design.ErrorCorrectionLevel = command.ErrorCorrectionLevel;
            design.Logo = command.Logo;
            design.Gradient = command.Gradient;
            qrDesignRepository.Update(design);
        }

        await qrDesignRepository.SaveChangesAsync();
        return GetMyQrDesign.GetMyQrDesignHandler.MapToResult(design);
    }
}
