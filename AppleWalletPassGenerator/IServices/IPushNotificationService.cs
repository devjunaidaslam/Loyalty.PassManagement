namespace AppleWalletPassGenerator.IServices
{
    public interface IPushNotificationService
    {
        Task<bool> SendPushNotificationAsync(string deviceToken, string passTypeId, string serialNumber);
    }
}
