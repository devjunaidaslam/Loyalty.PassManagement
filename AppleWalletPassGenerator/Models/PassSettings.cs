namespace AppleWalletPassGenerator.Models
{
    public class PassSettings
    {
        public string PassTypeIdentifier { get; set; }
        public string TeamIdentifier { get; set; }
        public string Description { get; set; }
        public string OrganizationName { get; set; }
        public string LogoText { get; set; }
        public string BackgroundColor { get; set; }
        public string LabelColor { get; set; }
        public string ForegroundColor { get; set; }

        public string WebServiceURL { get; set; }
        public string AuthenticationToken { get; set; }
    }
}
