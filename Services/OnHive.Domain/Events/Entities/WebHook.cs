using OnHive.Core.Library.Enums.WebHook;
using MongoDB.Bson;

namespace OnHive.Core.Library.Entities.Events
{
    public class WebHook : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string ApiKey { get; set; } = string.Empty;

        public bool UseAuthorization { get; set; } = false;

        public string UserId { get; set; } = string.Empty;

        public List<WebHookStep> Steps { get; set; } = [];

        public override bool Equals(object? obj)
        {
            if (obj is WebHook hook)
            {
                return hook.Name == Name && hook.Slug == Slug && hook.Method == Method && hook.Description == Description && hook.ApiKey == ApiKey && hook.UseAuthorization == UseAuthorization && hook.UserId == UserId && hook.Steps.SequenceEqual(Steps);
            }
            return false;
        }
    }

    public class WebHookStep
    {
        public string Name { get; set; } = string.Empty;

        public WebHookStepTypes Type { get; set; } = WebHookStepTypes.None;

        public string? Script { get; set; }

        public List<WebHookAction> Actions { get; set; } = [];

        public override bool Equals(object? obj)
        {
            if (obj is WebHookStep step)
            {
                return step.Name == Name && step.Type == Type && step.Script == Script && step.Actions.SequenceEqual(Actions);
            }
            return false;
        }
    }

    public class WebHookAction
    {
        public WebHookActionTypes Type { get; set; } = WebHookActionTypes.Replace;

        public WebHookFieldSourceTypes SourceType { get; set; } = WebHookFieldSourceTypes.Body;

        public string SourceField { get; set; } = string.Empty;

        public string SourceIndexField { get; set; } = string.Empty;

        public string TargetCollection { get; set; } = string.Empty;

        public string TargetIndexField { get; set; } = string.Empty;

        public string TargetField { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            if (obj is WebHookAction action)
            {
                return action.Type == Type && action.SourceType == SourceType && action.SourceField == SourceField && action.SourceIndexField == SourceIndexField && action.TargetCollection == TargetCollection && action.TargetIndexField == TargetIndexField && action.TargetField == TargetField;
            }
            return false;
        }
    }
}