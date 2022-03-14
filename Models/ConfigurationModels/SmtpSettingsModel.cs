namespace Models.ConfigurationModels
{
    public class SmtpSettingsModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool UseSSL { get; set; }
    }
}
