using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(i4myapps.Startup))]
namespace i4myapps
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
