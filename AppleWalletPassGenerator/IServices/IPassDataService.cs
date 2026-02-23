using AppleWalletPassGenerator.Models;

namespace AppleWalletPassGenerator.IServices
{
    public interface IPassDataService
    {
        Task<PassData?> GetPassBySerialNumberAsync(string serialNumber);
        Task<PassData> CreatePassAsync(PassData passData);
        Task<PassData> UpdatePassAsync(PassData passData);
        Task<bool> DeletePassAsync(string serialNumber);
        Task<bool> UpdatePointsAsync(string serialNumber, int points, string? deviceToken = null);
        Task<IEnumerable<PassData>> GetPassesByDeviceTokenAsync(string deviceToken);
        Task<IEnumerable<PassData>> GetAllCustomersAsync();
    }
}
