using Patrick.Models;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface IGistGithubService
    {
        bool IsAuthenticated { get; }
        Task<bool> Authenticate();
        Task<string?> Create(GistModel gist);
        Task<string?> Update(string id, GistModel gist);
        Task<GistModel?> Find(string id);
    }
}
