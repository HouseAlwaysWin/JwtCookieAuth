namespace Server.Models
{
    public class OAuthProviderConfig
    {
        public string ProviderName { get; set; }

        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public string RedirectUrl { get; set; }

        public string Scope { get; set; }
 
    }
}