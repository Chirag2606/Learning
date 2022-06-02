namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System.Threading.Tasks;

    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Admin.Models.Notification;

    using MediatR;

    public static class TemplateViewModelExtensions
    {
        public static async Task<TemplateViewModel> Prime(this TemplateViewModel value, IMediator mediator, int templateId, bool applyDefaults = false)
        {
            if (applyDefaults)
            {
                var template = await mediator.Send(new CustomerNotificationsGetTemplateQuery { TemplateId = templateId });

                if (template.IsSuccess && template.Value != null)
                {
                    value.TemplateId = templateId;
                    value.Subject = template.Value.Subject;
                    value.TemplateName = template.Value.Name;
                    value.MessageBody = template.Value.Message;
                }
            }
            
            value.TagsJson = SupportedTags.CreateJson();

            return value;
        }
    }
}