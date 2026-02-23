namespace AppleWalletPassGenerator.Models
{
    public class LoyaltyCardData
    {
        public string? SerialNumber { get; set; }    
        public string? CustomerName { get; set; }  
        public string? CustomerEmail { get; set; }  
        public int Points { get; set; }            
        public string? BarcodeMessage { get; set; } 
        public string? QrCodeData { get; set; }
    }

}
