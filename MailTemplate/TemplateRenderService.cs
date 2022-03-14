using MailTemplates.Models;
using MailTemplates.Properties;
using RazorEngineCore;
using System.Collections.Concurrent;

namespace MailTemplate
{

    public class TemplateRenderService : ITemplateRenderService
    {
        /// <summary>
        /// Thread safe Template cache.
        /// </summary>
        private static readonly ConcurrentDictionary<int, IRazorEngineCompiledTemplate> TemplateCache = new();

        /// <summary>
        /// Run a template render and compile and save a template if template does not exist in Static TemplateCache.
        /// </summary>
        /// <param name="template">Template string</param>
        /// <param name="model">Strongly typed template model</param>
        /// <returns></returns>
        private static async Task<string> RenderTemplateAsync(string template, object model)
        {
            int templateId = template.GetHashCode();
            IRazorEngineCompiledTemplate compiledTemplate = TemplateCache.GetOrAdd(
                templateId, i =>
                {
                    RazorEngine engine = new();
                    return engine.Compile(template); // TODO : Make async?
                });
            return await compiledTemplate.RunAsync(model);
        }

        // Generic PIN-code template.
        public async Task<string> GenericPinTemplateAsync(GenericPinTemplateModel model) =>
            await RenderTemplateAsync(Resources.GenericPinTemplate.ToString(), model);

    }
}
