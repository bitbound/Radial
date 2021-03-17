using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Radial.Areas.Identity.IdentityHostingStartup))]
namespace Radial.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}