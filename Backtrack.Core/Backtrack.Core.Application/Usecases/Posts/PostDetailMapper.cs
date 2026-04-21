using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.Posts;

public static class PostDetailMapper
{
    // ── Input → New entity ───────────────────────────────────────────────

    public static PostPersonalBelongingDetail ToEntity(this PersonalBelongingDetailDto input, Guid postId) =>
        new()
        {
            PostId            = postId,
            ItemName          = input.ItemName,
            Color             = input.Color,
            Brand             = input.Brand,
            Material          = input.Material,
            Size              = input.Size,
            Condition         = input.Condition,
            DistinctiveMarks  = input.DistinctiveMarks,
            AiDescription     = input.AiDescription,
            AdditionalDetails = input.AdditionalDetails
        };

    public static PostCardDetail ToEntity(this CardDetailDto input, Guid postId, IHasher hasher) =>
        new()
        {
            PostId               = postId,
            ItemName             = input.ItemName,
            CardNumberHash       = input.CardNumber is not null ? hasher.Hash(input.CardNumber) : null,
            CardNumberMasked     = MaskCardNumber(input.CardNumber),
            HolderName           = input.HolderName,
            HolderNameNormalized = input.HolderNameNormalized,
            DateOfBirth          = input.DateOfBirth,
            IssueDate            = input.IssueDate,
            ExpiryDate           = input.ExpiryDate,
            IssuingAuthority     = input.IssuingAuthority,
            OcrText              = input.OcrText,
            AdditionalDetails    = input.AdditionalDetails
        };

    public static PostElectronicDetail ToEntity(this ElectronicDetailDto input, Guid postId) =>
        new()
        {
            PostId                 = postId,
            ItemName               = input.ItemName,
            Brand                  = input.Brand,
            Model                  = input.Model,
            Color                  = input.Color,
            HasCase                = input.HasCase,
            CaseDescription        = input.CaseDescription,
            ScreenCondition        = input.ScreenCondition,
            LockScreenDescription  = input.LockScreenDescription,
            DistinguishingFeatures = input.DistinguishingFeatures,
            AiDescription          = input.AiDescription,
            AdditionalDetails      = input.AdditionalDetails
        };

    public static PostOtherDetail ToEntity(this OtherDetailDto input, Guid postId) =>
        new()
        {
            PostId            = postId,
            ItemName          = input.ItemName,
            PrimaryColor      = input.PrimaryColor,
            AiDescription     = input.AiDescription,
            AdditionalDetails = input.AdditionalDetails
        };

    // ── Input → Apply to existing entity (patch semantics) ───────────────

    public static void ApplyTo(this PersonalBelongingDetailDto input, PostPersonalBelongingDetail detail)
    {
        detail.ItemName          = input.ItemName          ?? detail.ItemName;
        detail.Color             = input.Color             ?? detail.Color;
        detail.Brand             = input.Brand             ?? detail.Brand;
        detail.Material          = input.Material          ?? detail.Material;
        detail.Size              = input.Size              ?? detail.Size;
        detail.Condition         = input.Condition         ?? detail.Condition;
        detail.DistinctiveMarks  = input.DistinctiveMarks  ?? detail.DistinctiveMarks;
        detail.AiDescription     = input.AiDescription     ?? detail.AiDescription;
        detail.AdditionalDetails = input.AdditionalDetails ?? detail.AdditionalDetails;
    }

    public static void ApplyTo(this CardDetailDto input, PostCardDetail detail, IHasher hasher)
    {
        if (input.CardNumber is not null)
        {
            detail.CardNumberHash   = hasher.Hash(input.CardNumber);
            detail.CardNumberMasked = MaskCardNumber(input.CardNumber);
        }
        detail.ItemName             = input.ItemName             ?? detail.ItemName;
        detail.HolderName           = input.HolderName           ?? detail.HolderName;
        detail.HolderNameNormalized = input.HolderNameNormalized ?? detail.HolderNameNormalized;
        detail.DateOfBirth          = input.DateOfBirth          ?? detail.DateOfBirth;
        detail.IssueDate            = input.IssueDate            ?? detail.IssueDate;
        detail.ExpiryDate           = input.ExpiryDate           ?? detail.ExpiryDate;
        detail.IssuingAuthority     = input.IssuingAuthority     ?? detail.IssuingAuthority;
        detail.OcrText              = input.OcrText              ?? detail.OcrText;
        detail.AdditionalDetails    = input.AdditionalDetails    ?? detail.AdditionalDetails;
    }

