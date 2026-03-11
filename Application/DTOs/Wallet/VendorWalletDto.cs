namespace Application.DTOs.Wallet
{

    public record VendorWalletDto(
        decimal Balance,
        decimal EscrowBalance,
        List<WalletTransactionDto> Transactions
    );
}