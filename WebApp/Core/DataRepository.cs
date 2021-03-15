using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Core
{
    public class DataRepository
    {
        private ConcurrentDictionary<DateTime, MdnData> DataByDate { get; } = new ConcurrentDictionary<DateTime, MdnData>();
        private ApplicationDbContext ApplicationDbContext { get; }


        public DataRepository(ApplicationDbContext applicationDbContext)
        {
            ApplicationDbContext = applicationDbContext;
        }

        public bool Add(DateTime dateTime, MdnData data)
        {
            ApplicationDbContext.Items.AddRange(data.Items);
            ApplicationDbContext.MdnData.Add(data);

            ApplicationDbContext.SaveChanges();
            return DataByDate.TryAdd(dateTime, data);
        }

        public void Remove()
        {
            var items = ApplicationDbContext.Items;
            ApplicationDbContext.Items.RemoveRange(items);

            var mdnData = ApplicationDbContext.MdnData;
            ApplicationDbContext.MdnData.RemoveRange(mdnData);

            ApplicationDbContext.SaveChanges();
            DataByDate.Clear();
        }

        public MdnData GetLast()
        {
            var mdnData = ApplicationDbContext.MdnData.Include(x => x.Items).FirstOrDefault();
            return mdnData ?? new MdnData();
        }

        public bool Exists(DateTime dateTime)
        {
            var key = DataByDate.Keys.FirstOrDefault(keyDt => DateTime.Compare(keyDt, dateTime) == 0);

            if (key == default)
            {
                var mdnData = ApplicationDbContext.MdnData.FirstOrDefault(mdn => DateTime.Compare(mdn.Timestamp, dateTime) == 0);
                if (mdnData != null && mdnData.Count > 0)
                {
                    key = mdnData.Timestamp;
                }
            }

            return key != default;
        }

        public int Count()
        {
            return ApplicationDbContext.Items.Count();
        }
    }
}
