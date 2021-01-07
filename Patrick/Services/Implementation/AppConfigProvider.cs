using Patrick.Models;

namespace Patrick.Services.Implementation
{
    class AppConfigProvider : IAppConfigProvider
    {
        public AppConfigProvider(AppConfiguration configuration)
        {
            Configuration = configuration;
        }

        public AppConfiguration Configuration { get; }
    }
}
