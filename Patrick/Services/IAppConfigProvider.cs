using Patrick.Models;

namespace Patrick.Services
{
    interface IAppConfigProvider
    {
        AppConfiguration Configuration { get; }
    }
}
