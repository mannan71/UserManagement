using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UserManagementUI.Startup))]
namespace UserManagementUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
