using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface IRepository
	{
		Task<HashSet<T>> GetList<T>(string collectionName, CancellationToken cancellationToken = default);
		Task<string?> Add<T>(string collectionName, T value, CancellationToken cancellationToken = default);
		Task<bool> Remove<T>(string collectionName, T value, CancellationToken cancellationToken = default);
		Task<bool> Update<T>(string collectionName, string key, T value, CancellationToken cancellationToken = default);
		void Clear(string collectionName);
		void ClearAll();
	}
}
