using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class HttpService : IHttpService
	{
		private readonly HttpClient httpClient = new HttpClient();

		public HttpService() { }
		public HttpService(Uri baseAddress) => httpClient.BaseAddress = baseAddress;

		public Task<T?> Get<T>(string relativePath, CancellationToken cancellationToken)
			=> Get<T>(new Uri(relativePath, UriKind.Relative), cancellationToken);

		public Task<string?> GetString(string relativePath, CancellationToken cancellationToken)
			=> GetString(new Uri(relativePath, UriKind.Relative), cancellationToken);

		public Task<T?> PostJson<T>(string relativePath, object? data, CancellationToken cancellationToken)
			=> PostJson<T?>(new Uri(relativePath, UriKind.Relative), data, cancellationToken);

		public Task<T?> PostUrlEncoded<T>(string relativePath, IDictionary<string, string?> data, CancellationToken cancellationToken)
			=> PostUrlEncoded<T?>(new Uri(relativePath, UriKind.Relative), data, cancellationToken);

		public Task<T?> PostMultipart<T>(string relativePath, IDictionary<string, object?> data, CancellationToken cancellationToken)
			=> PostMultipart<T?>(new Uri(relativePath, UriKind.Relative), data, cancellationToken);

		public Task<T?> PutForm<T>(string relativePath, IDictionary<string, string?> data, CancellationToken cancellationToken)
			=> PutForm<T>(new Uri(relativePath, UriKind.Relative), data, cancellationToken);

		public Task<T?> Get<T>(Uri absolutePath, CancellationToken cancellationToken)
			=> httpClient.GetFromJsonAsync<T>(absolutePath, cancellationToken);

		public async Task<string?> GetString(Uri absolutePath, CancellationToken cancellationToken)
		{
			var response = await httpClient.GetAsync(absolutePath, cancellationToken);
			return await response.Content.ReadAsStringAsync(cancellationToken);
		}

		public async Task<T?> PostJson<T>(Uri absolutePath, object? data, CancellationToken cancellationToken)
		{
			var response = await httpClient.PostAsJsonAsync(absolutePath, data, cancellationToken);
			var stream = await response.Content.ReadAsStreamAsync();
			return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
		}

		public async Task<T?> PostUrlEncoded<T>(Uri absolutePath, IDictionary<string, string?> data, CancellationToken cancellationToken)
		{
			var content = DictionaryToUrlEncodedContent(data);
			var response = await httpClient.PostAsync(absolutePath, content, cancellationToken);
			var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
		}

		public async Task<T?> PostMultipart<T>(Uri absolutePath, IDictionary<string, object?> data, CancellationToken cancellationToken)
		{
			var content = DictionaryToMultipartContent(data);
			var response = await httpClient.PostAsync(absolutePath, content, cancellationToken);
			var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
		}

		public Task<HttpResponseMessage> Send(Uri absolutePath, HttpMethod method, HttpContent data, CancellationToken cancellationToken)
		{
			var message = new HttpRequestMessage(method, absolutePath) { Content = data };
			return httpClient.SendAsync(message, cancellationToken);
		}

		public async Task<T?> PutForm<T>(Uri absolutePath, IDictionary<string, string?> data, CancellationToken cancellationToken)
		{
			var content = DictionaryToUrlEncodedContent(data);
			var response = await httpClient.PutAsync(absolutePath, content, cancellationToken);
			var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
		}

		private static FormUrlEncodedContent DictionaryToUrlEncodedContent(IDictionary<string, string?> data) =>
			// What's up with .NET team allowing null keys? 
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            new FormUrlEncodedContent(data);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        private static MultipartFormDataContent DictionaryToMultipartContent(IDictionary<string, object?> data)
		{
			var content = new MultipartFormDataContent();
			foreach (var entry in data)
			{
				if (entry.Value == null)
					continue;

				if (entry.Value is string stringValue)
					content.Add(new StringContent(stringValue), entry.Key);
				else if (entry.Value is bool boolValue)
					content.Add(new StringContent(boolValue.ToString()), entry.Key);
				else if (IsNumber(entry.Value))
					content.Add(new StringContent(entry.Value!.ToString()!), entry.Key);
				else if (entry.Value is byte[] bytesValue)
					content.Add(new ByteArrayContent(bytesValue), entry.Key);
				else
				{
					// TODO in case it's a POCO, should we serialize it as JSON-formatted StringContent?
					throw new ArgumentException("UnsupportedType");
				}
			}

			// TODO wtf?
#pragma warning disable IDE1006 // Naming Styles
			static bool IsNumber(object value)
#pragma warning restore IDE1006 // Naming Styles
			{
				return value is sbyte
					|| value is byte
					|| value is short
					|| value is ushort
					|| value is int
					|| value is uint
					|| value is long
					|| value is ulong
					|| value is float
					|| value is double
					|| value is decimal;
			}
			return content;
		}
    }
}
