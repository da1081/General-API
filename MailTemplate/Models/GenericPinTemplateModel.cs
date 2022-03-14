using Models.ConfigurationModels;

namespace MailTemplates.Models
{
    public class GenericPinTemplateModel
    {
        public ApplicationIdentityModel AppIdentity { get; set; } = new();
        public string PinType { get; set; } = string.Empty;
        public string ExpiresIn { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }
}
