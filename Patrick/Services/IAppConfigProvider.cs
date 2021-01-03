using Patrick.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Patrick.Services
{
    interface IAppConfigProvider
    {
        AppConfiguration Configuration { get; }
    }
}
