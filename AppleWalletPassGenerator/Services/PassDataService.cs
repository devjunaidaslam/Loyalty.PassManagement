using AppleWalletPassGenerator.IServices;
using AppleWalletPassGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace AppleWalletPassGenerator.Services
{
    public class PassDataService : IPassDataService
    {
        private readonly PassDbContext _context;
        private readonly ILogger<PassDataService> _logger;

        public PassDataService(PassDbContext context, ILogger<PassDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PassData?> GetPassBySerialNumberAsync(string serialNumber)
        {
            return await _context.Passes
                .FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);
        }

        public async Task<PassData> CreatePassAsync(PassData passData)
        {
            _context.Passes.Add(passData);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created pass with serial {SerialNumber}", passData.SerialNumber);
            return passData;
        }

        public async Task<PassData> UpdatePassAsync(PassData passData)
        {
            passData.UpdatedAt = DateTime.UtcNow;
            passData.Version++;
            _context.Passes.Update(passData);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated pass with serial {SerialNumber}", passData.SerialNumber);
            return passData;
        }

        public async Task<bool> DeletePassAsync(string serialNumber)
        {
            var pass = await _context.Passes.FindAsync(serialNumber);
            if (pass == null)
                return false;

            _context.Passes.Remove(pass);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted pass with serial {SerialNumber}", serialNumber);
            return true;
        }

        public async Task<bool> UpdatePointsAsync(string serialNumber, int points, string? deviceToken = null)
        {
            var pass = await _context.Passes.FindAsync(serialNumber);
            if (pass == null)
                return false;

            pass.Points = points;
            pass.UpdatedAt = DateTime.UtcNow;
            pass.Version++;

            if (!string.IsNullOrEmpty(deviceToken))
            {
                pass.DeviceToken = deviceToken;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated points for pass with serial {SerialNumber} to {Points}", serialNumber, points);
            return true;
        }

        public async Task<IEnumerable<PassData>> GetPassesByDeviceTokenAsync(string deviceToken)
        {
            return await _context.Passes
                .Where(p => p.DeviceToken == deviceToken)
                .ToListAsync();
        }

        public async Task<IEnumerable<PassData>> GetAllCustomersAsync()
        {
           return await _context.Passes
                .ToListAsync();
        }
    }
}
