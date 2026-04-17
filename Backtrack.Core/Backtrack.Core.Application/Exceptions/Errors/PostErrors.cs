namespace Backtrack.Core.Application.Exceptions.Errors
{
    public static class PostErrors
    {
        public static readonly Error InvalidPostType = new(
            Code: "InvalidPostType",
            Message: "PostType must be either 'Lost' or 'Found'.");

        public static readonly Error InvalidCategory = new(
            Code: "InvalidCategory",
            Message: "Category must be one of: PersonalBelongings, Cards, Electronics, Others.");

        public static readonly Error NotFound = new(
            Code: "PostNotFound",
            Message: "Post not found.");

        public static readonly Error Forbidden = new(
            Code: "PostForbidden",
            Message: "You are not authorized to perform this action on this post.");

        public static readonly Error LostPostCannotBeAssociatedWithOrganization = new(
            Code: "LostPostCannotBeAssociatedWithOrganization",
            Message: "A lost post cannot be associated with an organization.");

        public static readonly Error ImageFetchFailed = new(
            Code: "ImageFetchFailed",
            Message: "Failed to fetch image from the provided URL.");

        public static readonly Error NotInStorage = new(
            Code: "PostNotInStorage",
            Message: "Post must be in 'InStorage' status to be published.");

        public static readonly Error SubcategoryNotFound = new(
            Code: "SubcategoryNotFound",
            Message: "Subcategory not found.");

        public static readonly Error ImagesNotConsistent = new(
            Code: "ImagesNotConsistent",
            Message: "The provided images do not all show the same item.");

        public static readonly Error ImageDoesNotMatchSubcategory = new(
            Code: "ImageDoesNotMatchSubcategory",
            Message: "The item in the image does not match the specified subcategory.");
    }
}
