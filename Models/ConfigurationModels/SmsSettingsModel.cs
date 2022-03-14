namespace Models.ConfigurationModels
{
    public class SmsSettingsModel
    {
        public string? User { get; set; }
        public string? Pass { get; set; }
        public string? Msid { get; set; }
        public string? CallbackUrl { get; set; }
    }
}
