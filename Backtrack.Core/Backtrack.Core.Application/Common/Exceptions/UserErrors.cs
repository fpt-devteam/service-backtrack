using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public static class UserErrors
    {
        public static readonly Error NotFound = new(
            Code: "UserNotFound",
            Message: "User not found.",
            HttpStatusCode.NotFound);

        public static readonly Error EmailAlreadyExists = new(
            Code: "EmailAlreadyExists",
            Message: "Email already exists.",
            HttpStatusCode.Conflict);

        public static readonly Error AccountNotActivated = new(
            Code: "AccountNotActivated",
            Message: "Account is existed but not activated.",
            HttpStatusCode.Forbidden);

        public static readonly Error AccountAlreadyActivated = new(
            Code: "AccountAlreadyActivated",
            Message: "Account is already activated.",
            HttpStatusCode.Forbidden);

        public static readonly Error NoFileUploaded = new(
            Code: "NoFileUploaded",
            Message: "No file uploaded.",
            HttpStatusCode.BadRequest);

        public static readonly Error InvalidUserId = new(
            Code: "InvalidUserId",
            Message: "Invalid user identifier format.",
            HttpStatusCode.BadRequest);

        public static readonly Error InsufficientCredits = new(
            Code: "InsufficientCredits",
            Message: "User does not have enough credits to perform this action.",
            HttpStatusCode.BadRequest);

        public static readonly Error MissingAuthHeaders = new(
            Code: "MissingAuthHeaders",
            Message: "Required authentication headers (X-Auth-Id, X-Auth-Email, X-Auth-Name) are missing.",
            HttpStatusCode.BadRequest);

        public static readonly Error InvalidAuthId = new(
            Code: "InvalidAuthId",
            Message: "The provided authentication ID is invalid.",
            HttpStatusCode.BadRequest);
    }
}