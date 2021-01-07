﻿using MonkeyCache.SQLite;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services.Repositories
{
    class MonkeyCacheRepository : IRepository
	{
		public MonkeyCacheRepository(string applicationId) =>
			Barrel.ApplicationId = applicationId;

		public async Task<string?> Add<T>(string collectionName, T value, CancellationToken cancellationToken)
		{
			var list = await GetList<T>(collectionName, cancellationToken);
			list.Add(value);
			Barrel.Current.Add(collectionName, list, Timeout.InfiniteTimeSpan);
			// TODO return the inserted id?
			return null;
		}

		public async Task<bool> Remove<T>(string collectionName, T value, CancellationToken cancellationToken)
        {
			var list = await GetList<T>(collectionName, cancellationToken);
			var result = list.Remove(value);
			Barrel.Current.Add(collectionName, list, Timeout.InfiniteTimeSpan);
			return result;
		}

		public async Task<bool> Update<T>(string collectionName, string key, T value, CancellationToken cancellationToken = default)
		{
			var list = await GetList<T>(collectionName, cancellationToken);
			var result = list.Remove(value);
			result = result && list.Add(value);
			Barrel.Current.Add(collectionName, list, Timeout.InfiniteTimeSpan);
			return result;
		}

		public Task<HashSet<T>> GetList<T>(string collectionName, CancellationToken cancellationToken)
		{
			var data = Barrel.Current.Get<HashSet<T>>(collectionName);
			return Task.FromResult(data ?? new HashSet<T>());
		}

		public void Clear(string collectionName) => Barrel.Current.Empty(collectionName);

		public void ClearAll() => Barrel.Current.EmptyAll();
    }
}
