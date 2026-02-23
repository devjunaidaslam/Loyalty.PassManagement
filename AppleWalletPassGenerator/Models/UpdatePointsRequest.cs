namespace AppleWalletPassGenerator.Models
{
    public class UpdatePointsRequest
    {
        public string SerialNumber { get; set; } = string.Empty;
        public int Points { get; set; }
    }
}
