using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Our.Umbraco.ContentAppGroupPermissions.Core.Compose
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ContentAppPermissionsComposer : ComponentComposer<ContentAppPermissionsComponent>
    {
    }

    public class ContentAppPermissionsComponent : IComponent
    {
        public void Initialize()
        {
            EditorModelEventManager.SendingMediaModel += EditorModelEventManager_SendingMediaModel;
            EditorModelEventManager.SendingContentModel += EditorModelEventManager_SendingContentModel;
        }

        public void Terminate()
        {
        }

        private void EditorModelEventManager_SendingMediaModel(HttpActionExecutedContext sender, EditorModelEventArgs<MediaItemDisplay> e)
        {
            e.Model.ContentApps = GetAllowedContentApps(e.UmbracoContext.Security.CurrentUser, e.Model.ContentApps);
        }

        private void EditorModelEventManager_SendingContentModel(HttpActionExecutedContext sender, EditorModelEventArgs<ContentItemDisplay> e)
        {
            e.Model.ContentApps = GetAllowedContentApps(e.UmbracoContext.Security.CurrentUser, e.Model.ContentApps);
        }

        private IEnumerable<ContentApp> GetAllowedContentApps(IUser user, IEnumerable<ContentApp> contentApps)
        {
            // Read the app setting for restricting content apps
            var contentAppsRestrictedByGroups = ConfigurationManager.AppSettings["ContentAppsRestrictedByGroup"]?.Split(',');

            if (contentAppsRestrictedByGroups != null)
            {
                // Loop through the content app settings to find the content app name and allowed group list
                foreach (var contentAppSetting in contentAppsRestrictedByGroups.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    var contentAppName = contentAppSetting.Split('[').FirstOrDefault();
                    var allowedGroupList = GetAllowedGroupList(contentAppSetting);
                    if (!string.IsNullOrWhiteSpace(contentAppName) && allowedGroupList != null)
                    {
                        // check if the user is in any of the allowed groups
                        // if they are not, then remove the content app from the list
                        contentApps = UserIsInAnyOfTheseGroups(user, allowedGroupList)
                            ? contentApps
                            : contentApps.Where(x => x.Name != contentAppName);
                    }
                }
            }

            return contentApps;
        }

        // Reads the app setting and returns a list of group names that are allowed to use this content app
        private static string[] GetAllowedGroupList(string contentAppSetting)
        {
            var groupNamesStart = contentAppSetting.IndexOf('[');
            var groupNamesEnd = contentAppSetting.IndexOf(']');
            if (groupNamesStart != -1 && groupNamesEnd != -1)
            {
                var allowedGroups = contentAppSetting.Substring(groupNamesStart + 1, (groupNamesEnd - groupNamesStart) - 1);
                var allowedGroupList = allowedGroups.Split('|');
                return allowedGroupList;
            }

            return null;
        }

        private static bool UserIsInAnyOfTheseGroups(IUser user, IEnumerable<string> groupNames)
        {
            return user?.Groups?.Any(x => groupNames.Contains(x.Name)) ?? false;
        }
    }
}