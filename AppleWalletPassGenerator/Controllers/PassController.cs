using AppleWalletPassGenerator.IServices;
using AppleWalletPassGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace AppleWalletPassGenerator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PassController : ControllerBase
    {
      
        private readonly ILogger<PassController> _logger;
        private readonly IPassGeneratorService _passGenerationService;
        private readonly IPassDataService _passDataService;
        private readonly IPushNotificationService _pushNotificationService;

        public PassController(
            ILogger<PassController> logger, 
            IPassGeneratorService passGenerationService,
            IPassDataService passDataService,
            IPushNotificationService pushNotificationService)
        {
            _logger = logger;
            _passGenerationService = passGenerationService;
            _passDataService = passDataService;
            _pushNotificationService = pushNotificationService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePass(LoyaltyCardDto cardData)
        {
            try
            {
               
                    var serialNumber = Guid.NewGuid().ToString();
                

                if (string.IsNullOrEmpty(cardData.CustomerName))
                {
                    return BadRequest("Customer name is required");
                }

                if (string.IsNullOrEmpty(cardData.CustomerEmail))
                {
                    return BadRequest("Customer email is required");
                }

                if (string.IsNullOrEmpty(cardData.BarcodeMessage))
                {
                    cardData.BarcodeMessage = serialNumber; // Use serial number as default barcode
                }

                if (string.IsNullOrEmpty(cardData.QrCodeData))
                {
                    cardData.QrCodeData = $"LOYALTY_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                }

                _logger.LogInformation("Generating pass for customer {CustomerName} with serial {SerialNumber}",
                    cardData.CustomerName, serialNumber);

                // Save pass data to database
                var passData = new PassData
                {
                    SerialNumber = serialNumber,
                    CustomerName = cardData.CustomerName,
                    CustomerEmail = cardData.CustomerEmail,
                    Points = cardData.Points,
                    BarcodeMessage = cardData.BarcodeMessage,
                    QrCodeData = cardData.QrCodeData
                };

                await _passDataService.CreatePassAsync(passData);

                // Generate the pass
                var passBytes = await _passGenerationService.GeneratePassAsync(cardData , serialNumber);

                // Return the pass file with proper headers for email attachment
                var fileName = $"loyalty-{serialNumber}.pkpass";

                // Set proper headers for email attachment compatibility
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                Response.Headers["Content-Transfer-Encoding"] = "binary";

                return File(passBytes, "application/vnd.apple.pkpass", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pass for serial {CustomerName}", cardData.CustomerName);
                return StatusCode(500, "An error occurred while generating the pass");
            }
        }

        [HttpPost("update-points")]
        public async Task<IActionResult> UpdatePoints(UpdatePointsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SerialNumber))
                {
                    return BadRequest("Serial number is required");
                }

                _logger.LogInformation("Updating points for pass {SerialNumber} to {Points}", 
                    request.SerialNumber, request.Points);

                // Update points in database
                var success = await _passDataService.UpdatePointsAsync(
                    request.SerialNumber, 
                    request.Points
                     );

                if (!success)
                {
                    return NotFound($"Pass with serial number {request.SerialNumber} not found");
                }

                // Get the updated pass data
                var passData = await _passDataService.GetPassBySerialNumberAsync(request.SerialNumber);
                if (passData == null)
                {
                    return NotFound($"Pass with serial number {request.SerialNumber} not found");
                }

                // Send push notification if device token is available
                // This will trigger Apple Wallet to show a notification using the change message
                if (!string.IsNullOrEmpty(passData.PushToken))
                {
                    var pushSent = await _pushNotificationService.SendPushNotificationAsync(
                        passData.PushToken, 
                        "pass.com.fandeercoffee.app", // PassTypeIdentifier from settings
                        request.SerialNumber);

                    if (pushSent)
                    {
                        _logger.LogInformation("Push notification sent for pass {SerialNumber} - Apple Wallet will show notification using change message", request.SerialNumber);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send push notification for pass {SerialNumber}", request.SerialNumber);
                        return BadRequest($"Failed to send push notification for pass {request.SerialNumber}");
                    }
                }

                return Ok(new { 
                    message = "Points updated successfully", 
                    serialNumber = request.SerialNumber, 
                    points = request.Points,
                    pushNotificationSent = !string.IsNullOrEmpty(passData.DeviceToken)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating points for serial {SerialNumber}", request.SerialNumber);
                return StatusCode(500, "An error occurred while updating points");
            }
        }

        // Apple Wallet Web Service Endpoints (following Apple's REST API standards)
        
        [HttpGet("v1/passes/{passTypeIdentifier}/{serialNumber}")]
        public async Task<IActionResult> GetPass(string passTypeIdentifier, string serialNumber )
        {
            try
            {
                if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(passTypeIdentifier))
                {
                    return BadRequest("Pass type identifier and serial number are required");
                }

                _logger.LogInformation("Fetching pass for type {PassTypeIdentifier} and serial {SerialNumber}", 
                    passTypeIdentifier, serialNumber);

                var passData = await _passDataService.GetPassBySerialNumberAsync(serialNumber);
                if (passData == null)
                {
                    return NotFound($"Pass with serial number {serialNumber} not found");
                }

                // Convert PassData to LoyaltyCardData for pass generation
                var loyaltyCardDto = new LoyaltyCardDto
                {
                    CustomerName = passData.CustomerName,
                    CustomerEmail = passData.CustomerEmail,
                    Points = passData.Points,
                    BarcodeMessage = passData.BarcodeMessage,
                    QrCodeData = passData.QrCodeData
                };

                // Generate the updated pass
                var passBytes = await _passGenerationService.GeneratePassAsync(loyaltyCardDto , serialNumber);

                // Return the pass file
                var fileName = $"loyalty-{serialNumber}.pkpass";
                return File(passBytes, "application/vnd.apple.pkpass", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pass for serial {SerialNumber}", serialNumber);
                return StatusCode(500, "An error occurred while fetching the pass");
            }
        }

        [HttpPost("v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}")]
        public async Task<IActionResult> RegisterDevice(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber , [FromBody] RegisterDeviceRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(passTypeIdentifier) || string.IsNullOrEmpty(deviceLibraryIdentifier))
                {
                    return BadRequest("Device library identifier, pass type identifier, and serial number are required");
                }

                _logger.LogInformation("Registering device {DeviceLibraryIdentifier} for pass type {PassTypeIdentifier} and serial {SerialNumber}", 
                    deviceLibraryIdentifier, passTypeIdentifier, serialNumber);

                var passData = await _passDataService.GetPassBySerialNumberAsync(serialNumber);
                if (passData == null)
                {
                    return NotFound($"Pass with serial number {serialNumber} not found");
                }

                // Update device token (using deviceLibraryIdentifier as the device token)
                passData.DeviceToken = deviceLibraryIdentifier;
                passData.PushToken = request.PushToken; // Optional push token from request body
                await _passDataService.UpdatePassAsync(passData);

                return Ok(new { message = "Device registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device for serial {SerialNumber}", serialNumber);
                return StatusCode(500, "An error occurred while registering device");
            }
        }

        [HttpGet("v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}")]
        public async Task<IActionResult> GetDeviceRegistrations(string deviceLibraryIdentifier, string passTypeIdentifier , [FromQuery] DateTime passesUpdatedSince)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceLibraryIdentifier) || string.IsNullOrEmpty(passTypeIdentifier))
                {
                    return BadRequest("Device library identifier and pass type identifier are required");
                }

                _logger.LogInformation("Getting registrations for device {DeviceLibraryIdentifier} and pass type {PassTypeIdentifier}", 
                    deviceLibraryIdentifier, passTypeIdentifier);

                // Get all passes registered to this device
                var registeredPasses = await _passDataService.GetPassesByDeviceTokenAsync(deviceLibraryIdentifier);

                if (registeredPasses == null || !registeredPasses.Any())
                {
                    return NotFound("No passes found for this device");
                }

                // Return list of serial numbers
                var serialNumbers = registeredPasses.Select(p => p.SerialNumber).ToList();

                var response = new
                {
                    serialNumbers = serialNumbers,
                    lastUpdated = passesUpdatedSince
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device registrations for device {DeviceLibraryIdentifier}", deviceLibraryIdentifier);
                return StatusCode(500, "An error occurred while getting device registrations");
            }
        }

        [HttpDelete("/v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}")]
        public async Task<IActionResult> UnregisterDevice(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(deviceLibraryIdentifier))
                {
                    return BadRequest("Serial number and device token are required");
                }

                _logger.LogInformation("Unregistering device {DeviceToken} for pass {SerialNumber}",
                    deviceLibraryIdentifier, serialNumber);

                var passData = await _passDataService.GetPassBySerialNumberAsync(serialNumber);
                if (passData == null)
                {
                    return NotFound($"Pass with serial number {serialNumber} not found");
                }

                // Clear device token if it matches
                if (passData.DeviceToken == deviceLibraryIdentifier)
                {
                    passData.DeviceToken = null;
                    passData.PushToken = null;
                    await _passDataService.UpdatePassAsync(passData);
                }

                return Ok(new { message = "Device unregistered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering device for serial {SerialNumber}", serialNumber);
                return StatusCode(500, "An error occurred while unregistering device");
            }
        }

        [HttpGet("passes/{serialNumber}/log")]
        public async Task<IActionResult> GetPassLog(string serialNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(serialNumber))
                {
                    return BadRequest("Serial number is required");
                }

                _logger.LogInformation("Fetching log for pass {SerialNumber}", serialNumber);

                var passData = await _passDataService.GetPassBySerialNumberAsync(serialNumber);
                if (passData == null)
                {
                    return NotFound($"Pass with serial number {serialNumber} not found");
                }

                // Return pass log information
                return Ok(new
                {
                    serialNumber = passData.SerialNumber,
                    version = passData.Version,
                    lastUpdated = passData.UpdatedAt,
                    points = passData.Points,
                    hasDeviceToken = !string.IsNullOrEmpty(passData.DeviceToken)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching log for serial {SerialNumber}", serialNumber);
                return StatusCode(500, "An error occurred while fetching pass log");
            }
        }

        [HttpGet("GetAllCustomer")]
        public async Task<IActionResult> GetAllCustomer()
        {
          var customers = await _passDataService.GetAllCustomersAsync();
            return Ok(customers.Select(x => new { CustomerName = x.CustomerName, Email = x.CustomerEmail, SerialNumber = x.SerialNumber, Point = x.Points }));
        }


    }
    public class RegisterDeviceRequest
    {
        [JsonPropertyName("pushToken")]
        public string? PushToken { get; set; }
    }
}
