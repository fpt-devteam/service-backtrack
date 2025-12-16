using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Posts.Commands.UpdatePostContentEmbedding;

public sealed record UpdatePostContentEmbeddingCommand(Guid PostId) : IRequest;
