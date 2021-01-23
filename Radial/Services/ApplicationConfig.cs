using Microsoft.Extensions.Configuration;

namespace Radial.Services
{
    public interface IApplicationConfig
    {
        string SmtpDisplayName { get; }
        string SmtpEmail { get; }
        bool SmtpEnableSsl { get; }
        string SmtpHost { get; }
        string SmtpPassword { get; }
        int SmtpPort { get; }
        string SmtpUserName { get; }
    }

    public class ApplicationConfig : IApplicationConfig
    {
        public ApplicationConfig(IConfiguration config)
        {
            Config = config;
        }

        public string SmtpDisplayName => Config["ApplicationOptions:SmtpDisplayName"];
        public string SmtpEmail => Config["ApplicationOptions:SmtpEmail"];
        public bool SmtpEnableSsl => bool.Parse(Config["ApplicationOptions:SmtpEnableSsl"] ?? "true");
        public string SmtpHost => Config["ApplicationOptions:SmtpHost"];
        public string SmtpPassword => Config["ApplicationOptions:SmtpPassword"];
        public int SmtpPort => int.Parse(Config["ApplicationOptions:SmtpPort"] ?? "25");
        public string SmtpUserName => Config["ApplicationOptions:SmtpUserName"];

        private IConfiguration Config { get; set; }
    }
}