    public static void ApplyTo(this ElectronicDetailDto input, PostElectronicDetail detail)
    {
        detail.ItemName               = input.ItemName               ?? detail.ItemName;
        detail.Brand                  = input.Brand                  ?? detail.Brand;
        detail.Model                  = input.Model                  ?? detail.Model;
        detail.Color                  = input.Color                  ?? detail.Color;
        detail.HasCase                = input.HasCase                ?? detail.HasCase;
        detail.CaseDescription        = input.CaseDescription        ?? detail.CaseDescription;
        detail.ScreenCondition        = input.ScreenCondition        ?? detail.ScreenCondition;
        detail.LockScreenDescription  = input.LockScreenDescription  ?? detail.LockScreenDescription;
        detail.DistinguishingFeatures = input.DistinguishingFeatures ?? detail.DistinguishingFeatures;
        detail.AiDescription          = input.AiDescription          ?? detail.AiDescription;
        detail.AdditionalDetails      = input.AdditionalDetails      ?? detail.AdditionalDetails;
    }

    public static void ApplyTo(this OtherDetailDto input, PostOtherDetail detail)
    {
        detail.ItemName          = input.ItemName;
        detail.PrimaryColor      = input.PrimaryColor      ?? detail.PrimaryColor;
        detail.AiDescription     = input.AiDescription     ?? detail.AiDescription;
        detail.AdditionalDetails = input.AdditionalDetails ?? detail.AdditionalDetails;
    }

    // ── Entity → Input (read/display) ────────────────────────────────────

    public static PersonalBelongingDetailDto ToDto(this PostPersonalBelongingDetail detail) =>
        new()
        {
            ItemName          = detail.ItemName ?? string.Empty,
            Color             = detail.Color,
            Brand             = detail.Brand,
            Material          = detail.Material,
            Size              = detail.Size,
            Condition         = detail.Condition,
            DistinctiveMarks  = detail.DistinctiveMarks,
            AiDescription     = detail.AiDescription,
            AdditionalDetails = detail.AdditionalDetails
        };

    public static CardDetailDto ToDto(this PostCardDetail detail) =>
        new()
        {
            ItemName             = detail.ItemName ?? string.Empty,
            CardNumber           = detail.CardNumberMasked,
            HolderName           = detail.HolderName,
            HolderNameNormalized = detail.HolderNameNormalized,
            DateOfBirth          = detail.DateOfBirth,
            IssueDate            = detail.IssueDate,
            ExpiryDate           = detail.ExpiryDate,
            IssuingAuthority     = detail.IssuingAuthority,
            OcrText              = detail.OcrText,
            AdditionalDetails    = detail.AdditionalDetails
        };

    public static ElectronicDetailDto ToDto(this PostElectronicDetail detail) =>
        new()
        {
            ItemName               = detail.ItemName ?? string.Empty,
            Brand                  = detail.Brand,
            Model                  = detail.Model,
            Color                  = detail.Color,
            HasCase                = detail.HasCase,
            CaseDescription        = detail.CaseDescription,
            ScreenCondition        = detail.ScreenCondition,
            LockScreenDescription  = detail.LockScreenDescription,
            DistinguishingFeatures = detail.DistinguishingFeatures,
            AiDescription          = detail.AiDescription,
            AdditionalDetails      = detail.AdditionalDetails
        };

    public static OtherDetailDto ToDto(this PostOtherDetail detail) =>
        new()
        {
            ItemName          = detail.ItemName,
            PrimaryColor      = detail.PrimaryColor,
            AiDescription     = detail.AiDescription,
            AdditionalDetails = detail.AdditionalDetails
        };

    // ── Shared helpers ────────────────────────────────────────────────────

    public static string? MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber)) return null;
        var digits = cardNumber.Replace("-", "").Replace(" ", "");
        var last4 = digits.Length >= 4 ? digits[^4..] : digits;
        return $"***{last4}";
    }
}
