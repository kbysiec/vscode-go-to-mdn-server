using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WebApp.Core.Models;
using WebApp.Models;
using WebApp.Utils;

namespace WebApp.Core
{
    public class Parser
    {
        private string RootUrlsRegexPattern { get; set; }

        private string SplitNormalizedNameRegex { get; } = "(?=[A-Z][a-z])";
        private string MdnRepositoryBranchNameForRootUrlsRegexPattern { get; set; }

        private UrlNormalizer UrlNormalizer { get; }
        private IConfiguration Configuration { get; }


        public Parser(UrlNormalizer urlNormalizer, IConfiguration configuration)
        {
            UrlNormalizer = urlNormalizer;
            Configuration = configuration;
            MdnRepositoryBranchNameForRootUrlsRegexPattern = Configuration["MdnRepositoryBranchNameForRootUrlsRegexPattern"];
            RootUrlsRegexPattern = $"https://github.com/mdn/browser-compat-data/tree/{MdnRepositoryBranchNameForRootUrlsRegexPattern}/[a-zA-Z]+";

        }

        public HashSet<Item> GetRootDirectories(string content)
        {
            var rootUrls = GetRootUrls(content);
            var result = new HashSet<Item>();

            for (var i = 0; i < rootUrls.Count; i++)
            {
                var element = rootUrls.ElementAt(i);
                var url = UrlNormalizer.NormalizeUrl(element);
                var name = UrlNormalizer.GetNameFromUrl(url);
                var breadcrumbs = new List<string> {name};
                var breadcrumbsString = breadcrumbs.Join("|");

                var item = new Item
                {
                    Url = url,
                    Type = ItemType.Dir,
                    Name = name,
                    Breadcrumbs = breadcrumbs,
                    BreadcrumbsString = breadcrumbsString,
                    Timestamp = DateTime.Today,
                };
                result.Add(item);
            }

            return result;
        }

        public HashSet<Item> GetDirectories(List<ParsedDirectoryItemDto> tempItemsArray, Item item)
        {
            var result = new HashSet<Item>();

            for (var i = 0; i < tempItemsArray.Count; i++)
            {
                var tempItem = tempItemsArray[i];
                var type = GetItemType(tempItem.Type, tempItem.Content);
                var name = UrlNormalizer.GetNameFromUrl(tempItem.Url);
                name = NormalizeItemName(name);
                var breadcrumbs = new List<string>(item.Breadcrumbs) {name};
                var breadcrumbsString = breadcrumbs.Join("|");

                result.Add(new Item
                {
                    Name = name,
                    Url = tempItem.Url,
                    Type = type,
                    Parent = item,
                    RootParent = item.Parent ?? item,
                    Breadcrumbs = breadcrumbs,
                    BreadcrumbsString = breadcrumbsString,
                    Timestamp = DateTime.Today,
                });
            }

            return result;
        }

        public HashSet<Item> GetElements(string content, Item item)
        {
            var result = new HashSet<Item>();
            if (item.Parent == null || item.RootParent == null)
            {
                return result;
            }

            var parsedItemDtos = GetListOfFileLinksWithBreadcrumbs(content, item);

            for (var i = 0; i < parsedItemDtos.Count(); i++)
            {
                var tempItem = parsedItemDtos[i];

                result.Add(new Item
                {
                    Name = tempItem.Name,
                    Url = tempItem.Url,
                    Type = ItemType.File,
                    Parent = item,
                    RootParent = item.Parent ?? item,
                    Breadcrumbs = tempItem.Breadcrumbs,
                    BreadcrumbsString = tempItem.Breadcrumbs.Join("|"),
                    Timestamp = DateTime.Today,
                });
            }

            return result;
        }

        private HashSet<string> GetRootUrls(string content)
        {
            var regex = new Regex(RootUrlsRegexPattern,
                RegexOptions.Multiline);
            var matches = regex.Matches(content);
            var results = new HashSet<string>();

            foreach (Match match in matches)
            {
                results.Add(match.Value);
            }

            return results;
        }

        private List<ParsedElementItemDto> GetListOfFileLinksWithBreadcrumbs(string response, Item item)
        {
            var parsedResponse = JObject.Parse(response);
            var path = string.Join('.', item.Breadcrumbs.Take(item.Breadcrumbs.Count() - 1));
            var token = parsedResponse.SelectToken($"$.{path}");
            if (token == null)
            {
                path = string.Join('.', item.Breadcrumbs.Take(item.Breadcrumbs.Count()));
                token = parsedResponse.SelectToken($"$.{path}");
            }

            var tokens = token.FindTokens("mdn_url");

            return GetLinkWithBreadcrumbs(tokens);
        }

        private List<ParsedElementItemDto> GetLinkWithBreadcrumbs(IEnumerable<JToken> tokens)
        {
            var results = new List<ParsedElementItemDto>();

            foreach (var token in tokens)
            {
                var url = token.Value<string>();
                var splitPath = token.Path.Split('.');
                var breadcrumbs = splitPath.Take(splitPath.Count() - 2).Select(NormalizeItemName).ToList();
                var name = breadcrumbs.Last();
                breadcrumbs = breadcrumbs.Take(breadcrumbs.Count()).ToList();
                var first = $"{name} - Reference";

                results.Add(!results.Any()
                    ? new ParsedElementItemDto {Name = first, Url = url, Breadcrumbs = breadcrumbs}
                    : new ParsedElementItemDto {Name = name, Url = url, Breadcrumbs = breadcrumbs});
            }

            return results;
        }

        private string NormalizeItemName(string name)
        {
            var normalizedName = name.Replace(".json", "");
            var splitNormalizedName = Regex.Split(normalizedName, SplitNormalizedNameRegex);
            return string.Join(' ', splitNormalizedName.Where(str => !string.IsNullOrWhiteSpace(str)));
        }

        private ItemType GetItemType(string type, string content)
        {
            if (type == ItemType.Dir.ToString().ToLower())
            {
                return ItemType.Dir;
            }

            if (type == ItemType.File.ToString().ToLower() && string.IsNullOrEmpty(content))
            {
                return ItemType.DirFile;
            }

            return ItemType.File;
        }
    }
}
