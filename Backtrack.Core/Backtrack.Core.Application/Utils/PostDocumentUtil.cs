using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Utils;

public static class PostDocumentUtil
{
    /// <summary>
    /// Builds a natural-language description of a post for embedding/reranking input.
    /// Natural prose scores significantly better than key-value strings on cross-encoders.
    /// </summary>
    public static string BuildDocument(Post post)
    {
        var item = post.Item;
        var sb   = new System.Text.StringBuilder();

        sb.Append($"{post.PostType} item: {item.ItemName}.");

        var attrs = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.Color))    attrs.Add(item.Color);
        if (!string.IsNullOrWhiteSpace(item.Brand))    attrs.Add(item.Brand);
        if (!string.IsNullOrWhiteSpace(item.Material)) attrs.Add(item.Material);
        if (!string.IsNullOrWhiteSpace(item.Size))     attrs.Add($"{item.Size} size");
        if (item.Category != default)                   attrs.Add(item.Category.ToString());

        if (attrs.Count > 0)
            sb.Append($" It is a {string.Join(", ", attrs)} item.");

        if (!string.IsNullOrWhiteSpace(item.Condition))
            sb.Append($" Condition: {item.Condition}.");

        if (!string.IsNullOrWhiteSpace(item.DistinctiveMarks))
            sb.Append($" Distinctive marks: {item.DistinctiveMarks}.");

        if (!string.IsNullOrWhiteSpace(item.AdditionalDetails))
            sb.Append($" Additional details: {item.AdditionalDetails}.");

        if (!string.IsNullOrWhiteSpace(post.DisplayAddress))
            sb.Append($" Found/lost at: {post.DisplayAddress}.");

        return sb.ToString();
    }
}
