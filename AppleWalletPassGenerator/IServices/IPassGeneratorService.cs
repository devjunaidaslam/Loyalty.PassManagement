using AppleWalletPassGenerator.Models;

namespace AppleWalletPassGenerator.IServices
{
    public interface IPassGeneratorService
    {
        Task<byte[]> GeneratePassAsync(LoyaltyCardDto loyaltyCardData, string serialNumber);
    }
}
