using System.ComponentModel.DataAnnotations;

namespace AppleWalletPassGenerator.Models
{
    public class PassData
    {
        [Key]
        public string SerialNumber { get; set; } = string.Empty;
        
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        public string CustomerEmail { get; set; } = string.Empty;
        
        public int Points { get; set; }
        
        public string? BarcodeMessage { get; set; }
        
        public string? QrCodeData { get; set; }
        
        public string? DeviceToken { get; set; } 

        public string? PushToken { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public int Version { get; set; } = 1; 
    }
}
