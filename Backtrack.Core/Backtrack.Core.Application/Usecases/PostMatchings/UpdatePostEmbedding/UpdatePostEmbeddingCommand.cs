using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;

public sealed record UpdatePostEmbeddingCommand(Guid PostId) : IRequest;
