using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Utils;

public static class PostDocumentUtil
{
    /// <summary>
    /// Builds a natural-language description of a post for embedding/reranking input.
    /// Natural prose scores significantly better than key-value strings on cross-encoders.
    /// The content is built from the category-specific detail table.
    /// </summary>
    public static string BuildDocument(Post post)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"{post.PostType} item (Category: {post.Category}).");

        switch (post.Category)
        {
            case ItemCategory.PersonalBelongings when post.PersonalBelongingDetail is { } d:
                if (!string.IsNullOrWhiteSpace(d.Brand))  sb.Append($" Brand: {d.Brand}.");
                if (!string.IsNullOrWhiteSpace(d.Color))  sb.Append($" Color: {d.Color}.");
                if (!string.IsNullOrWhiteSpace(d.Material)) sb.Append($" Material: {d.Material}.");
                if (!string.IsNullOrWhiteSpace(d.Condition)) sb.Append($" Condition: {d.Condition}.");
                if (!string.IsNullOrWhiteSpace(d.DistinctiveMarks)) sb.Append($" Distinctive marks: {d.DistinctiveMarks}.");
                if (!string.IsNullOrWhiteSpace(d.AiDescription)) sb.Append($" {d.AiDescription}");
                if (!string.IsNullOrWhiteSpace(d.AdditionalDetails)) sb.Append($" Additional details: {d.AdditionalDetails}.");
                break;

            case ItemCategory.Cards when post.CardDetail is { } c:
                // if (!string.IsNullOrWhiteSpace(c.HolderName)) sb.Append($" Holder: {c.HolderName}.");
                // if (!string.IsNullOrWhiteSpace(c.IssuingAuthority)) sb.Append($" Issued by: {c.IssuingAuthority}.");
                // if (!string.IsNullOrWhiteSpace(c.AiDescription)) sb.Append($" {c.AiDescription}");
                // if (!string.IsNullOrWhiteSpace(c.OcrText)) sb.Append($" OCR: {c.OcrText}.");
                throw new InvalidOperationException("Card details are not included in the document content for embedding/reranking, as they often contain personally identifiable information. This is a deliberate design choice to protect user privacy and comply with data protection regulations. If you need to include card details for specific use cases, please implement a secure handling mechanism that ensures sensitive information is properly anonymized or encrypted before processing.");

            case ItemCategory.Electronics when post.ElectronicDetail is { } e:
                if (!string.IsNullOrWhiteSpace(e.Brand)) sb.Append($" Brand: {e.Brand}.");
                if (!string.IsNullOrWhiteSpace(e.Model)) sb.Append($" Model: {e.Model}.");
                if (!string.IsNullOrWhiteSpace(e.Color)) sb.Append($" Color: {e.Color}.");
                if (!string.IsNullOrWhiteSpace(e.LockScreenDescription)) sb.Append($" Lock screen: {e.LockScreenDescription}.");
                if (!string.IsNullOrWhiteSpace(e.DistinguishingFeatures)) sb.Append($" Features: {e.DistinguishingFeatures}.");
                if (!string.IsNullOrWhiteSpace(e.AiDescription)) sb.Append($" {e.AiDescription}");
                if (!string.IsNullOrWhiteSpace(e.AdditionalDetails)) sb.Append($" Additional details: {e.AdditionalDetails}.");
                break;

            case ItemCategory.Others when post.OtherDetail is { } o:
                sb.Append($" Item: {o.ItemIdentifier}.");
                if (!string.IsNullOrWhiteSpace(o.PrimaryColor)) sb.Append($" Color: {o.PrimaryColor}.");
                if (!string.IsNullOrWhiteSpace(o.AiDescription)) sb.Append($" {o.AiDescription}");
                if (!string.IsNullOrWhiteSpace(o.Notes)) sb.Append($" Notes: {o.Notes}.");
                break;
        }

        if (!string.IsNullOrWhiteSpace(post.DisplayAddress))
            sb.Append($" Location: {post.DisplayAddress}.");

        return sb.ToString();
    }
}
