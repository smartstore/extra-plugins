using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Verizon
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("Verizon SMS Provider")
                .ResKey("Plugins.FriendlyName.SmartStore.Verizon")
                .Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.Verizon", area = "Admin" })
                .ToItem();

            pluginsNode.Prepend(menuItem);
        }

        public override int Ordinal => 100;
    }
}
