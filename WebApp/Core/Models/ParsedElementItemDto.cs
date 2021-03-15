using System.Collections.Generic;

namespace WebApp.Core.Models
{
    public class ParsedElementItemDto
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public List<string> Breadcrumbs { get; set; }
    }
}
