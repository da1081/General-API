using MailTemplates.Models;

namespace MailTemplate
{
    public interface ITemplateRenderService
    {
        /// <summary>
        /// Generates a generic pin template using data specified in the GenericPinTemplateModel.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Rendered email html string.</returns>
        Task<string> GenericPinTemplateAsync(GenericPinTemplateModel model);
    }
}
