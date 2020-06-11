using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ControlCalidad.Startup))]
namespace ControlCalidad
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
