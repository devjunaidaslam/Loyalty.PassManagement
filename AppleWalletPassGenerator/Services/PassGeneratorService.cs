using AppleWalletPassGenerator.IServices;
using AppleWalletPassGenerator.Models;
using Microsoft.Extensions.Options;
using Passbook.Generator;
using Passbook.Generator.Fields;
using System.Security.Cryptography.X509Certificates;

namespace AppleWalletPassGenerator.Services
{
    public class PassGeneratorService : IPassGeneratorService
    {
        private readonly PassSettings _settings;
        private readonly IWebHostEnvironment _env;
        public PassGeneratorService(IOptions<PassSettings> options , IWebHostEnvironment env) 
        { 
            _settings = options.Value; 
            _env = env;
        }

        public async Task<byte[]> GeneratePassAsync(LoyaltyCardDto loyaltyCardData , string serialNumber)
        {
            PassGenerator generator = new PassGenerator();
            PassGeneratorRequest request = new PassGeneratorRequest();
            request.PassTypeIdentifier = _settings.PassTypeIdentifier;
            request.TeamIdentifier = _settings.TeamIdentifier;
            request.SerialNumber = serialNumber;
            request.Description = _settings.Description;
            request.OrganizationName = _settings.OrganizationName;
            request.LogoText = _settings.LogoText;
            request.BackgroundColor = _settings.BackgroundColor;
            request.LabelColor = _settings.LabelColor;
            request.ForegroundColor = _settings.ForegroundColor;
            request.Style = PassStyle.StoreCard;
            request.Barcodes.Add(new Barcode(
                 BarcodeType.PKBarcodeFormatQR,
                 loyaltyCardData.BarcodeMessage,
                 "iso-8859-1",
                 $"Loyalty ID: {loyaltyCardData.BarcodeMessage}" ));


            request.AddPrimaryField(new StandardField("company", "Company Name", _settings.OrganizationName));

            var loyaltyPointsField = new StandardField("loyalty-points", "Loyalty Points", loyaltyCardData.Points.ToString());
            loyaltyPointsField.ChangeMessage = "Your loyalty points have been updated to %@!";
            request.AddSecondaryField(loyaltyPointsField);

           
            request.AddAuxiliaryField(new StandardField("customer-name", "Customer", loyaltyCardData.CustomerName));

            var cerFile = Path.Combine(_env.WebRootPath, "Data", "AppleWWDRCAG4.cer");
            var p12File = Path.Combine(_env.WebRootPath, "Data", "fandeer-pass.p12");

            request.AppleWWDRCACertificate = new X509Certificate2(cerFile);
            request.PassbookCertificate = new X509Certificate2(p12File, "Aa0108534828@#$");

            request.WebServiceUrl = _settings.WebServiceURL;
            request.AuthenticationToken = _settings.AuthenticationToken;

            request.Images.Add(PassbookImage.Icon, await LoadImageAsync("Data/Images/icon.png"));
            request.Images.Add(PassbookImage.Icon2X, await LoadImageAsync("Data/Images/icon@2x.png"));
            request.Images.Add(PassbookImage.Icon3X, await LoadImageAsync("Data/Images/icon@3x.png"));

            request.Images.Add(PassbookImage.Logo, await LoadImageAsync("Data/Images/logo.png"));
            request.Images.Add(PassbookImage.Logo2X, await LoadImageAsync("Data/Images/logo@2x.png"));
            request.Images.Add(PassbookImage.Logo3X, await LoadImageAsync("Data/Images/logo@3x.png"));
            byte[] generatedPass = generator.Generate(request);
            return generatedPass;
        }

        private async Task<byte[]> LoadImageAsync(string relativePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Image not found: {fullPath}");

            return await File.ReadAllBytesAsync(fullPath);
        }
    }
}
