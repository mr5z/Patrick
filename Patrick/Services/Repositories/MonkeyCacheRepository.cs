using MonkeyCache.SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services.Repositories
{
	class MonkeyCacheRepository : IRepository
	{
		public MonkeyCacheRepository(string applicationId) =>
			Barrel.ApplicationId = applicationId;

		public async Task<string?> Add<T>(string name, T value, CancellationToken cancellationToken)
		{
			var list = await GetList<T>(name, cancellationToken);
			list.Add(value);
			Barrel.Current.Add(name, list, Timeout.InfiniteTimeSpan);
			// TODO return the inserted id?
			return null;
		}

		public async Task<bool> Remove<T>(string name, T value, CancellationToken cancellationToken)
        {
			var list = await GetList<T>(name, cancellationToken);
			var result = list.Remove(value);
			Barrel.Current.Add(name, list, Timeout.InfiniteTimeSpan);
			return result;
		}

		public Task<HashSet<T>> GetList<T>(string name, CancellationToken cancellationToken)
		{
			var data = Barrel.Current.Get<HashSet<T>>(name);
			return Task.FromResult(data ?? new HashSet<T>());
		}

		public void Clear(string collectionName) => Barrel.Current.Empty(collectionName);

		public void ClearAll() => Barrel.Current.EmptyAll();
	}
}
