using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage
{
    public sealed record AnalyzePostImageCommand : IRequest<PostItem>
    {
        public required string ImageUrl { get; init; }
    }
    }