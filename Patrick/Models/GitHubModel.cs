namespace Patrick.Models
{
    class GitHubModel
    {
		public string? AppId { get; set; }
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public string? Username { get; set; }
		public string? Password { get; set; }
        public string? RedirectUrl { get; set; }
        public string? TargetRedirectKey { get; set; }
        public string[]? Scopes { get; set; }
    }
}
