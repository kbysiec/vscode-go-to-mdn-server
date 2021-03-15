using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MdnData
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public HashSet<Item> Items { get; set; }
        public int Count { get; set; }
    }
}
