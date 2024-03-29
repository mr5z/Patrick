﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Patrick.Helpers;
using Patrick.Models;
using Patrick.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Patrick.Commands
{
    // TODO refactor this
    // this is a fucking mess!
    class CustomCommand : BaseCommand
    {
        enum Parameters { Alias, Type, Method, Content, Path, Input }
        enum ResponseType { Consume, Ignore }
        enum Method { Get, Post }
        enum ContentType { Json, Form, Multi }
        enum InputType { Plain, Json }

		private class OptionA
        {
			private readonly CliHelper.OptionResult<Parameters> options;
            public OptionA(CliHelper.OptionResult<Parameters> options)
            {
				this.options = options;
            }
			public string? Alias => options.TryGetFirst(Parameters.Alias, out var value) ? value : null;
			public string? JsonPath => options.TryGetFirst(Parameters.Path, out var value) ? value : null;
			public ResponseType ResponseType => ParseResponseType(options.TryGetFirst(Parameters.Type, out var value) ? value : null);
			public Method Method => ParseMethod(options.TryGetFirst(Parameters.Method, out var value) ? value : null);
			public ContentType ContentType => ParseContentType(options.TryGetFirst(Parameters.Content, out var value) ? value : null);
			public InputType InputType => ParseInputType(options.TryGetFirst(Parameters.Input, out var value) ? value : null);
		}

		private readonly IHttpService? httpService;

        public CustomCommand(IHttpService httpService) : this(string.Empty)
        {
			this.httpService = httpService;
		}

		public CustomCommand() : this(string.Empty)
		{
		}

		public CustomCommand(string name) : base(name)
        {
            IsNative = false;
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            var oldComponents = OldArguments!.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var api = oldComponents!.First();
			if (oldComponents?.Length <= 1)
            {
				oldComponents = new string[] { api, "-m get -t ignore" };
            }

            if (string.IsNullOrEmpty(api))
				return new CommandResponse(Name, OldArguments);

            if (Uri.TryCreate(api, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
				var parameters = oldComponents!.Last();
				var options = CliHelper.ParseOptions(parameters,
					new CliHelper.Option<Parameters>(Parameters.Alias, "-a", "--alias"),
					new CliHelper.Option<Parameters>(Parameters.Content, "-c", "--content"),
					new CliHelper.Option<Parameters>(Parameters.Method, "-m", "--method"),
					new CliHelper.Option<Parameters>(Parameters.Type, "-t", "--type"),
					new CliHelper.Option<Parameters>(Parameters.Path, "-p", "--path"),
					new CliHelper.Option<Parameters>(Parameters.Input, "-i", "--input")
				);
				var opt = new OptionA(options);
				var argsCount = Regex.Matches(api, "({\\d+})").Count;

				if (argsCount > 0 && user.MessageArgument == null)
					return new CommandResponse(Name,
						$"Args count mismatch. Expecting {argsCount}, found none.");

				var param = HttpUtility.HtmlDecode(user.MessageArgument ?? string.Empty);
				var args = param?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				args = CliHelper.CombineOption(args, ' ').ToArray();
				if (args.Length != argsCount)
					return new CommandResponse(Name, 
						$"Args count mismatch. Expecting {argsCount}, found {args.Length}");

				if (opt.InputType == InputType.Plain)
					args = args.Select(HttpUtility.UrlEncode).ToArray();
				api = HttpUtility.HtmlDecode(string.Format(api, args));

				if (opt.Method == Method.Get && opt.ResponseType == ResponseType.Ignore)
                {
					var alias = opt.Alias?.Trim();
					UseEmbed = !string.IsNullOrEmpty(alias);
					return new CommandResponse(Name, 
						string.IsNullOrEmpty(alias) ? api : $"[{alias}]({api})");
				}

				var timeout = TimeSpan.FromSeconds(10);
				try
                {
					var cts = new CancellationTokenSource(timeout);
					var apiResponse = await Fetch(api, opt.Method, opt.ContentType, opt.InputType, cts.Token);
					var stringContent = apiResponse ?? "{}";
					if (string.IsNullOrEmpty(opt.JsonPath))
                    {
						return new CommandResponse(Name, stringContent);
					}

					string? response = ExtractStringFrom(stringContent, opt.JsonPath);
					return new CommandResponse(Name, response ?? stringContent);
				}
                catch (OperationCanceledException)
				{
					return new CommandResponse(Name, "API timeout!");
				}
            }

			return new CommandResponse(Name, OldArguments);
        }

		private static string? ExtractStringFrom(string content, string jsonPath)
        {
			try
			{
				var obj = JObject.Parse(content);
				return obj.SelectToken(jsonPath)?.ToString();
			}
			catch (JsonReaderException)
			{
				try
				{
					var arr = JArray.Parse(content);
					return arr.SelectToken(jsonPath)?.ToString();
				}
				catch (JsonReaderException) { }
			}
			return null;
		}

		private async Task<string?> Fetch(string api, Method method, ContentType contentType, InputType inputType, CancellationToken cancellationToken)
        {
			var httpService = this.httpService!;

			var components = api.Split('?', 2, StringSplitOptions.RemoveEmptyEntries);
			var domain = components.First()!;
			switch (method)
            {
				case Method.Post:
                    {
						switch (contentType)
						{
							case ContentType.Json:
							default:
								var d1 = inputType == InputType.Plain ? 
										(components.Length > 1 ? QueryStringHelper.ToObject<object>(components.Last()) : null) :
										(components.Length > 1 ? components.Last() : null);
								var r1 = await httpService.PostJson<object>(new Uri(domain), d1?.ToString(), cancellationToken!);
								return r1?.ToString();
							case ContentType.Form:
								var d2 = components.Length > 1 ? QueryStringHelper.ToDictionary(components.Last()) : new Dictionary<string, string>();
								var r2 = await httpService.PostUrlEncoded<object>(new Uri(domain), d2!, cancellationToken!);
								return r2?.ToString();
							case ContentType.Multi:
								break;
                        }
                    }
					break;
				case Method.Get:
				default:
                    {
						return await httpService.GetString(new Uri(api), cancellationToken!);
					}
            }

			return null;
        }

		private static ResponseType ParseResponseType(string? value)
		{
			return value switch
			{
				"consume" => ResponseType.Consume,
				"ignore" => ResponseType.Ignore,
				_ => ResponseType.Ignore
			};
		}

		private static Method ParseMethod(string? value)
		{
			return value switch
			{
				"get" => Method.Get,
				"post" => Method.Post,
				_ => Method.Get
			};
		}

		private static ContentType ParseContentType(string? value)
		{
			return value switch
			{
				"json" => ContentType.Json,
				"form" => ContentType.Form,
				"multi" => ContentType.Multi,
				_ => ContentType.Json
			};
		}

		private static InputType ParseInputType(string? value)
		{
			return value switch
			{
				"json" => InputType.Json,
				"plain" => InputType.Plain,
				_ => InputType.Plain
			};
		}

	}
}
