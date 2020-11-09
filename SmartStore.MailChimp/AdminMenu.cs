using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Mailchimp
{
    public class AdminMenu : AdminMenuProvider
    {

        protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("MailChimp E-Mail Synchronization")
                .ResKey("Plugins.FriendlyName.SmartStore.MailChimp")
                .Icon("fab fa-mailchimp")
                .Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.Mailchimp", area = "Admin" })
                .ToItem();

            pluginsNode.Prepend(menuItem);
        }

    }
}
