using Patrick.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
