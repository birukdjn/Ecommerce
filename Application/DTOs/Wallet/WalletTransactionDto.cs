namespace Application.DTOs.Wallet
{
    public record WalletTransactionDto(
        Guid Id,
        decimal Amount,
        string Type,
        string Description,
        DateTime CreatedAt,
        Guid? RelatedOrderId
    );
}