using Octokit;
using Patrick.Helpers;
using Patrick.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class GistGithubService : IGistGithubService
    {
        private readonly GitHubClient client =
            new GitHubClient(new ProductHeaderValue("Patrick-Star-Helper"));
        private readonly PuppeteerSharp.BrowserFetcher browserFetcher =
            new PuppeteerSharp.BrowserFetcher();

        private readonly GitHubModel gitHubModel;

        public bool IsAuthenticated { get; private set; }

        private readonly ICredentialStore credentialStore;

        public GistGithubService(
            IAppConfigProvider configProvider,
            ICredentialStore credentialStore)
        {
            this.credentialStore = credentialStore;

            gitHubModel = configProvider.Configuration.GitHub!;
            browserFetcher.DownloadProgressChanged += Fetcher_DownloadProgressChanged;
        }

        private void Fetcher_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("Downloading Chromium: {0}%", e.ProgressPercentage);
        }

        public async Task<bool> Authenticate()
        {
            var accessToken = await credentialStore.LoadAccessToken();

            //if (!string.IsNullOrEmpty(accessToken))
            //{
            //    IsAuthenticated = true;
            //    client.Credentials = new Credentials(accessToken);
            //    return true;
            //}

            var loginRequest = new OauthLoginRequest(gitHubModel.ClientId);
            foreach (var scope in gitHubModel.Scopes!)
                loginRequest.Scopes.Add(scope);

            var redirectPage = client.Oauth.GetGitHubLoginUrl(loginRequest);
            client.Credentials = await FetchGitCredential(redirectPage);

            await credentialStore.StoreAccessToken(client.Credentials.GetToken());

            IsAuthenticated = true;

            return true;
        }

        public async Task<string?> Create(GistModel gist)
        {
            try
            {
                var result = await client.Gist.Create(new NewGist
                {
                    Files =
                    {
                        [gist.Name] = gist.Content
                    },
                    Description = gist.Description
                });

                return result?.Id;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }

        public async Task<string?> Update(string id, GistModel gist)
        {
            var result = await client.Gist.Edit(id, new GistUpdate
            {
                Files =
                {
                    [gist.Name] = new GistFileUpdate
                    {
                        Content = gist.Content
                    }
                },
                Description = gist.Description
            });

            return result?.Id;
        }

        public async Task<GistModel?> Find(string id)
        {
            var result = await client.Gist.Get(id);
            if (result != null)
            {
                var firstEntry = result.Files.FirstOrDefault();
                return new GistModel(firstEntry.Value.Filename, firstEntry.Value.Content)
                {

                };
            }

            return null;
        }

        private async Task<Credentials> FetchGitCredential(Uri redirectPage)
        {
            var targetUrl = await GetRedirectCallbackResult(redirectPage);
            var queryString = QueryStringHelper.ToDictionary(targetUrl);
            var code = queryString[gitHubModel.TargetRedirectKey!];
            var result = await client.Oauth.CreateAccessToken(
                new OauthTokenRequest(gitHubModel.ClientId, gitHubModel.ClientSecret, code));

            return new Credentials(result.AccessToken);
        }

        private async Task<Uri> GetRedirectCallbackResult(Uri redirectPage)
        {
            var revisionInfo = await browserFetcher
                .DownloadAsync(PuppeteerSharp.BrowserFetcher.DefaultRevision);
            using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new PuppeteerSharp.LaunchOptions
            {
                Headless = false,
                LogProcess = true,
                DumpIO = true
            });

            var page = await browser.NewPageAsync();
            var redirectResult = await page.GoToAsync(redirectPage.AbsoluteUri);
            var target = await browser.WaitForTargetAsync(e => e.Url.Contains(gitHubModel.RedirectUrl!));

            return new Uri(target.Url);
        }
    }
}
