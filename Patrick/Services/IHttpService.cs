using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services
{
	interface IHttpService
	{
		Task<T?> Get<T>(string relativePath, CancellationToken cancellationToken = default);
		Task<T?> Get<T>(Uri absolutePath, CancellationToken cancellationToken = default);
		Task<string?> GetString(string relativePath, CancellationToken cancellationToken = default);
		Task<string?> GetString(Uri absolutePath, CancellationToken cancellationToken = default);
		Task<T?> PostJson<T>(string relativePath, object? data = null, CancellationToken cancellationToken = default);
		Task<T?> PostJson<T>(Uri absolutePath, object? data = null, CancellationToken cancellationToken = default);
		Task<T?> PostUrlEncoded<T>(string relativePath, IDictionary<string, string?> data, CancellationToken cancellationToken = default);
		Task<T?> PostUrlEncoded<T>(Uri absolutePath, IDictionary<string, string?> data, CancellationToken cancellationToken = default);
		Task<T?> PostMultipart<T>(string relativePath, IDictionary<string, object> data, CancellationToken cancellationToken = default);
		Task<T?> PostMultipart<T>(Uri absolutePath, IDictionary<string, object> data, CancellationToken cancellationToken = default);
		Task<T?> PutForm<T>(string relativePath, IDictionary<string, string?> data, CancellationToken cancellationToken = default);
		Task<T?> PutForm<T>(Uri absolutePath, IDictionary<string, string?> data, CancellationToken cancellationToken = default);
		Task<HttpResponseMessage?> Send(Uri absolutePath, HttpMethod method, HttpContent data, CancellationToken cancellationToken = default);
	}
}
