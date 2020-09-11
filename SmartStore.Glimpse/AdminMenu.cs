using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Glimpse
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("Glimpse Web Debugger")
                .ResKey("Plugins.FriendlyName.SmartStore.Glimpse")
                .Icon("fa fa-bug")
                .Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.Glimpse", area = "Admin" })
                .ToItem();

            pluginsNode.Prepend(menuItem);
        }

    }
}
