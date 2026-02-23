using AppleWalletPassGenerator.IServices;
using AppleWalletPassGenerator.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace AppleWalletPassGenerator.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ILogger<PushNotificationService> _logger;
        private readonly PassSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _env;

        public PushNotificationService(
            ILogger<PushNotificationService> logger,
            IOptions<PassSettings> settings,
            HttpClient httpClient,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
            _env = env;
        }


        public async Task<bool> SendPushNotificationAsync(string deviceToken, string passTypeId, string serialNumber)
        {
            var p12File = Path.Combine(_env.WebRootPath, "Data", "fandeer-pass.p12");
            var cert = new X509Certificate2(p12File, "Aa0108534828@#$",
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.Exportable);

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);

            using var client = new HttpClient(handler);
            client.DefaultRequestVersion = new Version(2, 0); // Use HTTP/2

            var url = $"https://api.push.apple.com/3/device/{deviceToken}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Version = new Version(2, 0); // HTTP/2
            request.Headers.Add("apns-topic", passTypeId);

            // Wallet pass notifications use empty payload - the visible notification comes from change messages
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.SendAsync(request);
                Console.WriteLine($"Status Code: {(int)response.StatusCode}");
                Console.WriteLine($"Reason: {response.ReasonPhrase}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Push notification sent successfully to device {DeviceToken} for pass {SerialNumber}", deviceToken, serialNumber);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send push notification to device {DeviceToken} for pass {SerialNumber}. Status: {StatusCode}, Error: {Error}",
                        deviceToken, serialNumber, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to device {DeviceToken} for pass {SerialNumber}", deviceToken, serialNumber);
                return false;
            }
        }







        //public async Task<bool> SendPushNotificationAsync(string devicePushToken, string passTypeId, string serialNumber)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(devicePushToken))
        //        {
        //            _logger.LogWarning("Device token is null or empty for serial {SerialNumber}", serialNumber);
        //            return false;
        //        }

        //        // Create the push notification payload
        //        var payload = new
        //        {
        //            aps = new
        //            {
        //                alert = "Your loyalty points have been updated!",
        //                sound = "default"
        //            }
        //        };

        //        var jsonPayload = JsonSerializer.Serialize(payload);
        //        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        //        // For Apple Wallet push notifications, we need to use APNs
        //        // This is a simplified implementation - in production, you'd use proper APNs certificates
        //        var apnsUrl = $"https://api.push.apple.com/3/device/{devicePushToken}";

        //        // Add APNs headers
        //        _httpClient.DefaultRequestHeaders.Clear();
        //        _httpClient.DefaultRequestHeaders.Add("apns-topic", passTypeId);
        //        _httpClient.DefaultRequestHeaders.Add("apns-push-type", "passbook");
        //        _httpClient.DefaultRequestHeaders.Add("apns-expiration", "0");
        //        _httpClient.DefaultRequestHeaders.Add("apns-priority", "10");

        //        var response = await _httpClient.PostAsync(apnsUrl, content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            _logger.LogInformation("Push notification sent successfully for serial {SerialNumber}", serialNumber);
        //            return true;
        //        }
        //        else
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            _logger.LogError("Failed to send push notification for serial {SerialNumber}. Status: {StatusCode}, Error: {Error}",
        //                serialNumber, response.StatusCode, errorContent);
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error sending push notification for serial {SerialNumber}", serialNumber);
        //        return false;
        //    }
        //}

    }
}
