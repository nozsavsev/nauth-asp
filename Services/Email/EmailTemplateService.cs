using nauth_asp.Models;
using nauth_asp.Repositories;

namespace nauth_asp.Services
{
    public class EmailTemplateService : GenericService<DB_EmailTemplate>
    {
        private readonly EmailTemplateRepository _templateRepository;
        public EmailTemplateService(EmailTemplateRepository templateRepository) : base(templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public async Task<DB_EmailTemplate?> PopulateEmailTemplateAsync(long id, object parameters)
        {
            return await PopulateEmailTemplateAsync(await _templateRepository.GetByIdAsync(id), parameters);

        }

        public async Task<DB_EmailTemplate?> PopulateEmailTemplateAsync(EmailTemplateType type, object parameters)
        {
            return await PopulateEmailTemplateAsync(await _templateRepository.MostRelevantByTypeAsync(type), parameters);
        }

        public async Task<DB_EmailTemplate?> PopulateEmailTemplateAsync(DB_EmailTemplate? template, object parameters)
        {
            if (template == null)
                return null;

            foreach (var prop in parameters.GetType().GetProperties())
            {
                var value = prop.GetValue(parameters)?.ToString() ?? string.Empty;
                template.Subject = template.Subject.Replace($"{{{{{prop.Name}}}}}", value);
                template.Body = template.Body.Replace($"{{{{{prop.Name}}}}}", value);
                template.HtmlBody = template.HtmlBody.Replace($"{{{{{prop.Name}}}}}", value);
            }

            return template;
        }

        public async Task<DB_EmailTemplate?> CreateEmailTemplateAsync(CreateEmailTemplateDTO template)
        {

            if (template == null) return null;


            if (template.isActive)
            {
                var otherTemplates = await _templateRepository.DynamicQueryManyAsync(q =>
                    q.Where(t => t.Type == template.Type && t.isActive));

                if (otherTemplates.Any())
                {
                    foreach (var other in otherTemplates)
                    {
                        other.isActive = false;
                    }
                    await _templateRepository.UpdateManyAsync(otherTemplates);
                }
            }

            var newTemplate = new DB_EmailTemplate
            {
                Name = template.Name,
                isActive = template.isActive,
                Type = template.Type,
                Subject = template.Subject,
                Body = template.Body,
                HtmlBody = template.htmlBody,
            };

            return await _templateRepository.AddAsync(newTemplate);
        }

        public async Task<DB_EmailTemplate?> UpdateEmailTemplateAsync(EmailTemplateDTO template)
        {

            if (template == null) return null;

            var templateToUpdate = await _templateRepository.GetByIdAsync(long.Parse(template.Id));

            if (templateToUpdate == null) return null;

            if (template.isActive)
            {
                var otherTemplates = await _templateRepository.DynamicQueryManyAsync(q =>
                    q.Where(t => t.Type == template.Type && t.isActive && t.Id != templateToUpdate.Id));

                if (otherTemplates.Any())
                {
                    foreach (var other in otherTemplates)
                    {
                        other.isActive = false;
                    }
                    await _templateRepository.UpdateManyAsync(otherTemplates);
                }
            }
            else
            {
                if (templateToUpdate.isActive)
                {
                    var templatesOfType = await _templateRepository.DynamicQueryManyAsync(q => q.Where(t => t.Type == templateToUpdate.Type));
                    if (templatesOfType.Count <= 1)
                    {
                        template.isActive = true;
                    }
                }
            }

            templateToUpdate.Name = template.Name;
            templateToUpdate.isActive = template.isActive;
            templateToUpdate.Type = template.Type;
            templateToUpdate.Subject = template.Subject;
            templateToUpdate.Body = template.Body;
            templateToUpdate.HtmlBody = template.htmlBody;

            return await _templateRepository.UpdateAsync(templateToUpdate);
        }

        public async Task DeleteEmailTemplateAsync(long id)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template != null)
            {
                await _templateRepository.DeleteAsync(template);
            }
        }

        public async Task<List<DB_EmailTemplate>> GetAllAsync()
        {
            return await _templateRepository.DynamicQueryManyAsync();
        }
    }
}
