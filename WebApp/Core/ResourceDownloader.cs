using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WebApp.Core.Models;
using WebApp.Models;
using WebApp.Utils;

namespace WebApp.Core
{
    public class ResourceDownloader
    {
        private string RootUrl { get; } =
            "https://api.github.com/repos/mdn/browser-compat-data/contents/README.md?ref=master";
        private string GithubAccessToken { get; set; }

        private Logger Logger { get; }
        private Parser Parser { get; }
        private DataRepository DataRepository { get; }
        private IConfiguration Configuration { get; }

        public ResourceDownloader(DataRepository dataRepository, Parser parser, Logger logger, IConfiguration configuration)
        {
            DataRepository = dataRepository;
            Parser = parser;
            Logger = logger;
            Configuration = configuration;
            GithubAccessToken = Configuration["GithubAccessToken"];
        }


        public async Task<bool> CacheAllData()
        {
            Logger.Log("Start");

            try
            {
                var rootItems = await GetRootItems();
                var items = await GetAllItems(rootItems);
                var today = DateTime.Today;
                var mdnData = new MdnData {Timestamp = today, Items = items, Count = items.Count()};

                DataRepository.Remove();
                DataRepository.Add(today, mdnData);

                Logger.Log($@"Success! Downloaded {items.Count()} items. DICTIONARY COUNT: {DataRepository.Count()}");
                
                return true;
            }
            catch (Exception e)
            {
                Logger.Log($@"{e.Message} | STACKTRACE: {e.StackTrace}");
                
                return false;
            }
        }

        private async Task<string> CallApi(string url)
        {
            Logger.Log($@"CallApi: {url}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("Mozilla", "5.0"));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("token",
                    GithubAccessToken);

            using var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        private string GetContent(string response)
        {
            var obj = JObject.Parse(response);
            var token = obj.SelectToken("$.content");
            var contentBase64 = token.Value<string>();
            var contentByte = Convert.FromBase64String(contentBase64);
            var content = Encoding.UTF8.GetString(contentByte);

            return content;
        }

        private List<ParsedDirectoryItemDto> GetArrayContent(string response)
        {
            var results = new List<ParsedDirectoryItemDto>();
            var objects = JArray.Parse(response);

            foreach (var jToken in objects.Children())
            {
                var obj = (JObject) jToken;
                var type = obj.SelectToken("$.type").Value<string>();
                var url = obj.SelectToken("$.url").Value<string>();
                var content = obj.SelectToken("$.content")?.Value<string>();

                results.Add(new ParsedDirectoryItemDto {Type = type, Url = url, Content = content});
            }

            return results;
        }

        private async Task<HashSet<Item>> GetAllItems(HashSet<Item> items)
        {
            var flatItems = new HashSet<Item>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items.ElementAt(i);
                if (item.Type == ItemType.Dir || item.Type == ItemType.DirFile)
                {
                    var fetchedItems = await DownloadData(item);
                    flatItems.UnionWith(await GetAllItems(fetchedItems));
                }
                else
                {
                    flatItems.UnionWith(items);
                    return flatItems;
                }
            }

            return flatItems;
        }

        private async Task<HashSet<Item>> GetRootItems()
        {
            return await DownloadData();
        }

        private async Task<HashSet<Item>> DownloadData(Item item = null)
        {
            var items = new HashSet<Item>();
            var itemInput = item ?? new Item
            {
                Name = "root",
                Url = RootUrl,
                Type = ItemType.Dir,
                Breadcrumbs = new List<string>(),
            };

            try
            {
                var response = await CallApi(itemInput.Url);

                if (itemInput.Name == "root")
                {
                    var content = GetContent(response);
                    items = Parser.GetRootDirectories(content);
                }
                else if (response.StartsWith('['))
                {
                    var tempItemsArray = GetArrayContent(response);
                    items = Parser.GetDirectories(tempItemsArray, itemInput);
                }
                else
                {
                    var content = GetContent(response);
                    items = Parser.GetElements(content, itemInput);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($@"{ex.Message} | STACKTRACE: {ex.StackTrace}  | CallApi: {item?.Url}");
            }

            return items;
        }
    }
}
