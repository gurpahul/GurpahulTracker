using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GurBhugTracker.Startup))]
namespace GurBhugTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
