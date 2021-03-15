using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using WebApp.Core.Models;

namespace WebApp.Models
{
    public class Item
    {

        private List<string> _breadcrumbs;
        public long Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Item Parent { get; set; }
        public Item RootParent { get; set; }
        public ItemType Type { get; set; }

        [NotMapped]
        public List<string> Breadcrumbs
        {
            get { return _breadcrumbs ?? BreadcrumbsString.Split('|').ToList(); }
            set { _breadcrumbs = value; }
        }

        [JsonIgnore]
        [Column("Breadcrumbs")]
        public string BreadcrumbsString { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
