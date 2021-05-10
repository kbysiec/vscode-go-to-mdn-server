using Microsoft.Extensions.Configuration;

namespace WebApp.Core
{
    public class UrlNormalizer
    {
        private string From { get; } = "https://github.com/";
        private string To { get; } = "https://api.github.com/";
        private string QueryString { get; set; }
        private string MdnRepositoryBranchNameForUrls { get; set; }
        
        private IConfiguration Configuration { get; }

        public UrlNormalizer(IConfiguration configuration)
        {
            Configuration = configuration;
            MdnRepositoryBranchNameForUrls = Configuration["MdnRepositoryBranchNameForUrls"];
            QueryString = $"?ref={MdnRepositoryBranchNameForUrls}";
        }

        public string NormalizeUrl(string url)
        {
            var splitPathName = GetPathNameSplit(url);
            return $@"{To}repos/{splitPathName[0]}/{splitPathName[1]}/contents/{splitPathName[4]}{QueryString}";
        }

        public string GetNameFromUrl(string url)
        {
            var splitPathName = GetPathNameSplit(url);
            return splitPathName[splitPathName.Length - 1];
        }

        private string[] GetPathNameSplit(string url)
        {
            return url
                .Replace(From, "")
                .Replace(To, "")
                .Replace(QueryString, "")
                .Split('/');
        }
    }

}
