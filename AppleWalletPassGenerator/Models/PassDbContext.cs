using Microsoft.EntityFrameworkCore;

namespace AppleWalletPassGenerator.Models
{
    public class PassDbContext : DbContext
    {
        public PassDbContext(DbContextOptions<PassDbContext> options) : base(options)
        {
        }

        public DbSet<PassData> Passes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PassData>(entity =>
            {
                entity.HasKey(e => e.SerialNumber);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.CustomerName).HasMaxLength(200);
                entity.Property(e => e.CustomerEmail).HasMaxLength(200);
                entity.Property(e => e.BarcodeMessage).HasMaxLength(500);
                entity.Property(e => e.QrCodeData).HasMaxLength(500);
                entity.Property(e => e.DeviceToken).HasMaxLength(500);
            });
        }
    }
}
